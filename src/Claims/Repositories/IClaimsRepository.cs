using Claims.Models;

namespace Claims.Repositories;

public interface IClaimsRepository
{
    Task<IReadOnlyList<Claim>> GetAllAsync(CancellationToken cancellationToken);
    Task<Claim?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task AddAsync(Claim claim, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}
