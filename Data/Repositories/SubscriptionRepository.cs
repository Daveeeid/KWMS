using KWMS.Models;
using MySqlConnector;

namespace KWMS.Data.Repositories
{
    public class SubscriptionRepository : BaseRepository
    {
        private const string SelectAll =
            @"SELECT sub.SubscriptionID, sub.ResidentID, sub.ServiceID, sub.StartDate, sub.EndDate, sub.Status, sub.CreatedAt,
                     r.FullName AS ResidentName, r.Phone AS ResidentPhone,
                     CONCAT_WS(', ', r.HouseAddress, r.Sector, r.District) AS ResidentLocation,
                     sv.ServiceName, sv.MonthlyPrice, sv.CompanyID, c.CompanyName
              FROM Subscriptions sub
              JOIN Residents r ON r.ResidentID = sub.ResidentID
              JOIN Services sv ON sv.ServiceID = sub.ServiceID
              JOIN Companies c ON c.CompanyID = sv.CompanyID ";

        private static Subscription MapSubscription(MySqlDataReader r) => new Subscription
        {
            SubscriptionID = r.ReadInt("SubscriptionID"),
            ResidentID = r.ReadInt("ResidentID"),
            ServiceID = r.ReadInt("ServiceID"),
            StartDate = r.ReadDate("StartDate"),
            EndDate = r.ReadDateOrNull("EndDate"),
            Status = EnumMapper.ToEnum<SubscriptionStatus>(r.ReadString("Status")),
            CreatedAt = r.ReadDate("CreatedAt"),
            ResidentName = r.ReadString("ResidentName"),
            ResidentPhone = r.ReadStringOrNull("ResidentPhone"),
            ResidentLocation = r.ReadString("ResidentLocation"),
            ServiceName = r.ReadString("ServiceName"),
            MonthlyPrice = r.ReadDecimal("MonthlyPrice"),
            CompanyID = r.ReadInt("CompanyID"),
            CompanyName = r.ReadString("CompanyName")
        };

        public Subscription? GetById(int subscriptionId) =>
            GetOne(SelectAll + "WHERE sub.SubscriptionID = @id;", MapSubscription, P("@id", subscriptionId));

        // Full history of one resident (newest first)
        public List<Subscription> GetByResident(int residentId) =>
            GetList(SelectAll + "WHERE sub.ResidentID = @residentId ORDER BY sub.StartDate DESC, sub.SubscriptionID DESC;",
                MapSubscription, P("@residentId", residentId));

        public Subscription? GetActiveByResident(int residentId) =>
            GetOne(SelectAll + "WHERE sub.ResidentID = @residentId AND sub.Status = 'Active';",
                MapSubscription, P("@residentId", residentId));

        // Manager view: residents subscribed to THIS company's services
        public List<Subscription> GetByCompany(int companyId, bool activeOnly = false) =>
            GetList(SelectAll + @"WHERE sv.CompanyID = @companyId AND (@activeOnly = 0 OR sub.Status = 'Active')
                                  ORDER BY r.FullName;",
                MapSubscription, P("@companyId", companyId), P("@activeOnly", activeOnly ? 1 : 0));

        /// <summary>
        /// Subscribes a resident to a service starting today.
        /// Returns the new SubscriptionID, or 0 if the service or its company is inactive (rule 4).
        /// THROWS MySqlException (DbHelper.IsDuplicateKey = true) if the resident already has an
        /// active subscription (rule 2). The controller in Stage 8 will catch that.
        /// </summary>
        public int Subscribe(int residentId, int serviceId)
        {
            const string sql = @"INSERT INTO Subscriptions (ResidentID, ServiceID, StartDate)
                                 SELECT @residentId, sv.ServiceID, CURDATE()
                                 FROM Services sv JOIN Companies c ON c.CompanyID = sv.CompanyID
                                 WHERE sv.ServiceID = @serviceId AND sv.Status = 'Active' AND c.Status = 'Active';";
            return ExecuteInsert(sql, P("@residentId", residentId), P("@serviceId", serviceId));
        }

        // A resident can only cancel their OWN active subscription
        public bool Cancel(int subscriptionId, int residentId) =>
            ExecuteUpdate(@"UPDATE Subscriptions SET Status = 'Cancelled', EndDate = CURDATE()
                            WHERE SubscriptionID = @id AND ResidentID = @residentId AND Status = 'Active';",
                P("@id", subscriptionId), P("@residentId", residentId)) > 0;

        public bool CancelAndCleanUp(int subscriptionId, int residentId)
        {
            const string cancelSql = @"UPDATE Subscriptions SET Status = 'Cancelled', EndDate = CURDATE()
                WHERE SubscriptionID = @id AND ResidentID = @residentId AND Status = 'Active';";
            const string schedulesSql = @"UPDATE CollectionSchedules cs SET cs.Status = 'Cancelled'
                WHERE cs.SubscriptionID = @id AND cs.Status = 'Scheduled'
                  AND NOT EXISTS (SELECT 1 FROM CollectionTasks t WHERE t.ScheduleID = cs.ScheduleID
                                  AND t.Status IN ('InProgress', 'Completed'));";
            const string requestsSql = @"UPDATE PickupRequests pr SET pr.Status = 'Rejected'
                WHERE pr.SubscriptionID = @id AND pr.Status IN ('Pending', 'Approved')
                  AND NOT EXISTS (SELECT 1 FROM CollectionTasks t WHERE t.PickupRequestID = pr.RequestID
                                  AND t.Status IN ('InProgress', 'Completed'));";
            const string tasksSql = @"UPDATE CollectionTasks t
                LEFT JOIN CollectionSchedules cs ON cs.ScheduleID = t.ScheduleID
                LEFT JOIN PickupRequests pr ON pr.RequestID = t.PickupRequestID
                SET t.Status = 'Cancelled'
                WHERE COALESCE(cs.SubscriptionID, pr.SubscriptionID) = @id AND t.Status IN ('Pending', 'Assigned');";

            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    int changed = ExecuteUpdateIn(connection, transaction, cancelSql,
                        P("@id", subscriptionId), P("@residentId", residentId));
                    if (changed == 0) return false;

                    ExecuteUpdateIn(connection, transaction, schedulesSql, P("@id", subscriptionId));
                    ExecuteUpdateIn(connection, transaction, requestsSql, P("@id", subscriptionId));
                    ExecuteUpdateIn(connection, transaction, tasksSql, P("@id", subscriptionId));

                    transaction.Commit();
                    return true;
                }
            }
        }
    }
}