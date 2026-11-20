using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    public class PaymentController : ManagerControllerBase
    {
        private readonly PaymentRepository _payments = new PaymentRepository();
        private readonly SubscriptionRepository _subscriptions = new SubscriptionRepository();

        public DataResult<List<Payment>> GetPayments()
        {
            int? companyId = CompanyId;
            if (companyId == null) return DataResult<List<Payment>>.Fail(NoPermission);
            try { return DataResult<List<Payment>>.Ok(_payments.GetByCompany(companyId.Value)); }
            catch (MySqlException) { return DataResult<List<Payment>>.Fail(DatabaseDown); }
        }


        public DataResult<List<Subscription>> GetSubscriptions()
        {
            int? companyId = CompanyId;
            if (companyId == null) return DataResult<List<Subscription>>.Fail(NoPermission);
            try { return DataResult<List<Subscription>>.Ok(_subscriptions.GetByCompany(companyId.Value, false)); }
            catch (MySqlException) { return DataResult<List<Subscription>>.Fail(DatabaseDown); }
        }

        public OperationResult RecordPayment(int subscriptionId, decimal amount, PaymentMethod method,
                                             DateTime paymentDate, string? reference, string? notes)
        {
            int? companyId = CompanyId;
            if (companyId == null) return OperationResult.Fail(NoPermission);

            reference = ValidationHelper.NullIfEmpty(reference);
            notes = ValidationHelper.NullIfEmpty(notes);

            List<string> errors = new List<string>();
            if (subscriptionId <= 0) errors.Add("Please select a subscription.");
            if (amount <= 0 || amount > 10000000) errors.Add("Amount must be more than 0 and at most 10,000,000.");
            if (paymentDate.Date > DateTime.Today) errors.Add("The payment date cannot be in the future.");
            if (method != PaymentMethod.Cash && reference == null)
                errors.Add("A reference number is required for Mobile Money and Bank Transfer payments.");
            if (reference != null && reference.Length > 50) errors.Add("Reference number must be at most 50 characters.");
            if (notes != null && notes.Length > 255) errors.Add("Notes must be at most 255 characters.");
            if (errors.Count > 0) return OperationResult.Fail(ValidationHelper.FormatErrors(errors));

            try
            {
                Payment payment = new Payment
                {
                    SubscriptionID = subscriptionId,
                    RecordedByUserID = SessionManager.CurrentUser!.UserID,
                    Amount = amount,
                    PaymentMethod = method,
                    PaymentDate = paymentDate.Date,
                    ReferenceNumber = reference,
                    Notes = notes
                };


                return _payments.Record(payment, companyId.Value) > 0
                    ? OperationResult.Ok($"Payment of {amount:N0} RWF was recorded.")
                    : OperationResult.Fail("The payment could not be recorded. The subscription was not found in your company.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }
    }
}