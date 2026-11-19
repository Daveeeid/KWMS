using KWMS.Models;
using MySqlConnector;

namespace KWMS.Data.Repositories
{
    public class PaymentRepository : BaseRepository
    {
        private const string SelectAll =
            @"SELECT p.PaymentID, p.SubscriptionID, p.RecordedByUserID, p.Amount, p.PaymentMethod, p.PaymentDate,
                     p.ReferenceNumber, p.Notes, p.CreatedAt,
                     r.FullName AS ResidentName, sv.ServiceName, c.CompanyName, u.UserName AS RecordedByName
              FROM Payments p
              JOIN Subscriptions sub ON sub.SubscriptionID = p.SubscriptionID
              JOIN Residents r ON r.ResidentID = sub.ResidentID
              JOIN Services sv ON sv.ServiceID = sub.ServiceID
              JOIN Companies c ON c.CompanyID = sv.CompanyID
              JOIN Users u ON u.UserID = p.RecordedByUserID ";

        private static Payment MapPayment(MySqlDataReader r) => new Payment
        {
            PaymentID = r.ReadInt("PaymentID"),
            SubscriptionID = r.ReadInt("SubscriptionID"),
            RecordedByUserID = r.ReadInt("RecordedByUserID"),
            Amount = r.ReadDecimal("Amount"),
            PaymentMethod = EnumMapper.ToPaymentMethod(r.ReadString("PaymentMethod")),
            PaymentDate = r.ReadDate("PaymentDate"),
            ReferenceNumber = r.ReadStringOrNull("ReferenceNumber"),
            Notes = r.ReadStringOrNull("Notes"),
            CreatedAt = r.ReadDate("CreatedAt"),
            ResidentName = r.ReadString("ResidentName"),
            ServiceName = r.ReadString("ServiceName"),
            CompanyName = r.ReadString("CompanyName"),
            RecordedByName = r.ReadString("RecordedByName")
        };

        /// <summary>
        /// Rules 9 and 10. The payment is inserted only if the subscription exists AND belongs to
        /// this company AND RecordedByUserID is a COMPANY_MANAGER of that same company.
        /// Returns the new PaymentID, or 0 if any condition fails.
        /// </summary>
        public int Record(Payment payment, int companyId)
        {
            const string sql = @"INSERT INTO Payments (SubscriptionID, RecordedByUserID, Amount, PaymentMethod, PaymentDate, ReferenceNumber, Notes)
                                 SELECT sub.SubscriptionID, @recordedBy, @amount, @method, @paymentDate, @reference, @notes
                                 FROM Subscriptions sub JOIN Services sv ON sv.ServiceID = sub.ServiceID
                                 WHERE sub.SubscriptionID = @subscriptionId
                                   AND sv.CompanyID = @companyId
                                   AND EXISTS (SELECT 1 FROM Users u
                                               WHERE u.UserID = @recordedBy AND u.Role = 'COMPANY_MANAGER'
                                                 AND u.CompanyID = @companyId);";
            return ExecuteInsert(sql,
                P("@recordedBy", payment.RecordedByUserID), P("@amount", payment.Amount),
                P("@method", EnumMapper.ToDb(payment.PaymentMethod)), P("@paymentDate", payment.PaymentDate.Date),
                P("@reference", payment.ReferenceNumber), P("@notes", payment.Notes),
                P("@subscriptionId", payment.SubscriptionID), P("@companyId", companyId));
        }

        public List<Payment> GetByCompany(int companyId) =>
            GetList(SelectAll + "WHERE sv.CompanyID = @companyId ORDER BY p.PaymentDate DESC, p.PaymentID DESC;",
                MapPayment, P("@companyId", companyId));

        public List<Payment> GetByResident(int residentId) =>
            GetList(SelectAll + "WHERE sub.ResidentID = @residentId ORDER BY p.PaymentDate DESC, p.PaymentID DESC;",
                MapPayment, P("@residentId", residentId));
    }
}