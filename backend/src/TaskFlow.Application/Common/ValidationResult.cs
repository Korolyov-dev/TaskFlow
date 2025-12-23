namespace TaskFlow.Application.Common;

public class ValidationResult
{
    public bool IsValid { get; }
    public IEnumerable<string> Errors { get; }

    public ValidationResult(bool isValid, IEnumerable<string> errors = null)
    {
        IsValid = isValid;
        Errors = errors ?? Enumerable.Empty<string>();
    }

    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(IEnumerable<string> errors) => new(false, errors);
}
