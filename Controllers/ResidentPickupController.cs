using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    public class ResidentPickupController : ResidentControllerBase
    {
        private readonly PickupRequestRepository _requests = new PickupRequestRepository();
        private readonly SubscriptionRepository _subscriptions = new SubscriptionRepository();

        public DataResult<List<PickupRequest>> GetMyRequests()
        {
            try
            {
                Resident? resident = GetCurrentResident(out string error);
                if (resident == null) return DataResult<List<PickupRequest>>.Fail(error);
                return DataResult<List<PickupRequest>>.Ok(_requests.GetByResident(resident.ResidentID));
            }
            catch (MySqlException) { return DataResult<List<PickupRequest>>.Fail(DatabaseDown); }
        }

        public OperationResult CreateRequest(DateTime date, string description)
        {
            try
            {
                Resident? resident = GetCurrentResident(out string error);
                if (resident == null) return OperationResult.Fail(error);

                description = description.Trim();
                List<string> errors = new List<string>();
                if (date.Date < DateTime.Today) errors.Add("The requested date cannot be in the past.");
                if (description.Length < 5 || description.Length > 255) errors.Add("Description must be 5 to 255 characters.");
                if (errors.Count > 0) return OperationResult.Fail(ValidationHelper.FormatErrors(errors));

                // Rule 3: an active subscription is required
                Subscription? active = _subscriptions.GetActiveByResident(resident.ResidentID);
                if (active == null)
                    return OperationResult.Fail("You need an active subscription before you can request an additional collection.");

                PickupRequest request = new PickupRequest
                {
                    SubscriptionID = active.SubscriptionID,
                    RequestedDate = date.Date,
                    Description = description
                };

                return _requests.Create(request, resident.ResidentID) > 0
                    ? OperationResult.Ok("Your request was sent to " + active.CompanyName + ". You can follow its status here.")
                    : OperationResult.Fail("The request could not be created.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }
    }
}