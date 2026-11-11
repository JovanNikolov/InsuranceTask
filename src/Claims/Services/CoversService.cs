using Claims.DTOs;
using Claims.Models;
using Claims.Repositories;

namespace Claims.Services;

public sealed class CoversService(
    ICoversRepository coversRepository,
    IAuditRepository auditRepository,
    CoverValidator validator,
    PremiumCalculator premiumCalculator) : ICoversService
{
    public async Task<IReadOnlyList<CoverResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var covers = await coversRepository.GetAllAsync(cancellationToken);
        return covers.Select(CoverResponse.From).ToList();
    }

    public async Task<CoverResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var cover = await coversRepository.GetByIdAsync(id, cancellationToken);
        return cover is null ? null : CoverResponse.From(cover);
    }

    public async Task<ServiceResult<CoverResponse>> CreateAsync(CreateCoverRequest request, CancellationToken cancellationToken)
    {
        var cover = new Cover
        {
            Id = Guid.NewGuid().ToString(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Type = request.Type
        };

        var errors = validator.Validate(cover);
        if (errors.Count > 0)
            return ServiceResult<CoverResponse>.Failure(errors);

        cover.Premium = premiumCalculator.Calculate(cover.StartDate, cover.EndDate, cover.Type);
        await coversRepository.AddAsync(cover, cancellationToken);
        await auditRepository.AuditCoverAsync(cover.Id, "POST", cancellationToken);
        return ServiceResult<CoverResponse>.Success(CoverResponse.From(cover));
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var deleted = await coversRepository.DeleteAsync(id, cancellationToken);
        if (deleted)
            await auditRepository.AuditCoverAsync(id, "DELETE", cancellationToken);
        return deleted;
    }

    public ServiceResult<decimal> ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        var errors = validator.Validate(new Cover { StartDate = startDate, EndDate = endDate, Type = coverType });
        if (errors.Count > 0)
            return ServiceResult<decimal>.Failure(errors);

        return ServiceResult<decimal>.Success(premiumCalculator.Calculate(startDate, endDate, coverType));
    }
}
