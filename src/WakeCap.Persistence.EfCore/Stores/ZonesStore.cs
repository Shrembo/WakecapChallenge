using Microsoft.EntityFrameworkCore;
using WakeCap.Application.Contacts.Stores;
using WakeCap.Domain;

namespace WakeCap.Persistence.EfCore.Stores;

internal sealed class ZonesStore(IDbContextFactory<WakeCapDbContext> contextFactory) : IZonesStore
{
    private readonly IDbContextFactory<WakeCapDbContext> _contextFactory = contextFactory;

    public async Task<Dictionary<string, Zone>> List(IEnumerable<string> codes, CancellationToken cancellationToken = default)
    {
        if (codes == null || !codes.Any())
        {
            return [];
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Set<Zone>()
            .AsQueryable()
            .AsNoTracking()
            .Where(z => codes.Contains(z.Code))
            .ToDictionaryAsync(z => z.Code, cancellationToken);
    }
}