using KWMS.Models;
using MySqlConnector;

namespace KWMS.Data.Repositories
{
    public class PickupRequestRepository : BaseRepository
    {
        private const string SelectAll =
            @"SELECT pr.RequestID, pr.SubscriptionID, pr.RequestedDate, pr.Description, pr.Status, pr.CreatedAt,
                     r.FullName AS ResidentName, sv.ServiceName,
                     CONCAT_WS(', ', r.HouseAddress, r.Sector, r.District) AS Location
              FROM PickupRequests pr
              JOIN Subscriptions sub ON sub.SubscriptionID = pr.SubscriptionID
              JOIN Residents r ON r.ResidentID = sub.ResidentID
              JOIN Services sv ON sv.ServiceID = sub.ServiceID ";

        // Only changes a request that is still Pending and belongs to this company
        private const string ChangeStatusSql =
            @"UPDATE PickupRequests pr
              JOIN Subscriptions sub ON sub.SubscriptionID = pr.SubscriptionID
              JOIN Services sv ON sv.ServiceID = sub.ServiceID
              SET pr.Status = @newStatus
              WHERE pr.RequestID = @requestId AND pr.Status = 'Pending' AND sv.CompanyID = @companyId;";

        private static PickupRequest MapRequest(MySqlDataReader r) => new PickupRequest
        {
            RequestID = r.ReadInt("RequestID"),
            SubscriptionID = r.ReadInt("SubscriptionID"),
            RequestedDate = r.ReadDate("RequestedDate"),
            Description = r.ReadString("Description"),
            Status = EnumMapper.ToEnum<PickupRequestStatus>(r.ReadString("Status")),
            CreatedAt = r.ReadDate("CreatedAt"),
            ResidentName = r.ReadString("ResidentName"),
            ServiceName = r.ReadString("ServiceName"),
            Location = r.ReadString("Location")
        };

        public List<PickupRequest> GetByResident(int residentId) =>
            GetList(SelectAll + "WHERE sub.ResidentID = @residentId ORDER BY pr.CreatedAt DESC;",
                MapRequest, P("@residentId", residentId));

        public List<PickupRequest> GetByCompany(int companyId) =>
            GetList(SelectAll + "WHERE sv.CompanyID = @companyId ORDER BY pr.CreatedAt DESC;",
                MapRequest, P("@companyId", companyId));

        /// <summary>
        /// Rule 3: the request is only inserted if the subscription is Active and belongs to
        /// this resident. Returns the new RequestID, or 0 if those conditions fail.
        /// </summary>
        public int Create(PickupRequest request, int residentId)
        {
            const string sql = @"INSERT INTO PickupRequests (SubscriptionID, RequestedDate, Description)
                                 SELECT sub.SubscriptionID, @date, @description
                                 FROM Subscriptions sub
                                 WHERE sub.SubscriptionID = @subscriptionId
                                   AND sub.ResidentID = @residentId AND sub.Status = 'Active';";
            return ExecuteInsert(sql,
                P("@date", request.RequestedDate.Date), P("@description", request.Description),
                P("@subscriptionId", request.SubscriptionID), P("@residentId", residentId));
        }

        // Approving a request also creates its Pending collection task (one transaction)
        public bool Approve(int requestId, int companyId)
        {
            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    int changed = ExecuteUpdateIn(connection, transaction, ChangeStatusSql,
                        P("@newStatus", PickupRequestStatus.Approved.ToString()),
                        P("@requestId", requestId), P("@companyId", companyId));

                    if (changed == 0) return false;

                    ExecuteInsertIn(connection, transaction,
                        "INSERT INTO CollectionTasks (PickupRequestID) VALUES (@requestId);",
                        P("@requestId", requestId));

                    transaction.Commit();
                    return true;
                }
            }
        }

        public bool Reject(int requestId, int companyId) =>
            ExecuteUpdate(ChangeStatusSql,
                P("@newStatus", PickupRequestStatus.Rejected.ToString()),
                P("@requestId", requestId), P("@companyId", companyId)) > 0;
    }
}