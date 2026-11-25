using DentalLink.Application.Common.Interfaces;
using DentalLink.Domain.Constants;
using DentalLink.Domain.Entities;
using DentalLink.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalLink.Application.Features.Slots;

public record AvailableSlotDto(
    Guid Id,
    Guid DoctorId,
    string DoctorName,
    Guid CabinetId,
    string CabinetName,
    DateOnly SlotDate,
    TimeOnly StartTime,
    TimeOnly EndTime
);

public record HoldSlotResultDto(
    Guid SlotId,
    DateTimeOffset ExpiresAt,
    string Message
);

// -----------------------------------------------------------------------------
// 1. Get Available Slots Query (Free slots or expired holds)
// -----------------------------------------------------------------------------
public record GetAvailableSlotsQuery(
    Guid? DoctorId = null,
    Guid? SpecializationId = null,
    DateOnly? Date = null
) : IRequest<IReadOnlyList<AvailableSlotDto>>;

public class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, IReadOnlyList<AvailableSlotDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableSlotsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AvailableSlotDto>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        var targetDate = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var nowUtc = DateTimeOffset.UtcNow;

        var query = _context.TimeSlots
            .Include(t => t.Doctor)
            .ThenInclude(d => d.User)
            .Include(t => t.Cabinet)
            .Where(t => t.SlotDate == targetDate)
            .Where(t => t.StatusId == LookupIds.TimeSlotStatus.Free ||
                       (t.StatusId == LookupIds.TimeSlotStatus.Reserved && t.ReservedUntil < nowUtc))
            .AsNoTracking();

        if (request.DoctorId.HasValue)
        {
            query = query.Where(t => t.DoctorId == request.DoctorId.Value);
        }

        if (request.SpecializationId.HasValue)
        {
            query = query.Where(t => t.Doctor.SpecializationId == request.SpecializationId.Value);
        }

        var slots = await query
            .OrderBy(t => t.StartTime)
            .Select(t => new AvailableSlotDto(
                t.Id,
                t.DoctorId,
                t.Doctor.User.FullName,
                t.CabinetId,
                t.Cabinet.Name,
                t.SlotDate,
                t.StartTime,
                t.EndTime
            ))
            .ToListAsync(cancellationToken);

        return slots;
    }
}

// -----------------------------------------------------------------------------
// 2. Hold Slot Command (10-minute hold with Distributed Lock)
// -----------------------------------------------------------------------------
public record HoldSlotCommand(Guid SlotId) : IRequest<HoldSlotResultDto>;

public class HoldSlotCommandValidator : AbstractValidator<HoldSlotCommand>
{
    public HoldSlotCommandValidator()
    {
        RuleFor(x => x.SlotId).NotEqual(Guid.Empty);
    }
}

public class HoldSlotCommandHandler : IRequestHandler<HoldSlotCommand, HoldSlotResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedLockService _lockService;

    public HoldSlotCommandHandler(
        IApplicationDbContext context,
        IDistributedLockService lockService)
    {
        _context = context;
        _lockService = lockService;
    }

    public async Task<HoldSlotResultDto> Handle(HoldSlotCommand request, CancellationToken cancellationToken)
    {
        var lockKey = $"lock:slot:{request.SlotId}";
        using var distributedLock = await _lockService.TryAcquireLockAsync(
            lockKey,
            expiry: TimeSpan.FromSeconds(30),
            waitTimeout: TimeSpan.FromSeconds(5),
            cancellationToken);

        if (distributedLock == null)
        {
            throw new ConflictException("The selected time slot is currently being processed by another patient. Please choose another slot.");
        }

        var slot = await _context.TimeSlots
            .FirstOrDefaultAsync(t => t.Id == request.SlotId, cancellationToken);

        if (slot == null)
        {
            throw new NotFoundException(nameof(TimeSlot), request.SlotId);
        }

        var nowUtc = DateTimeOffset.UtcNow;

        // Check if free or expired hold
        bool isAvailable = slot.StatusId == LookupIds.TimeSlotStatus.Free ||
            (slot.StatusId == LookupIds.TimeSlotStatus.Reserved && slot.ReservedUntil < nowUtc);

        if (!isAvailable)
        {
            throw new ConflictException("The selected time slot is already booked or reserved.");
        }

        var holdDuration = TimeSpan.FromMinutes(10);
        var expiresAt = nowUtc.Add(holdDuration);

        slot.StatusId = LookupIds.TimeSlotStatus.Reserved;
        slot.ReservedUntil = expiresAt;

        await _context.SaveChangesAsync(cancellationToken);

        return new HoldSlotResultDto(
            slot.Id,
            expiresAt,
            "Slot held successfully for 10 minutes."
        );
    }
}

// -----------------------------------------------------------------------------
// 3. Release Slot Hold Command
// -----------------------------------------------------------------------------
public record ReleaseSlotHoldCommand(Guid SlotId) : IRequest<bool>;

public class ReleaseSlotHoldCommandHandler : IRequestHandler<ReleaseSlotHoldCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedLockService _lockService;

    public ReleaseSlotHoldCommandHandler(IApplicationDbContext context, IDistributedLockService lockService)
    {
        _context = context;
        _lockService = lockService;
    }

    public async Task<bool> Handle(ReleaseSlotHoldCommand request, CancellationToken cancellationToken)
    {
        var lockKey = $"lock:slot:{request.SlotId}";
        using var distributedLock = await _lockService.TryAcquireLockAsync(
            lockKey,
            expiry: TimeSpan.FromSeconds(15),
            waitTimeout: TimeSpan.FromSeconds(3),
            cancellationToken);

        var slot = await _context.TimeSlots
            .FirstOrDefaultAsync(t => t.Id == request.SlotId, cancellationToken);

        if (slot == null)
        {
            return false;
        }

        if (slot.StatusId == LookupIds.TimeSlotStatus.Reserved)
        {
            slot.StatusId = LookupIds.TimeSlotStatus.Free;
            slot.ReservedUntil = null;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        return false;
    }
}
