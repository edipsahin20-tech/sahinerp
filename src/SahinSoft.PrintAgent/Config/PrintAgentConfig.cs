namespace SahinSoft.PrintAgent.Config;

public sealed class PrintAgentConfig
{
    public ListenConfig Listen { get; set; } = new();
    public List<string> AllowedOrigins { get; set; } = [];
}

public sealed class ListenConfig
{
    public int Port { get; set; } = 5058;
}
