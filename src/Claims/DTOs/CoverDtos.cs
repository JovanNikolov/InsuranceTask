using Claims.Models;

namespace Claims.DTOs;

public sealed record CreateCoverRequest(DateTime StartDate, DateTime EndDate, CoverType Type);

public sealed record CoverResponse(string Id, DateTime StartDate, DateTime EndDate, CoverType Type, decimal Premium)
{
    public static CoverResponse From(Cover cover) =>
        new(cover.Id, cover.StartDate, cover.EndDate, cover.Type, cover.Premium);
}
