using DentalLink.Application.Common.Interfaces;
using DentalLink.Domain.Constants;
using DentalLink.Domain.Entities;
using DentalLink.Domain.Entities.Lookup;
using Microsoft.EntityFrameworkCore;

namespace DentalLink.Infrastructure.Data;

public class DentalLinkDbContext(DbContextOptions<DentalLinkDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<DoctorSpecialization> DoctorSpecializations => Set<DoctorSpecialization>();
    public DbSet<TimeSlotStatus> TimeSlotStatuses => Set<TimeSlotStatus>();
    public DbSet<AppointmentStatus> AppointmentStatuses => Set<AppointmentStatus>();
    public DbSet<NotificationType> NotificationTypes => Set<NotificationType>();
    public DbSet<NotificationStatus> NotificationStatuses => Set<NotificationStatus>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Cabinet> Cabinets => Set<Cabinet>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<DentalService> DentalServices => Set<DentalService>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentService> AppointmentServices => Set<AppointmentService>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //modelBuilder.Entity<UserRole>(e =>
        //{
        //    e.ToTable("user_roles");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        //    e.Property(x => x.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        //    e.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        //    e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("uq_user_roles_code");
        //    e.HasData(
        //        new UserRole { Id = LookupIds.UserRole.Admin, Code = LookupCodes.UserRole.Admin, Name = "Administrator" },
        //        new UserRole { Id = LookupIds.UserRole.Receptionist, Code = LookupCodes.UserRole.Receptionist, Name = "Receptionist" },
        //        new UserRole { Id = LookupIds.UserRole.Doctor, Code = LookupCodes.UserRole.Doctor, Name = "Doctor" });
        //});

        //modelBuilder.Entity<DoctorSpecialization>(e =>
        //{
        //    e.ToTable("doctor_specializations");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        //    e.Property(x => x.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        //    e.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        //    e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("uq_doctor_specializations_code");
        //    e.HasData(
        //        new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Dentist, Code = LookupCodes.DoctorSpecialization.Dentist, Name = "Dentist" },
        //        new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Therapist, Code = LookupCodes.DoctorSpecialization.Therapist, Name = "Therapist" },
        //        new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Orthopedist, Code = LookupCodes.DoctorSpecialization.Orthopedist, Name = "Orthopedist" },
        //        new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Orthodontist, Code = LookupCodes.DoctorSpecialization.Orthodontist, Name = "Orthodontist" },
        //        new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Surgeon, Code = LookupCodes.DoctorSpecialization.Surgeon, Name = "Surgeon" },
        //        new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Implantologist, Code = LookupCodes.DoctorSpecialization.Implantologist, Name = "Implantologist" },
        //        new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Pediatric, Code = LookupCodes.DoctorSpecialization.Pediatric, Name = "Pediatric dentist" },
        //        new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Hygienist, Code = LookupCodes.DoctorSpecialization.Hygienist, Name = "Hygienist" });
        //});

        //modelBuilder.Entity<TimeSlotStatus>(e =>
        //{
        //    e.ToTable("time_slot_statuses");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        //    e.Property(x => x.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        //    e.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        //    e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("uq_time_slot_statuses_code");
        //    e.HasData(
        //        new TimeSlotStatus { Id = LookupIds.TimeSlotStatus.Free, Code = LookupCodes.TimeSlotStatus.Free, Name = "Free" },
        //        new TimeSlotStatus { Id = LookupIds.TimeSlotStatus.Reserved, Code = LookupCodes.TimeSlotStatus.Reserved, Name = "Reserved" },
        //        new TimeSlotStatus { Id = LookupIds.TimeSlotStatus.Booked, Code = LookupCodes.TimeSlotStatus.Booked, Name = "Booked" });
        //});

        //modelBuilder.Entity<AppointmentStatus>(e =>
        //{
        //    e.ToTable("appointment_statuses");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        //    e.Property(x => x.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        //    e.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        //    e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("uq_appointment_statuses_code");
        //    e.HasData(
        //        new AppointmentStatus { Id = LookupIds.AppointmentStatus.Confirmed, Code = LookupCodes.AppointmentStatus.Confirmed, Name = "Confirmed" },
        //        new AppointmentStatus { Id = LookupIds.AppointmentStatus.Arrived, Code = LookupCodes.AppointmentStatus.Arrived, Name = "Arrived" },
        //        new AppointmentStatus { Id = LookupIds.AppointmentStatus.InProgress, Code = LookupCodes.AppointmentStatus.InProgress, Name = "In progress" },
        //        new AppointmentStatus { Id = LookupIds.AppointmentStatus.Completed, Code = LookupCodes.AppointmentStatus.Completed, Name = "Completed" },
        //        new AppointmentStatus { Id = LookupIds.AppointmentStatus.Cancelled, Code = LookupCodes.AppointmentStatus.Cancelled, Name = "Cancelled" },
        //        new AppointmentStatus { Id = LookupIds.AppointmentStatus.NoShow, Code = LookupCodes.AppointmentStatus.NoShow, Name = "No show" });
        //});

        //modelBuilder.Entity<NotificationType>(e =>
        //{
        //    e.ToTable("notification_types");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        //    e.Property(x => x.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        //    e.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        //    e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("uq_notification_types_code");
        //    e.HasData(
        //        new NotificationType { Id = LookupIds.NotificationType.Confirmation, Code = LookupCodes.NotificationType.Confirmation, Name = "Confirmation" },
        //        new NotificationType { Id = LookupIds.NotificationType.Reminder24Hours, Code = LookupCodes.NotificationType.Reminder24Hours, Name = "Reminder (24 hours)" },
        //        new NotificationType { Id = LookupIds.NotificationType.Reminder2Hours, Code = LookupCodes.NotificationType.Reminder2Hours, Name = "Reminder (2 hours)" },
        //        new NotificationType { Id = LookupIds.NotificationType.Cancellation, Code = LookupCodes.NotificationType.Cancellation, Name = "Cancellation" });
        //});

        //modelBuilder.Entity<NotificationStatus>(e =>
        //{
        //    e.ToTable("notification_statuses");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        //    e.Property(x => x.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        //    e.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        //    e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("uq_notification_statuses_code");
        //    e.HasData(
        //        new NotificationStatus { Id = LookupIds.NotificationStatus.Pending, Code = LookupCodes.NotificationStatus.Pending, Name = "Pending" },
        //        new NotificationStatus { Id = LookupIds.NotificationStatus.Sent, Code = LookupCodes.NotificationStatus.Sent, Name = "Sent" },
        //        new NotificationStatus { Id = LookupIds.NotificationStatus.Failed, Code = LookupCodes.NotificationStatus.Failed, Name = "Failed" });
        //});

        //modelBuilder.Entity<User>(e =>
        //{
        //    e.ToTable("users");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        //    e.Property(x => x.Name).HasColumnName("full_name").HasMaxLength(120).IsRequired();
        //    e.Property(x => x.Email).HasColumnName("email").HasMaxLength(120).IsRequired();
        //    e.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
        //    e.Property(x => x.RoleId).HasColumnName("role_id").IsRequired();
        //    e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        //    e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        //    e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        //    e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("uq_users_email");

        //    e.HasOne(x => x.Role)
        //        .WithMany(r => r.Users)
        //        .HasForeignKey(x => x.RoleId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_users_role");
        //});

        //modelBuilder.Entity<RefreshToken>(e =>
        //{
        //    e.ToTable("refresh_tokens");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        //    e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        //    e.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(255).IsRequired();
        //    e.Property(x => x.ExpiresAt).HasColumnName("expires_at").IsRequired();
        //    e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        //    e.Property(x => x.RevokedAt).HasColumnName("revoked_at");
        //    e.Property(x => x.ReplacedByTokenHash).HasColumnName("replaced_by_token_hash").HasMaxLength(255);

        //    e.HasIndex(x => x.TokenHash).IsUnique().HasDatabaseName("uq_refresh_tokens_hash");

        //    e.HasOne(x => x.User)
        //        .WithMany(u => u.RefreshTokens)
        //        .HasForeignKey(x => x.UserId)
        //        .OnDelete(DeleteBehavior.Cascade)
        //        .HasConstraintName("fk_refresh_tokens_user");
        //});

        //modelBuilder.Entity<Doctor>(e =>
        //{
        //    e.ToTable("doctors");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        //    e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        //    e.Property(x => x.SpecializationId).HasColumnName("specialization_id").IsRequired();
        //    e.Property(x => x.Qualification).HasColumnName("qualification").HasMaxLength(100);
        //    e.Property(x => x.ExperienceYears).HasColumnName("experience_years").HasDefaultValue(0);
        //    e.Property(x => x.Bio).HasColumnName("bio").HasColumnType("text");
        //    e.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(255);
        //    e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        //    e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        //    e.HasIndex(x => x.UserId).IsUnique().HasDatabaseName("uq_doctors_user_id");


        //    e.HasOne(x => x.Specialization)
        //        .WithMany(s => s.Doctors)
        //        .HasForeignKey(x => x.SpecializationId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_doctors_specialization");
        //});

        //modelBuilder.Entity<Cabinet>(e =>
        //{
        //    e.ToTable("cabinets");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        //    e.Property(x => x.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        //    e.Property(x => x.ChairNumber).HasColumnName("chair_number").HasDefaultValue(1);
        //    e.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);
        //    e.Property(x => x.IsAvailable).HasColumnName("is_available").HasDefaultValue(true);

        //    e.HasIndex(x => x.Name).IsUnique().HasDatabaseName("uq_cabinets_name");
        //});

        //modelBuilder.Entity<ServiceCategory>(e =>
        //{
        //    e.ToTable("service_categories");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        //    e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        //    e.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);

        //    e.HasIndex(x => x.Name).IsUnique().HasDatabaseName("uq_service_categories_name");
        //});

        //modelBuilder.Entity<DentalService>(e =>
        //{
        //    e.ToTable("dental_services");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        //    e.Property(x => x.CategoryId).HasColumnName("category_id").IsRequired();
        //    e.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        //    e.Property(x => x.Description).HasColumnName("description").HasColumnType("text");
        //    e.Property(x => x.Price).HasColumnName("price").HasPrecision(10, 2).IsRequired();
        //    e.Property(x => x.DurationMinutes).HasColumnName("duration_minutes").HasDefaultValue(30);
        //    e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        //    e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        //    e.HasIndex(x => new { x.CategoryId, x.IsActive }).HasDatabaseName("idx_dental_services_category_active");

        //    e.HasOne(x => x.Category)
        //        .WithMany(c => c.Services)
        //        .HasForeignKey(x => x.CategoryId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_dental_services_category");
        //});

        //modelBuilder.Entity<DoctorSchedule>(e =>
        //{
        //    e.ToTable("doctor_schedules");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        //    e.Property(x => x.DoctorId).HasColumnName("doctor_id").IsRequired();
        //    e.Property(x => x.CabinetId).HasColumnName("cabinet_id").IsRequired();
        //    e.Property(x => x.WorkDate).HasColumnName("work_date").IsRequired();
        //    e.Property(x => x.StartTime).HasColumnName("start_time").IsRequired();
        //    e.Property(x => x.EndTime).HasColumnName("end_time").IsRequired();
        //    e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        //    e.HasIndex(x => new { x.DoctorId, x.WorkDate, x.StartTime }).IsUnique().HasDatabaseName("uq_doctor_shift");

        //    e.HasOne(x => x.Doctor)
        //        .WithMany(d => d.Schedules)
        //        .HasForeignKey(x => x.DoctorId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_schedules_doctor");

        //    e.HasOne(x => x.Cabinet)
        //        .WithMany(c => c.Schedules)
        //        .HasForeignKey(x => x.CabinetId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_schedules_cabinet");
        //});

        //modelBuilder.Entity<TimeSlot>(e =>
        //{
        //    e.ToTable("time_slots");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        //    e.Property(x => x.ScheduleId).HasColumnName("schedule_id").IsRequired();
        //    e.Property(x => x.DoctorId).HasColumnName("doctor_id").IsRequired();
        //    e.Property(x => x.CabinetId).HasColumnName("cabinet_id").IsRequired();
        //    e.Property(x => x.SlotDate).HasColumnName("slot_date").IsRequired();
        //    e.Property(x => x.StartTime).HasColumnName("start_time").IsRequired();
        //    e.Property(x => x.EndTime).HasColumnName("end_time").IsRequired();
        //    e.Property(x => x.StatusId).HasColumnName("status_id").HasDefaultValue(LookupIds.TimeSlotStatus.Free);
        //    e.Property(x => x.ReservedUntil).HasColumnName("reserved_until");

        //    e.HasIndex(x => new { x.DoctorId, x.SlotDate, x.StatusId }).HasDatabaseName("idx_time_slots_lookup");
        //    e.HasIndex(x => new { x.DoctorId, x.SlotDate, x.StartTime }).IsUnique().HasDatabaseName("uq_time_slots_doctor_interval");
        //    e.HasIndex(x => new { x.CabinetId, x.SlotDate, x.StartTime }).IsUnique().HasDatabaseName("uq_time_slots_cabinet_interval");

        //    e.HasOne(x => x.Schedule)
        //        .WithMany(s => s.TimeSlots)
        //        .HasForeignKey(x => x.ScheduleId)
        //        .OnDelete(DeleteBehavior.Cascade)
        //        .HasConstraintName("fk_time_slots_schedule");

        //    e.HasOne(x => x.Doctor)
        //        .WithMany(d => d.TimeSlots)
        //        .HasForeignKey(x => x.DoctorId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_time_slots_doctor");

        //    e.HasOne(x => x.Cabinet)
        //        .WithMany(c => c.TimeSlots)
        //        .HasForeignKey(x => x.CabinetId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_time_slots_cabinet");

        //    e.HasOne(x => x.Status)
        //        .WithMany(s => s.TimeSlots)
        //        .HasForeignKey(x => x.StatusId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_time_slots_status");
        //});

        //modelBuilder.Entity<Contact>(e =>
        //{
        //    e.ToTable("contacts");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        //    e.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(120).IsRequired();
        //    e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
        //    e.Property(x => x.BirthDate).HasColumnName("birth_date");
        //    e.Property(x => x.TelegramChatId).HasColumnName("telegram_chat_id");
        //    e.Property(x => x.Notes).HasColumnName("notes").HasColumnType("text");
        //    e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        //    e.HasIndex(x => x.Phone).IsUnique().HasDatabaseName("uq_patients_phone");
        //    e.HasIndex(x => x.TelegramChatId).IsUnique().HasDatabaseName("uq_patients_telegram");
        //});

        //modelBuilder.Entity<Appointment>(e =>
        //{
        //    e.ToTable("appointments");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        //    e.Property(x => x.AppointmentNumber).HasColumnName("appointment_number").HasMaxLength(32).IsRequired();
        //    e.Property(x => x.Contact).HasColumnName("contact_id").IsRequired();
        //    e.Property(x => x.DoctorId).HasColumnName("doctor_id").IsRequired();
        //    e.Property(x => x.SlotId).HasColumnName("slot_id").IsRequired();
        //    e.Property(x => x.CabinetId).HasColumnName("cabinet_id").IsRequired();
        //    e.Property(x => x.CreatedById).HasColumnName("created_by_user_id");
        //    e.Property(x => x.StatusId).HasColumnName("status_id").HasDefaultValue(LookupIds.AppointmentStatus.Confirmed);
        //    e.Property(x => x.PatientComplaint).HasColumnName("patient_complaint").HasColumnType("text");
        //    e.Property(x => x.DoctorNotes).HasColumnName("doctor_notes").HasColumnType("text");
        //    e.Property(x => x.IsConfirmedByContact).HasColumnName("is_confirmed_by_contact").HasDefaultValue(false);
        //    e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        //    e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        //    e.HasIndex(x => x.AppointmentNumber).IsUnique().HasDatabaseName("uq_appointments_number");
        //    e.HasIndex(x => new { x.DoctorId, x.CreatedAt }).HasDatabaseName("idx_appointments_doctor_date");
        //    e.HasIndex(x => x.StatusId).HasDatabaseName("idx_appointments_status");

        //    e.HasOne(x => x.Contact)
        //        .WithMany(c => c.Appointments)
        //        .HasForeignKey(x => x.ContactId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_appointments_contact");

        //    e.HasOne(x => x.Doctor)
        //        .WithMany(d => d.Appointments)
        //        .HasForeignKey(x => x.DoctorId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_appointments_doctor");

        //    e.HasOne(x => x.Slot)
        //        .WithOne(t => t.Appointment)
        //        .HasForeignKey<Appointment>(x => x.SlotId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_appointments_slot");

        //    e.HasOne(x => x.Cabinet)
        //        .WithMany(c => c.Appointments)
        //        .HasForeignKey(x => x.CabinetId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_appointments_cabinet");


        //    e.HasOne(x => x.Status)
        //        .WithMany(s => s.Appointments)
        //        .HasForeignKey(x => x.StatusId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_appointments_status");
        //});

        //modelBuilder.Entity<AppointmentService>(e =>
        //{
        //    e.ToTable("appointment_services");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        //    e.Property(x => x.AppointmentId).HasColumnName("appointment_id").IsRequired();
        //    e.Property(x => x.ServiceId).HasColumnName("service_id").IsRequired();
        //    e.Property(x => x.Quantity).HasColumnName("quantity").HasDefaultValue(1);
        //    e.Property(x => x.PriceAtBooking).HasColumnName("price_at_booking").HasPrecision(10, 2).IsRequired();

        //    e.HasIndex(x => new { x.AppointmentId, x.ServiceId }).IsUnique().HasDatabaseName("uq_appointment_service");

        //    e.HasOne(x => x.Appointment)
        //        .WithMany(a => a.AppointmentServices)
        //        .HasForeignKey(x => x.AppointmentId)
        //        .OnDelete(DeleteBehavior.Cascade)
        //        .HasConstraintName("fk_app_services_appointment");

        //    e.HasOne(x => x.Service)
        //        .WithMany(s => s.AppointmentServices)
        //        .HasForeignKey(x => x.ServiceId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_app_services_service");
        //});

        //modelBuilder.Entity<Notification>(e =>
        //{
        //    e.ToTable("notifications");
        //    e.HasKey(x => x.Id);
        //    e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        //    e.Property(x => x.AppointmentId).HasColumnName("appointment_id").IsRequired();
        //    e.Property(x => x.TelegramChatId).HasColumnName("telegram_chat_id").IsRequired();
        //    e.Property(x => x.TypeId).HasColumnName("type_id").IsRequired();
        //    e.Property(x => x.StatusId).HasColumnName("status_id").HasDefaultValue(LookupIds.NotificationStatus.Pending);
        //    e.Property(x => x.ScheduledAt).HasColumnName("scheduled_at").IsRequired();
        //    e.Property(x => x.SentAt).HasColumnName("sent_at");
        //    e.Property(x => x.ResponsePayload).HasColumnName("response_payload").HasColumnType("text");

        //    e.HasIndex(x => new { x.StatusId, x.ScheduledAt }).HasDatabaseName("idx_notifications_status_scheduled");

        //    e.HasOne(x => x.Appointment)
        //        .WithMany(a => a.Notifications)
        //        .HasForeignKey(x => x.AppointmentId)
        //        .OnDelete(DeleteBehavior.Cascade)
        //        .HasConstraintName("fk_notifications_appointment");

        //    e.HasOne(x => x.Type)
        //        .WithMany(t => t.Notifications)
        //        .HasForeignKey(x => x.TypeId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_notifications_type");

        //    e.HasOne(x => x.Status)
        //        .WithMany(s => s.Notifications)
        //        .HasForeignKey(x => x.StatusId)
        //        .OnDelete(DeleteBehavior.Restrict)
        //        .HasConstraintName("fk_notifications_status");
        //});
    }
}
