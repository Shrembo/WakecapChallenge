namespace WakeCap.Infrastructure.CsvParsers;

public sealed class CsvParserOptions
{
    public int MaxRows { get; init; } = 50000;
    public string[] ExpectedHeaders { get; init; } = [];
    public bool ValidateHeader { get; init; } = true;
}