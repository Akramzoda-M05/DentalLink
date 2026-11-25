using DentalLink.Application.Features.Cabinets;
using Microsoft.AspNetCore.Mvc;

namespace DentalLink.API.Controllers;

public class CabinetsController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CabinetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CabinetDto>>> GetCabinets(
        [FromQuery] bool onlyAvailable = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetCabinetsQuery(onlyAvailable), ct);
        return Ok(result);
    }
}
