using Claims.Repositories;

namespace Claims.Auditing;

public sealed class AuditWorker(AuditQueue queue, IServiceScopeFactory scopeFactory, ILogger<AuditWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in queue.ReadAllAsync())
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var repository = scope.ServiceProvider.GetRequiredService<IAuditRepository>();
                await repository.AddAsync(message, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not save audit {HttpRequestType} for {Entity} {EntityId}",
                    message.HttpRequestType, message.Entity, message.EntityId);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        queue.Complete();
        await base.StopAsync(cancellationToken);
    }
}
