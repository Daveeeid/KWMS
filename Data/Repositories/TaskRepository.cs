using KWMS.Models;
using MySqlConnector;

namespace KWMS.Data.Repositories
{
    public class TaskRepository : BaseRepository
    {


        private const string FromAndJoins =
            @" FROM CollectionTasks t
               LEFT JOIN CollectionSchedules cs ON cs.ScheduleID = t.ScheduleID
               LEFT JOIN PickupRequests pr ON pr.RequestID = t.PickupRequestID
               JOIN Subscriptions sub ON sub.SubscriptionID = COALESCE(cs.SubscriptionID, pr.SubscriptionID)
               JOIN Residents r ON r.ResidentID = sub.ResidentID
               JOIN Services sv ON sv.ServiceID = sub.ServiceID
               LEFT JOIN Workers w ON w.WorkerID = t.WorkerID ";

        private const string SelectAll =
            @"SELECT t.TaskID, t.ScheduleID, t.PickupRequestID, t.WorkerID, t.Status,
                     t.AssignedAt, t.StartedAt, t.CompletedAt, t.CreatedAt,
                     IF(t.ScheduleID IS NOT NULL, 'Scheduled collection', 'Additional pickup') AS TaskType,
                     COALESCE(cs.CollectionDate, pr.RequestedDate) AS CollectionDate,
                     r.FullName AS ResidentName, sv.ServiceName,
                     CONCAT_WS(', ', r.HouseAddress, r.Sector, r.District) AS Location,
                     w.FullName AS WorkerName " + FromAndJoins;

        private static CollectionTask MapTask(MySqlDataReader r) => new CollectionTask
        {
            TaskID = r.ReadInt("TaskID"),
            ScheduleID = r.ReadIntOrNull("ScheduleID"),
            PickupRequestID = r.ReadIntOrNull("PickupRequestID"),
            WorkerID = r.ReadIntOrNull("WorkerID"),
            Status = EnumMapper.ToEnum<CollectionTaskStatus>(r.ReadString("Status")),
            AssignedAt = r.ReadDateOrNull("AssignedAt"),
            StartedAt = r.ReadDateOrNull("StartedAt"),
            CompletedAt = r.ReadDateOrNull("CompletedAt"),
            CreatedAt = r.ReadDate("CreatedAt"),
            TaskType = r.ReadString("TaskType"),
            CollectionDate = r.ReadDate("CollectionDate"),
            ResidentName = r.ReadString("ResidentName"),
            ServiceName = r.ReadString("ServiceName"),
            Location = r.ReadString("Location"),
            WorkerName = r.ReadStringOrNull("WorkerName")
        };

        public List<CollectionTask> GetByCompany(int companyId) =>
            GetList(SelectAll + "WHERE sv.CompanyID = @companyId ORDER BY CollectionDate DESC, t.TaskID DESC;",
                MapTask, P("@companyId", companyId));


        public List<CollectionTask> GetByWorker(int workerId) =>
            GetList(SelectAll + "WHERE t.WorkerID = @workerId ORDER BY CollectionDate DESC, t.TaskID DESC;",
                MapTask, P("@workerId", workerId));






        public bool AssignWorker(int taskId, int workerId, int companyId)
        {
            const string sql = @"UPDATE CollectionTasks t
                                 LEFT JOIN CollectionSchedules cs ON cs.ScheduleID = t.ScheduleID
                                 LEFT JOIN PickupRequests pr ON pr.RequestID = t.PickupRequestID
                                 JOIN Subscriptions sub ON sub.SubscriptionID = COALESCE(cs.SubscriptionID, pr.SubscriptionID)
                                 JOIN Services sv ON sv.ServiceID = sub.ServiceID
                                 SET t.WorkerID = @workerId, t.Status = 'Assigned', t.AssignedAt = NOW()
                                 WHERE t.TaskID = @taskId
                                   AND sv.CompanyID = @companyId
                                   AND t.Status IN ('Pending', 'Assigned')
                                   AND EXISTS (SELECT 1 FROM Workers w
                                               WHERE w.WorkerID = @workerId AND w.CompanyID = @companyId AND w.Status = 'Active');";
            return ExecuteUpdate(sql, P("@workerId", workerId), P("@taskId", taskId), P("@companyId", companyId)) > 0;
        }


        public bool Start(int taskId, int workerId) =>
            ExecuteUpdate(@"UPDATE CollectionTasks SET Status = 'InProgress', StartedAt = NOW()
                            WHERE TaskID = @taskId AND WorkerID = @workerId AND Status = 'Assigned';",
                P("@taskId", taskId), P("@workerId", workerId)) > 0;



        public bool Complete(int taskId, int workerId)
        {
            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    int changed = ExecuteUpdateIn(connection, transaction,
                        @"UPDATE CollectionTasks SET Status = 'Completed', CompletedAt = NOW()
                          WHERE TaskID = @taskId AND WorkerID = @workerId AND Status = 'InProgress';",
                        P("@taskId", taskId), P("@workerId", workerId));

                    if (changed == 0) return false;

                    ExecuteUpdateIn(connection, transaction,
                        @"UPDATE PickupRequests pr JOIN CollectionTasks t ON t.PickupRequestID = pr.RequestID
                          SET pr.Status = 'Completed' WHERE t.TaskID = @taskId;",
                        P("@taskId", taskId));

                    transaction.Commit();
                    return true;
                }
            }
        }
    }
}