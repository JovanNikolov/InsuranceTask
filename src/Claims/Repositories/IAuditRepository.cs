using Claims.Auditing;

namespace Claims.Repositories;

public interface IAuditRepository
{
    Task AddAsync(AuditMessage message, CancellationToken cancellationToken);
}
