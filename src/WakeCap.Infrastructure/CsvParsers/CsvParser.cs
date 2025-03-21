namespace WakeCap.Infrastructure.CsvParsers;

public sealed class CsvParser : ICsvParser
{
    public async Task<IEnumerable<ValidationResult<T>>> ParseAndPreValidateAsync<T>(
        Stream stream,
        string[] expectedHeaders,
        Func<string[], ValidationResult<T>> mapAndValidate,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return [ValidationResult<T>.Failed("File is empty or missing headers.")];
        }

        var headers = headerLine.Split(',');
        if (headers.Length != expectedHeaders.Length)
        {
            return [ValidationResult<T>.Failed($"CSV must contain exactly {expectedHeaders.Length} columns.")];
        }

        if (!headers.SequenceEqual(expectedHeaders, StringComparer.OrdinalIgnoreCase))
        {
            return [ValidationResult<T>.Failed($"CSV headers must be: {string.Join(", ", expectedHeaders)}")];
        }

        var results = new List<ValidationResult<T>>();
        int rowCount = 0;
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            rowCount++;
            if (rowCount > 50000)
            {
                return [ValidationResult<T>.Failed("CSV file exceeds the 50000:N0 row limit.")];
            }

            var columns = line.Split(',');
            if (columns.Length != expectedHeaders.Length)
            {
                results.Add(ValidationResult<T>.Failed("Invalid column count."));
                continue;
            }

            var validationResult = mapAndValidate(columns);
            results.Add(validationResult);
        }

        return results;
    }
}