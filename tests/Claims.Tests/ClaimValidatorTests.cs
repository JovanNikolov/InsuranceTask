using Claims.Models;
using Claims.Services;
using Xunit;

namespace Claims.Tests;

public class ClaimValidatorTests
{
    private readonly ClaimValidator _validator = new();
    private readonly Cover _cover = new() { StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31) };

    [Fact]
    public void Valid_claim_passes()
    {
        var claim = new Claim { DamageCost = 100_000, Created = new DateTime(2026, 6, 1) };

        Assert.Empty(_validator.Validate(claim, _cover));
    }

    [Fact]
    public void DamageCost_over_limit_fails()
    {
        var claim = new Claim { DamageCost = 100_000.01m, Created = new DateTime(2026, 6, 1) };

        Assert.Contains(nameof(Claim.DamageCost), _validator.Validate(claim, _cover).Keys);
    }

    [Fact]
    public void Negative_DamageCost_fails()
    {
        var claim = new Claim { DamageCost = -1, Created = new DateTime(2026, 6, 1) };

        Assert.Contains(nameof(Claim.DamageCost), _validator.Validate(claim, _cover).Keys);
    }

    [Fact]
    public void Created_outside_cover_period_fails()
    {
        var claim = new Claim { DamageCost = 500, Created = new DateTime(2027, 1, 1) };

        Assert.Contains(nameof(Claim.Created), _validator.Validate(claim, _cover).Keys);
    }

    [Fact]
    public void Missing_cover_fails()
    {
        var claim = new Claim { DamageCost = 500, Created = new DateTime(2026, 6, 1) };

        Assert.Contains(nameof(Claim.CoverId), _validator.Validate(claim, null).Keys);
    }
}
