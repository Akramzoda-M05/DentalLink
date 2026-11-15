namespace DentalLink.Domain.Entities;

public static class LookupCodes
{
    public static class UserRole
    {
        public const string Admin = "admin";
        public const string Receptionist = "receptionist";
        public const string Doctor = "doctor";
    }

    public static class DoctorSpecialization
    {
        public const string Dentist = "dentist";
        public const string Therapist = "therapist";
        public const string Orthopedist = "orthopedist";
        public const string Orthodontist = "orthodontist";
        public const string Surgeon = "surgeon";
        public const string Implantologist = "implantologist";
        public const string Pediatric = "pediatric";
        public const string Hygienist = "hygienist";
    }

    public static class TimeSlotStatus
    {
        public const string Free = "free";
        public const string Reserved = "reserved";
        public const string Booked = "booked";
    }

    public static class AppointmentStatus
    {
        public const string Confirmed = "confirmed";
        public const string Arrived = "arrived";
        public const string InProgress = "in_progress";
        public const string Completed = "completed";
        public const string Cancelled = "cancelled";
        public const string NoShow = "no_show";
    }

    public static class NotificationType
    {
        public const string Confirmation = "confirmation";
        public const string Reminder24Hours = "reminder_24h";
        public const string Reminder2Hours = "reminder_2h";
        public const string Cancellation = "cancellation";
    }

    public static class NotificationStatus
    {
        public const string Pending = "pending";
        public const string Sent = "sent";
        public const string Failed = "failed";
    }
}
