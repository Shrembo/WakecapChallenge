using Microsoft.EntityFrameworkCore;
using WakeCap.Application.Contacts.Stores;
using WakeCap.Domain;

namespace WakeCap.Persistence.EfCore.Stores;

internal sealed class WorkerZonesStore(IDbContextFactory<WakeCapDbContext> contextFactory) : IWorkerZonesStore
{
    readonly IDbContextFactory<WakeCapDbContext> _contextFactory = contextFactory;

    public async Task Add(List<WorkerZoneAssignment> assignments, CancellationToken cancellationToken)
    {
        if (assignments == null || assignments.Count == 0)
            return;

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        await context.Set<WorkerZoneAssignment>().AddRangeAsync(assignments, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<WorkerZoneAssignment>> List(IEnumerable<(int workerId, DateOnly date)> workerDatePairs, CancellationToken cancellationToken)
    {
        if (workerDatePairs == null || !workerDatePairs.Any())
            return [];

        var (workerIds, dates) = (
            workerDatePairs.Select(k => k.workerId).ToHashSet(),
            workerDatePairs.Select(k => k.date).ToHashSet()
        );

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Set<WorkerZoneAssignment>()
            .AsNoTracking()
            .Where(wza => workerIds.Contains(wza.WorkerId) && dates.Contains(wza.EffectiveDate))
            .ToListAsync(cancellationToken);
    }
}