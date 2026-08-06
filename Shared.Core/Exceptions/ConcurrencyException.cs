namespace Shared.Core.Exceptions;

/// <summary>
/// Thrown when an update conflicts with another concurrent update
/// (RowVersion / optimistic concurrency mismatch).
/// Maps to HTTP 409 in the API layer.
/// </summary>
public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message)
    {
    }
}
