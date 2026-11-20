using KWMS.Models;
using MySqlConnector;

namespace KWMS.Data.Repositories
{
    public class ScheduleRepository : BaseRepository
    {
        private const string SelectAll =
            @"SELECT cs.ScheduleID, cs.SubscriptionID, cs.CollectionDate, cs.CollectionTime, cs.Notes, cs.Status, cs.CreatedAt,
                     r.FullName AS ResidentName, sv.ServiceName,
                     CONCAT_WS(', ', r.HouseAddress, r.Sector, r.District) AS Location
              FROM CollectionSchedules cs
              JOIN Subscriptions sub ON sub.SubscriptionID = cs.SubscriptionID
              JOIN Residents r ON r.ResidentID = sub.ResidentID
              JOIN Services sv ON sv.ServiceID = sub.ServiceID ";

        private static CollectionSchedule MapSchedule(MySqlDataReader r) => new CollectionSchedule
        {
            ScheduleID = r.ReadInt("ScheduleID"),
            SubscriptionID = r.ReadInt("SubscriptionID"),
            CollectionDate = r.ReadDate("CollectionDate"),
            CollectionTime = r.ReadTime("CollectionTime"),
            Notes = r.ReadStringOrNull("Notes"),
            Status = EnumMapper.ToEnum<ScheduleStatus>(r.ReadString("Status")),
            CreatedAt = r.ReadDate("CreatedAt"),
            ResidentName = r.ReadString("ResidentName"),
            ServiceName = r.ReadString("ServiceName"),
            Location = r.ReadString("Location")
        };

        public List<CollectionSchedule> GetByCompany(int companyId) =>
            GetList(SelectAll + "WHERE sv.CompanyID = @companyId ORDER BY cs.CollectionDate DESC, cs.CollectionTime;",
                MapSchedule, P("@companyId", companyId));

        public List<CollectionSchedule> GetByResident(int residentId) =>
            GetList(SelectAll + "WHERE sub.ResidentID = @residentId ORDER BY cs.CollectionDate DESC, cs.CollectionTime;",
                MapSchedule, P("@residentId", residentId));

        /// <summary>
        /// Creates a schedule AND its matching Pending task in one transaction.
        /// Returns the new ScheduleID, or 0 if the subscription is not Active or does not
        /// belong to this company (rule 5).
        /// </summary>
        public int Create(CollectionSchedule schedule, int companyId)
        {
            const string scheduleSql = @"INSERT INTO CollectionSchedules (SubscriptionID, CollectionDate, CollectionTime, Notes)
                                         SELECT sub.SubscriptionID, @date, @time, @notes
                                         FROM Subscriptions sub JOIN Services sv ON sv.ServiceID = sub.ServiceID
                                         WHERE sub.SubscriptionID = @subscriptionId
                                           AND sub.Status = 'Active' AND sv.CompanyID = @companyId;";

            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    int scheduleId = ExecuteInsertIn(connection, transaction, scheduleSql,
                        P("@date", schedule.CollectionDate.Date), P("@time", schedule.CollectionTime),
                        P("@notes", schedule.Notes), P("@subscriptionId", schedule.SubscriptionID),
                        P("@companyId", companyId));

                    if (scheduleId == 0) return 0;   // nothing inserted; transaction is disposed (rolled back)

                    ExecuteInsertIn(connection, transaction,
                        "INSERT INTO CollectionTasks (ScheduleID) VALUES (@scheduleId);",
                        P("@scheduleId", scheduleId));

                    transaction.Commit();
                    return scheduleId;
                }
            }
        }
        /// <summary>
        /// Cancels a Scheduled collection AND its task in one transaction.
        /// Returns false if the schedule is not this company's, is already cancelled, or its task
        /// has already started or finished.
        /// </summary>
        public bool Cancel(int scheduleId, int companyId)
        {
            const string cancelTaskSql = @"
                UPDATE CollectionTasks t
                JOIN CollectionSchedules cs ON cs.ScheduleID = t.ScheduleID
                JOIN Subscriptions sub ON sub.SubscriptionID = cs.SubscriptionID
                JOIN Services sv ON sv.ServiceID = sub.ServiceID
                SET t.Status = 'Cancelled'
                WHERE cs.ScheduleID = @scheduleId AND cs.Status = 'Scheduled'
                  AND sv.CompanyID = @companyId AND t.Status IN ('Pending', 'Assigned');";

            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    int changed = ExecuteUpdateIn(connection, transaction, cancelTaskSql,
                        P("@scheduleId", scheduleId), P("@companyId", companyId));

                    if (changed == 0) return false;

                    ExecuteUpdateIn(connection, transaction,
                        "UPDATE CollectionSchedules SET Status = 'Cancelled' WHERE ScheduleID = @scheduleId;",
                        P("@scheduleId", scheduleId));

                    transaction.Commit();
                    return true;
                }
            }
        }
    }
}