using KWMS.Data;
using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    public class ServiceController : ManagerControllerBase
    {
        private readonly ServiceRepository _services = new ServiceRepository();

        public DataResult<List<Service>> GetServices()
        {
            int? companyId = CompanyId;
            if (companyId == null) return DataResult<List<Service>>.Fail(NoPermission);

            try { return DataResult<List<Service>>.Ok(_services.GetByCompany(companyId.Value)); }
            catch (MySqlException) { return DataResult<List<Service>>.Fail(DatabaseDown); }
        }

        public OperationResult AddService(Service service)
        {
            int? companyId = CompanyId;
            if (companyId == null) return OperationResult.Fail(NoPermission);

            List<string> errors = CleanAndValidate(service);
            if (errors.Count > 0) return OperationResult.Fail(ValidationHelper.FormatErrors(errors));

            service.CompanyID = companyId.Value;
            service.Status = RecordStatus.Active;

            try
            {
                if (_services.NameExists(service.CompanyID, service.ServiceName))
                    return OperationResult.Fail("Your company already has a service with this name.");

                _services.Insert(service);
                return OperationResult.Ok($"Service '{service.ServiceName}' was added.");
            }
            catch (MySqlException ex) when (DbHelper.IsDuplicateKey(ex))
            {
                return OperationResult.Fail("Your company already has a service with this name.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }

        public OperationResult UpdateService(Service service)
        {
            int? companyId = CompanyId;
            if (companyId == null) return OperationResult.Fail(NoPermission);

            List<string> errors = CleanAndValidate(service);
            if (errors.Count > 0) return OperationResult.Fail(ValidationHelper.FormatErrors(errors));

            service.CompanyID = companyId.Value;

            try
            {
                if (_services.NameExists(service.CompanyID, service.ServiceName, service.ServiceID))
                    return OperationResult.Fail("Your company already has another service with this name.");


                if (!_services.Update(service))
                    return OperationResult.Fail("The service could not be found.");

                return OperationResult.Ok($"Service '{service.ServiceName}' was updated.");
            }
            catch (MySqlException ex) when (DbHelper.IsDuplicateKey(ex))
            {
                return OperationResult.Fail("Your company already has another service with this name.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }

        public OperationResult SetServiceStatus(int serviceId, RecordStatus status)
        {
            int? companyId = CompanyId;
            if (companyId == null) return OperationResult.Fail(NoPermission);

            try
            {
                if (!_services.SetStatus(serviceId, companyId.Value, status))
                    return OperationResult.Fail("The service could not be found.");

                return OperationResult.Ok(status == RecordStatus.Active
                    ? "The service is active again."
                    : "The service was deactivated. Residents can no longer subscribe to it.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }

        private static List<string> CleanAndValidate(Service service)
        {
            service.ServiceName = service.ServiceName.Trim();
            service.Description = ValidationHelper.NullIfEmpty(service.Description);

            List<string> errors = new List<string>();
            if (service.ServiceName.Length < 2 || service.ServiceName.Length > 100)
                errors.Add("Service name must be 2 to 100 characters.");
            if (service.Description != null && service.Description.Length > 255)
                errors.Add("Description must be at most 255 characters.");
            if (service.MonthlyPrice <= 0 || service.MonthlyPrice > 1000000)
                errors.Add("Monthly price must be more than 0 and at most 1,000,000.");
            return errors;
        }
    }
}