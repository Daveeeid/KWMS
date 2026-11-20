using KWMS.Models;

namespace KWMS.Data.Repositories
{
    public class StatisticsRepository : BaseRepository
    {
        /// <summary>One query with seven small sub-queries, so the screen needs only one database round trip.</summary>
        public SystemStatistics GetSystemStatistics()
        {
            const string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM Companies)                           AS TotalCompanies,
                    (SELECT COUNT(*) FROM Companies WHERE Status = 'Active')   AS ActiveCompanies,
                    (SELECT COUNT(*) FROM Users)                               AS TotalUsers,
                    (SELECT COUNT(*) FROM Residents)                           AS TotalResidents,
                    (SELECT COUNT(*) FROM Workers)                             AS TotalWorkers,
                    (SELECT COUNT(*) FROM Subscriptions WHERE Status = 'Active') AS ActiveSubscriptions,
                    (SELECT COUNT(*) FROM CollectionTasks WHERE Status = 'Completed') AS CompletedCollections;";

            SystemStatistics? stats = GetOne<SystemStatistics>(sql, r => new SystemStatistics
            {
                TotalCompanies = r.ReadCount("TotalCompanies"),
                ActiveCompanies = r.ReadCount("ActiveCompanies"),
                TotalUsers = r.ReadCount("TotalUsers"),
                TotalResidents = r.ReadCount("TotalResidents"),
                TotalWorkers = r.ReadCount("TotalWorkers"),
                ActiveSubscriptions = r.ReadCount("ActiveSubscriptions"),
                CompletedCollections = r.ReadCount("CompletedCollections")
            });

            return stats ?? new SystemStatistics();
        }

        public CompanyStatistics GetCompanyStatistics(int companyId)
        {
            const string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM Services WHERE CompanyID = @companyId AND Status = 'Active') AS ActiveServices,
                    (SELECT COUNT(*) FROM Workers WHERE CompanyID = @companyId AND Status = 'Active') AS ActiveWorkers,
                    (SELECT COUNT(*) FROM Subscriptions sub JOIN Services sv ON sv.ServiceID = sub.ServiceID
                      WHERE sv.CompanyID = @companyId AND sub.Status = 'Active') AS ActiveSubscribers,
                    (SELECT COUNT(*) FROM Complaints WHERE CompanyID = @companyId AND Status <> 'Resolved') AS OpenComplaints,
                    COALESCE(SUM(tk.Status = 'Pending'), 0) AS TasksToAssign,
                    COALESCE(SUM(tk.Status IN ('Assigned', 'InProgress')), 0) AS OpenTasks,
                    COALESCE(SUM(tk.Status = 'Completed'), 0) AS CompletedTasks
                FROM (SELECT t.Status
                      FROM CollectionTasks t
                      LEFT JOIN CollectionSchedules cs ON cs.ScheduleID = t.ScheduleID
                      LEFT JOIN PickupRequests pr ON pr.RequestID = t.PickupRequestID
                      JOIN Subscriptions sub ON sub.SubscriptionID = COALESCE(cs.SubscriptionID, pr.SubscriptionID)
                      JOIN Services sv ON sv.ServiceID = sub.ServiceID
                      WHERE sv.CompanyID = @companyId) tk;";

            CompanyStatistics? stats = GetOne<CompanyStatistics>(sql, r => new CompanyStatistics
            {
                ActiveServices = r.ReadCount("ActiveServices"),
                ActiveWorkers = r.ReadCount("ActiveWorkers"),
                ActiveSubscribers = r.ReadCount("ActiveSubscribers"),
                OpenComplaints = r.ReadCount("OpenComplaints"),
                TasksToAssign = r.ReadCount("TasksToAssign"),
                OpenTasks = r.ReadCount("OpenTasks"),
                CompletedTasks = r.ReadCount("CompletedTasks")
            }, P("@companyId", companyId));

            return stats ?? new CompanyStatistics();
        }

        public WorkerStatistics GetWorkerStatistics(int workerId)
        {
            const string sql = @"
                SELECT
                    COALESCE(SUM(Status = 'Assigned'), 0)   AS ToStart,
                    COALESCE(SUM(Status = 'InProgress'), 0) AS InProgress,
                    COALESCE(SUM(Status = 'Completed' AND DATE(CompletedAt) = CURDATE()), 0) AS CompletedToday,
                    COALESCE(SUM(Status = 'Completed'), 0)  AS CompletedTotal
                FROM CollectionTasks
                WHERE WorkerID = @workerId;";

            WorkerStatistics? stats = GetOne<WorkerStatistics>(sql, r => new WorkerStatistics
            {
                ToStart = r.ReadCount("ToStart"),
                InProgress = r.ReadCount("InProgress"),
                CompletedToday = r.ReadCount("CompletedToday"),
                CompletedTotal = r.ReadCount("CompletedTotal")
            }, P("@workerId", workerId));

            return stats ?? new WorkerStatistics();
        }
    }
}