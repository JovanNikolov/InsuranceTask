using Claims.Auditing;
using Claims.Data;
using Claims.Models;

namespace Claims.Repositories;

public sealed class AuditRepository(AuditContext context) : IAuditRepository
{
    public async Task AddAsync(AuditMessage message, CancellationToken cancellationToken)
    {
        if (message.Entity == AuditEntity.Claim)
            context.ClaimAudits.Add(new ClaimAudit { ClaimId = message.EntityId, HttpRequestType = message.HttpRequestType, Created = message.Created });
        else
            context.CoverAudits.Add(new CoverAudit { CoverId = message.EntityId, HttpRequestType = message.HttpRequestType, Created = message.Created });

        await context.SaveChangesAsync(cancellationToken);
    }
}
