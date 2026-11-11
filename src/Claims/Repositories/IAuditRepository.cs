namespace Claims.Repositories;

public interface IAuditRepository
{
    Task AuditClaimAsync(string claimId, string httpRequestType, CancellationToken cancellationToken);
    Task AuditCoverAsync(string coverId, string httpRequestType, CancellationToken cancellationToken);
}
