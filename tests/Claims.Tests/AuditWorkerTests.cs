using Claims.Auditing;
using Claims.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Claims.Tests;

public class AuditWorkerTests
{
    [Fact]
    public async Task Worker_saves_queued_audits_and_keeps_going_after_a_failure()
    {
        var repository = new FakeAuditRepository();
        var provider = new ServiceCollection().AddSingleton<IAuditRepository>(repository).BuildServiceProvider();
        var queue = new AuditQueue(new FixedTimeProvider(new DateTime(2026, 6, 15)));
        var worker = new AuditWorker(queue, provider.GetRequiredService<IServiceScopeFactory>(), NullLogger<AuditWorker>.Instance);

        await worker.StartAsync(CancellationToken.None);
        queue.Enqueue(AuditEntity.Cover, "fails", "POST");
        queue.Enqueue(AuditEntity.Claim, "1", "DELETE");
        await worker.StopAsync(CancellationToken.None);

        var saved = Assert.Single(repository.Saved);
        Assert.Equal(new AuditMessage(AuditEntity.Claim, "1", "DELETE", new DateTime(2026, 6, 15)), saved);
    }

    private sealed class FakeAuditRepository : IAuditRepository
    {
        public List<AuditMessage> Saved { get; } = [];

        public Task AddAsync(AuditMessage message, CancellationToken cancellationToken)
        {
            if (message.EntityId == "fails")
                throw new InvalidOperationException();

            Saved.Add(message);
            return Task.CompletedTask;
        }
    }
}
