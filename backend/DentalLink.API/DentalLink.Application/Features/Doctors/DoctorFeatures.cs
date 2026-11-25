using DentalLink.Application.Common.Interfaces;
using DentalLink.Domain.Entities;
using DentalLink.Domain.Entities.Lookup;
using DentalLink.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalLink.Application.Features.Doctors;

public record SpecializationDto(Guid Id, string Code, string Name);

public record DoctorDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string Phone,
    SpecializationDto Specialization,
    string? Qualification,
    int ExperienceYears,
    string? Bio,
    string? AvatarUrl,
    bool IsActive
);

// -----------------------------------------------------------------------------
// 1. Get Doctors Query
// -----------------------------------------------------------------------------
public record GetDoctorsQuery(
    Guid? SpecializationId = null,
    string? Search = null,
    bool OnlyActive = true
) : IRequest<IReadOnlyList<DoctorDto>>;

public class GetDoctorsQueryHandler : IRequestHandler<GetDoctorsQuery, IReadOnlyList<DoctorDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDoctorsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DoctorDto>> Handle(GetDoctorsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .AsNoTracking();

        if (request.OnlyActive)
        {
            query = query.Where(d => d.IsActive && d.User.IsActive);
        }

        if (request.SpecializationId.HasValue)
        {
            query = query.Where(d => d.SpecializationId == request.SpecializationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(d => d.User.FullName.ToLower().Contains(search) ||
                                     d.Specialization.Name.ToLower().Contains(search) ||
                                     (d.Qualification != null && d.Qualification.ToLower().Contains(search)));
        }

        var doctors = await query
            .OrderBy(d => d.User.FullName)
            .Select(d => new DoctorDto(
                d.Id,
                d.UserId,
                d.User.FullName,
                d.User.Email,
                d.User.Phone,
                new SpecializationDto(d.Specialization.Id, d.Specialization.Code, d.Specialization.Name),
                d.Qualification,
                d.ExperienceYears,
                d.Bio,
                d.AvatarUrl,
                d.IsActive
            ))
            .ToListAsync(cancellationToken);

        return doctors;
    }
}

// -----------------------------------------------------------------------------
// 2. Get Doctor By Id Query
// -----------------------------------------------------------------------------
public record GetDoctorByIdQuery(Guid Id) : IRequest<DoctorDto>;

public class GetDoctorByIdQueryHandler : IRequestHandler<GetDoctorByIdQuery, DoctorDto>
{
    private readonly IApplicationDbContext _context;

    public GetDoctorByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DoctorDto> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (doctor == null)
        {
            throw new NotFoundException(nameof(Doctor), request.Id);
        }

        return new DoctorDto(
            doctor.Id,
            doctor.UserId,
            doctor.User.FullName,
            doctor.User.Email,
            doctor.User.Phone,
            new SpecializationDto(doctor.Specialization.Id, doctor.Specialization.Code, doctor.Specialization.Name),
            doctor.Qualification,
            doctor.ExperienceYears,
            doctor.Bio,
            doctor.AvatarUrl,
            doctor.IsActive
        );
    }
}

// -----------------------------------------------------------------------------
// 3. Update Doctor Command (Admin or Doctor self)
// -----------------------------------------------------------------------------
public record UpdateDoctorCommand(
    Guid Id,
    Guid? SpecializationId,
    string? Qualification,
    int? ExperienceYears,
    string? Bio,
    string? AvatarUrl,
    bool? IsActive
) : IRequest<DoctorDto>;

public class UpdateDoctorCommandValidator : AbstractValidator<UpdateDoctorCommand>
{
    public UpdateDoctorCommandValidator()
    {
        RuleFor(x => x.Id).NotEqual(Guid.Empty);
        When(x => x.ExperienceYears.HasValue, () =>
        {
            RuleFor(x => x.ExperienceYears!.Value).GreaterThanOrEqualTo(0);
        });
    }
}

public class UpdateDoctorCommandHandler : IRequestHandler<UpdateDoctorCommand, DoctorDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateDoctorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DoctorDto> Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (doctor == null)
        {
            throw new NotFoundException(nameof(Doctor), request.Id);
        }

        if (request.SpecializationId.HasValue && request.SpecializationId.Value != doctor.SpecializationId)
        {
            var specExists = await _context.DoctorSpecializations
                .AnyAsync(s => s.Id == request.SpecializationId.Value, cancellationToken);
            if (!specExists)
            {
                throw new NotFoundException(nameof(DoctorSpecialization), request.SpecializationId.Value);
            }
            doctor.SpecializationId = request.SpecializationId.Value;
        }

        if (request.Qualification != null) doctor.Qualification = request.Qualification.Trim();
        if (request.ExperienceYears.HasValue) doctor.ExperienceYears = request.ExperienceYears.Value;
        if (request.Bio != null) doctor.Bio = request.Bio.Trim();
        if (request.AvatarUrl != null) doctor.AvatarUrl = request.AvatarUrl.Trim();
        if (request.IsActive.HasValue) doctor.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync(cancellationToken);

        // Reload specialization if changed
        await _context.Doctors.Entry(doctor).Reference(d => d.Specialization).LoadAsync(cancellationToken);

        return new DoctorDto(
            doctor.Id,
            doctor.UserId,
            doctor.User.FullName,
            doctor.User.Email,
            doctor.User.Phone,
            new SpecializationDto(doctor.Specialization.Id, doctor.Specialization.Code, doctor.Specialization.Name),
            doctor.Qualification,
            doctor.ExperienceYears,
            doctor.Bio,
            doctor.AvatarUrl,
            doctor.IsActive
        );
    }
}
