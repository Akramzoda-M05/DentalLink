using DentalLink.Domain.Entities;
using DentalLink.Domain.Entities.Lookup;
using Microsoft.EntityFrameworkCore;

namespace DentalLink.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<UserRole> UserRoles { get; }
    DbSet<DoctorSpecialization> DoctorSpecializations { get; }
    DbSet<TimeSlotStatus> TimeSlotStatuses { get; }
    DbSet<AppointmentStatus> AppointmentStatuses { get; }
    DbSet<NotificationType> NotificationTypes { get; }
    DbSet<NotificationStatus> NotificationStatuses { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<Cabinet> Cabinets { get; }
    DbSet<ServiceCategory> ServiceCategories { get; }
    DbSet<DentalService> DentalServices { get; }
    DbSet<DoctorSchedule> DoctorSchedules { get; }
    DbSet<TimeSlot> TimeSlots { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<AppointmentService> AppointmentServices { get; }
    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
