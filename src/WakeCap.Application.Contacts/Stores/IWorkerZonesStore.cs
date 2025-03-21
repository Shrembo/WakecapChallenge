using WakeCap.Domain;

namespace WakeCap.Application.Contacts.Stores;

public interface IWorkerZonesStore
{
    Task<List<WorkerZoneAssignment>> List(IEnumerable<(int workerId, DateOnly date)> workerDatePairs, CancellationToken cancellationToken);
    Task Add(List<WorkerZoneAssignment> assignments, CancellationToken cancellationToken);
}