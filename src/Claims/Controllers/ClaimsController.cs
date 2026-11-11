using Claims.DTOs;
using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class ClaimsController(IClaimsService claimsService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Returns all claims.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClaimResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await claimsService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id}")]
    [EndpointSummary("Returns the claim with the given id.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimResponse>> GetById(string id, CancellationToken cancellationToken)
    {
        var claim = await claimsService.GetByIdAsync(id, cancellationToken);
        return claim is null ? NotFound() : Ok(claim);
    }

    [HttpPost]
    [EndpointSummary("Creates a claim for an existing cover.")]
    [EndpointDescription("DamageCost not be less than 0 and at most 100000. Created must be within the cover period.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClaimResponse>> Create(CreateClaimRequest request, CancellationToken cancellationToken)
    {
        var result = await claimsService.CreateAsync(request, cancellationToken);
        if (!result.Succeeded)
            return ValidationProblem(new ValidationProblemDetails(result.Errors));

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Deletes the claim with the given id.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        return await claimsService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }
}
