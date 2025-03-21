using System.Text.Json.Serialization;

namespace WakeCap.Application.Contacts;

public sealed record ValidationResult
{
    public WorkerAssignmentParameters? Data { get; set; }
    public Dictionary<string, string>? Error { get; set; }

    [JsonIgnore]
    public bool Succeeded => Data != null && Error == null;
}