using DentalLink.Application.Features.Schedules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalLink.API.Controllers;

public class SchedulesController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DoctorShiftDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DoctorShiftDto>>> GetSchedule(
        [FromQuery] Guid? doctorId,
        [FromQuery] Guid? cabinetId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetDoctorScheduleQuery(doctorId, cabinetId, startDate, endDate), ct);
        return Ok(result);
    }

    [HttpPost("generate")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> GenerateSchedule(
        [FromBody] GenerateTimeSlotsCommand command,
        CancellationToken ct)
    {
        var count = await Mediator.Send(command, ct);
        return Ok(new { generatedSlots = count });
    }
}
