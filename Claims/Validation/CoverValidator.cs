namespace Claims.Validation;

public sealed class CoverValidator(TimeProvider timeProvider)
{
    public Dictionary<string, string[]> Validate(Cover cover)
    {
        var errors = new Dictionary<string, string[]>();
        var today = timeProvider.GetUtcNow().UtcDateTime.Date;
        var startDate = cover.StartDate.Date;
        var endDate = cover.EndDate.Date;

        if (startDate < today)
            errors[nameof(Cover.StartDate)] = ["StartDate cannot be in the past."];

        if (endDate <= startDate)
            errors[nameof(Cover.EndDate)] = ["EndDate must be after StartDate."];
        else if (endDate >= startDate.AddYears(1))
            errors[nameof(Cover.EndDate)] = ["Insurance period cannot exceed 1 year."];

        return errors;
    }
}
