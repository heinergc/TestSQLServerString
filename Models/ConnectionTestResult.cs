namespace TestingSQLServerCnx.Models;

public class ConnectionTestResult
{
    public bool IsSuccessful { get; set; }
    public string Message { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
    public DateTime TestedAt { get; set; } = DateTime.Now;
    public string? SqlServerVersion { get; set; }
    public string? DatabaseName { get; set; }
    public Exception? Exception { get; set; }

    public static ConnectionTestResult Success(TimeSpan responseTime, string? serverVersion = null, string? databaseName = null)
    {
        return new ConnectionTestResult
        {
            IsSuccessful = true,
            Message = "Conexión exitosa",
            ResponseTime = responseTime,
            SqlServerVersion = serverVersion,
            DatabaseName = databaseName
        };
    }

    public static ConnectionTestResult Failure(string message, Exception? exception = null, TimeSpan responseTime = default)
    {
        return new ConnectionTestResult
        {
            IsSuccessful = false,
            Message = message,
            ResponseTime = responseTime,
            Exception = exception
        };
    }

    public override string ToString()
    {
        if (IsSuccessful)
        {
            return $"✓ EXITOSA - {ResponseTime.TotalMilliseconds:F0}ms" + 
                   (SqlServerVersion != null ? $" - {SqlServerVersion}" : "");
        }
        else
        {
            return $"✗ FALLÓ - {Message}";
        }
    }
}