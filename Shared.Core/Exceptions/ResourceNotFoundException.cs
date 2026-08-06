namespace Shared.Core.Exceptions;

/// <summary>
/// Thrown when a requested resource does not exist.
/// Maps to HTTP 404 in the API layer.
/// </summary>
public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string resourceType, string identifier)
        : base($"{resourceType} with identifier '{identifier}' was not found.")
    {
    }
}
