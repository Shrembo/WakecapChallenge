using Microsoft.EntityFrameworkCore;
using WakeCap.Application.Contacts.Stores;
using WakeCap.Domain;

namespace WakeCap.Persistence.EfCore.Stores;

internal sealed class ImportLogsStore(IDbContextFactory<WakeCapDbContext> factory) : IImportLogsStore
{
    private readonly IDbContextFactory<WakeCapDbContext> _factory = factory;

    public async Task Add(WorkerZoneImportLog log, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        await context.Set<WorkerZoneImportLog>().AddAsync(log, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<WorkerZoneImportLog>> List(DateTime? from, DateTime? to, WorkerZoneImportStatus? status, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        var query = context.Set<WorkerZoneImportLog>().AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= to.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}