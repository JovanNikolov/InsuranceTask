using Claims.Auditing;
using Claims.DTOs;
using Claims.Models;
using Claims.Services;
using Xunit;

namespace Claims.Tests;

public class ClaimsServiceTests
{
    private readonly FakeClaimsRepository _claims = new();
    private readonly FakeCoversRepository _covers = new();
    private readonly FakeAuditQueue _auditQueue = new();
    private readonly ClaimsService _service;

    public ClaimsServiceTests()
    {
        _covers.Covers.Add(new Cover { Id = "cover", StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31) });
        _service = new ClaimsService(_claims, _covers, _auditQueue, new ClaimValidator());
    }

    [Fact]
    public async Task Create_saves_claim_and_queues_audit()
    {
        var result = await _service.CreateAsync(new CreateClaimRequest("cover", new DateTime(2026, 6, 1), "Hull", ClaimType.Collision, 500), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(_claims.Claims);
        Assert.Equal((AuditEntity.Claim, result.Value!.Id, "POST"), Assert.Single(_auditQueue.Messages));
    }

    [Fact]
    public async Task Claim_for_missing_cover_is_not_saved_or_audited()
    {
        var result = await _service.CreateAsync(new CreateClaimRequest("missing", new DateTime(2026, 6, 1), "Hull", ClaimType.Collision, 500), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Empty(_claims.Claims);
        Assert.Empty(_auditQueue.Messages);
    }

    [Fact]
    public async Task Delete_queues_audit()
    {
        _claims.Claims.Add(new Claim { Id = "claim" });

        Assert.True(await _service.DeleteAsync("claim", CancellationToken.None));
        Assert.Equal((AuditEntity.Claim, "claim", "DELETE"), Assert.Single(_auditQueue.Messages));
    }
}
