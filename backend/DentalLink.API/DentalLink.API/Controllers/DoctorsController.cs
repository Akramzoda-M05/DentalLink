using DentalLink.Application.Features.Doctors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalLink.API.Controllers;

public class DoctorsController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DoctorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DoctorDto>>> GetDoctors(
        [FromQuery] Guid? specializationId,
        [FromQuery] string? search,
        [FromQuery] bool onlyActive = true,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetDoctorsQuery(specializationId, search, onlyActive), ct);
        return Ok(result);
    }

    [HttpGet("{id:Guid}")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDto>> GetDoctorById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetDoctorByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPut("{id:Guid}")]
    [Authorize(Roles = "admin,doctor")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDto>> UpdateDoctor(Guid id, [FromBody] UpdateDoctorCommand command, CancellationToken ct)
    {
        if (id != command.Id)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Id Mismatch",
                Detail = "URL parameter 'id' does not match command payload 'id'."
            });
        }

        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }
}
