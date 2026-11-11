using Claims.Models;

namespace Claims.Services;

public sealed class PremiumCalculator
{
    private const decimal BaseDayRate = 1250m;
    private const int FullRateDays = 30;
    private const int FirstDiscountDays = 150;

    public decimal Calculate(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        var days = (endDate.Date - startDate.Date).Days + 1;
        var dayRate = BaseDayRate * Multiplier(coverType);
        var isYacht = coverType == CoverType.Yacht;

        var fullRateDays = Math.Clamp(days, 0, FullRateDays);
        var firstDiscountDays = Math.Clamp(days - FullRateDays, 0, FirstDiscountDays);
        var remainingDays = Math.Max(days - FullRateDays - FirstDiscountDays, 0);

        return fullRateDays * dayRate
            + firstDiscountDays * dayRate * (1 - (isYacht ? 0.05m : 0.02m))
            + remainingDays * dayRate * (1 - (isYacht ? 0.08m : 0.03m));
    }

    private static decimal Multiplier(CoverType coverType) => coverType switch
    {
        CoverType.Yacht => 1.1m,
        CoverType.PassengerShip => 1.2m,
        CoverType.Tanker => 1.5m,
        _ => 1.3m
    };
}
