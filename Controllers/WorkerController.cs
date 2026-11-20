using KWMS.Data;
using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    public class WorkerController : ManagerControllerBase
    {
        private readonly WorkerRepository _workers = new WorkerRepository();
        private readonly UserRepository _users = new UserRepository();
        private readonly TaskRepository _tasks = new TaskRepository();

        public DataResult<List<Worker>> GetWorkers(string? searchText)
        {
            int? companyId = CompanyId;
            if (companyId == null) return DataResult<List<Worker>>.Fail(NoPermission);

            try { return DataResult<List<Worker>>.Ok(_workers.GetByCompany(companyId.Value, searchText?.Trim())); }
            catch (MySqlException) { return DataResult<List<Worker>>.Fail(DatabaseDown); }
        }

        /// <summary>Creates the worker AND their login account (one transaction, Stage 3).</summary>
        public OperationResult AddWorker(Worker worker, string userName, string email,
                                         string password, string confirmPassword)
        {
            int? companyId = CompanyId;
            if (companyId == null) return OperationResult.Fail(NoPermission);

            userName = userName.Trim();
            email = email.Trim();

            List<string> errors = CleanAndValidate(worker);
            if (!InputValidator.IsValidUserName(userName))
                errors.Add("Username must be 3 to 30 characters: letters, digits or underscore only.");
            if (!InputValidator.IsValidEmail(email))
                errors.Add("Please enter a valid e-mail address.");
            if (!InputValidator.IsValidPassword(password))
                errors.Add("Password must be 8 to 72 characters with at least one letter and one digit.");
            else if (password != confirmPassword)
                errors.Add("The two passwords do not match.");

            if (errors.Count > 0) return OperationResult.Fail(ValidationHelper.FormatErrors(errors));

            worker.CompanyID = companyId.Value;

            try
            {
                if (_users.UserNameExists(userName)) errors.Add("That username is already taken.");
                if (_users.EmailExists(email)) errors.Add("That e-mail address is already registered.");
                if (errors.Count > 0) return OperationResult.Fail(ValidationHelper.FormatErrors(errors));

                User user = new User
                {
                    UserName = userName,
                    Email = email,
                    PasswordHash = PasswordHasher.Hash(password)
                };

                _workers.Add(user, worker);
                return OperationResult.Ok($"Worker '{worker.FullName}' was added. Login username: {userName}");
            }
            catch (MySqlException ex) when (DbHelper.IsDuplicateKey(ex))
            {
                return OperationResult.Fail("That username or e-mail was just used by someone else. Nothing was saved.");
            }
            catch (MySqlException ex)
            {
                return OperationResult.Fail("Database error. Nothing was saved.\n\n" + ex.Message);
            }
        }

        public OperationResult UpdateWorker(Worker worker)
        {
            int? companyId = CompanyId;
            if (companyId == null) return OperationResult.Fail(NoPermission);

            List<string> errors = CleanAndValidate(worker);
            if (errors.Count > 0) return OperationResult.Fail(ValidationHelper.FormatErrors(errors));

            worker.CompanyID = companyId.Value;

            try
            {
                if (!_workers.Update(worker))
                    return OperationResult.Fail("The worker could not be found.");
                return OperationResult.Ok($"Worker '{worker.FullName}' was updated.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }

        public OperationResult SetWorkerStatus(int workerId, RecordStatus status)
        {
            int? companyId = CompanyId;
            if (companyId == null) return OperationResult.Fail(NoPermission);

            try
            {
                // GetById includes the company filter, so this also proves the worker is ours
                Worker? worker = _workers.GetById(workerId, companyId.Value);
                if (worker == null) return OperationResult.Fail("The worker could not be found.");

                if (status == RecordStatus.Inactive
                    && _tasks.GetByWorker(workerId).Any(t => t.Status == CollectionTaskStatus.InProgress))
                {
                    return OperationResult.Fail(
                        "This worker has a collection in progress. Wait until it is completed before deactivating the worker.");
                }

                if (!_workers.SetStatus(workerId, companyId.Value, status))
                    return OperationResult.Fail("The worker could not be found.");

                return OperationResult.Ok(status == RecordStatus.Active
                    ? "The worker is active again and can log in."
                    : "The worker was deactivated and can no longer log in. Reassign any tasks still assigned to them.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }

        private static List<string> CleanAndValidate(Worker worker)
        {
            worker.FullName = worker.FullName.Trim();
            worker.Phone = ValidationHelper.NullIfEmpty(worker.Phone);

            List<string> errors = new List<string>();
            if (worker.FullName.Length < 2 || worker.FullName.Length > 100)
                errors.Add("Full name must be 2 to 100 characters.");
            if (worker.Phone != null && (!InputValidator.IsValidPhone(worker.Phone) || worker.Phone.Length > 20))
                errors.Add("Phone must contain 9 to 15 digits (optionally starting with +).");
            return errors;
        }
    }
}