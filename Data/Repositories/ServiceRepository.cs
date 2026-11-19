using KWMS.Models;
using MySqlConnector;

namespace KWMS.Data.Repositories
{
    public class ServiceRepository : BaseRepository
    {
        private const string SelectAll =
            @"SELECT s.ServiceID, s.CompanyID, s.ServiceName, s.Description, s.MonthlyPrice,
                     s.CollectionFrequency, s.Status, s.CreatedAt, c.CompanyName
              FROM Services s JOIN Companies c ON c.CompanyID = s.CompanyID ";

        private static Service MapService(MySqlDataReader r) => new Service
        {
            ServiceID = r.ReadInt("ServiceID"),
            CompanyID = r.ReadInt("CompanyID"),
            ServiceName = r.ReadString("ServiceName"),
            Description = r.ReadStringOrNull("Description"),
            MonthlyPrice = r.ReadDecimal("MonthlyPrice"),
            CollectionFrequency = EnumMapper.ToEnum<CollectionFrequency>(r.ReadString("CollectionFrequency")),
            Status = EnumMapper.ToRecordStatus(r.ReadStringOrNull("Status")),
            CreatedAt = r.ReadDate("CreatedAt"),
            CompanyName = r.ReadString("CompanyName")
        };

        // All services of one company (manager view)
        public List<Service> GetByCompany(int companyId) =>
            GetList(SelectAll + "WHERE s.CompanyID = @companyId ORDER BY s.ServiceName;",
                MapService, P("@companyId", companyId));

        // Only what a resident may subscribe to: Active service of an Active company
        public List<Service> GetAvailableByCompany(int companyId) =>
            GetList(SelectAll + @"WHERE s.CompanyID = @companyId AND s.Status = 'Active' AND c.Status = 'Active'
                                  ORDER BY s.ServiceName;",
                MapService, P("@companyId", companyId));

        public Service? GetById(int serviceId) =>
            GetOne(SelectAll + "WHERE s.ServiceID = @id;", MapService, P("@id", serviceId));

        public int Insert(Service service) =>
            ExecuteInsert(@"INSERT INTO Services (CompanyID, ServiceName, Description, MonthlyPrice, CollectionFrequency, Status)
                            VALUES (@companyId, @name, @description, @price, @frequency, @status);",
                P("@companyId", service.CompanyID), P("@name", service.ServiceName),
                P("@description", service.Description), P("@price", service.MonthlyPrice),
                P("@frequency", service.CollectionFrequency.ToString()), P("@status", service.Status.ToString()));

        public bool Update(Service service) =>
            ExecuteUpdate(@"UPDATE Services
                            SET ServiceName = @name, Description = @description,
                                MonthlyPrice = @price, CollectionFrequency = @frequency
                            WHERE ServiceID = @id AND CompanyID = @companyId;",
                P("@name", service.ServiceName), P("@description", service.Description),
                P("@price", service.MonthlyPrice), P("@frequency", service.CollectionFrequency.ToString()),
                P("@id", service.ServiceID), P("@companyId", service.CompanyID)) > 0;

        public bool SetStatus(int serviceId, int companyId, RecordStatus status) =>
            ExecuteUpdate("UPDATE Services SET Status = @status WHERE ServiceID = @id AND CompanyID = @companyId;",
                P("@status", status.ToString()), P("@id", serviceId), P("@companyId", companyId)) > 0;

        // The database has UNIQUE (CompanyID, ServiceName); this gives a friendly pre-check
        public bool NameExists(int companyId, string serviceName, int excludeServiceId = 0) =>
            Exists(@"SELECT COUNT(*) FROM Services
                     WHERE CompanyID = @companyId AND ServiceName = @name AND ServiceID <> @id;",
                P("@companyId", companyId), P("@name", serviceName), P("@id", excludeServiceId));
    }
}