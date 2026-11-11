using Claims.DTOs;
using Claims.Models;

namespace Claims.Services;

public interface ICoversService
{
    Task<IReadOnlyList<CoverResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<CoverResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<ServiceResult<CoverResponse>> CreateAsync(CreateCoverRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
    ServiceResult<decimal> ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType);
}
