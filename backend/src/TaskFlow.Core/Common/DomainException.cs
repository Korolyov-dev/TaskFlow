// TaskFlow.Core/Common/DomainException.cs
namespace TaskFlow.Core.Common;

/// <summary>
/// Исключение, выбрасываемое при нарушении инвариантов домена
/// и критических бизнес-правил, которые нельзя проверить заранее.
/// </summary>
public class DomainException : Exception
{
    public string ErrorCode { get; }

    public DomainException(string message, string errorCode = "DOMAIN_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, Exception innerException, string errorCode = "DOMAIN_ERROR")
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}