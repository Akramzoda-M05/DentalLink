using DentalLink.Application.Common.Interfaces;
using DentalLink.Domain.Constants;
using DentalLink.Domain.Entities;
using DentalLink.Domain.Entities.Lookup;
using DentalLink.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DentalLink.Application.Features.Appointments;

public record AppointmentServiceItemDto(
    Guid ServiceId,
    string ServiceName,
    decimal Price,
    int DurationMinutes
);

public record AppointmentDto(
    Guid Id,
    string AppointmentNumber,
    Guid PatientId,
    string PatientName,
    string PatientPhone,
    Guid DoctorId,
    string DoctorName,
    string DoctorSpecialization,
    Guid CabinetId,
    string CabinetName,
    Guid SlotId,
    DateOnly SlotDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid StatusId,
    string StatusCode,
    string StatusName,
    string? PatientComplaint,
    string? DoctorNotes,
    bool IsConfirmedByPatient,
    DateTimeOffset CreatedAt,
    IReadOnlyList<AppointmentServiceItemDto> Services
);

public record BookAppointmentResultDto(
    AppointmentDto Appointment,
    string TelegramBotDeepLink,
    string Message
);

// -----------------------------------------------------------------------------
// 1. Online Booking by Patient (No password needed)
// -----------------------------------------------------------------------------
public record BookAppointmentCommand(
    Guid SlotId,
    Guid ServiceId,
    string PatientName,
    string PatientPhone,
    string? PatientEmail = null,
    string? Complaint = null
) : IRequest<BookAppointmentResultDto>;

public class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentCommandValidator()
    {
        RuleFor(x => x.SlotId).NotEqual(Guid.Empty);
        RuleFor(x => x.ServiceId).NotEqual(Guid.Empty);
        RuleFor(x => x.PatientName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.PatientPhone).NotEmpty().Matches(@"^\+?[0-9\s\-\(\)]{8,20}$")
            .WithMessage("Invalid phone number format.");
        When(x => !string.IsNullOrEmpty(x.PatientEmail), () =>
        {
            RuleFor(x => x.PatientEmail).EmailAddress();
        });
    }
}

public class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, BookAppointmentResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedLockService _lockService;

    public BookAppointmentCommandHandler(
        IApplicationDbContext context,
        IDistributedLockService lockService)
    {
        _context = context;
        _lockService = lockService;
    }

    public async Task<BookAppointmentResultDto> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        var lockKey = $"lock:slot:{request.SlotId}";
        using var distributedLock = await _lockService.TryAcquireLockAsync(
            lockKey,
            expiry: TimeSpan.FromSeconds(30),
            waitTimeout: TimeSpan.FromSeconds(5),
            cancellationToken);

        if (distributedLock == null)
        {
            throw new ConflictException("Another patient is currently booking this slot. Please try another time.");
        }

        var slot = await _context.TimeSlots
            .Include(t => t.Doctor).ThenInclude(d => d.Contact)
            .Include(t => t.Doctor.Specialization)
            .Include(t => t.Cabinet)
            .FirstOrDefaultAsync(t => t.Id == request.SlotId, cancellationToken);

        if (slot == null)
        {
            throw new NotFoundException(nameof(TimeSlot), request.SlotId);
        }

        var nowUtc = DateTimeOffset.UtcNow;
        bool isFree = slot.StatusId == LookupIds.TimeSlotStatus.Free ||
            (slot.StatusId == LookupIds.TimeSlotStatus.Reserved && slot.ReservedUntil >= nowUtc);

        if (!isFree && slot.StatusId != LookupIds.TimeSlotStatus.Reserved)
        {
            throw new ConflictException("This slot is already booked.");
        }

        var service = await _context.DentalServices
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.IsActive, cancellationToken);

        if (service == null)
        {
            throw new NotFoundException(nameof(DentalService), request.ServiceId);
        }

        // Upsert patient by phone
        var cleanPhone = request.PatientPhone.Trim();
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(p => p.Phone == cleanPhone, cancellationToken);

        if (contact == null)
        {
            contact = new Contact
            {
                FullName = request.PatientName.Trim(),
                Phone = cleanPhone,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            if (contact.FullName != request.PatientName.Trim())
            {
                contact.FullName = request.PatientName.Trim();
            }
        }

        // Generate unique appointment number DL-YYYYMMDD-XXXX
        var randomSuffix = RandomNumberGenerator.GetInt32(1000, 9999);
        var apptNumber = $"DL-{slot.SlotDate:yyyyMMdd}-{randomSuffix}";

        var appointment = new Appointment
        {
            AppointmentNumber = apptNumber,
            ContactId =  contact.Id,
            DoctorId = slot.DoctorId,
            SlotId = slot.Id,
            CabinetId = slot.CabinetId,
            CreatedById = null,
            StatusId = LookupIds.AppointmentStatus.Confirmed,
            PatientComplaint = request.Complaint?.Trim(),
            DoctorNotes = null,
            IsConfirmedByContact = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);

        // Link service
        var apptService = new AppointmentService
        {
            AppointmentId = appointment.Id,
            ServiceId = service.Id,
            Quantity = 1,
            PriceAtBooking = service.Price
        };
        _context.AppointmentServices.Add(apptService);

        // Update slot to Booked
        slot.StatusId = LookupIds.TimeSlotStatus.Booked;
        slot.ReservedUntil = null;

        // Queue notifications
        var apptDateTime = slot.SlotDate.ToDateTime(slot.StartTime);
        var apptDateTimeOffset = new DateTimeOffset(apptDateTime, TimeSpan.FromHours(5)); // Dushanbe timezone UTC+5

        if (contact.TelegramChatId.HasValue)
        {
            var notifConfirmation = new Notification
            {
                AppointmentId = appointment.Id,
                TelegramChatId = contact.TelegramChatId.Value,
                TypeId = LookupIds.NotificationType.Confirmation,
                StatusId = LookupIds.NotificationStatus.Pending,
                ScheduledAt = DateTimeOffset.UtcNow
            };
            _context.Notifications.Add(notifConfirmation);

            // Reminder 24h before
            var reminder24hTime = apptDateTimeOffset.AddHours(-24);
            if (reminder24hTime > DateTimeOffset.UtcNow)
            {
                _context.Notifications.Add(new Notification
                {
                    AppointmentId = appointment.Id,
                    TelegramChatId = contact.TelegramChatId.Value,
                    TypeId = LookupIds.NotificationType.Reminder24Hours,
                    StatusId = LookupIds.NotificationStatus.Pending,
                    ScheduledAt = reminder24hTime
                });
            }

            // Reminder 2h before
            var reminder2hTime = apptDateTimeOffset.AddHours(-2);
            if (reminder2hTime > DateTimeOffset.UtcNow)
            {
                _context.Notifications.Add(new Notification
                {
                    AppointmentId = appointment.Id,
                    TelegramChatId = contact.TelegramChatId.Value,
                    TypeId = LookupIds.NotificationType.Reminder2Hours,
                    StatusId = LookupIds.NotificationStatus.Pending,
                    ScheduledAt = reminder2hTime
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var status = await _context.AppointmentStatuses.FirstAsync(s => s.Id == LookupIds.AppointmentStatus.Confirmed, cancellationToken);

        var dto = new AppointmentDto(
            appointment.Id,
            appointment.AppointmentNumber,
            contact.Id,
            contact.FullName,
            contact.Phone,
            slot.DoctorId,
            slot.Doctor.Contact.FullName,
            slot.Doctor.Specialization.Name,
            slot.CabinetId,
            slot.Cabinet.Name,
            slot.Id,
            slot.SlotDate,
            slot.StartTime,
            slot.EndTime,
            status.Id,
            status.Code,
            status.Name,
            appointment.PatientComplaint,
            appointment.DoctorNotes,
            appointment.CreatedAt,
            [new AppointmentServiceItemDto(service.Id, service.Name, service.Price, service.DurationMinutes)]
        );

        var deepLink = $"https://t.me/dentallink_bot?start=appt_{appointment.Id}";

        return new BookAppointmentResultDto(
            dto,
            deepLink,
            "Your appointment has been successfully booked!"
        );
    }
}

// -----------------------------------------------------------------------------
// 2. Manual Booking by Staff (Receptionist / Admin)
// -----------------------------------------------------------------------------
public record CreateManualAppointmentCommand(
    Guid SlotId,
    Guid ServiceId,
    string PatientName,
    string PatientPhone,
    string? PatientEmail = null,
    string? Complaint = null
) : IRequest<AppointmentDto>;

public class CreateManualAppointmentCommandHandler : IRequestHandler<CreateManualAppointmentCommand, AppointmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateManualAppointmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<AppointmentDto> Handle(CreateManualAppointmentCommand request, CancellationToken cancellationToken)
    {
        var slot = await _context.TimeSlots
            .Include(t => t.Doctor).ThenInclude(d => d.Contact)
            .Include(t => t.Doctor.Specialization)
            .Include(t => t.Cabinet)
            .FirstOrDefaultAsync(t => t.Id == request.SlotId, cancellationToken);

        if (slot == null) throw new NotFoundException(nameof(TimeSlot), request.SlotId);

        if (slot.StatusId == LookupIds.TimeSlotStatus.Booked)
        {
            throw new ConflictException("Slot is already booked.");
        }

        var service = await _context.DentalServices
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);
        if (service == null) throw new NotFoundException(nameof(DentalService), request.ServiceId);

        var cleanPhone = request.PatientPhone.Trim();
        var contact = await _context.Contacts.FirstOrDefaultAsync(p => p.Phone == cleanPhone, cancellationToken);
        if (contact == null)
        {
            contact = new Contact
            {
                FullName = request.PatientName.Trim(),
                Phone = cleanPhone,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var randomSuffix = RandomNumberGenerator.GetInt32(1000, 9999);
        var apptNumber = $"DL-{slot.SlotDate:yyyyMMdd}-{randomSuffix}";

        var appointment = new Appointment
        {
            AppointmentNumber = apptNumber,
            ContactId = contact.Id,
            DoctorId = slot.DoctorId,
            SlotId = slot.Id,
            CabinetId = slot.CabinetId,
            CreatedById = _currentUserService.UserId,
            StatusId = LookupIds.AppointmentStatus.Confirmed,
            PatientComplaint = request.Complaint?.Trim(),
            DoctorNotes = null,
            IsConfirmedByContact = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);

        _context.AppointmentServices.Add(new AppointmentService
        {
            AppointmentId = appointment.Id,
            ServiceId = service.Id,
            Quantity = 1,
            PriceAtBooking = service.Price
        });

        slot.StatusId = LookupIds.TimeSlotStatus.Booked;
        slot.ReservedUntil = null;
        await _context.SaveChangesAsync(cancellationToken);

        var status = await _context.AppointmentStatuses.FirstAsync(s => s.Id == LookupIds.AppointmentStatus.Confirmed, cancellationToken);

        return new AppointmentDto(
            appointment.Id,
            appointment.AppointmentNumber,
            contact.Id,
            contact.FullName,
            contact.Phone,
            slot.DoctorId,
            slot.Doctor.Contact.FullName,
            slot.Doctor.Specialization.Name,
            slot.CabinetId,
            slot.Cabinet.Name,
            slot.Id,
            slot.SlotDate,
            slot.StartTime,
            slot.EndTime,
            status.Id,
            status.Code,
            status.Name,
            appointment.PatientComplaint,
            appointment.DoctorNotes,
            appointment.IsConfirmedByContact,
            appointment.CreatedAt,
            [new AppointmentServiceItemDto(service.Id, service.Name, service.Price, service.DurationMinutes)]
        );
    }
}

// -----------------------------------------------------------------------------
// 3. Get Appointments Timeline Query (For Calendar / Receptionist grid)
// -----------------------------------------------------------------------------
public record GetAppointmentsTimelineQuery(
    DateOnly Date,
    Guid? CabinetId = null,
    Guid? DoctorId = null
) : IRequest<IReadOnlyList<AppointmentDto>>;

public class GetAppointmentsTimelineQueryHandler : IRequestHandler<GetAppointmentsTimelineQuery, IReadOnlyList<AppointmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAppointmentsTimelineQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AppointmentDto>> Handle(GetAppointmentsTimelineQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Appointments
            .Include(a => a.Contact)
            .Include(a => a.Doctor)
            .Include(a => a.Doctor.Specialization)
            .Include(a => a.Cabinet)
            .Include(a => a.Slot)
            .Include(a => a.Status)
            .Include(a => a.AppointmentServices).ThenInclude(asv => asv.Service)
            .Where(a => a.Slot.SlotDate == request.Date)
            .AsNoTracking();

        if (request.CabinetId.HasValue)
        {
            query = query.Where(a => a.CabinetId == request.CabinetId.Value);
        }

        if (request.DoctorId.HasValue)
        {
            query = query.Where(a => a.DoctorId == request.DoctorId.Value);
        }

        var appointments = await query
            .OrderBy(a => a.Slot.StartTime)
            .Select(a => new AppointmentDto(
                a.Id,
                a.AppointmentNumber,
                a.ContactId,
                a.Contact.FullName,
                a.Contact.Phone,
                a.DoctorId,
                a.Doctor.Contact.FullName,
                a.Doctor.Specialization.Name,
                a.CabinetId,
                a.Cabinet.Name,
                a.SlotId,
                a.Slot.SlotDate,
                a.Slot.StartTime,
                a.Slot.EndTime,
                a.StatusId,
                a.Status.Code,
                a.Status.Name,
                a.PatientComplaint,
                a.DoctorNotes,
                a.IsConfirmedByContact,
                a.CreatedAt,
                a.AppointmentServices.Select(s => new AppointmentServiceItemDto(
                    s.ServiceId,
                    s.Service.Name,
                    s.PriceAtBooking,
                    s.Service.DurationMinutes
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        return appointments;
    }
}

// -----------------------------------------------------------------------------
// 4. Get Patient Appointments (By Phone or Active User)
// -----------------------------------------------------------------------------
public record GetPatientAppointmentsQuery(string Phone) : IRequest<IReadOnlyList<AppointmentDto>>;

public class GetPatientAppointmentsQueryHandler : IRequestHandler<GetPatientAppointmentsQuery, IReadOnlyList<AppointmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPatientAppointmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AppointmentDto>> Handle(GetPatientAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var cleanPhone = request.Phone.Trim();

        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor.Specialization)
            .Include(a => a.Cabinet)
            .Include(a => a.Slot)
            .Include(a => a.Status)
            .Include(a => a.AppointmentServices).ThenInclude(asv => asv.Service)
            .Where(a => a.Patient.Phone == cleanPhone)
            .OrderByDescending(a => a.Slot.SlotDate)
            .ThenByDescending(a => a.Slot.StartTime)
            .AsNoTracking()
            .Select(a => new AppointmentDto(
                a.Id,
                a.AppointmentNumber,
                a.PatientId,
                a.Patient.FullName,
                a.Patient.Phone,
                a.DoctorId,
                a.Doctor.User.FullName,
                a.Doctor.Specialization.Name,
                a.CabinetId,
                a.Cabinet.Name,
                a.SlotId,
                a.Slot.SlotDate,
                a.Slot.StartTime,
                a.Slot.EndTime,
                a.StatusId,
                a.Status.Code,
                a.Status.Name,
                a.PatientComplaint,
                a.DoctorNotes,
                a.IsConfirmedByPatient,
                a.CreatedAt,
                a.AppointmentServices.Select(s => new AppointmentServiceItemDto(
                    s.ServiceId,
                    s.Service.Name,
                    s.PriceAtBooking,
                    s.Service.DurationMinutes
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        return appointments;
    }
}

// -----------------------------------------------------------------------------
// 5. Update Appointment Status
// (arrived, in_progress, completed, cancelled, no_show)
// -----------------------------------------------------------------------------
public record UpdateAppointmentStatusCommand(
    Guid AppointmentId,
    string StatusCode,
    string? DoctorNotes = null
) : IRequest<AppointmentDto>;

public class UpdateAppointmentStatusCommandHandler : IRequestHandler<UpdateAppointmentStatusCommand, AppointmentDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateAppointmentStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AppointmentDto> Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor.Specialization)
            .Include(a => a.Cabinet)
            .Include(a => a.Slot)
            .Include(a => a.Status)
            .Include(a => a.AppointmentServices).ThenInclude(asv => asv.Service)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment == null)
        {
            throw new NotFoundException(nameof(Appointment), request.AppointmentId);
        }

        var newStatus = await _context.AppointmentStatuses
            .FirstOrDefaultAsync(s => s.Code == request.StatusCode, cancellationToken);
        if (newStatus == null)
        {
            throw new NotFoundException(nameof(AppointmentStatus), request.StatusCode);
        }

        appointment.StatusId = newStatus.Id;
        appointment.UpdatedAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.DoctorNotes))
        {
            appointment.DoctorNotes = request.DoctorNotes.Trim();
        }

        // If cancelled, release slot to free
        if (request.StatusCode == LookupCodes.AppointmentStatus.Cancelled)
        {
            appointment.Slot.StatusId = LookupIds.TimeSlotStatus.Free;
            appointment.Slot.ReservedUntil = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Reload status
        await _context.Appointments.Entry(appointment).Reference(a => a.Status).LoadAsync(cancellationToken);

        return new AppointmentDto(
            appointment.Id,
            appointment.AppointmentNumber,
            appointment.PatientId,
            appointment.Patient.FullName,
            appointment.Patient.Phone,
            appointment.DoctorId,
            appointment.Doctor.User.FullName,
            appointment.Doctor.Specialization.Name,
            appointment.CabinetId,
            appointment.Cabinet.Name,
            appointment.SlotId,
            appointment.Slot.SlotDate,
            appointment.Slot.StartTime,
            appointment.Slot.EndTime,
            appointment.Status.Id,
            appointment.Status.Code,
            appointment.Status.Name,
            appointment.PatientComplaint,
            appointment.DoctorNotes,
            appointment.IsConfirmedByPatient,
            appointment.CreatedAt,
            appointment.AppointmentServices.Select(s => new AppointmentServiceItemDto(
                s.ServiceId,
                s.Service.Name,
                s.PriceAtBooking,
                s.Service.DurationMinutes
            )).ToList()
        );
    }
}

// -----------------------------------------------------------------------------
// 6. Reschedule Appointment Command
// -----------------------------------------------------------------------------
public record RescheduleAppointmentCommand(Guid AppointmentId, Guid NewSlotId) : IRequest<AppointmentDto>;

public class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand, AppointmentDto>
{
    private readonly IApplicationDbContext _context;

    public RescheduleAppointmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AppointmentDto> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor.Specialization)
            .Include(a => a.Cabinet)
            .Include(a => a.Slot)
            .Include(a => a.Status)
            .Include(a => a.AppointmentServices).ThenInclude(asv => asv.Service)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment == null) throw new NotFoundException(nameof(Appointment), request.AppointmentId);

        var newSlot = await _context.TimeSlots
            .Include(t => t.Cabinet)
            .Include(t => t.Doctor)
            .FirstOrDefaultAsync(t => t.Id == request.NewSlotId, cancellationToken);

        if (newSlot == null) throw new NotFoundException(nameof(TimeSlot), request.NewSlotId);

        if (newSlot.StatusId == LookupIds.TimeSlotStatus.Booked)
        {
            throw new ConflictException("The selected slot is already booked.");
        }

        // Release old slot
        appointment.Slot.StatusId = LookupIds.TimeSlotStatus.Free;
        appointment.Slot.ReservedUntil = null;

        // Take new slot
        newSlot.StatusId = LookupIds.TimeSlotStatus.Booked;
        newSlot.ReservedUntil = null;

        appointment.SlotId = newSlot.Id;
        appointment.CabinetId = newSlot.CabinetId;
        appointment.DoctorId = newSlot.DoctorId;
        appointment.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new AppointmentDto(
            appointment.Id,
            appointment.AppointmentNumber,
            appointment.PatientId,
            appointment.Patient.FullName,
            appointment.Patient.Phone,
            newSlot.DoctorId,
            appointment.Doctor.User.FullName,
            appointment.Doctor.Specialization.Name,
            newSlot.CabinetId,
            newSlot.Cabinet.Name,
            newSlot.Id,
            newSlot.SlotDate,
            newSlot.StartTime,
            newSlot.EndTime,
            appointment.Status.Id,
            appointment.Status.Code,
            appointment.Status.Name,
            appointment.PatientComplaint,
            appointment.DoctorNotes,
            appointment.IsConfirmedByPatient,
            appointment.CreatedAt,
            appointment.AppointmentServices.Select(s => new AppointmentServiceItemDto(
                s.ServiceId,
                s.Service.Name,
                s.PriceAtBooking,
                s.Service.DurationMinutes
            )).ToList()
        );
    }
}
