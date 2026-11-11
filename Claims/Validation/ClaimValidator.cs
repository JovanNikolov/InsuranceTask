namespace Claims.Validation;

public sealed class ClaimValidator
{
    private const decimal MaxDamageCost = 100000;

    public Dictionary<string, string[]> Validate(Claim claim, Cover? cover)
    {
        var errors = new Dictionary<string, string[]>();

        if (claim.DamageCost <= 0)
            errors[nameof(Claim.DamageCost)] = ["DamageCost must be greater than 0."];
        else if (claim.DamageCost > MaxDamageCost)
            errors[nameof(Claim.DamageCost)] = [$"DamageCost cannot exceed {MaxDamageCost}."];

        if (cover is null)
            errors[nameof(Claim.CoverId)] = ["Cover does not exist."];
        else if (claim.Created.Date < cover.StartDate.Date || claim.Created.Date > cover.EndDate.Date)
            errors[nameof(Claim.Created)] = ["Created date must be within the cover period."];

        return errors;
    }
}
