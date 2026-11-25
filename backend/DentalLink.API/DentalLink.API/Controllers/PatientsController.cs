using DentalLink.Application.Common.Models;
using DentalLink.Application.Features.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalLink.API.Controllers;

[Authorize(Roles = "admin,receptionist,doctor")]
public class PatientsController : BaseApiController
{
    [HttpGet]
    [Authorize(Roles = "admin,receptionist")]
    [ProducesResponseType(typeof(PagedResult<PatientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PatientDto>>> GetPatients(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetPatientsQuery(search, pageNumber, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:Guid}")]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientDetailDto>> GetPatientById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPatientByIdQuery(id), ct);
        return Ok(result);
    }
}
