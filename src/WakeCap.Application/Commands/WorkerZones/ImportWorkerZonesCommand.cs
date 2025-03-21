using MediatR;
using WakeCap.Application.Contacts;
using WakeCap.Infrastructure.CsvParsers;

namespace WakeCap.Application.Commands.WorkerZones;

public sealed record ImportWorkerZonesCommand(
    Stream FileStream,
    string FileName)
    : IRequest<IEnumerable<ValidationResult<WorkerAssignmentParameters>>>;