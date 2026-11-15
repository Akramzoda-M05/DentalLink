namespace DentalLink.Domain.Entities;

public static class LookupIds
{
    public static class UserRole
    {
        public const long Admin = 1;
        public const long Receptionist = 2;
        public const long Doctor = 3;
    }

    public static class DoctorSpecialization
    {
        public const long Dentist = 1;
        public const long Therapist = 2;
        public const long Orthopedist = 3;
        public const long Orthodontist = 4;
        public const long Surgeon = 5;
        public const long Implantologist = 6;
        public const long Pediatric = 7;
        public const long Hygienist = 8;
    }

    public static class TimeSlotStatus
    {
        public const long Free = 1;
        public const long Reserved = 2;
        public const long Booked = 3;
    }

    public static class AppointmentStatus
    {
        public const long Confirmed = 1;
        public const long Arrived = 2;
        public const long InProgress = 3;
        public const long Completed = 4;
        public const long Cancelled = 5;
        public const long NoShow = 6;
    }

    public static class NotificationType
    {
        public const long Confirmation = 1;
        public const long Reminder24Hours = 2;
        public const long Reminder2Hours = 3;
        public const long Cancellation = 4;
    }

    public static class NotificationStatus
    {
        public const long Pending = 1;
        public const long Sent = 2;
        public const long Failed = 3;
    }
}
