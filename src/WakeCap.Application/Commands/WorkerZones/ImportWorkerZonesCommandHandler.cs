using System.Globalization;
using MediatR;
using WakeCap.Application.Contacts;
using WakeCap.Application.Contacts.Stores;
using WakeCap.Domain;

namespace WakeCap.Application.Commands.WorkerZones;

public sealed class ImportWorkerZonesCommandHandler(
    IWorkersStore workersStore,
    IZonesStore zonesStore,
    IWorkerZonesStore workerZonesStore) : IRequestHandler<ImportWorkerZonesCommand, IEnumerable<ValidationResult>>
{
    private readonly IWorkersStore _workersStore = workersStore;
    private readonly IZonesStore _zonesStore = zonesStore;
    private readonly IWorkerZonesStore _workerZonesStore = workerZonesStore;

    public async Task<IEnumerable<ValidationResult>> Handle(ImportWorkerZonesCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await ParseAndPreValidateAsync(request.FileStream, cancellationToken);
        if (validationResult.Any(a => a.Succeeded == true))
        {
            await RunPostValidationAndSaveAsync(validationResult, cancellationToken);
        }

        return validationResult.Where(a => a.Succeeded == false);
    }


    private static async Task<IEnumerable<ValidationResult>> ParseAndPreValidateAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return [ new ValidationResult { Error = new() { ["file"] = "File is empty or missing headers." } } ];
        }

        var headers = headerLine.Split(',');
        if (headers.Length != 3)
        {
            return [ new ValidationResult { Error = new() { ["file"] = "CSV must contain exactly 3 columns." } } ];
        }

        if (!headers.SequenceEqual(["Worker Code", "Zone Code", "Assignment Date"], StringComparer.OrdinalIgnoreCase))
        {
            return [ new ValidationResult { Error = new() { ["file"] = "CSV headers must be: Worker Code, Zone Code, Assignment Date" } } ];
        }

        var results = new List<ValidationResult>();
        int rowCount = 0;
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            rowCount++;
            if (rowCount > 50000)
            {
                return [new ValidationResult { Error = new() { ["file"] = "CSV file exceeds the 50,000 row limit." } }];
            }

            var columns = line.Split(',');
            if (columns.Length != 3)
            {
                results.Add(new ValidationResult { Error = new() { ["row"] = $"Invalid column count at row {rowCount}" } });
                continue;
            }

            var (workerCode, zoneCode, assignmentDateStr) = (columns[0].Trim(), columns[1].Trim(), columns[2].Trim());
            var rowErrors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(workerCode) || workerCode.Length > 10)
            {
                rowErrors["worker_code"] = "Missing or exceeds 10 characters.";
            }

            if (string.IsNullOrWhiteSpace(zoneCode) || zoneCode.Length > 10)
            {
                rowErrors["zone_code"] = "Missing or exceeds 10 characters.";
            }

            if (!DateOnly.TryParseExact(assignmentDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                rowErrors["assignment_date"] = "Invalid date format. Expected format: yyyy-MM-dd.";
            }

            var data = new WorkerAssignmentParameters(workerCode, zoneCode, assignmentDateStr);
            results.Add(new ValidationResult
            {
                Data = data,
                Error = rowErrors.Count > 0 ? rowErrors : null
            });
        }

        return results;
    }

    private async Task RunPostValidationAndSaveAsync(IEnumerable<ValidationResult> results, CancellationToken cancellationToken)
    {
        var rows = results.Where(r => r.Succeeded).Select(r => r.Data!).ToList();

        var workers = await _workersStore.List(rows.Select(r => r.WorkerCode).Distinct().ToHashSet(), cancellationToken);
        var zones = await _zonesStore.List(rows.Select(r => r.ZoneCode).Distinct().ToHashSet(), cancellationToken);

        var workerDatePairs = rows
            .Where(r => workers.ContainsKey(r.WorkerCode))
            .Select(r => (workers[r.WorkerCode].Id, DateOnly.ParseExact(r.AssignmentDate, "yyyy-MM-dd")))
            .Distinct()
            .ToList();

        var existingAssignments = await _workerZonesStore.List(workerDatePairs, cancellationToken);

        var duplicateSet = new HashSet<(string worker, string zone, string date)>();

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
                var date = DateOnly.ParseExact(row.AssignmentDate, "yyyy-MM-dd");
                return WorkerZoneAssignment.Create(worker.Id, zone.Id, date);
            })
            .ToList();

        await _workerZonesStore.Add(validAssignments, cancellationToken);
    }

    private static void ValidateWorkerExistence(IEnumerable<ValidationResult> results, Dictionary<string, Worker> workers)
    {
        foreach (var r in results.Where(r => r.Succeeded))
        {
            if (!workers.ContainsKey(r.Data!.WorkerCode))
                r.Error = new() { ["worker_code"] = "Worker does not exist" };
        }
    }

    private static void ValidateZoneExistence(IEnumerable<ValidationResult> results, Dictionary<string, Zone> zones)
    {
        foreach (var r in results.Where(r => r.Succeeded))
        {
            if (!zones.ContainsKey(r.Data!.ZoneCode))
                r.Error = new() { ["zone_code"] = "Zone does not exist" };
        }
    }
    
    private static void ValidateDuplicatesInFile(IEnumerable<ValidationResult> results, HashSet<(string, string, string)> duplicateSet)
    {
        foreach (var r in results.Where(r => r.Succeeded))
        {
            var d = r.Data!;
            var key = (d.WorkerCode, d.ZoneCode, d.AssignmentDate);
            if (!duplicateSet.Add(key))
                r.Error = new() { ["rowError"] = "Duplicate row in file" };
        }
    }

    private static void ValidateMultiZoneAssignments(IEnumerable<ValidationResult> results, HashSet<(string worker, string zone, string date)> allKeys)
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

    private static void ValidateDbConflicts(IEnumerable<ValidationResult> results, Dictionary<string, Worker> workers, Dictionary<string, Zone> zones, List<WorkerZoneAssignment> existingAssignments)
    {
        foreach (var r in results.Where(r => r.Succeeded))
        {
            var d = r.Data!;
            var date = DateOnly.ParseExact(d.AssignmentDate, "yyyy-MM-dd");
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