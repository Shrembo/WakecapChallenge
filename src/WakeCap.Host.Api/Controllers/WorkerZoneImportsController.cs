using MediatR;
using Microsoft.AspNetCore.Mvc;
using WakeCap.Application.Commands.WorkerZones;
using WakeCap.Application.Queries;
using WakeCap.Domain;

namespace WakeCap.Host.Api.Controllers;

[ApiController]
[Route("worker-zone-imports")]
public sealed class WorkerZoneImportsController(ISender mediator) : ControllerBase
{
    readonly ISender _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] WorkerZoneImportStatus? status,
        CancellationToken cancellationToken)
    {
        var query = new GetWorkerZoneImportsQuery(from, to, status);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Import([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        using var stream = file.OpenReadStream();
        var command = new ImportWorkerZonesCommand(stream, file.FileName);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}