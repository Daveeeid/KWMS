using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    public class ResidentController : ResidentControllerBase
    {
        private readonly ResidentRepository _residents = new ResidentRepository();
        private readonly SubscriptionRepository _subscriptions = new SubscriptionRepository();
        private readonly ScheduleRepository _schedules = new ScheduleRepository();
        private readonly PickupRequestRepository _requests = new PickupRequestRepository();
        private readonly ComplaintRepository _complaints = new ComplaintRepository();
        private readonly PaymentRepository _payments = new PaymentRepository();

        public DataResult<Resident> GetMyProfile()
        {
            try
            {
                Resident? resident = GetCurrentResident(out string error);
                return resident == null ? DataResult<Resident>.Fail(error) : DataResult<Resident>.Ok(resident);
            }
            catch (MySqlException) { return DataResult<Resident>.Fail(DatabaseDown); }
        }

        public OperationResult UpdateProfile(Resident input)
        {
            try
            {
                Resident? current = GetCurrentResident(out string error);
                if (current == null) return OperationResult.Fail(error);


                input.ResidentID = current.ResidentID;
                input.UserID = current.UserID;
                input.FullName = input.FullName.Trim();
                input.Sector = input.Sector.Trim();
                input.Phone = ValidationHelper.NullIfEmpty(input.Phone);
                input.HouseAddress = ValidationHelper.NullIfEmpty(input.HouseAddress);

                List<string> errors = new List<string>();
                if (input.FullName.Length < 2 || input.FullName.Length > 100) errors.Add("Full name must be 2 to 100 characters.");
                if (input.Phone != null && (!InputValidator.IsValidPhone(input.Phone) || input.Phone.Length > 20))
                    errors.Add("Phone must contain 9 to 15 digits (optionally starting with +).");
                if (!AuthController.KigaliDistricts.Contains(input.District)) errors.Add("Please select your district.");
                if (input.Sector.Length < 2 || input.Sector.Length > 50) errors.Add("Sector must be 2 to 50 characters.");
                if (input.HouseAddress != null && input.HouseAddress.Length > 150) errors.Add("House address must be at most 150 characters.");
                if (input.HouseholdSize < 1) errors.Add("Household size must be at least 1.");
                if (errors.Count > 0) return OperationResult.Fail(ValidationHelper.FormatErrors(errors));

                return _residents.UpdateProfile(input)
                    ? OperationResult.Ok("Your profile was updated.")
                    : OperationResult.Fail("Your profile could not be updated.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }

        public DataResult<ResidentStatistics> GetMyStatistics()
        {
            try
            {
                Resident? resident = GetCurrentResident(out string error);
                if (resident == null) return DataResult<ResidentStatistics>.Fail(error);

                Subscription? active = _subscriptions.GetActiveByResident(resident.ResidentID);

                ResidentStatistics stats = new ResidentStatistics
                {
                    SubscriptionText = active == null
                        ? "No active subscription. Open 'Companies' to subscribe."
                        : $"{active.ServiceName} - {active.CompanyName} ({active.MonthlyPrice:N0} RWF per month)",
                    UpcomingCollections = _schedules.GetByResident(resident.ResidentID)
                        .Count(s => s.Status == ScheduleStatus.Scheduled && s.CollectionDate.Date >= DateTime.Today),
                    PendingRequests = _requests.GetByResident(resident.ResidentID)
                        .Count(r => r.Status == PickupRequestStatus.Pending),
                    OpenComplaints = _complaints.GetByResident(resident.ResidentID)
                        .Count(c => c.Status != ComplaintStatus.Resolved),
                    PaymentsMade = _payments.GetByResident(resident.ResidentID).Count
                };
                return DataResult<ResidentStatistics>.Ok(stats);
            }
            catch (MySqlException) { return DataResult<ResidentStatistics>.Fail(DatabaseDown); }
        }
    }
}