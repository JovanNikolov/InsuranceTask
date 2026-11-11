namespace Claims.Auditing;

public interface IAuditQueue
{
    void Enqueue(AuditEntity entity, string entityId, string httpRequestType);
}
