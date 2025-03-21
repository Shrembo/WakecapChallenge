namespace WakeCap.Infrastructure.CsvParsers;

public interface ICsvParser
{
    Task<IEnumerable<ValidationResult<T>>> ParseAndPreValidateAsync<T>(
        Stream stream,
        string[] expectedHeaders,
        Func<string[], ValidationResult<T>> mapAndValidate,
        CancellationToken cancellationToken);
}