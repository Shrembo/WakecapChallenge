using MediatR;
using WakeCap.Application.Contacts;
using WakeCap.Application.Contacts.Stores;

namespace WakeCap.Application.Queries;

public sealed class GetWorkerZoneImportsQueryHandler(IImportLogsStore importLogStore) : IRequestHandler<GetWorkerZoneImportsQuery, IEnumerable<WorkerZoneImportLogDetails>>
{
    public async Task<IEnumerable<WorkerZoneImportLogDetails>> Handle(GetWorkerZoneImportsQuery query, CancellationToken cancellationToken)
    {
        var logs = await importLogStore.List(query.DateFrom, query.DateTo, query.Status, cancellationToken);

        if (logs == null)
        {
            return [];
        }

        return [.. logs.Select(log => new WorkerZoneImportLogDetails(
            log.Id,
            log.CreatedAt,
            log.FileName,
            log.Status,
            log.ErrorSummary
        ))];
    }
}
