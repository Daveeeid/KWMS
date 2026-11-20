using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    public class ResidentRecordsController : ResidentControllerBase
    {
        private readonly ScheduleRepository _schedules = new ScheduleRepository();
        private readonly PaymentRepository _payments = new PaymentRepository();

        public DataResult<List<CollectionSchedule>> GetMySchedules()
        {
            try
            {
                Resident? resident = GetCurrentResident(out string error);
                if (resident == null) return DataResult<List<CollectionSchedule>>.Fail(error);
                return DataResult<List<CollectionSchedule>>.Ok(_schedules.GetByResident(resident.ResidentID));
            }
            catch (MySqlException) { return DataResult<List<CollectionSchedule>>.Fail(DatabaseDown); }
        }

        public DataResult<List<Payment>> GetMyPayments()
        {
            try
            {
                Resident? resident = GetCurrentResident(out string error);
                if (resident == null) return DataResult<List<Payment>>.Fail(error);
                return DataResult<List<Payment>>.Ok(_payments.GetByResident(resident.ResidentID));
            }
            catch (MySqlException) { return DataResult<List<Payment>>.Fail(DatabaseDown); }
        }
    }
}