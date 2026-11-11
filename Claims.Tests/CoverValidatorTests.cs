using Claims.Validation;
using Xunit;

namespace Claims.Tests;

public class CoverValidatorTests
{
    private static readonly DateTime Today = new(2026, 6, 15);
    private readonly CoverValidator _validator = new(new FixedTimeProvider(Today));

    [Fact]
    public void Valid_cover_passes()
    {
        var errors = _validator.Validate(new Cover { StartDate = Today, EndDate = Today.AddYears(1).AddDays(-1) });

        Assert.Empty(errors);
    }

    [Fact]
    public void StartDate_in_the_past_fails()
    {
        var errors = _validator.Validate(new Cover { StartDate = Today.AddDays(-1), EndDate = Today.AddMonths(1) });

        Assert.Contains(nameof(Cover.StartDate), errors.Keys);
    }

    [Fact]
    public void Period_longer_than_a_year_fails()
    {
        var errors = _validator.Validate(new Cover { StartDate = Today, EndDate = Today.AddYears(1) });

        Assert.Contains(nameof(Cover.EndDate), errors.Keys);
    }
}
