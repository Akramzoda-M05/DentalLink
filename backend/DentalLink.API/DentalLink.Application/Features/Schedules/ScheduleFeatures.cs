using DentalLink.Application.Common.Interfaces;
using DentalLink.Domain.Constants;
using DentalLink.Domain.Entities;
using DentalLink.Domain.Entities.Lookup;
using DentalLink.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalLink.Application.Features.Schedules;

public record TimeSlotSummaryDto(
    Guid Id,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid StatusId,
    string StatusCode,
    string StatusName,
    DateTimeOffset? ReservedUntil
);

public record DoctorShiftDto(
    Guid Id,
    Guid DoctorId,
    string DoctorName,
    Guid CabinetId,
    string CabinetName,
    DateOnly WorkDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    IReadOnlyList<TimeSlotSummaryDto> Slots
);

// -----------------------------------------------------------------------------
// 1. Get Doctor Schedule Query
// -----------------------------------------------------------------------------
public record GetDoctorScheduleQuery(
    Guid? DoctorId = null,
    Guid? CabinetId = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null
) : IRequest<IReadOnlyList<DoctorShiftDto>>;

public class GetDoctorScheduleQueryHandler : IRequestHandler<GetDoctorScheduleQuery, IReadOnlyList<DoctorShiftDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDoctorScheduleQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DoctorShiftDto>> Handle(GetDoctorScheduleQuery request, CancellationToken cancellationToken)
    {
        var start = request.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var end = request.EndDate ?? start.AddDays(7);

        var query = _context.DoctorSchedules
            .Include(s => s.Doctor)
            .ThenInclude(d => d.User)
            .Include(s => s.Cabinet)
            .Include(s => s.TimeSlots)
            .ThenInclude(t => t.Status)
            .Where(s => s.WorkDate >= start && s.WorkDate <= end)
            .AsNoTracking();

        if (request.DoctorId.HasValue)
        {
            query = query.Where(s => s.DoctorId == request.DoctorId.Value);
        }

        if (request.CabinetId.HasValue)
        {
            query = query.Where(s => s.CabinetId == request.CabinetId.Value);
        }

        var shifts = await query
            .OrderBy(s => s.WorkDate)
            .ThenBy(s => s.StartTime)
            .Select(s => new DoctorShiftDto(
                s.Id,
                s.DoctorId,
                s.Doctor.User.FullName,
                s.CabinetId,
                s.Cabinet.Name,
                s.WorkDate,
                s.StartTime,
                s.EndTime,
                s.TimeSlots
                    .OrderBy(t => t.StartTime)
                    .Select(t => new TimeSlotSummaryDto(
                        t.Id,
                        t.StartTime,
                        t.EndTime,
                        t.StatusId,
                        t.Status.Code,
                        t.Status.Name,
                        t.ReservedUntil
                    ))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);

        return shifts;
    }
}

// -----------------------------------------------------------------------------
// 2. Generate Time Slots Command (Admin: batch create schedule & 30-min slots)
// -----------------------------------------------------------------------------
public record GenerateTimeSlotsCommand(
    Guid DoctorId,
    Guid CabinetId,
    DateOnly StartDate,
    DateOnly EndDate,
    TimeOnly ShiftStartTime,
    TimeOnly ShiftEndTime,
    List<DayOfWeek> DaysOfWeek
) : IRequest<int>;

public class GenerateTimeSlotsCommandValidator : AbstractValidator<GenerateTimeSlotsCommand>
{
    public GenerateTimeSlotsCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEqual(Guid.Empty);
        RuleFor(x => x.CabinetId).NotEqual(Guid.Empty);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("EndDate must be greater than or equal to StartDate.");
        RuleFor(x => x.ShiftEndTime).GreaterThan(x => x.ShiftStartTime)
            .WithMessage("ShiftEndTime must be after ShiftStartTime.");
        RuleFor(x => x.DaysOfWeek).NotEmpty().WithMessage("At least one day of week must be selected.");
    }
}

public class GenerateTimeSlotsCommandHandler : IRequestHandler<GenerateTimeSlotsCommand, int>
{
    private readonly IApplicationDbContext _context;

    public GenerateTimeSlotsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(GenerateTimeSlotsCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);
        if (doctor == null) throw new NotFoundException(nameof(Doctor), request.DoctorId);

        var cabinet = await _context.Cabinets.FirstOrDefaultAsync(c => c.Id == request.CabinetId, cancellationToken);
        if (cabinet == null) throw new NotFoundException(nameof(Cabinet), request.CabinetId);

        int createdSlotsCount = 0;
        var freeStatus = await _context.TimeSlotStatuses
            .FirstOrDefaultAsync(s => s.Id == LookupIds.TimeSlotStatus.Free, cancellationToken);

        for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
        {
            if (!request.DaysOfWeek.Contains(date.DayOfWeek))
            {
                continue;
            }

            // Check if doctor or cabinet already has a schedule for this date/time range
            var hasConflict = await _context.DoctorSchedules
                .AnyAsync(s => s.WorkDate == date &&
                               (s.DoctorId == request.DoctorId || s.CabinetId == request.CabinetId) &&
                               s.StartTime < request.ShiftEndTime && s.EndTime > request.ShiftStartTime,
                               cancellationToken);

            if (hasConflict)
            {
                continue; // Skip overlapping shift to keep idempotency
            }

            var schedule = new DoctorSchedule
            {
                DoctorId = request.DoctorId,
                CabinetId = request.CabinetId,
                WorkDate = date,
                StartTime = request.ShiftStartTime,
                EndTime = request.ShiftEndTime,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.DoctorSchedules.Add(schedule);
            await _context.SaveChangesAsync(cancellationToken);

            // Generate 30-minute slots
            var currentSlotStart = request.ShiftStartTime;
            while (currentSlotStart.AddMinutes(30) <= request.ShiftEndTime)
            {
                var slotEnd = currentSlotStart.AddMinutes(30);

                var slot = new TimeSlot
                {
                    ScheduleId = schedule.Id,
                    DoctorId = request.DoctorId,
                    CabinetId = request.CabinetId,
                    SlotDate = date,
                    StartTime = currentSlotStart,
                    EndTime = slotEnd,
                    StatusId = LookupIds.TimeSlotStatus.Free,
                    ReservedUntil = null
                };

                _context.TimeSlots.Add(slot);
                createdSlotsCount++;
                currentSlotStart = slotEnd;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return createdSlotsCount;
    }
}
