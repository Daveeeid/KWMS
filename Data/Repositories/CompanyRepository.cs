using KWMS.Models;
using MySqlConnector;

namespace KWMS.Data.Repositories
{
    public class CompanyRepository : BaseRepository
    {
        private const string SelectAll =
            "SELECT CompanyID, CompanyName, RegistrationNumber, Phone, Address, Status, CreatedAt FROM Companies ";

        private readonly UserRepository _userRepository = new UserRepository();

        /// <summary>
        /// Creates the Companies row AND the company's first manager (Users row) in ONE transaction.
        /// If the user insert fails, the company insert is rolled back too. Returns the new CompanyID.
        /// </summary>
        public int RegisterWithManager(Company company, User manager)
        {
            const string sql = @"INSERT INTO Companies (CompanyName, RegistrationNumber, Phone, Address, Status)
                                 VALUES (@name, @reg, @phone, @address, 'Active');";

            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    int companyId = ExecuteInsertIn(connection, transaction, sql,
                        P("@name", company.CompanyName), P("@reg", company.RegistrationNumber),
                        P("@phone", company.Phone), P("@address", company.Address));

                    // Rules 1 and 5: fixed role, and the manager belongs to the new company
                    manager.Role = UserRole.CompanyManager;
                    manager.CompanyID = companyId;
                    manager.AccountStatus = RecordStatus.Active;
                    _userRepository.Insert(manager, connection, transaction);

                    transaction.Commit();
                    return companyId;
                }
            }
        }

        private static Company MapCompany(MySqlDataReader r) => new Company
        {
            CompanyID = r.ReadInt("CompanyID"),
            CompanyName = r.ReadString("CompanyName"),
            RegistrationNumber = r.ReadString("RegistrationNumber"),
            Phone = r.ReadStringOrNull("Phone"),
            Address = r.ReadStringOrNull("Address"),
            Status = EnumMapper.ToRecordStatus(r.ReadStringOrNull("Status")),
            CreatedAt = r.ReadDate("CreatedAt")
        };

        // Optional search text (name or registration number) and optional status filter
        public List<Company> GetAll(string? searchText = null, RecordStatus? status = null)
        {
            string sql = SelectAll + @"
                WHERE (CompanyName LIKE @search OR RegistrationNumber LIKE @search)
                  AND (@status IS NULL OR Status = @status)
                ORDER BY CompanyName;";

            return GetList(sql, MapCompany,
                P("@search", "%" + searchText + "%"),
                P("@status", status?.ToString()));
        }

        public Company? GetById(int companyId) =>
            GetOne(SelectAll + "WHERE CompanyID = @id;", MapCompany, P("@id", companyId));

        public int Insert(Company company)
        {
            const string sql = @"INSERT INTO Companies (CompanyName, RegistrationNumber, Phone, Address, Status)
                                 VALUES (@name, @reg, @phone, @address, @status);";
            return ExecuteInsert(sql,
                P("@name", company.CompanyName), P("@reg", company.RegistrationNumber),
                P("@phone", company.Phone), P("@address", company.Address),
                P("@status", company.Status.ToString()));
        }

        public bool Update(Company company)
        {
            const string sql = @"UPDATE Companies
                                 SET CompanyName = @name, RegistrationNumber = @reg, Phone = @phone, Address = @address
                                 WHERE CompanyID = @id;";
            return ExecuteUpdate(sql,
                P("@name", company.CompanyName), P("@reg", company.RegistrationNumber),
                P("@phone", company.Phone), P("@address", company.Address),
                P("@id", company.CompanyID)) > 0;
        }

        public bool SetStatus(int companyId, RecordStatus status) =>
            ExecuteUpdate("UPDATE Companies SET Status = @status WHERE CompanyID = @id;",
                P("@status", status.ToString()), P("@id", companyId)) > 0;

        // excludeCompanyId lets an UPDATE check "is it used by ANOTHER company?"
        public bool RegistrationNumberExists(string registrationNumber, int excludeCompanyId = 0) =>
            Exists("SELECT COUNT(*) FROM Companies WHERE RegistrationNumber = @reg AND CompanyID <> @id;",
                P("@reg", registrationNumber), P("@id", excludeCompanyId));
    }
}