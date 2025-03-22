using WakeCap.Domain;

namespace WakeCap.Application.Contacts;

public sealed record WorkerZoneImportLogDetails(
int Id,
DateTime CreatedAt,
string FileName,
WorkerZoneImportStatus Status,
string? ErrorSummary);
