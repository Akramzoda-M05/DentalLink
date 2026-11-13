namespace DentalLink.Domain.Enums
{
    public enum UserRole { Admin, Receptionist, Doctor }

    public enum DoctorSpecialization
    {
        Dentist, Therapist, Orthopedist, Orthodontist,
        Surgeon, Implantologist, Pediatric, Hygienist
    }

    public enum SlotStatus { Free, Reserved, Booked }

    public enum AppointmentStatus
    {
        Confirmed, Arrived, InProgress, Completed, Cancelled, NoShow
    }

    public enum NotificationType
    {
        Confirmation, Reminder24h, Reminder2h, Cancellation
    }

    public enum NotificationStatus { Pending, Sent, Failed }
}
