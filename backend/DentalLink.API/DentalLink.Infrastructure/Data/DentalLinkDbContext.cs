using DentalLink.Domain.Entities;
using DentalLink.Domain.Enums;
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
                e.Property(x => x.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(20).IsRequired();
                e.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
                e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
                e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("uq_users_email");
                e.HasIndex(x => x.Phone).IsUnique().HasDatabaseName("uq_users_phone");
            });

            // ================================================================
            // doctors
            // ================================================================
            modelBuilder.Entity<Doctor>(e =>
            {
                e.ToTable("doctors");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
                e.Property(x => x.Specialization).HasColumnName("specialization").HasConversion<string>().HasMaxLength(30).IsRequired();
                e.Property(x => x.Qualification).HasColumnName("qualification").HasMaxLength(100);
                e.Property(x => x.ExperienceYears).HasColumnName("experience_years").IsRequired();
                e.Property(x => x.Bio).HasColumnName("bio");
                e.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(255);
                e.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
                e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

                e.HasIndex(x => x.UserId).IsUnique().HasDatabaseName("uq_doctors_user_id");
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
                e.ToTable("cabinets");
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
                e.ToTable("service_categories");
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
                e.ToTable("dental_services");
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
            });

            // ================================================================
            // doctor_schedules
            // ================================================================
            modelBuilder.Entity<DoctorSchedule>(e =>
            {
                e.ToTable("doctor_schedules");
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
            });

            // ================================================================
            // time_slots
            // ================================================================
            modelBuilder.Entity<TimeSlot>(e =>
            {
                e.ToTable("time_slots");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                e.Property(x => x.ScheduleId).HasColumnName("schedule_id").IsRequired();
                e.Property(x => x.DoctorId).HasColumnName("doctor_id").IsRequired();
                e.Property(x => x.CabinetId).HasColumnName("cabinet_id").IsRequired();
                e.Property(x => x.SlotDate).HasColumnName("slot_date").IsRequired();
                e.Property(x => x.StartTime).HasColumnName("start_time").IsRequired();
                e.Property(x => x.EndTime).HasColumnName("end_time").IsRequired();
                e.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
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

                e.HasIndex(x => new { x.DoctorId, x.SlotDate, x.StartTime })
                    .IsUnique().HasDatabaseName("uq_time_slots_doctor_interval");
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
                e.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
                e.Property(x => x.PatientComplaint).HasColumnName("patient_complaint");
                e.Property(x => x.DoctorNotes).HasColumnName("doctor_notes");
                e.Property(x => x.IsConfirmedByPatient).HasColumnName("is_confirmed_by_patient").IsRequired();
                e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

                e.HasIndex(x => x.AppointmentNumber).IsUnique().HasDatabaseName("uq_appointments_number");
                e.HasIndex(x => x.SlotId).IsUnique().HasDatabaseName("uq_appointments_slot");

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
            });

            // ================================================================
            // appointment_services (M:N bridge)
            // ================================================================
            modelBuilder.Entity<AppointmentService>(e =>
            {
                e.ToTable("appointment_services");
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
                e.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(30).IsRequired();
                e.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
                e.Property(x => x.ScheduledAt).HasColumnName("scheduled_at").IsRequired();
                e.Property(x => x.SentAt).HasColumnName("sent_at");
                e.Property(x => x.ResponsePayload).HasColumnName("response_payload");

                e.HasOne(x => x.Appointment).WithMany(a => a.Notifications)
                    .HasForeignKey(x => x.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_notifications_appointment");
            });
        }
    }
}
