using DentalLink.Application.Features.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalLink.API.Controllers;

public class ServicesController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DentalServiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DentalServiceDto>>> GetServices(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search,
        [FromQuery] bool onlyActive = true,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetDentalServicesQuery(categoryId, search, onlyActive), ct);
        return Ok(result);
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceCategoryDto>>> GetCategories(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetServiceCategoriesQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(DentalServiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DentalServiceDto>> CreateService([FromBody] CreateDentalServiceCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetServices), new { categoryId = result.CategoryId }, result);
    }

    [HttpPut("{id:Guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(DentalServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DentalServiceDto>> UpdateService(Guid id, [FromBody] UpdateDentalServiceCommand command, CancellationToken ct)
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
