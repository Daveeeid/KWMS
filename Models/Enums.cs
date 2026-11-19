namespace KWMS.Models
{
    public enum UserRole { Admin, CompanyManager, CollectionWorker, Resident }

    // Used for Companies.Status, Users.AccountStatus, Workers.Status, Services.Status
    public enum RecordStatus { Active, Inactive }

    public enum SubscriptionStatus { Active, Cancelled, Expired }
    public enum CollectionFrequency { Weekly, Biweekly, Monthly }
    public enum ScheduleStatus { Scheduled, Cancelled }
    public enum PickupRequestStatus { Pending, Approved, Rejected, Completed }

    // Named CollectionTaskStatus (not TaskStatus) to avoid a clash with System.Threading.Tasks.TaskStatus
    public enum CollectionTaskStatus { Pending, Assigned, InProgress, Completed, Cancelled }

    public enum PaymentMethod { Cash, MobileMoney, BankTransfer }
    public enum ComplaintStatus { Open, InProgress, Resolved }
}