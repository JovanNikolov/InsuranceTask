using Claims.Data;
using Claims.Models;

namespace Claims.Repositories;

public sealed class AuditRepository(AuditContext context) : IAuditRepository
{
    public async Task AuditClaimAsync(string claimId, string httpRequestType, CancellationToken cancellationToken)
    {
        context.ClaimAudits.Add(new ClaimAudit { ClaimId = claimId, HttpRequestType = httpRequestType, Created = DateTime.Now });
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AuditCoverAsync(string coverId, string httpRequestType, CancellationToken cancellationToken)
    {
        context.CoverAudits.Add(new CoverAudit { CoverId = coverId, HttpRequestType = httpRequestType, Created = DateTime.Now });
        await context.SaveChangesAsync(cancellationToken);
    }
}
