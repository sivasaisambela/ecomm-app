namespace Shared.Core.Exceptions;

/// <summary>
/// Thrown when a business/domain rule is violated (e.g. duplicate SKU,
/// insufficient stock). Maps to HTTP 400 in the API layer.
/// </summary>
public class BusinessRuleException : Exception
{
    public string ErrorCode { get; }

    public BusinessRuleException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
