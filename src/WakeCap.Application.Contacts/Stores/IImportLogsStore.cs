using WakeCap.Domain;

namespace WakeCap.Application.Contacts.Stores;

public interface IImportLogsStore
{
    Task Add(WorkerZoneImportLog log, CancellationToken cancellationToken);
    Task<List<WorkerZoneImportLog>> List(DateTime? from, DateTime? to, WorkerZoneImportStatus? status, CancellationToken cancellationToken);
}