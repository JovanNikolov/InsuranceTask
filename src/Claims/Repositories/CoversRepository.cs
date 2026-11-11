using Claims.Data;
using Claims.Models;
using Microsoft.EntityFrameworkCore;

namespace Claims.Repositories;

public sealed class CoversRepository(ClaimsContext context) : ICoversRepository
{
    public async Task<IReadOnlyList<Cover>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Covers.ToListAsync(cancellationToken);

    public Task<Cover?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        context.Covers.SingleOrDefaultAsync(cover => cover.Id == id, cancellationToken);

    public async Task AddAsync(Cover cover, CancellationToken cancellationToken)
    {
        context.Covers.Add(cover);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var cover = await GetByIdAsync(id, cancellationToken);
        if (cover is null)
            return false;

        context.Covers.Remove(cover);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
