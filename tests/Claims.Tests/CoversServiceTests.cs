using Claims.Auditing;
using Claims.DTOs;
using Claims.Models;
using Claims.Repositories;
using Claims.Services;
using Xunit;

namespace Claims.Tests;

public class CoversServiceTests
{
    private static readonly DateTime Today = new(2026, 6, 15);
    private readonly FakeCoversRepository _repository = new();
    private readonly FakeAuditQueue _auditQueue = new();
    private readonly CoversService _service;

    public CoversServiceTests()
    {
        _service = new CoversService(_repository, _auditQueue, new CoverValidator(new FixedTimeProvider(Today)), new PremiumCalculator());
    }

    [Fact]
    public async Task Create_saves_cover_and_queues_audit()
    {
        var result = await _service.CreateAsync(new CreateCoverRequest(Today, Today.AddDays(10), CoverType.Yacht), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(_repository.Covers);
        Assert.Equal((AuditEntity.Cover, result.Value!.Id, "POST"), Assert.Single(_auditQueue.Messages));
    }

    [Fact]
    public async Task Dates_are_saved_as_utc_days()
    {
        await _service.CreateAsync(new CreateCoverRequest(Today.AddHours(15), Today.AddDays(10), CoverType.Yacht), CancellationToken.None);

        var cover = Assert.Single(_repository.Covers);
        Assert.Equal(Today, cover.StartDate);
        Assert.Equal(DateTimeKind.Utc, cover.StartDate.Kind);
    }

    [Fact]
    public async Task Invalid_cover_is_not_saved_or_audited()
    {
        var result = await _service.CreateAsync(new CreateCoverRequest(Today.AddDays(-1), Today.AddDays(10), CoverType.Yacht), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Empty(_repository.Covers);
        Assert.Empty(_auditQueue.Messages);
    }

    [Fact]
    public async Task Deleting_missing_cover_is_not_audited()
    {
        Assert.False(await _service.DeleteAsync("missing", CancellationToken.None));
        Assert.Empty(_auditQueue.Messages);
    }
}
