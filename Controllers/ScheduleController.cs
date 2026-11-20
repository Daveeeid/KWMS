using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    public class ScheduleController : ManagerControllerBase
    {
        private readonly ScheduleRepository _schedules = new ScheduleRepository();

        public DataResult<List<CollectionSchedule>> GetSchedules()
        {
            int? companyId = CompanyId;
            if (companyId == null) return DataResult<List<CollectionSchedule>>.Fail(NoPermission);

            try { return DataResult<List<CollectionSchedule>>.Ok(_schedules.GetByCompany(companyId.Value)); }
            catch (MySqlException) { return DataResult<List<CollectionSchedule>>.Fail(DatabaseDown); }
        }

        public OperationResult CreateSchedule(int subscriptionId, DateTime date, TimeSpan time, string? notes)
        {
            int? companyId = CompanyId;
            if (companyId == null) return OperationResult.Fail(NoPermission);

            notes = ValidationHelper.NullIfEmpty(notes);

            List<string> errors = new List<string>();
            if (subscriptionId <= 0) errors.Add("Please select a resident.");
            if (date.Date < DateTime.Today) errors.Add("The collection date cannot be in the past.");
            if (notes != null && notes.Length > 255) errors.Add("Notes must be at most 255 characters.");
            if (errors.Count > 0) return OperationResult.Fail(ValidationHelper.FormatErrors(errors));

            try
            {
                CollectionSchedule schedule = new CollectionSchedule
                {
                    SubscriptionID = subscriptionId,
                    CollectionDate = date.Date,
                    CollectionTime = time,
                    Notes = notes
                };

                // Creates the schedule and its Pending task together; returns 0 if the subscription
                // is not Active or does not belong to this company
                int id = _schedules.Create(schedule, companyId.Value);
                if (id == 0)
                    return OperationResult.Fail("This resident has no active subscription with your company.");

                return OperationResult.Ok("The collection was scheduled. Assign a worker on the Tasks screen.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }

        public OperationResult CancelSchedule(int scheduleId)
        {
            int? companyId = CompanyId;
            if (companyId == null) return OperationResult.Fail(NoPermission);

            try
            {
                if (!_schedules.Cancel(scheduleId, companyId.Value))
                    return OperationResult.Fail(
                        "This schedule cannot be cancelled. It may already be cancelled, or its collection has started or finished.");

                return OperationResult.Ok("The schedule and its task were cancelled.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }
    }
}