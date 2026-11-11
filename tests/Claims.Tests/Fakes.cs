using Claims.Auditing;
using Claims.Models;
using Claims.Repositories;

namespace Claims.Tests;

internal sealed class FakeCoversRepository : ICoversRepository
{
    public List<Cover> Covers { get; } = [];

    public Task<IReadOnlyList<Cover>> GetAllAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Cover>>(Covers);

    public Task<Cover?> GetByIdAsync(string id, CancellationToken cancellationToken) => Task.FromResult(Covers.FirstOrDefault(c => c.Id == id));

    public Task AddAsync(Cover cover, CancellationToken cancellationToken)
    {
        Covers.Add(cover);
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken) => Task.FromResult(Covers.RemoveAll(c => c.Id == id) > 0);
}

internal sealed class FakeClaimsRepository : IClaimsRepository
{
    public List<Claim> Claims { get; } = [];

    public Task<IReadOnlyList<Claim>> GetAllAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Claim>>(Claims);

    public Task<Claim?> GetByIdAsync(string id, CancellationToken cancellationToken) => Task.FromResult(Claims.FirstOrDefault(c => c.Id == id));

    public Task AddAsync(Claim claim, CancellationToken cancellationToken)
    {
        Claims.Add(claim);
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken) => Task.FromResult(Claims.RemoveAll(c => c.Id == id) > 0);
}

internal sealed class FakeAuditQueue : IAuditQueue
{
    public List<(AuditEntity, string, string)> Messages { get; } = [];

    public void Enqueue(AuditEntity entity, string entityId, string httpRequestType) => Messages.Add((entity, entityId, httpRequestType));
}
