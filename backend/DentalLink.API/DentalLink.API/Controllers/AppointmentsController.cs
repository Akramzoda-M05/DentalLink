using DentalLink.Application.Features.Appointments;
using DentalLink.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalLink.API.Controllers;

public class AppointmentsController : BaseApiController
{
    [HttpPost("book")]
    [ProducesResponseType(typeof(BookAppointmentResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookAppointmentResultDto>> Book([FromBody] BookAppointmentCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("manual")]
    [Authorize(Roles = "admin,receptionist")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentDto>> CreateManual([FromBody] CreateManualAppointmentCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("timeline")]
    [Authorize(Roles = "admin,receptionist,doctor")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetTimeline(
        [FromQuery] DateOnly date,
        [FromQuery] Guid? cabinetId,
        [FromQuery] Guid? doctorId,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetAppointmentsTimelineQuery(date, cabinetId, doctorId), ct);
        return Ok(result);
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetMyAppointments(
        [FromQuery] string phone,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Missing Parameter",
                Detail = "Phone query parameter is required."
            });
        }

        var result = await Mediator.Send(new GetPatientAppointmentsQuery(phone), ct);
        return Ok(result);
    }

    [HttpPut("{id:Guid}/status")]
    [Authorize(Roles = "admin,receptionist,doctor")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDto>> UpdateStatus(
        Guid id,
        [FromBody] UpdateAppointmentStatusRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateAppointmentStatusCommand(id, request.StatusCode, request.DoctorNotes), ct);
        return Ok(result);
    }

    [HttpPut("{id:Guid}/reschedule")]
    [Authorize(Roles = "admin,receptionist")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentDto>> Reschedule(
        Guid id,
        [FromBody] RescheduleRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new RescheduleAppointmentCommand(id, request.NewSlotId), ct);
        return Ok(result);
    }

    [HttpPost("{id:Guid}/cancel")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDto>> Cancel(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateAppointmentStatusCommand(id, LookupCodes.AppointmentStatus.Cancelled), ct);
        return Ok(result);
    }
}

public record UpdateAppointmentStatusRequest(string StatusCode, string? DoctorNotes = null);
public record RescheduleRequest(Guid NewSlotId);
