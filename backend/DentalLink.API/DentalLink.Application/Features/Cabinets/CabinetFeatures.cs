using DentalLink.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalLink.Application.Features.Cabinets;

public record CabinetDto(
    Guid Id,
    string Name,
    int ChairNumber,
    string? Description,
    bool IsAvailable
);

public record GetCabinetsQuery(bool OnlyAvailable = false) : IRequest<IReadOnlyList<CabinetDto>>;

public class GetCabinetsQueryHandler : IRequestHandler<GetCabinetsQuery, IReadOnlyList<CabinetDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCabinetsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CabinetDto>> Handle(GetCabinetsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Cabinets.AsNoTracking();

        if (request.OnlyAvailable)
        {
            query = query.Where(c => c.IsAvailable);
        }

        var cabinets = await query
            .OrderBy(c => c.Name)
            .Select(c => new CabinetDto(
                c.Id,
                c.Name,
                c.ChairNumber,
                c.Description,
                c.IsAvailable
            ))
            .ToListAsync(cancellationToken);

        return cabinets;
    }
}
