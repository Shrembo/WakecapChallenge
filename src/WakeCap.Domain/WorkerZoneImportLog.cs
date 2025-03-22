namespace WakeCap.Domain;

public sealed class WorkerZoneImportLog
{
    public int Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string FileName { get; private set; } = null!;
    public WorkerZoneImportStatus Status { get; private set; }
    public string? ErrorSummary { get; private set; }

    private WorkerZoneImportLog() { }
    private WorkerZoneImportLog(DateTime createdAt, string fileName, WorkerZoneImportStatus status, string? errorSummary)
    {
        CreatedAt = createdAt;
        FileName = fileName;
        Status = status;
        ErrorSummary = errorSummary;
    }

    public static WorkerZoneImportLog Create(string fileName, WorkerZoneImportStatus status, string? errorSummary = null)
    {
        return new WorkerZoneImportLog(DateTime.UtcNow, fileName, status, errorSummary);
    }
}
