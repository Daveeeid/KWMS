using KWMS.Models;
using MySqlConnector;

namespace KWMS.Data.Repositories
{
    public class WorkerRepository : BaseRepository
    {
        private readonly UserRepository _userRepository = new UserRepository();

        private const string SelectAll =
            @"SELECT w.WorkerID, w.UserID, w.CompanyID, w.FullName, w.Phone, w.Status, w.CreatedAt,
                     u.UserName, u.Email
              FROM Workers w JOIN Users u ON u.UserID = w.UserID ";

        private static Worker MapWorker(MySqlDataReader r) => new Worker
        {
            WorkerID = r.ReadInt("WorkerID"),
            UserID = r.ReadInt("UserID"),
            CompanyID = r.ReadInt("CompanyID"),
            FullName = r.ReadString("FullName"),
            Phone = r.ReadStringOrNull("Phone"),
            Status = EnumMapper.ToRecordStatus(r.ReadStringOrNull("Status")),
            CreatedAt = r.ReadDate("CreatedAt"),
            UserName = r.ReadString("UserName"),
            Email = r.ReadString("Email")
        };

        public List<Worker> GetByCompany(int companyId, string? searchText = null) =>
            GetList(SelectAll + @"WHERE w.CompanyID = @companyId
                                    AND (w.FullName LIKE @search OR u.UserName LIKE @search)
                                  ORDER BY w.FullName;",
                MapWorker, P("@companyId", companyId), P("@search", "%" + searchText + "%"));

        public Worker? GetByUserId(int userId) =>
            GetOne(SelectAll + "WHERE w.UserID = @id;", MapWorker, P("@id", userId));

        public Worker? GetById(int workerId, int companyId) =>
            GetOne(SelectAll + "WHERE w.WorkerID = @id AND w.CompanyID = @companyId;",
                MapWorker, P("@id", workerId), P("@companyId", companyId));

        /// <summary>
        /// Creates the Users row and the Workers row in one transaction. Returns the new WorkerID.
        /// </summary>
        public int Add(User user, Worker worker)
        {
            // Rules 1 and 5: role is fixed, and the login belongs to the same company as the worker
            user.Role = UserRole.CollectionWorker;
            user.CompanyID = worker.CompanyID;
            user.AccountStatus = RecordStatus.Active;

            const string sql = @"INSERT INTO Workers (UserID, CompanyID, FullName, Phone, Status)
                                 VALUES (@userId, @companyId, @fullName, @phone, 'Active');";

            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    int userId = _userRepository.Insert(user, connection, transaction);

                    int workerId = ExecuteInsertIn(connection, transaction, sql,
                        P("@userId", userId), P("@companyId", worker.CompanyID),
                        P("@fullName", worker.FullName), P("@phone", worker.Phone));

                    transaction.Commit();
                    return workerId;
                }
            }
        }

        public bool Update(Worker worker) =>
            ExecuteUpdate(@"UPDATE Workers SET FullName = @fullName, Phone = @phone
                            WHERE WorkerID = @id AND CompanyID = @companyId;",
                P("@fullName", worker.FullName), P("@phone", worker.Phone),
                P("@id", worker.WorkerID), P("@companyId", worker.CompanyID)) > 0;

        // Changes the worker's status AND their login account status in one statement,
        // so a deactivated worker can no longer log in.
        public bool SetStatus(int workerId, int companyId, RecordStatus status) =>
            ExecuteUpdate(@"UPDATE Workers w JOIN Users u ON u.UserID = w.UserID
                            SET w.Status = @status, u.AccountStatus = @status
                            WHERE w.WorkerID = @id AND w.CompanyID = @companyId;",
                P("@status", status.ToString()), P("@id", workerId), P("@companyId", companyId)) > 0;
    }
}