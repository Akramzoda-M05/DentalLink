using DentalLink.Application.Common.Interfaces;
using DentalLink.Application.Common.Models;
using DentalLink.Application.Features.Appointments;
using DentalLink.Domain.Entities;
using DentalLink.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalLink.Application.Features.Patients;

public record PatientDto(
    Guid Id,
    string FullName,
    string Phone,
    string? Email,
    DateOnly? BirthDate,
    Guid? TelegramChatId,
    string? Notes,
    DateTimeOffset CreatedAt,
    int TotalAppointmentsCount
);

public record PatientDetailDto(
    Guid Id,
    string FullName,
    string Phone,
    string? Email,
    DateOnly? BirthDate,
    Guid? TelegramChatId,
    string? Notes,
    DateTimeOffset CreatedAt,
    IReadOnlyList<AppointmentDto> Appointments
);

// -----------------------------------------------------------------------------
// 1. Get Patients Query (with Paging and Search)
// -----------------------------------------------------------------------------
public record GetPatientsQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<PagedResult<PatientDto>>;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, PagedResult<PatientDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPatientsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PatientDto>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Patients
            .Include(p => p.Appointments)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p => p.FullName.ToLower().Contains(search) || p.Phone.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var patients = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PatientDto(
                p.Id,
                p.FullName,
                p.Phone,
                p.Email,
                p.BirthDate,
                p.TelegramChatId,
                p.Notes,
                p.CreatedAt,
                p.Appointments.Count
            ))
            .ToListAsync(cancellationToken);

        return PagedResult<PatientDto>.Create(patients, totalCount, request.PageNumber, request.PageSize);
    }
}

// -----------------------------------------------------------------------------
// 2. Get Patient By Id Query
// -----------------------------------------------------------------------------
public record GetPatientByIdQuery(Guid Id) : IRequest<PatientDetailDto>;

public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, PatientDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetPatientByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PatientDetailDto> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .Include(p => p.Appointments).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
            .Include(p => p.Appointments).ThenInclude(a => a.Doctor.Specialization)
            .Include(p => p.Appointments).ThenInclude(a => a.Cabinet)
            .Include(p => p.Appointments).ThenInclude(a => a.Slot)
            .Include(p => p.Appointments).ThenInclude(a => a.Status)
            .Include(p => p.Appointments).ThenInclude(a => a.AppointmentServices).ThenInclude(asv => asv.Service)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (patient == null)
        {
            throw new NotFoundException(nameof(Patient), request.Id);
        }

        var apptDtos = patient.Appointments
            .OrderByDescending(a => a.Slot.SlotDate)
            .ThenByDescending(a => a.Slot.StartTime)
            .Select(a => new AppointmentDto(
                a.Id,
                a.AppointmentNumber,
                a.PatientId,
                patient.FullName,
                patient.Phone,
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
            .ToList();

        return new PatientDetailDto(
            patient.Id,
            patient.FullName,
            patient.Phone,
            patient.Email,
            patient.BirthDate,
            patient.TelegramChatId,
            patient.Notes,
            patient.CreatedAt,
            apptDtos
        );
    }
}
