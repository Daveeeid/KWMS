using System.Text;
using KWMS.Data;
using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    /// <summary>TEMPORARY: exercises every repository. Deleted after Stage 3 is confirmed.</summary>
    public class RepositoryTestController
    {
        private readonly CompanyRepository _companies = new CompanyRepository();
        private readonly UserRepository _users = new UserRepository();
        private readonly ResidentRepository _residents = new ResidentRepository();
        private readonly WorkerRepository _workers = new WorkerRepository();
        private readonly ServiceRepository _services = new ServiceRepository();
        private readonly SubscriptionRepository _subscriptions = new SubscriptionRepository();
        private readonly ScheduleRepository _schedules = new ScheduleRepository();
        private readonly PickupRequestRepository _requests = new PickupRequestRepository();
        private readonly TaskRepository _tasks = new TaskRepository();
        private readonly PaymentRepository _payments = new PaymentRepository();
        private readonly ComplaintRepository _complaints = new ComplaintRepository();

        private StringBuilder _report = new StringBuilder();

        private void Check(string description, bool passed) =>
            _report.AppendLine((passed ? "✔ " : "✘ ") + description);

        public string RunTests()
        {
            _report = new StringBuilder();
            try
            {
                RunAllChecks();
                _report.AppendLine();
                _report.AppendLine("Finished. Run Database/03_cleanup_stage3_test_data.sql to remove the test rows.");
            }
            catch (Exception ex)
            {
                _report.AppendLine();
                _report.AppendLine("✘ Test stopped by an error: " + ex.Message);
            }
            return _report.ToString();
        }

        private void RunAllChecks()
        {
            string tag = DateTime.Now.ToString("HHmmss");
            const int WrongId = 999999;   // an ID that cannot exist, used to simulate "someone else's data"

            // ---------- Companies ----------
            Company company = new Company
            {
                CompanyName = "S3 Test Company " + tag,
                RegistrationNumber = "S3TEST-" + tag,
                Phone = "0780000000",
                Address = "Kigali"
            };
            int companyId = _companies.Insert(company);
            Check("Company inserted", companyId > 0);
            Check("Registration number now exists", _companies.RegistrationNumberExists(company.RegistrationNumber));
            Company? loadedCompany = _companies.GetById(companyId);
            Check("Company read back correctly", loadedCompany != null && loadedCompany.CompanyName == company.CompanyName);
            Check("Search finds the company", _companies.GetAll(tag).Any(c => c.CompanyID == companyId));

            // ---------- Manager user ----------
            User manager = new User
            {
                CompanyID = companyId,
                UserName = "s3test_mgr_" + tag,
                Email = $"s3test_mgr_{tag}@example.com",
                PasswordHash = "dummy-hash",
                Role = UserRole.CompanyManager,
                AccountStatus = RecordStatus.Active
            };
            int managerUserId = _users.Insert(manager);
            Check("Manager user inserted", managerUserId > 0);
            User? loadedManager = _users.GetByUserName(manager.UserName);
            Check("Role round-trips (COMPANY_MANAGER <-> CompanyManager)",
                loadedManager != null && loadedManager.Role == UserRole.CompanyManager);
            Check("User lists never expose password hashes",
                _users.GetAll(manager.UserName).All(u => u.PasswordHash == ""));

            // ---------- Service ----------
            Service service = new Service
            {
                CompanyID = companyId,
                ServiceName = "Weekly pickup " + tag,
                Description = "Test service",
                MonthlyPrice = 5000,
                CollectionFrequency = CollectionFrequency.Weekly
            };
            int serviceId = _services.Insert(service);
            Check("Service inserted", serviceId > 0);
            Check("Service available to residents", _services.GetAvailableByCompany(companyId).Count == 1);

            // ---------- Resident registration (transaction) ----------
            User residentUser = new User
            {
                UserName = "s3test_res_" + tag,
                Email = $"s3test_res_{tag}@example.com",
                PasswordHash = "dummy-hash"
            };
            Resident resident = new Resident
            {
                FullName = "S3 Test Resident",
                Phone = "0781111111",
                District = "Gasabo",
                Sector = "Remera",
                HouseAddress = "KG 1 Ave",
                HouseholdSize = 4
            };
            int residentId = _residents.Register(residentUser, resident);
            Check("Resident registered (Users + Residents in one transaction)", residentId > 0);

            bool duplicateBlocked = false;
            try { _residents.Register(residentUser, resident); }
            catch (MySqlException ex) when (DbHelper.IsDuplicateKey(ex)) { duplicateBlocked = true; }
            Check("Duplicate user name rejected (and rolled back)", duplicateBlocked);

            // ---------- Subscriptions ----------
            int subscriptionId = _subscriptions.Subscribe(residentId, serviceId);
            Check("Resident subscribed", subscriptionId > 0);

            bool secondBlocked = false;
            try { _subscriptions.Subscribe(residentId, serviceId); }
            catch (MySqlException ex) when (DbHelper.IsDuplicateKey(ex)) { secondBlocked = true; }
            Check("Second active subscription blocked by the database (rule 2)", secondBlocked);
            Check("GetActiveByResident finds the subscription",
                _subscriptions.GetActiveByResident(residentId)?.SubscriptionID == subscriptionId);
            Check("Manager sees the subscribed resident", _subscriptions.GetByCompany(companyId).Count == 1);

            // ---------- Pickup request -> task ----------
            PickupRequest request = new PickupRequest
            {
                SubscriptionID = subscriptionId,
                RequestedDate = DateTime.Today.AddDays(2),
                Description = "Extra garden waste"
            };
            int requestId = _requests.Create(request, residentId);
            Check("Pickup request created for an active subscription (rule 3)", requestId > 0);
            Check("Pickup request refused for someone else's subscription",
                _requests.Create(request, residentId + WrongId) == 0);
            Check("Manager approves the request", _requests.Approve(requestId, companyId));
            Check("Approving the same request twice fails", !_requests.Approve(requestId, companyId));

            List<CollectionTask> tasks = _tasks.GetByCompany(companyId);
            Check("Pending task created automatically on approval",
                tasks.Count > 0 && tasks[0].Status == CollectionTaskStatus.Pending);
            int taskId = tasks.Count > 0 ? tasks[0].TaskID : 0;

            // ---------- Worker and task workflow ----------
            User workerUser = new User
            {
                UserName = "s3test_wrk_" + tag,
                Email = $"s3test_wrk_{tag}@example.com",
                PasswordHash = "dummy-hash"
            };
            Worker worker = new Worker { CompanyID = companyId, FullName = "S3 Test Worker", Phone = "0782222222" };
            int workerId = _workers.Add(workerUser, worker);
            Check("Worker added (Users + Workers in one transaction)", workerId > 0);

            Check("Unassigned task cannot be started (rule 7)", !_tasks.Start(taskId, workerId));
            Check("Another company cannot assign the task (rule 5)",
                !_tasks.AssignWorker(taskId, workerId, companyId + WrongId));
            Check("Task assigned to an active worker", _tasks.AssignWorker(taskId, workerId, companyId));
            Check("Assigned task cannot be completed before starting (rule 8)", !_tasks.Complete(taskId, workerId));
            Check("A different worker cannot start it (rule 6)", !_tasks.Start(taskId, workerId + WrongId));
            Check("Worker starts the task", _tasks.Start(taskId, workerId));
            Check("Worker completes the task", _tasks.Complete(taskId, workerId));
            Check("Pickup request became Completed",
                _requests.GetByResident(residentId).Any(r => r.RequestID == requestId && r.Status == PickupRequestStatus.Completed));
            Check("Worker sees the task in their own list",
                _tasks.GetByWorker(workerId).Any(t => t.TaskID == taskId && t.Status == CollectionTaskStatus.Completed));

            // ---------- Schedule + inactive worker ----------
            CollectionSchedule schedule = new CollectionSchedule
            {
                SubscriptionID = subscriptionId,
                CollectionDate = DateTime.Today.AddDays(7),
                CollectionTime = new TimeSpan(9, 0, 0),
                Notes = "Morning round"
            };
            int scheduleId = _schedules.Create(schedule, companyId);
            Check("Schedule created", scheduleId > 0);
            Check("Schedule refused for another company", _schedules.Create(schedule, companyId + WrongId) == 0);

            CollectionTask? scheduleTask = _tasks.GetByCompany(companyId).FirstOrDefault(t => t.ScheduleID == scheduleId);
            Check("Task created automatically with the schedule", scheduleTask != null);

            Check("Worker deactivated", _workers.SetStatus(workerId, companyId, RecordStatus.Inactive));
            Check("Worker's login account is deactivated too",
                _users.GetByUserName(workerUser.UserName)?.AccountStatus == RecordStatus.Inactive);
            Check("Inactive worker cannot receive tasks (rule 13)",
                scheduleTask != null && !_tasks.AssignWorker(scheduleTask.TaskID, workerId, companyId));

            // ---------- Payments ----------
            Payment payment = new Payment
            {
                SubscriptionID = subscriptionId,
                RecordedByUserID = managerUserId,
                Amount = 5000,
                PaymentMethod = PaymentMethod.MobileMoney,
                PaymentDate = DateTime.Today,
                ReferenceNumber = "MM-" + tag
            };
            Check("Payment recorded by the manager", _payments.Record(payment, companyId) > 0);
            Check("Another company cannot record a payment", _payments.Record(payment, companyId + WrongId) == 0);
            List<Payment> history = _payments.GetByResident(residentId);
            Check("Payment history shows 'Mobile Money' correctly",
                history.Count == 1 && history[0].PaymentMethod == PaymentMethod.MobileMoney);

            // ---------- Complaints ----------
            Complaint complaint = new Complaint
            {
                ResidentID = residentId,
                CompanyID = companyId,
                Subject = "Missed pickup",
                Description = "Nobody came on the scheduled day."
            };
            int complaintId = _complaints.Create(complaint);
            Check("Complaint created", complaintId > 0);
            ComplaintResponse response = new ComplaintResponse
            {
                ComplaintID = complaintId,
                ManagerUserID = managerUserId,
                ResponseText = "Sorry, we will visit tomorrow."
            };
            Check("Manager responds to the complaint", _complaints.AddResponse(response, companyId) > 0);
            Check("Another company cannot respond", _complaints.AddResponse(response, companyId + WrongId) == 0);
            Check("Complaint marked Resolved", _complaints.UpdateStatus(complaintId, companyId, ComplaintStatus.Resolved));
            Check("Resident can read the response", _complaints.GetResponses(complaintId).Count == 1);

            // ---------- Inactive company (rule 4) ----------
            Check("Company deactivated", _companies.SetStatus(companyId, RecordStatus.Inactive));
            User resident2User = new User
            {
                UserName = "s3test_res2_" + tag,
                Email = $"s3test_res2_{tag}@example.com",
                PasswordHash = "dummy-hash"
            };
            int resident2Id = _residents.Register(resident2User,
                new Resident { FullName = "S3 Test Resident 2", District = "Kicukiro", Sector = "Niboye" });
            Check("Inactive company cannot accept new subscriptions (rule 4)",
                _subscriptions.Subscribe(resident2Id, serviceId) == 0);
            Check("Inactive company's services are hidden from residents",
                _services.GetAvailableByCompany(companyId).Count == 0);
        }
    }
}