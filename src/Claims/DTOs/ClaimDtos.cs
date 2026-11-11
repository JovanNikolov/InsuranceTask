using Claims.Models;

namespace Claims.DTOs;

public sealed record CreateClaimRequest(string CoverId, DateTime Created, string Name, ClaimType Type, decimal DamageCost);

public sealed record ClaimResponse(string Id, string CoverId, DateTime Created, string Name, ClaimType Type, decimal DamageCost)
{
    public static ClaimResponse From(Claim claim) =>
        new(claim.Id, claim.CoverId, claim.Created, claim.Name, claim.Type, claim.DamageCost);
}
