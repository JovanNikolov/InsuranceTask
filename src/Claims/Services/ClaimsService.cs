using Claims.DTOs;
using Claims.Models;
using Claims.Repositories;

namespace Claims.Services;

public sealed class ClaimsService(
    IClaimsRepository claimsRepository,
    ICoversRepository coversRepository,
    IAuditRepository auditRepository,
    ClaimValidator validator) : IClaimsService
{
    public async Task<IReadOnlyList<ClaimResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var claims = await claimsRepository.GetAllAsync(cancellationToken);
        return claims.Select(ClaimResponse.From).ToList();
    }

    public async Task<ClaimResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var claim = await claimsRepository.GetByIdAsync(id, cancellationToken);
        return claim is null ? null : ClaimResponse.From(claim);
    }

    public async Task<ServiceResult<ClaimResponse>> CreateAsync(CreateClaimRequest request, CancellationToken cancellationToken)
    {
        var claim = new Claim
        {
            Id = Guid.NewGuid().ToString(),
            CoverId = request.CoverId,
            Created = request.Created,
            Name = request.Name,
            Type = request.Type,
            DamageCost = request.DamageCost
        };

        var cover = await coversRepository.GetByIdAsync(claim.CoverId, cancellationToken);
        var errors = validator.Validate(claim, cover);
        if (errors.Count > 0)
            return ServiceResult<ClaimResponse>.Failure(errors);

        await claimsRepository.AddAsync(claim, cancellationToken);
        await auditRepository.AuditClaimAsync(claim.Id, "POST", cancellationToken);
        return ServiceResult<ClaimResponse>.Success(ClaimResponse.From(claim));
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var deleted = await claimsRepository.DeleteAsync(id, cancellationToken);
        if (deleted)
            await auditRepository.AuditClaimAsync(id, "DELETE", cancellationToken);
        return deleted;
    }
}
