using MediatR;
using WakeCap.Application.Contacts;
using WakeCap.Domain;

namespace WakeCap.Application.Queries;

public sealed record GetWorkerZoneImportsQuery(
DateTime? DateFrom,
DateTime? DateTo,
WorkerZoneImportStatus? Status
) : IRequest<IEnumerable<WorkerZoneImportLogDetails>>;