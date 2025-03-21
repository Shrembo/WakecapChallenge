using System.Text.Json.Serialization;

namespace WakeCap.Infrastructure.CsvParsers;

public sealed class ValidationResult<T>
{
    public T? Data { get; init; }
    public Dictionary<string, string>? Error { get; set; }
    [JsonIgnore]
    public bool Succeeded => Data != null && Error == null;

    public static ValidationResult<T> Failed(string message)
    {
        return new ValidationResult<T>
        {
            Data = default,
            Error = new Dictionary<string, string>
            {
                ["file"] = message
            }
        };
    }
}