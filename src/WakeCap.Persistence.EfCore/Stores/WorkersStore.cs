using Microsoft.EntityFrameworkCore;
using WakeCap.Application.Contacts.Stores;
using WakeCap.Domain;

namespace WakeCap.Persistence.EfCore.Stores;

internal sealed class WorkersStore(IDbContextFactory<WakeCapDbContext> contextFactory) : IWorkersStore
{
    readonly IDbContextFactory<WakeCapDbContext> _contextFactory = contextFactory;
    
    public async Task<Dictionary<string, Worker>> List(IEnumerable<string> codes, CancellationToken cancellationToken)
    {
        if (codes == null || !codes.Any())
        {
            return [];
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Set<Worker>()
            .AsQueryable()
            .AsNoTracking()
            .Where(w => codes.Contains(w.Code))
            .ToDictionaryAsync(w => w.Code, cancellationToken);
    }
}