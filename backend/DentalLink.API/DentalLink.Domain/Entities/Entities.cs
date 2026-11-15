namespace DentalLink.Domain.Entities
{
    public sealed class UserRole
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ICollection<User> Users { get; set; } = [];
    }

    public sealed class DoctorSpecialization
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ICollection<Doctor> Doctors { get; set; } = [];
    }

    public sealed class TimeSlotStatus
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ICollection<TimeSlot> TimeSlots { get; set; } = [];
    }

    public sealed class AppointmentStatus
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ICollection<Appointment> Appointments { get; set; } = [];
    }

    public sealed class NotificationType
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ICollection<Notification> Notifications { get; set; } = [];
    }

    public sealed class NotificationStatus
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ICollection<Notification> Notifications { get; set; } = [];
    }

    // -------------------------------------------------------------------------
    // users — учётные записи персонала клиники
    // -------------------------------------------------------------------------
    public class User
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public long RoleId { get; set; }
        public UserRole Role { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Навигационные свойства
        public Doctor? DoctorProfile { get; set; }
        public ICollection<Appointment> CreatedAppointments { get; set; } = [];
    }

    // -------------------------------------------------------------------------
    // doctors — профили врачей-стоматологов (1:1 → users)
    // -------------------------------------------------------------------------
    public class Doctor
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long SpecializationId { get; set; }
        public DoctorSpecialization Specialization { get; set; } = null!;
        public string? Qualification { get; set; }
        public int ExperienceYears { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Навигационные свойства
        public User User { get; set; } = null!;
        public ICollection<DoctorSchedule> Schedules { get; set; } = [];
        public ICollection<TimeSlot> TimeSlots { get; set; } = [];
        public ICollection<Appointment> Appointments { get; set; } = [];
    }

    // -------------------------------------------------------------------------
    // cabinets — лечебные кабинеты клиники
    // -------------------------------------------------------------------------
    public class Cabinet
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ChairNumber { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; } = true;

        public ICollection<DoctorSchedule> Schedules { get; set; } = [];
        public ICollection<TimeSlot> TimeSlots { get; set; } = [];
        public ICollection<Appointment> Appointments { get; set; } = [];
    }

    // -------------------------------------------------------------------------
    // service_categories — категории стоматологических услуг
    // -------------------------------------------------------------------------
    public class ServiceCategory
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; }

        public ICollection<DentalService> Services { get; set; } = [];
    }

    // -------------------------------------------------------------------------
    // dental_services — прейскурант стоматологических услуг
    // -------------------------------------------------------------------------
    public class DentalService
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ServiceCategory Category { get; set; } = null!;
        public ICollection<AppointmentService> AppointmentServices { get; set; } = [];
    }

    // -------------------------------------------------------------------------
    // doctor_schedules — график рабочих смен врачей
    // -------------------------------------------------------------------------
    public class DoctorSchedule
    {
        public long Id { get; set; }
        public long DoctorId { get; set; }
        public long CabinetId { get; set; }
        public DateOnly WorkDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public Doctor Doctor { get; set; } = null!;
        public Cabinet Cabinet { get; set; } = null!;
        public ICollection<TimeSlot> TimeSlots { get; set; } = [];
    }

    // -------------------------------------------------------------------------
    // time_slots — временные слоты расписания (шаг 30 минут)
    // -------------------------------------------------------------------------
    public class TimeSlot
    {
        public long Id { get; set; }
        public long ScheduleId { get; set; }
        public long DoctorId { get; set; }
        public long CabinetId { get; set; }
        public DateOnly SlotDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public long StatusId { get; set; } = LookupIds.TimeSlotStatus.Free;
        public TimeSlotStatus Status { get; set; } = null!;
        public DateTimeOffset? ReservedUntil { get; set; }

        public DoctorSchedule Schedule { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        public Cabinet Cabinet { get; set; } = null!;
        public Appointment? Appointment { get; set; }
    }

    // -------------------------------------------------------------------------
    // patients — картотека пациентов клиники
    // -------------------------------------------------------------------------
    public class Patient
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateOnly? BirthDate { get; set; }
        public long? TelegramChatId { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<Appointment> Appointments { get; set; } = [];
    }

    // -------------------------------------------------------------------------
    // appointments — журнал записей на приём (главная операционная таблица)
    // -------------------------------------------------------------------------
    public class Appointment
    {
        public long Id { get; set; }
        public string AppointmentNumber { get; set; } = string.Empty;
        public long PatientId { get; set; }
        public long DoctorId { get; set; }
        public long SlotId { get; set; }
        public long CabinetId { get; set; }
        public long? CreatedByUserId { get; set; }
        public long StatusId { get; set; } = LookupIds.AppointmentStatus.Confirmed;
        public AppointmentStatus Status { get; set; } = null!;
        public string? PatientComplaint { get; set; }
        public string? DoctorNotes { get; set; }
        public bool IsConfirmedByPatient { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        public TimeSlot Slot { get; set; } = null!;
        public Cabinet Cabinet { get; set; } = null!;
        public User? CreatedByUser { get; set; }
        public ICollection<AppointmentService> AppointmentServices { get; set; } = [];
        public ICollection<Notification> Notifications { get; set; } = [];
    }

    // -------------------------------------------------------------------------
    // appointment_services — услуги в приёме (таблица-связка M:N)
    // -------------------------------------------------------------------------
    public class AppointmentService
    {
        public long Id { get; set; }
        public long AppointmentId { get; set; }
        public long ServiceId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal PriceAtBooking { get; set; }

        public Appointment Appointment { get; set; } = null!;
        public DentalService Service { get; set; } = null!;
    }

    // -------------------------------------------------------------------------
    // notifications — очередь и журнал Telegram-уведомлений
    // -------------------------------------------------------------------------
    public class Notification
    {
        public long Id { get; set; }
        public long AppointmentId { get; set; }
        public long TelegramChatId { get; set; }
        public long TypeId { get; set; }
        public NotificationType Type { get; set; } = null!;
        public long StatusId { get; set; } = LookupIds.NotificationStatus.Pending;
        public NotificationStatus Status { get; set; } = null!;
        public DateTimeOffset ScheduledAt { get; set; }
        public DateTimeOffset? SentAt { get; set; }
        public string? ResponsePayload { get; set; }

        public Appointment Appointment { get; set; } = null!;
    }
}
