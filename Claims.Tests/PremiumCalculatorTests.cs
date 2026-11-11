using Claims.Premiums;
using Xunit;

namespace Claims.Tests;

public class PremiumCalculatorTests
{
    private static readonly DateTime Start = new(2026, 6, 15);
    private readonly PremiumCalculator _calculator = new();

    [Theory]
    [InlineData(CoverType.Yacht, 30, 41250)]
    [InlineData(CoverType.PassengerShip, 31, 46470)]
    [InlineData(CoverType.Tanker, 181, 333693.75)]
    [InlineData(CoverType.Yacht, 181, 238452.5)]
    [InlineData(CoverType.ContainerShip, 1, 1625)]
    public void Premium_is_computed_per_period(CoverType type, int days, double expected)
    {
        var premium = _calculator.Calculate(Start, Start.AddDays(days - 1), type);

        Assert.Equal((decimal)expected, premium);
    }
}
