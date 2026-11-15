using DentalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DentalLink.Infrastructure.Data
{
    /// <summary>
    /// EF Core DbContext для PostgreSQL-базы данных DentalLink.
    /// Отражает физическую схему из database/schema.sql (PostgreSQL 18.6).
    /// </summary>
    public class DentalLinkDbContext(DbContextOptions<DentalLinkDbContext> options)
        : DbContext(options)
    {
        // ------------------------------------------------------------------ //
        // DbSets — коллекции сущностей                                        //
        // ------------------------------------------------------------------ //
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<DoctorSpecialization> DoctorSpecializations => Set<DoctorSpecialization>();
        public DbSet<TimeSlotStatus> TimeSlotStatuses => Set<TimeSlotStatus>();
        public DbSet<AppointmentStatus> AppointmentStatuses => Set<AppointmentStatus>();
        public DbSet<NotificationType> NotificationTypes => Set<NotificationType>();
        public DbSet<NotificationStatus> NotificationStatuses => Set<NotificationStatus>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Cabinet> Cabinets => Set<Cabinet>();
        public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
        public DbSet<DentalService> DentalServices => Set<DentalService>();
        public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
        public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<AppointmentService> AppointmentServices => Set<AppointmentService>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>(e =>
            {
                e.ToTable("user_roles");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
                e.Property(x => x.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
                e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("uq_user_roles_code");
                e.HasData(
                    new UserRole { Id = LookupIds.UserRole.Admin, Code = LookupCodes.UserRole.Admin, Name = "Administrator" },
                    new UserRole { Id = LookupIds.UserRole.Receptionist, Code = LookupCodes.UserRole.Receptionist, Name = "Receptionist" },
                    new UserRole { Id = LookupIds.UserRole.Doctor, Code = LookupCodes.UserRole.Doctor, Name = "Doctor" });
            });

            modelBuilder.Entity<DoctorSpecialization>(e =>
            {
                e.ToTable("doctor_specializations");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
                e.Property(x => x.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
                e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("uq_doctor_specializations_code");
                e.HasData(
                    new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Dentist, Code = LookupCodes.DoctorSpecialization.Dentist, Name = "Dentist" },
                    new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Therapist, Code = LookupCodes.DoctorSpecialization.Therapist, Name = "Therapist" },
                    new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Orthopedist, Code = LookupCodes.DoctorSpecialization.Orthopedist, Name = "Orthopedist" },
                    new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Orthodontist, Code = LookupCodes.DoctorSpecialization.Orthodontist, Name = "Orthodontist" },
                    new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Surgeon, Code = LookupCodes.DoctorSpecialization.Surgeon, Name = "Surgeon" },
                    new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Implantologist, Code = LookupCodes.DoctorSpecialization.Implantologist, Name = "Implantologist" },
                    new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Pediatric, Code = LookupCodes.DoctorSpecialization.Pediatric, Name = "Pediatric dentist" },
                    new DoctorSpecialization { Id = LookupIds.DoctorSpecialization.Hygienist, Code = LookupCodes.DoctorSpecialization.Hygienist, Name = "Hygienist" });
            });

            modelBuilder.Entity<TimeSlotStatus>(e =>
            {
                e.ToTable("time_slot_statuses");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
                e.Property(x => x.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
                e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("uq_time_slot_statuses_code");
                e.HasData(
                    new TimeSlotStatus { Id = LookupIds.TimeSlotStatus.Free, Code = LookupCodes.TimeSlotStatus.Free, Name = "Free" },
                    new TimeSlotStatus { Id = LookupIds.TimeSlotStatus.Reserved, Code = LookupCodes.TimeSlotStatus.Reserved, Name = "Reserved" },
                    new TimeSlotStatus { Id = LookupIds.TimeSlotStatus.Booked, Code = LookupCodes.TimeSlotStatus.Booked, Name = "Booked" });
            });

            modelBuilder.Entity<AppointmentStatus>(e =>
            {
                e.ToTable("appointment_statuses");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
                e.Property(x => x.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
                e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("uq_appointment_statuses_code");
                e.HasData(
                    new AppointmentStatus { Id = LookupIds.AppointmentStatus.Confirmed, Code = LookupCodes.AppointmentStatus.Confirmed, Name = "Confirmed" },
                    new AppointmentStatus { Id = LookupIds.AppointmentStatus.Arrived, Code = LookupCodes.AppointmentStatus.Arrived, Name = "Arrived" },
                    new AppointmentStatus { Id = LookupIds.AppointmentStatus.InProgress, Code = LookupCodes.AppointmentStatus.InProgress, Name = "In progress" },
                    new AppointmentStatus { Id = LookupIds.AppointmentStatus.Completed, Code = LookupCodes.AppointmentStatus.Completed, Name = "Completed" },
                    new AppointmentStatus { Id = LookupIds.AppointmentStatus.Cancelled, Code = LookupCodes.AppointmentStatus.Cancelled, Name = "Cancelled" },
                    new AppointmentStatus { Id = LookupIds.AppointmentStatus.NoShow, Code = LookupCodes.AppointmentStatus.NoShow, Name = "No show" });
            });

            modelBuilder.Entity<NotificationType>(e =>
            {
                e.ToTable("notification_types");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
                e.Property(x => x.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
                e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("uq_notification_types_code");
                e.HasData(
                    new NotificationType { Id = LookupIds.NotificationType.Confirmation, Code = LookupCodes.NotificationType.Confirmation, Name = "Confirmation" },
                    new NotificationType { Id = LookupIds.NotificationType.Reminder24Hours, Code = LookupCodes.NotificationType.Reminder24Hours, Name = "Reminder (24 hours)" },
                    new NotificationType { Id = LookupIds.NotificationType.Reminder2Hours, Code = LookupCodes.NotificationType.Reminder2Hours, Name = "Reminder (2 hours)" },
                    new NotificationType { Id = LookupIds.NotificationType.Cancellation, Code = LookupCodes.NotificationType.Cancellation, Name = "Cancellation" });
            });

            modelBuilder.Entity<NotificationStatus>(e =>
            {
                e.ToTable("notification_statuses");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
                e.Property(x => x.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
                e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("uq_notification_statuses_code");
                e.HasData(
                    new NotificationStatus { Id = LookupIds.NotificationStatus.Pending, Code = LookupCodes.NotificationStatus.Pending, Name = "Pending" },
                    new NotificationStatus { Id = LookupIds.NotificationStatus.Sent, Code = LookupCodes.NotificationStatus.Sent, Name = "Sent" },
                    new NotificationStatus { Id = LookupIds.NotificationStatus.Failed, Code = LookupCodes.NotificationStatus.Failed, Name = "Failed" });
            });

            // ================================================================
            // users
            // ================================================================
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                e.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(120).IsRequired();
                e.Property(x => x.Email).HasColumnName("email").HasMaxLength(120).IsRequired();
                e.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
                e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
                e.Property(x => x.RoleId).HasColumnName("role_id").IsRequired();
                e.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
                e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
                e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("uq_users_email");
                e.HasIndex(x => x.Phone).IsUnique().HasDatabaseName("uq_users_phone");
                e.HasOne(x => x.Role).WithMany(x => x.Users)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_users_role");
            });

            // ================================================================
            // doctors
            // ================================================================
            modelBuilder.Entity<Doctor>(e =>
            {
                e.ToTable("doctors", table =>
                    table.HasCheckConstraint("chk_doctors_experience", "experience_years >= 0"));
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
                e.Property(x => x.SpecializationId).HasColumnName("specialization_id").IsRequired();
                e.Property(x => x.Qualification).HasColumnName("qualification").HasMaxLength(100);
                e.Property(x => x.ExperienceYears).HasColumnName("experience_years").IsRequired();
                e.Property(x => x.Bio).HasColumnName("bio");
                e.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(255);
                e.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
                e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

                e.HasIndex(x => x.UserId).IsUnique().HasDatabaseName("uq_doctors_user_id");
                e.HasOne(x => x.Specialization).WithMany(x => x.Doctors)
                    .HasForeignKey(x => x.SpecializationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_doctors_specialization");
                e.HasOne(x => x.User).WithOne(u => u.DoctorProfile)
                    .HasForeignKey<Doctor>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_doctors_user");
            });

            // ================================================================
            // cabinets
            // ================================================================
            modelBuilder.Entity<Cabinet>(e =>
            {
                e.ToTable("cabinets", table =>
                    table.HasCheckConstraint("chk_cabinets_chair_number", "chair_number > 0"));
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
                e.Property(x => x.ChairNumber).HasColumnName("chair_number").IsRequired();
                e.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);
                e.Property(x => x.IsAvailable).HasColumnName("is_available").IsRequired();
                e.HasIndex(x => x.Name).IsUnique().HasDatabaseName("uq_cabinets_name");
            });

            // ================================================================
            // service_categories
            // ================================================================
            modelBuilder.Entity<ServiceCategory>(e =>
            {
                e.ToTable("service_categories", table =>
                    table.HasCheckConstraint("chk_service_categories_sort_order", "sort_order >= 0"));
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                e.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);
                e.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();
                e.HasIndex(x => x.Name).IsUnique().HasDatabaseName("uq_service_categories_name");
            });

            // ================================================================
            // dental_services
            // ================================================================
            modelBuilder.Entity<DentalService>(e =>
            {
                e.ToTable("dental_services", table =>
                {
                    table.HasCheckConstraint("chk_dental_services_price", "price >= 0");
                    table.HasCheckConstraint("chk_dental_services_duration", "duration_minutes > 0 AND duration_minutes % 30 = 0");
                });
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                e.Property(x => x.CategoryId).HasColumnName("category_id").IsRequired();
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
                e.Property(x => x.Description).HasColumnName("description");
                e.Property(x => x.Price).HasColumnName("price").HasPrecision(10, 2).IsRequired();
                e.Property(x => x.DurationMinutes).HasColumnName("duration_minutes").IsRequired();
                e.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
                e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

                e.HasOne(x => x.Category).WithMany(c => c.Services)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_dental_services_category");

                e.HasIndex(x => new { x.CategoryId, x.IsActive })
                    .HasDatabaseName("idx_dental_services_category_active");
            });

            // ================================================================
            // doctor_schedules
            // ================================================================
            modelBuilder.Entity<DoctorSchedule>(e =>
            {
                e.ToTable("doctor_schedules", table =>
                    table.HasCheckConstraint("chk_schedules_time", "end_time > start_time"));
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                e.Property(x => x.DoctorId).HasColumnName("doctor_id").IsRequired();
                e.Property(x => x.CabinetId).HasColumnName("cabinet_id").IsRequired();
                e.Property(x => x.WorkDate).HasColumnName("work_date").IsRequired();
                e.Property(x => x.StartTime).HasColumnName("start_time").IsRequired();
                e.Property(x => x.EndTime).HasColumnName("end_time").IsRequired();
                e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

                e.HasOne(x => x.Doctor).WithMany(d => d.Schedules)
                    .HasForeignKey(x => x.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_schedules_doctor");
                e.HasOne(x => x.Cabinet).WithMany(c => c.Schedules)
                    .HasForeignKey(x => x.CabinetId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_schedules_cabinet");

                e.HasIndex(x => new { x.DoctorId, x.WorkDate, x.StartTime })
                    .IsUnique().HasDatabaseName("uq_doctor_shift");
            });

            // ================================================================
            // time_slots
            // ================================================================
            modelBuilder.Entity<TimeSlot>(e =>
            {
                e.ToTable("time_slots", table =>
                    table.HasCheckConstraint("chk_time_slots_time", "end_time > start_time"));
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                e.Property(x => x.ScheduleId).HasColumnName("schedule_id").IsRequired();
                e.Property(x => x.DoctorId).HasColumnName("doctor_id").IsRequired();
                e.Property(x => x.CabinetId).HasColumnName("cabinet_id").IsRequired();
                e.Property(x => x.SlotDate).HasColumnName("slot_date").IsRequired();
                e.Property(x => x.StartTime).HasColumnName("start_time").IsRequired();
                e.Property(x => x.EndTime).HasColumnName("end_time").IsRequired();
                e.Property(x => x.StatusId).HasColumnName("status_id").IsRequired();
                e.Property(x => x.ReservedUntil).HasColumnName("reserved_until");

                e.HasOne(x => x.Schedule).WithMany(s => s.TimeSlots)
                    .HasForeignKey(x => x.ScheduleId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_time_slots_schedule");
                e.HasOne(x => x.Doctor).WithMany(d => d.TimeSlots)
                    .HasForeignKey(x => x.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_time_slots_doctor");
                e.HasOne(x => x.Cabinet).WithMany(c => c.TimeSlots)
                    .HasForeignKey(x => x.CabinetId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_time_slots_cabinet");
                e.HasOne(x => x.Status).WithMany(x => x.TimeSlots)
                    .HasForeignKey(x => x.StatusId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_time_slots_status");

                e.HasIndex(x => new { x.DoctorId, x.SlotDate, x.StartTime })
                    .IsUnique().HasDatabaseName("uq_time_slots_doctor_interval");
                e.HasIndex(x => new { x.CabinetId, x.SlotDate, x.StartTime })
                    .IsUnique().HasDatabaseName("uq_time_slots_cabinet_interval");
                e.HasIndex(x => new { x.DoctorId, x.SlotDate, x.StatusId })
                    .HasDatabaseName("idx_time_slots_lookup");
            });

            // ================================================================
            // patients
            // ================================================================
            modelBuilder.Entity<Patient>(e =>
            {
                e.ToTable("patients");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                e.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(120).IsRequired();
                e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
                e.Property(x => x.Email).HasColumnName("email").HasMaxLength(120);
                e.Property(x => x.BirthDate).HasColumnName("birth_date");
                e.Property(x => x.TelegramChatId).HasColumnName("telegram_chat_id");
                e.Property(x => x.Notes).HasColumnName("notes");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

                e.HasIndex(x => x.Phone).IsUnique().HasDatabaseName("uq_patients_phone");
                e.HasIndex(x => x.TelegramChatId).IsUnique().HasDatabaseName("uq_patients_telegram");
            });

            // ================================================================
            // appointments
            // ================================================================
            modelBuilder.Entity<Appointment>(e =>
            {
                e.ToTable("appointments");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                e.Property(x => x.AppointmentNumber).HasColumnName("appointment_number").HasMaxLength(32).IsRequired();
                e.Property(x => x.PatientId).HasColumnName("patient_id").IsRequired();
                e.Property(x => x.DoctorId).HasColumnName("doctor_id").IsRequired();
                e.Property(x => x.SlotId).HasColumnName("slot_id").IsRequired();
                e.Property(x => x.CabinetId).HasColumnName("cabinet_id").IsRequired();
                e.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
                e.Property(x => x.StatusId).HasColumnName("status_id").IsRequired();
                e.Property(x => x.PatientComplaint).HasColumnName("patient_complaint");
                e.Property(x => x.DoctorNotes).HasColumnName("doctor_notes");
                e.Property(x => x.IsConfirmedByPatient).HasColumnName("is_confirmed_by_patient").IsRequired();
                e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

                e.HasIndex(x => x.AppointmentNumber).IsUnique().HasDatabaseName("uq_appointments_number");
                e.HasIndex(x => x.SlotId).IsUnique()
                    .HasFilter($"status_id <> {LookupIds.AppointmentStatus.Cancelled}")
                    .HasDatabaseName("uq_appointments_slot");

                e.HasOne(x => x.Status).WithMany(x => x.Appointments)
                    .HasForeignKey(x => x.StatusId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_appointments_status");

                e.HasOne(x => x.Patient).WithMany(p => p.Appointments)
                    .HasForeignKey(x => x.PatientId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_appointments_patient");
                e.HasOne(x => x.Doctor).WithMany(d => d.Appointments)
                    .HasForeignKey(x => x.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_appointments_doctor");
                e.HasOne(x => x.Slot).WithOne(s => s.Appointment)
                    .HasForeignKey<Appointment>(x => x.SlotId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_appointments_slot");
                e.HasOne(x => x.Cabinet).WithMany(c => c.Appointments)
                    .HasForeignKey(x => x.CabinetId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_appointments_cabinet");
                e.HasOne(x => x.CreatedByUser).WithMany(u => u.CreatedAppointments)
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("fk_appointments_created_by");
                e.HasIndex(x => new { x.DoctorId, x.CreatedAt })
                    .HasDatabaseName("idx_appointments_doctor_date");
                e.HasIndex(x => x.StatusId).HasDatabaseName("idx_appointments_status");
            });

            // ================================================================
            // appointment_services (M:N bridge)
            // ================================================================
            modelBuilder.Entity<AppointmentService>(e =>
            {
                e.ToTable("appointment_services", table =>
                {
                    table.HasCheckConstraint("chk_app_services_quantity", "quantity > 0");
                    table.HasCheckConstraint("chk_app_services_price", "price_at_booking >= 0");
                });
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                e.Property(x => x.AppointmentId).HasColumnName("appointment_id").IsRequired();
                e.Property(x => x.ServiceId).HasColumnName("service_id").IsRequired();
                e.Property(x => x.Quantity).HasColumnName("quantity").IsRequired();
                e.Property(x => x.PriceAtBooking).HasColumnName("price_at_booking").HasPrecision(10, 2).IsRequired();

                e.HasOne(x => x.Appointment).WithMany(a => a.AppointmentServices)
                    .HasForeignKey(x => x.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_app_services_appointment");
                e.HasOne(x => x.Service).WithMany(s => s.AppointmentServices)
                    .HasForeignKey(x => x.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_app_services_service");

                e.HasIndex(x => new { x.AppointmentId, x.ServiceId })
                    .IsUnique().HasDatabaseName("uq_appointment_service");
            });

            // ================================================================
            // notifications
            // ================================================================
            modelBuilder.Entity<Notification>(e =>
            {
                e.ToTable("notifications");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                e.Property(x => x.AppointmentId).HasColumnName("appointment_id").IsRequired();
                e.Property(x => x.TelegramChatId).HasColumnName("telegram_chat_id").IsRequired();
                e.Property(x => x.TypeId).HasColumnName("type_id").IsRequired();
                e.Property(x => x.StatusId).HasColumnName("status_id").IsRequired();
                e.Property(x => x.ScheduledAt).HasColumnName("scheduled_at").IsRequired();
                e.Property(x => x.SentAt).HasColumnName("sent_at");
                e.Property(x => x.ResponsePayload).HasColumnName("response_payload");

                e.HasOne(x => x.Appointment).WithMany(a => a.Notifications)
                    .HasForeignKey(x => x.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_notifications_appointment");
                e.HasOne(x => x.Type).WithMany(x => x.Notifications)
                    .HasForeignKey(x => x.TypeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_notifications_type");
                e.HasOne(x => x.Status).WithMany(x => x.Notifications)
                    .HasForeignKey(x => x.StatusId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_notifications_status");
                e.HasIndex(x => new { x.StatusId, x.ScheduledAt })
                    .HasDatabaseName("idx_notifications_status_scheduled");
            });
        }
    }
}
