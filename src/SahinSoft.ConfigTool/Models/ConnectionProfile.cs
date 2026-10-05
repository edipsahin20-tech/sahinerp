namespace SahinSoft.ConfigTool.Models;

public sealed class ConnectionProfile
{
    public string Name { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = "SahinSoftDb";
    public bool WindowsAuth { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public sealed class CentralConnectionSettings
{
    public bool Enabled { get; set; }
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; } = 1433;
    public string Database { get; set; } = string.Empty;
    public bool WindowsAuth { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
}

public sealed class OperationLogEntry
{
    public DateTime AtUtc { get; set; } = DateTime.UtcNow;
    public string Operation { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Detail { get; set; } = string.Empty;
}
