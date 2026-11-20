using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    public class ComplaintController : ManagerControllerBase
    {
        private readonly ComplaintRepository _complaints = new ComplaintRepository();

        public DataResult<List<Complaint>> GetComplaints(ComplaintStatus? status)
        {
            int? companyId = CompanyId;
            if (companyId == null) return DataResult<List<Complaint>>.Fail(NoPermission);
            try
            {
                List<Complaint> list = _complaints.GetByCompany(companyId.Value);
                if (status != null) list = list.Where(c => c.Status == status).ToList();
                return DataResult<List<Complaint>>.Ok(list);
            }
            catch (MySqlException) { return DataResult<List<Complaint>>.Fail(DatabaseDown); }
        }

        public DataResult<List<ComplaintResponse>> GetResponses(int complaintId)
        {
            int? companyId = CompanyId;
            if (companyId == null) return DataResult<List<ComplaintResponse>>.Fail(NoPermission);
            try
            {
                if (!_complaints.GetByCompany(companyId.Value).Any(c => c.ComplaintID == complaintId))
                    return DataResult<List<ComplaintResponse>>.Fail("This complaint does not belong to your company.");
                return DataResult<List<ComplaintResponse>>.Ok(_complaints.GetResponses(complaintId));
            }
            catch (MySqlException) { return DataResult<List<ComplaintResponse>>.Fail(DatabaseDown); }
        }

        public OperationResult Respond(int complaintId, string text)
        {
            int? companyId = CompanyId;
            if (companyId == null) return OperationResult.Fail(NoPermission);

            text = text.Trim();
            if (text.Length < 3 || text.Length > 2000)
                return OperationResult.Fail("The response must be 3 to 2000 characters.");

            try
            {
                Complaint? complaint = _complaints.GetByCompany(companyId.Value).FirstOrDefault(c => c.ComplaintID == complaintId);
                if (complaint == null) return OperationResult.Fail("This complaint does not belong to your company.");

                ComplaintResponse response = new ComplaintResponse
                {
                    ComplaintID = complaintId,
                    ManagerUserID = SessionManager.CurrentUser!.UserID,
                    ResponseText = text
                };

                if (_complaints.AddResponse(response, companyId.Value) == 0)
                    return OperationResult.Fail("The response could not be saved.");


                if (complaint.Status == ComplaintStatus.Open)
                    _complaints.UpdateStatus(complaintId, companyId.Value, ComplaintStatus.InProgress);

                return OperationResult.Ok("Your response was sent.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }

        public OperationResult SetStatus(int complaintId, ComplaintStatus status)
        {
            int? companyId = CompanyId;
            if (companyId == null) return OperationResult.Fail(NoPermission);
            try
            {
                return _complaints.UpdateStatus(complaintId, companyId.Value, status)
                    ? OperationResult.Ok("The complaint status was updated.")
                    : OperationResult.Fail("The complaint could not be found.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }
    }
}