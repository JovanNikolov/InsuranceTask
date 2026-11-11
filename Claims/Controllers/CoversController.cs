using Claims.Auditing;
using Claims.Premiums;
using Claims.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly ClaimsContext _claimsContext;
    private readonly ILogger<CoversController> _logger;
    private readonly Auditer _auditer;
    private readonly CoverValidator _validator;
    private readonly PremiumCalculator _premiumCalculator;

    public CoversController(ClaimsContext claimsContext, AuditContext auditContext, ILogger<CoversController> logger, CoverValidator validator, PremiumCalculator premiumCalculator)
    {
        _claimsContext = claimsContext;
        _logger = logger;
        _auditer = new Auditer(auditContext);
        _validator = validator;
        _premiumCalculator = premiumCalculator;
    }

    [HttpPost("compute")]
    public ActionResult<decimal> ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        var errors = _validator.Validate(new Cover { StartDate = startDate, EndDate = endDate, Type = coverType });
        if (errors.Count > 0)
            return ValidationProblem(new ValidationProblemDetails(errors));

        return Ok(_premiumCalculator.Calculate(startDate, endDate, coverType));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cover>>> GetAsync()
    {
        var results = await _claimsContext.Covers.ToListAsync();
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Cover>> GetAsync(string id)
    {
        var results = await _claimsContext.Covers.ToListAsync();
        return Ok(results.SingleOrDefault(cover => cover.Id == id));
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync(Cover cover)
    {
        var errors = _validator.Validate(cover);
        if (errors.Count > 0)
            return ValidationProblem(new ValidationProblemDetails(errors));

        cover.Id = Guid.NewGuid().ToString();
        cover.Premium = _premiumCalculator.Calculate(cover.StartDate, cover.EndDate, cover.Type);
        _claimsContext.Covers.Add(cover);
        await _claimsContext.SaveChangesAsync();
        _auditer.AuditCover(cover.Id, "POST");
        return Ok(cover);
    }

    [HttpDelete("{id}")]
    public async Task DeleteAsync(string id)
    {
        _auditer.AuditCover(id, "DELETE");
        var cover = await _claimsContext.Covers.Where(cover => cover.Id == id).SingleOrDefaultAsync();
        if (cover is not null)
        {
            _claimsContext.Covers.Remove(cover);
            await _claimsContext.SaveChangesAsync();
        }
    }

}
