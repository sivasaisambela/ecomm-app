namespace Shared.Core.Exceptions;

/// <summary>
/// Thrown when input validation fails.
/// Maps to HTTP 400 in the API layer, with a field-level error breakdown.
/// </summary>
public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
