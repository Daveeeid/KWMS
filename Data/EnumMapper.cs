using KWMS.Models;

namespace KWMS.Data
{





    public static class EnumMapper
    {
        public static string ToDb(UserRole role) => role switch
        {
            UserRole.Admin => "ADMIN",
            UserRole.CompanyManager => "COMPANY_MANAGER",
            UserRole.CollectionWorker => "COLLECTION_WORKER",
            UserRole.Resident => "RESIDENT",
            _ => throw new ArgumentOutOfRangeException(nameof(role))
        };

        public static UserRole ToRole(string? value) => value switch
        {
            "ADMIN" => UserRole.Admin,
            "COMPANY_MANAGER" => UserRole.CompanyManager,
            "COLLECTION_WORKER" => UserRole.CollectionWorker,
            "RESIDENT" => UserRole.Resident,
            _ => throw new InvalidDataException($"Unknown or missing user role in database: '{value}'.")
        };


        public static RecordStatus ToRecordStatus(string? value) =>
            value == "Active" ? RecordStatus.Active : RecordStatus.Inactive;

        public static string ToDb(PaymentMethod method) => method switch
        {
            PaymentMethod.Cash => "Cash",
            PaymentMethod.MobileMoney => "Mobile Money",
            PaymentMethod.BankTransfer => "Bank Transfer",
            _ => throw new ArgumentOutOfRangeException(nameof(method))
        };

        public static PaymentMethod ToPaymentMethod(string value) => value switch
        {
            "Cash" => PaymentMethod.Cash,
            "Mobile Money" => PaymentMethod.MobileMoney,
            "Bank Transfer" => PaymentMethod.BankTransfer,
            _ => throw new InvalidDataException($"Unknown payment method in database: '{value}'.")
        };


        public static T ToEnum<T>(string value) where T : struct, Enum => Enum.Parse<T>(value);
    }
}