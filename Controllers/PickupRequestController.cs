using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    public class PickupRequestController : ManagerControllerBase
    {
        private readonly PickupRequestRepository _requests = new PickupRequestRepository();

        public DataResult<List<PickupRequest>> GetRequests(PickupRequestStatus? status)
        {
            int? companyId = CompanyId;
            if (companyId == null) return DataResult<List<PickupRequest>>.Fail(NoPermission);
            try
            {
                List<PickupRequest> list = _requests.GetByCompany(companyId.Value);
                if (status != null) list = list.Where(r => r.Status == status).ToList();
                return DataResult<List<PickupRequest>>.Ok(list);
            }
            catch (MySqlException) { return DataResult<List<PickupRequest>>.Fail(DatabaseDown); }
        }

        // Approving also creates the Pending collection task (one transaction, Stage 3)
        public OperationResult Approve(int requestId)
        {
            int? companyId = CompanyId;
            if (companyId == null) return OperationResult.Fail(NoPermission);
            try
            {
                return _requests.Approve(requestId, companyId.Value)
                    ? OperationResult.Ok("The request was approved. Assign a worker on the Tasks screen.")
                    : OperationResult.Fail("This request can no longer be approved. It may already be handled.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }

        public OperationResult Reject(int requestId)
        {
            int? companyId = CompanyId;
            if (companyId == null) return OperationResult.Fail(NoPermission);
            try
            {
                return _requests.Reject(requestId, companyId.Value)
                    ? OperationResult.Ok("The request was rejected.")
                    : OperationResult.Fail("This request can no longer be rejected. It may already be handled.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }
    }
}