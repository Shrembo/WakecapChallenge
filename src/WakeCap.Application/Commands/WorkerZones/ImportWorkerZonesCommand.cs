using MediatR;
using WakeCap.Application.Contacts;

namespace WakeCap.Application.Commands.WorkerZones;

public sealed record ImportWorkerZonesCommand(
    Stream FileStream,
    string FileName)
    : IRequest<IEnumerable<ValidationResult>>;