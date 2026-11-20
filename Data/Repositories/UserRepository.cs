using KWMS.Models;
using MySqlConnector;

namespace KWMS.Data.Repositories
{
    public class UserRepository : BaseRepository
    {
        private const string FromClause = " FROM Users u LEFT JOIN Companies c ON u.CompanyID = c.CompanyID ";

        // Used for login: includes the real password hash
        private const string LoginColumns =
            "SELECT u.UserID, u.CompanyID, u.UserName, u.Email, u.PasswordHash, u.Role, u.AccountStatus, u.CreatedAt, c.CompanyName";

        // Used for lists/grids: the hash is replaced by '' so it can never reach a screen
        private const string ListColumns =
            "SELECT u.UserID, u.CompanyID, u.UserName, u.Email, '' AS PasswordHash, u.Role, u.AccountStatus, u.CreatedAt, c.CompanyName";

        private const string InsertSql = @"INSERT INTO Users (CompanyID, UserName, Email, PasswordHash, Role, AccountStatus)
                                           VALUES (@companyId, @userName, @email, @hash, @role, @status);";

        private static User MapUser(MySqlDataReader r) => new User
        {
            UserID = r.ReadInt("UserID"),
            CompanyID = r.ReadIntOrNull("CompanyID"),
            UserName = r.ReadString("UserName"),
            Email = r.ReadString("Email"),
            PasswordHash = r.ReadString("PasswordHash"),
            Role = EnumMapper.ToRole(r.ReadStringOrNull("Role")),
            AccountStatus = EnumMapper.ToRecordStatus(r.ReadStringOrNull("AccountStatus")),
            CreatedAt = r.ReadDate("CreatedAt"),
            CompanyName = r.ReadStringOrNull("CompanyName")
        };

        private static MySqlParameter[] InsertParameters(User u) => new[]
        {
            P("@companyId", u.CompanyID), P("@userName", u.UserName), P("@email", u.Email),
            P("@hash", u.PasswordHash), P("@role", EnumMapper.ToDb(u.Role)),
            P("@status", u.AccountStatus.ToString())
        };

        public User? GetByUserName(string userName) =>
            GetOne(LoginColumns + FromClause + "WHERE u.UserName = @userName;", MapUser, P("@userName", userName));

        public User? GetById(int userId) =>
            GetOne(ListColumns + FromClause + "WHERE u.UserID = @id;", MapUser, P("@id", userId));

        public List<User> GetAll(string? searchText = null, UserRole? role = null)
        {
            string sql = ListColumns + FromClause + @"
                WHERE (u.UserName LIKE @search OR u.Email LIKE @search)
                  AND (@role IS NULL OR u.Role = @role)
                ORDER BY u.UserName;";

            return GetList(sql, MapUser,
                P("@search", "%" + searchText + "%"),
                P("@role", role == null ? null : EnumMapper.ToDb(role.Value)));
        }

        public bool UserNameExists(string userName) =>
            Exists("SELECT COUNT(*) FROM Users WHERE UserName = @userName;", P("@userName", userName));

        public bool EmailExists(string email, int excludeUserId = 0) =>
            Exists("SELECT COUNT(*) FROM Users WHERE Email = @email AND UserID <> @id;",
                P("@email", email), P("@id", excludeUserId));

        public int Insert(User user) => ExecuteInsert(InsertSql, InsertParameters(user));

        // Same insert, but inside a transaction started by another repository
        public int Insert(User user, MySqlConnection connection, MySqlTransaction transaction) =>
            ExecuteInsertIn(connection, transaction, InsertSql, InsertParameters(user));

        public bool SetAccountStatus(int userId, RecordStatus status) =>
            ExecuteUpdate("UPDATE Users SET AccountStatus = @status WHERE UserID = @id;",
                P("@status", status.ToString()), P("@id", userId)) > 0;

        public bool UpdateEmail(int userId, string email) =>
            ExecuteUpdate("UPDATE Users SET Email = @email WHERE UserID = @id;",
                P("@email", email), P("@id", userId)) > 0;

        // Used at startup to decide whether the first admin must be created
        public bool AnyAdminExists() =>
            Exists("SELECT COUNT(*) FROM Users WHERE Role = 'ADMIN';");

        public bool UpdatePasswordHash(int userId, string passwordHash) =>
        ExecuteUpdate("UPDATE Users SET PasswordHash = @hash WHERE UserID = @id;",
        P("@hash", passwordHash), P("@id", userId)) > 0;
    }
}