using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    public class ResidentComplaintController : ResidentControllerBase
    {
        private readonly ComplaintRepository _complaints = new ComplaintRepository();
        private readonly SubscriptionRepository _subscriptions = new SubscriptionRepository();
        private readonly CompanyRepository _companies = new CompanyRepository();

        public DataResult<List<Complaint>> GetMyComplaints()
        {
            try
            {
                Resident? resident = GetCurrentResident(out string error);
                if (resident == null) return DataResult<List<Complaint>>.Fail(error);
                return DataResult<List<Complaint>>.Ok(_complaints.GetByResident(resident.ResidentID));
            }
            catch (MySqlException) { return DataResult<List<Complaint>>.Fail(DatabaseDown); }
        }

        // Companies the resident has (or had) a subscription with
        public DataResult<List<Company>> GetComplaintCompanies()
        {
            try
            {
                Resident? resident = GetCurrentResident(out string error);
                if (resident == null) return DataResult<List<Company>>.Fail(error);

                List<Company> list = _subscriptions.GetByResident(resident.ResidentID)
                    .Select(s => s.CompanyID).Distinct()
                    .Select(id => _companies.GetById(id))
                    .Where(c => c != null).Select(c => c!).ToList();
                return DataResult<List<Company>>.Ok(list);
            }
            catch (MySqlException) { return DataResult<List<Company>>.Fail(DatabaseDown); }
        }

        public OperationResult CreateComplaint(int companyId, string subject, string description)
        {
            try
            {
                Resident? resident = GetCurrentResident(out string error);
                if (resident == null) return OperationResult.Fail(error);

                subject = subject.Trim();
                description = description.Trim();

                List<string> errors = new List<string>();
                if (companyId <= 0) errors.Add("Please select a company.");
                if (subject.Length < 3 || subject.Length > 150) errors.Add("Subject must be 3 to 150 characters.");
                if (description.Length < 10 || description.Length > 2000) errors.Add("Description must be 10 to 2000 characters.");
                if (errors.Count > 0) return OperationResult.Fail(ValidationHelper.FormatErrors(errors));

                if (!_subscriptions.GetByResident(resident.ResidentID).Any(s => s.CompanyID == companyId))
                    return OperationResult.Fail("You can only submit complaints about companies you have subscribed to.");

                _complaints.Create(new Complaint
                {
                    ResidentID = resident.ResidentID,
                    CompanyID = companyId,
                    Subject = subject,
                    Description = description
                });
                return OperationResult.Ok("Your complaint was submitted. You can track it on this screen.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }

        // Only responses to the resident's OWN complaints are returned
        public DataResult<List<ComplaintResponse>> GetResponses(int complaintId)
        {
            try
            {
                Resident? resident = GetCurrentResident(out string error);
                if (resident == null) return DataResult<List<ComplaintResponse>>.Fail(error);

                if (!_complaints.GetByResident(resident.ResidentID).Any(c => c.ComplaintID == complaintId))
                    return DataResult<List<ComplaintResponse>>.Fail("This complaint is not yours.");

                return DataResult<List<ComplaintResponse>>.Ok(_complaints.GetResponses(complaintId));
            }
            catch (MySqlException) { return DataResult<List<ComplaintResponse>>.Fail(DatabaseDown); }
        }
    }
}