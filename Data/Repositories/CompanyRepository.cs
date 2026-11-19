using KWMS.Models;
using MySqlConnector;

namespace KWMS.Data.Repositories
{
    public class CompanyRepository : BaseRepository
    {
        private const string SelectAll =
            "SELECT CompanyID, CompanyName, RegistrationNumber, Phone, Address, Status, CreatedAt FROM Companies ";

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