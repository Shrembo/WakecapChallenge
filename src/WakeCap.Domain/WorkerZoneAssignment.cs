namespace WakeCap.Domain;

public sealed class WorkerZoneAssignment
{
    public int Id { get; private set; }
    public int WorkerId { get; set; }
    public int ZoneId { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public Worker Worker { get; private set; } = null!;
    public Zone Zone { get; private set; } = null!;
    private WorkerZoneAssignment() { }
    private WorkerZoneAssignment(int workerId, int zoneId, DateOnly effectiveDate)
    {
        WorkerId = workerId;
        ZoneId = zoneId;
        EffectiveDate = effectiveDate;
    }

    public static WorkerZoneAssignment Create(int workerId, int zoneId, DateOnly effectiveDate)
    {
        return new WorkerZoneAssignment(workerId, zoneId, effectiveDate);
    }
}