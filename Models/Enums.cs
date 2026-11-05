namespace ECMS;

public enum UserRole { Admin, Student, Instructor }

public enum CourseKind { Online, Offline }

public enum AccountStatus { Pending, Active, Rejected, Deactivated }

public enum TeachingPreference { Online, Offline, Both }

public enum BatchStatus { Scheduled, Ongoing, Completed, Cancelled }

public enum PaymentStatus { Pending, Successful, Failed, Refunded }

public enum PaymentMethod { CreditCard, DebitCard, VodafoneCash, Fawry, PlatformCredit }

public enum EnrollmentStatus { Confirmed, Cancelled, Refunded }

public enum ExamType { OnSite, Remote }

public enum ExamResult { Pending, Passed, Failed }

public enum DeleteRequestStatus { Pending, Approved, Rejected }

public enum RefundKind { Refund, Credit }
