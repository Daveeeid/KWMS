using KWMS.Data;
using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    public class ResidentSubscriptionController : ResidentControllerBase
    {
        private readonly CompanyRepository _companies = new CompanyRepository();
        private readonly ServiceRepository _services = new ServiceRepository();
        private readonly SubscriptionRepository _subscriptions = new SubscriptionRepository();

        public DataResult<List<Company>> GetActiveCompanies(string? searchText)
        {
            if (!SessionManager.IsInRole(UserRole.Resident)) return DataResult<List<Company>>.Fail(NoPermission);
            try { return DataResult<List<Company>>.Ok(_companies.GetAll(searchText?.Trim(), RecordStatus.Active)); }
            catch (MySqlException) { return DataResult<List<Company>>.Fail(DatabaseDown); }
        }

        public DataResult<List<Service>> GetServices(int companyId)
        {
            if (!SessionManager.IsInRole(UserRole.Resident)) return DataResult<List<Service>>.Fail(NoPermission);
            try { return DataResult<List<Service>>.Ok(_services.GetAvailableByCompany(companyId)); }
            catch (MySqlException) { return DataResult<List<Service>>.Fail(DatabaseDown); }
        }

        public DataResult<List<Subscription>> GetMySubscriptions()
        {
            try
            {
                Resident? resident = GetCurrentResident(out string error);
                if (resident == null) return DataResult<List<Subscription>>.Fail(error);
                return DataResult<List<Subscription>>.Ok(_subscriptions.GetByResident(resident.ResidentID));
            }
            catch (MySqlException) { return DataResult<List<Subscription>>.Fail(DatabaseDown); }
        }

        public OperationResult Subscribe(int serviceId)
        {
            try
            {
                Resident? resident = GetCurrentResident(out string error);
                if (resident == null) return OperationResult.Fail(error);

                // Rule 2 (friendly message; the database UNIQUE index is the safety net below)
                if (_subscriptions.GetActiveByResident(resident.ResidentID) != null)
                    return OperationResult.Fail("You already have an active subscription. Cancel it before subscribing to another service.");

                Service? service = _services.GetById(serviceId);
                if (service == null || service.Status != RecordStatus.Active)
                    return OperationResult.Fail("This service is not available.");

                // Rule 4
                Company? company = _companies.GetById(service.CompanyID);
                if (company == null || !company.CanAcceptNewSubscriptions())
                    return OperationResult.Fail("This company is inactive and cannot accept new subscriptions.");

                if (_subscriptions.Subscribe(resident.ResidentID, serviceId) == 0)
                    return OperationResult.Fail("This service is not available.");

                return OperationResult.Ok($"You are now subscribed to '{service.ServiceName}' with {company.CompanyName}.");
            }
            catch (MySqlException ex) when (DbHelper.IsDuplicateKey(ex))
            {
                return OperationResult.Fail("You already have an active subscription.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }

        public OperationResult CancelSubscription(int subscriptionId)
        {
            try
            {
                Resident? resident = GetCurrentResident(out string error);
                if (resident == null) return OperationResult.Fail(error);

                // The SQL contains "AND ResidentID = ...", so only your own subscription can be cancelled
                return _subscriptions.CancelAndCleanUp(subscriptionId, resident.ResidentID)
                    ? OperationResult.Ok("Your subscription was cancelled, together with its unstarted collections.")
                    : OperationResult.Fail("This subscription cannot be cancelled. It may already be cancelled.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }
    }
}