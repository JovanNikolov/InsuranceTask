using Claims.DTOs;
using Claims.Models;
using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class CoversController(ICoversService coversService) : ControllerBase
{
    [HttpPost("compute")]
    [EndpointSummary("Computes the premium for a cover without saving it.")]
    [EndpointDescription("The end date counts as an insurance day. The same rules as for creating a cover apply.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<decimal> ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        var result = coversService.ComputePremium(startDate, endDate, coverType);
        if (!result.Succeeded)
            return ValidationProblem(new ValidationProblemDetails(result.Errors));

        return Ok(result.Value);
    }

    [HttpGet]
    [EndpointSummary("Returns all covers.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CoverResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await coversService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id}")]
    [EndpointSummary("Returns the cover with the given id.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CoverResponse>> GetById(string id, CancellationToken cancellationToken)
    {
        var cover = await coversService.GetByIdAsync(id, cancellationToken);
        return cover is null ? NotFound() : Ok(cover);
    }

    [HttpPost]
    [EndpointSummary("Creates a cover and computes its premium.")]
    [EndpointDescription("StartDate cannot be in the past. EndDate must be after StartDate and before the same date one year later.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CoverResponse>> Create(CreateCoverRequest request, CancellationToken cancellationToken)
    {
        var result = await coversService.CreateAsync(request, cancellationToken);
        if (!result.Succeeded)
            return ValidationProblem(new ValidationProblemDetails(result.Errors));

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Deletes the cover with the given id.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        return await coversService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }
}
