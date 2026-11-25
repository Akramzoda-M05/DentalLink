using DentalLink.Application.Features.Slots;
using Microsoft.AspNetCore.Mvc;

namespace DentalLink.API.Controllers;

public class SlotsController : BaseApiController
{
    [HttpGet("available")]
    [ProducesResponseType(typeof(IReadOnlyList<AvailableSlotDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AvailableSlotDto>>> GetAvailableSlots(
        [FromQuery] Guid? doctorId,
        [FromQuery] Guid? specializationId,
        [FromQuery] DateOnly? date,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetAvailableSlotsQuery(doctorId, specializationId, date), ct);
        return Ok(result);
    }

    [HttpPost("{id:Guid}/hold")]
    [ProducesResponseType(typeof(HoldSlotResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HoldSlotResultDto>> HoldSlot(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new HoldSlotCommand(id), ct);
        return Ok(result);
    }

    [HttpPost("{id:Guid}/release")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> ReleaseHold(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ReleaseSlotHoldCommand(id), ct);
        return Ok(new { released = result });
    }
}
