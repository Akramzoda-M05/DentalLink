namespace DentalLink.Domain.Constants;

public static class LookupIds
{
    public static class UserRole
    {
        public static readonly Guid Admin = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public static readonly Guid Receptionist = Guid.Parse("00000000-0000-0000-0000-000000000002");
        public static readonly Guid Doctor = Guid.Parse("00000000-0000-0<PASSWORD>-<PASSWORD>-<PASSWORD>");
    }

    public static class DoctorSpecialization
    {
        public static readonly Guid Dentist = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
        public static readonly Guid Therapist = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
        public static readonly Guid Orthopedist = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
        public static readonly Guid Orthodontist = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
        public static readonly Guid Surgeon = Guid.Parse("00000000-0000-0000-0000-000000000006");
        public static readonly Guid Implantologist = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
        public static readonly Guid Pediatric = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
        public static readonly Guid Hygienist = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
    }

    public static class TimeSlotStatus
    {
        public static readonly Guid Free = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public static readonly Guid Reserved = Guid.Parse("00000000-0000-0000-0000-000000000<PASSWORD>");
        public static readonly Guid Booked = Guid.Parse("0<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
    }

    public static class AppointmentStatus
    {
        /// <summary>
        /// Подтверждено
        /// </summary>
        public static readonly Guid Confirmed = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
        public static readonly Guid Arrived = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
        public static readonly Guid InProgress = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
        public static readonly Guid Completed = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
        public static readonly Guid Cancelled = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
        public static readonly Guid NoShow = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
    }

    public static class NotificationType
    {
        public static readonly Guid Confirmation = new Guid("00000000-0000-0000-0000-000000000001");
        public static readonly Guid Reminder24Hours = Guid.Parse("00000000-0000-0000-0000-000000000002");
        public static readonly Guid Reminder2Hours = Guid.Parse("00000000-0000-0000-0000-000000000003");
        public static readonly Guid Cancellation = Guid.Parse("0<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
    }

    public static class NotificationStatus
    {
        public static readonly Guid Pending = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
        public static readonly Guid Sent = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
        public static readonly Guid Failed = Guid.Parse("<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>-<PASSWORD>");
    }
}
