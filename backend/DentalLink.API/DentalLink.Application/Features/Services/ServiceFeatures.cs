using DentalLink.Application.Common.Interfaces;
using DentalLink.Domain.Entities.Lookup;
using DentalLink.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalLink.Application.Features.Services;

public record ServiceCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    int ServicesCount
);

public record DentalServiceDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    decimal Price,
    int DurationMinutes,
    bool IsActive
);

// -----------------------------------------------------------------------------
// 1. Get Categories Query
// -----------------------------------------------------------------------------
public record GetServiceCategoriesQuery : IRequest<IReadOnlyList<ServiceCategoryDto>>;

public class GetServiceCategoriesQueryHandler : IRequestHandler<GetServiceCategoriesQuery, IReadOnlyList<ServiceCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetServiceCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ServiceCategoryDto>> Handle(GetServiceCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.ServiceCategories
            .Include(c => c.Services)
            .OrderBy(c => c.Name)
            .Select(c => new ServiceCategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.Services.Count(s => s.IsActive)
            ))
            .ToListAsync(cancellationToken);

        return categories;
    }
}

// -----------------------------------------------------------------------------
// 2. Get Dental Services Query
// -----------------------------------------------------------------------------
public record GetDentalServicesQuery(
    Guid? CategoryId = null,
    string? Search = null,
    bool OnlyActive = true
) : IRequest<IReadOnlyList<DentalServiceDto>>;

public class GetDentalServicesQueryHandler : IRequestHandler<GetDentalServicesQuery, IReadOnlyList<DentalServiceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDentalServicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DentalServiceDto>> Handle(GetDentalServicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DentalServices
            .Include(s => s.Category)
            .AsNoTracking();

        if (request.OnlyActive)
        {
            query = query.Where(s => s.IsActive);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(s => s.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(search) ||
                                     (s.Description != null && s.Description.ToLower().Contains(search)));
        }

        var services = await query
            .OrderBy(s => s.Name)
            .Select(s => new DentalServiceDto(
                s.Id,
                s.CategoryId,
                s.Category.Name,
                s.Name,
                s.Description,
                s.Price,
                s.DurationMinutes,
                s.IsActive
            ))
            .ToListAsync(cancellationToken);

        return services;
    }
}

// -----------------------------------------------------------------------------
// 3. Create Dental Service Command (Admin)
// -----------------------------------------------------------------------------
public record CreateDentalServiceCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    int DurationMinutes
) : IRequest<DentalServiceDto>;

public class CreateDentalServiceCommandValidator : AbstractValidator<CreateDentalServiceCommand>
{
    public CreateDentalServiceCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEqual(Guid.Empty);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .Must(d => d % 30 == 0)
            .WithMessage("Duration must be a multiple of 30 minutes (e.g. 30, 60, 90).");
    }
}

public class CreateDentalServiceCommandHandler : IRequestHandler<CreateDentalServiceCommand, DentalServiceDto>
{
    private readonly IApplicationDbContext _context;

    public CreateDentalServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DentalServiceDto> Handle(CreateDentalServiceCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ServiceCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException(nameof(ServiceCategory), request.CategoryId);
        }

        var service = new DentalService
        {
            CategoryId = category.Id,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            DurationMinutes = request.DurationMinutes,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.DentalServices.Add(service);
        await _context.SaveChangesAsync(cancellationToken);

        return new DentalServiceDto(
            service.Id,
            category.Id,
            category.Name,
            service.Name,
            service.Description,
            service.Price,
            service.DurationMinutes,
            service.IsActive
        );
    }
}

// -----------------------------------------------------------------------------
// 4. Update Dental Service Command (Admin)
// -----------------------------------------------------------------------------
public record UpdateDentalServiceCommand(
    Guid Id,
    Guid? CategoryId,
    string? Name,
    string? Description,
    decimal? Price,
    int? DurationMinutes,
    bool? IsActive
) : IRequest<DentalServiceDto>;

public class UpdateDentalServiceCommandValidator : AbstractValidator<UpdateDentalServiceCommand>
{
    public UpdateDentalServiceCommandValidator()
    {
        RuleFor(x => x.Id).NotEqual(Guid.Empty);
        When(x => x.Price.HasValue, () =>
        {
            RuleFor(x => x.Price!.Value).GreaterThanOrEqualTo(0);
        });
        When(x => x.DurationMinutes.HasValue, () =>
        {
            RuleFor(x => x.DurationMinutes!.Value)
                .GreaterThan(0)
                .Must(d => d % 30 == 0)
                .WithMessage("Duration must be a multiple of 30 minutes (e.g. 30, 60, 90).");
        });
    }
}

public class UpdateDentalServiceCommandHandler : IRequestHandler<UpdateDentalServiceCommand, DentalServiceDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateDentalServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DentalServiceDto> Handle(UpdateDentalServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.DentalServices
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service == null)
        {
            throw new NotFoundException(nameof(DentalService), request.Id);
        }

        if (request.CategoryId.HasValue && request.CategoryId.Value != service.CategoryId)
        {
            var category = await _context.ServiceCategories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value, cancellationToken);
            if (category == null)
            {
                throw new NotFoundException(nameof(ServiceCategory), request.CategoryId.Value);
            }
            service.CategoryId = category.Id;
        }

        if (request.Name != null) service.Name = request.Name.Trim();
        if (request.Description != null) service.Description = request.Description.Trim();
        if (request.Price.HasValue) service.Price = request.Price.Value;
        if (request.DurationMinutes.HasValue) service.DurationMinutes = request.DurationMinutes.Value;
        if (request.IsActive.HasValue) service.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync(cancellationToken);

        await _context.DentalServices.Entry(service).Reference(s => s.Category).LoadAsync(cancellationToken);

        return new DentalServiceDto(
            service.Id,
            service.Category.Id,
            service.Category.Name,
            service.Name,
            service.Description,
            service.Price,
            service.DurationMinutes,
            service.IsActive
        );
    }
}
