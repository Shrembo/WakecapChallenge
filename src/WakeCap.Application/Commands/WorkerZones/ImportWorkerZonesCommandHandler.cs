using System.Globalization;
using MediatR;
using WakeCap.Application.Contacts;
using WakeCap.Application.Contacts.Stores;
using WakeCap.Domain;
using WakeCap.Infrastructure.CsvParsers;

namespace WakeCap.Application.Commands.WorkerZones;

public sealed class ImportWorkerZonesCommandHandler(
    ICsvParser csvParser,
    IWorkersStore workersStore,
    IZonesStore zonesStore,
    IWorkerZonesStore workerZonesStore) : IRequestHandler<ImportWorkerZonesCommand, IEnumerable<ValidationResult<WorkerAssignmentParameters>>>
{
    readonly ICsvParser _csvParser = csvParser;
    readonly IWorkersStore _workersStore = workersStore;
    readonly IZonesStore _zonesStore = zonesStore;
    readonly IWorkerZonesStore _workerZonesStore = workerZonesStore;

    public async Task<IEnumerable<ValidationResult<WorkerAssignmentParameters>>> Handle(ImportWorkerZonesCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await ParseAndPreValidateAsync(request.FileStream, cancellationToken);
        if (validationResult.Any(a => a.Succeeded == true))
        {
            await RunPostValidationAndSaveAsync(validationResult, cancellationToken);
        }

        return validationResult.Where(a => a.Succeeded == false);
    }



    private async Task<IEnumerable<ValidationResult<WorkerAssignmentParameters>>> ParseAndPreValidateAsync(Stream stream, CancellationToken cancellationToken)
    {
        return await _csvParser.ParseAndPreValidateAsync(
            stream,
            [ "Worker Code", "Zone Code", "Assignment Date" ],
            columns =>
            {
                var (workerCode, zoneCode, dateStr) = (columns[0].Trim(), columns[1].Trim(), columns[2].Trim());
                var errors = new Dictionary<string, string>();
                if (string.IsNullOrWhiteSpace(workerCode) || workerCode.Length > 10)
                {
                    errors["worker_code"] = "Missing or exceeds 10 characters.";
                }

                if (string.IsNullOrWhiteSpace(zoneCode) || zoneCode.Length > 10)
                {
                    errors["zone_code"] = "Missing or exceeds 10 characters.";
                }

                if (!DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var assignmentDate))
                {
                    errors["assignment_date"] = "Invalid date format. Expected format: yyyy-MM-dd.";
                }

                return new ValidationResult<WorkerAssignmentParameters>
                {
                    Data = new WorkerAssignmentParameters(workerCode, zoneCode, assignmentDate),
                    Error = errors.Count > 0 ? errors : null
                };
            }, cancellationToken);
    }

    private async Task RunPostValidationAndSaveAsync(IEnumerable<ValidationResult<WorkerAssignmentParameters>> results, CancellationToken cancellationToken)
    {
        var rows = results.Where(r => r.Succeeded).Select(r => r.Data!).ToList();

        var workers = await _workersStore.List(rows.Select(r => r.WorkerCode).Distinct().ToHashSet(), cancellationToken);
        var zones = await _zonesStore.List(rows.Select(r => r.ZoneCode).Distinct().ToHashSet(), cancellationToken);

        var workerDatePairs = rows
            .Where(r => workers.ContainsKey(r.WorkerCode))
            .Select(r => (workers[r.WorkerCode].Id, r.AssignmentDate))
            .Distinct()
            .ToList();

        var existingAssignments = await _workerZonesStore.List(workerDatePairs, cancellationToken);

        var duplicateSet = new HashSet<(string worker, string zone, DateOnly date)>();

        ValidateWorkerExistence(results, workers);
        ValidateZoneExistence(results, zones);
        ValidateDuplicatesInFile(results, duplicateSet);
        ValidateMultiZoneAssignments(results, duplicateSet);
        ValidateDbConflicts(results, workers, zones, existingAssignments);


        if (results.Any(r => r.Succeeded == false)) return;

        var validAssignments = results
            .Where(r => r.Succeeded)
            .Select(r =>
            {
                var row = r.Data!;
                var worker = workers[row.WorkerCode];
                var zone = zones[row.ZoneCode];
                var date = row.AssignmentDate;
                return WorkerZoneAssignment.Create(worker.Id, zone.Id, date);
            })
            .ToList();

        await _workerZonesStore.Add(validAssignments, cancellationToken);
    }

    private static void ValidateWorkerExistence(IEnumerable<ValidationResult<WorkerAssignmentParameters>> results, Dictionary<string, Worker> workers)
    {
        foreach (var r in results.Where(r => r.Succeeded))
        {
            if (!workers.ContainsKey(r.Data!.WorkerCode))
                r.Error = new() { ["worker_code"] = "Worker does not exist" };
        }
    }

    private static void ValidateZoneExistence(IEnumerable<ValidationResult<WorkerAssignmentParameters>> results, Dictionary<string, Zone> zones)
    {
        foreach (var r in results.Where(r => r.Succeeded))
        {
            if (!zones.ContainsKey(r.Data!.ZoneCode))
                r.Error = new() { ["zone_code"] = "Zone does not exist" };
        }
    }
    
    private static void ValidateDuplicatesInFile(IEnumerable<ValidationResult<WorkerAssignmentParameters>> results, HashSet<(string, string, DateOnly)> duplicateSet)
    {
        foreach (var r in results.Where(r => r.Succeeded))
        {
            var d = r.Data!;
            var key = (d.WorkerCode, d.ZoneCode, d.AssignmentDate);
            if (!duplicateSet.Add(key))
                r.Error = new() { ["rowError"] = "Duplicate row in file" };
        }
    }

    private static void ValidateMultiZoneAssignments(IEnumerable<ValidationResult<WorkerAssignmentParameters>> results, HashSet<(string worker, string zone, DateOnly date)> allKeys)
    {
        foreach (var r in results.Where(r => r.Succeeded))
        {
            var d = r.Data!;
            bool hasConflict = allKeys.Any(k =>
                k.worker == d.WorkerCode &&
                k.date == d.AssignmentDate &&
                k.zone != d.ZoneCode);

            if (hasConflict)
                r.Error = new() { ["rowError"] = "Worker assigned to multiple zones on the same date" };
        }
    }

    private static void ValidateDbConflicts(IEnumerable<ValidationResult<WorkerAssignmentParameters>> results, Dictionary<string, Worker> workers, Dictionary<string, Zone> zones, List<WorkerZoneAssignment> existingAssignments)
    {
        foreach (var r in results.Where(r => r.Succeeded))
        {
            var d = r.Data!;
            var date = d.AssignmentDate;
            var worker = workers[d.WorkerCode];
            var zone = zones[d.ZoneCode];

            var conflicts = existingAssignments
                .Where(x => x.WorkerId == worker.Id && x.EffectiveDate == date)
                .ToList();

            if (conflicts.Any(x => x.ZoneId == zone.Id))
            {
                r.Error = new() { ["rowError"] = "Assignment already exists in worker_zone_assignment table" };
            }
            else if (conflicts.Any(x => x.ZoneId != zone.Id))
            {
                r.Error = new() { ["rowError"] = "If inserted, it would create a conflicting assignment for the same worker on the same date" };
            }
        }
    }
}