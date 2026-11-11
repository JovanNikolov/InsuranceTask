using Claims.DTOs;

namespace Claims.Services;

public interface IClaimsService
{
    Task<IReadOnlyList<ClaimResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<ClaimResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<ServiceResult<ClaimResponse>> CreateAsync(CreateClaimRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}
