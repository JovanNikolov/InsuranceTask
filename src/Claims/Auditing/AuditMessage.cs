namespace Claims.Auditing;

public enum AuditEntity
{
    Claim,
    Cover
}

public sealed record AuditMessage(AuditEntity Entity, string EntityId, string HttpRequestType, DateTime Created);
