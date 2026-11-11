using System.Threading.Channels;

namespace Claims.Auditing;

public sealed class AuditQueue(TimeProvider timeProvider) : IAuditQueue
{
    private readonly Channel<AuditMessage> _channel = Channel.CreateUnbounded<AuditMessage>(new UnboundedChannelOptions { SingleReader = true });

    public void Enqueue(AuditEntity entity, string entityId, string httpRequestType)
    {
        _channel.Writer.TryWrite(new AuditMessage(entity, entityId, httpRequestType, timeProvider.GetUtcNow().UtcDateTime));
    }

    public IAsyncEnumerable<AuditMessage> ReadAllAsync() => _channel.Reader.ReadAllAsync();

    public void Complete() => _channel.Writer.TryComplete();
}
