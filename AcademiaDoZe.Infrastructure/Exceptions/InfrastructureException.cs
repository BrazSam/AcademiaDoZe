namespace AcademiaDoZe.Infrastructure.Exceptions; //Samuel Braz dos Santos

public class InfrastructureException : Exception
{
    public string? ErrorCode { get; }
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public InfrastructureException(string message) : base(message)
    {
    }
    public InfrastructureException(string message, Exception innerException) : base(message, innerException)
    {
    }
    public InfrastructureException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
    public InfrastructureException(string errorCode, string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
    public override string ToString()
    {
        var codeInfo = string.IsNullOrWhiteSpace(ErrorCode) ? "" : $" [{ErrorCode}]";
        return $"[{Timestamp:yyyy-MM-dd HH:mm:ss UTC}]{codeInfo} {base.ToString()}";
    }
}