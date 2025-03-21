namespace WakeCap.Application.Contacts;

public sealed record WorkerAssignmentParameters(string WorkerCode, string ZoneCode, DateOnly AssignmentDate);