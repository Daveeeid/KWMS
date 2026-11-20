using KWMS.Models;
using MySqlConnector;

namespace KWMS.Data.Repositories
{
    public class ComplaintRepository : BaseRepository
    {
        private const string SelectAll =
            @"SELECT cp.ComplaintID, cp.ResidentID, cp.CompanyID, cp.Subject, cp.Description, cp.Status, cp.CreatedAt,
                     r.FullName AS ResidentName, c.CompanyName
              FROM Complaints cp
              JOIN Residents r ON r.ResidentID = cp.ResidentID
              JOIN Companies c ON c.CompanyID = cp.CompanyID ";

        private static Complaint MapComplaint(MySqlDataReader r) => new Complaint
        {
            ComplaintID = r.ReadInt("ComplaintID"),
            ResidentID = r.ReadInt("ResidentID"),
            CompanyID = r.ReadInt("CompanyID"),
            Subject = r.ReadString("Subject"),
            Description = r.ReadString("Description"),
            Status = EnumMapper.ToEnum<ComplaintStatus>(r.ReadString("Status")),
            CreatedAt = r.ReadDate("CreatedAt"),
            ResidentName = r.ReadString("ResidentName"),
            CompanyName = r.ReadString("CompanyName")
        };

        private static ComplaintResponse MapResponse(MySqlDataReader r) => new ComplaintResponse
        {
            ResponseID = r.ReadInt("ResponseID"),
            ComplaintID = r.ReadInt("ComplaintID"),
            ManagerUserID = r.ReadInt("ManagerUserID"),
            ResponseText = r.ReadString("ResponseText"),
            CreatedAt = r.ReadDate("CreatedAt"),
            ManagerName = r.ReadString("ManagerName")
        };


        public int Create(Complaint complaint) =>
            ExecuteInsert(@"INSERT INTO Complaints (ResidentID, CompanyID, Subject, Description)
                            VALUES (@residentId, @companyId, @subject, @description);",
                P("@residentId", complaint.ResidentID), P("@companyId", complaint.CompanyID),
                P("@subject", complaint.Subject), P("@description", complaint.Description));

        public List<Complaint> GetByResident(int residentId) =>
            GetList(SelectAll + "WHERE cp.ResidentID = @residentId ORDER BY cp.CreatedAt DESC;",
                MapComplaint, P("@residentId", residentId));

        public List<Complaint> GetByCompany(int companyId) =>
            GetList(SelectAll + "WHERE cp.CompanyID = @companyId ORDER BY cp.CreatedAt DESC;",
                MapComplaint, P("@companyId", companyId));

        public List<ComplaintResponse> GetResponses(int complaintId) =>
            GetList(@"SELECT cr.ResponseID, cr.ComplaintID, cr.ManagerUserID, cr.ResponseText, cr.CreatedAt,
                             u.UserName AS ManagerName
                      FROM ComplaintResponses cr JOIN Users u ON u.UserID = cr.ManagerUserID
                      WHERE cr.ComplaintID = @complaintId ORDER BY cr.CreatedAt;",
                MapResponse, P("@complaintId", complaintId));


        public int AddResponse(ComplaintResponse response, int companyId)
        {
            const string sql = @"INSERT INTO ComplaintResponses (ComplaintID, ManagerUserID, ResponseText)
                                 SELECT cp.ComplaintID, @managerId, @text
                                 FROM Complaints cp
                                 WHERE cp.ComplaintID = @complaintId AND cp.CompanyID = @companyId
                                   AND EXISTS (SELECT 1 FROM Users u
                                               WHERE u.UserID = @managerId AND u.Role = 'COMPANY_MANAGER'
                                                 AND u.CompanyID = @companyId);";
            return ExecuteInsert(sql,
                P("@managerId", response.ManagerUserID), P("@text", response.ResponseText),
                P("@complaintId", response.ComplaintID), P("@companyId", companyId));
        }

        public bool UpdateStatus(int complaintId, int companyId, ComplaintStatus status) =>
            ExecuteUpdate("UPDATE Complaints SET Status = @status WHERE ComplaintID = @id AND CompanyID = @companyId;",
                P("@status", status.ToString()), P("@id", complaintId), P("@companyId", companyId)) > 0;
    }
}