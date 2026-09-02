namespace SahinSoft.FiscalAgent.Config;

public sealed class FiscalAgentConfig
{
    public DeviceConfig Device { get; set; } = new();
    public ListenConfig Listen { get; set; } = new();
    public List<string> AllowedOrigins { get; set; } = [];

    // Gerçek cihaz/gmp3 eşlemesi henüz yokken ("cihaz elimize ulaştığında test ederiz") API'yi
    // uçtan uca deneyebilmek için - true iken InposDeviceService hiçbir native çağrı yapmaz,
    // gerçekçi sahte (ama tutarlı) yanıtlar üretir. Cihaz gelince agent.config.json'da false
    // yapılıp Device bilgileri (gmp3 eşlemesinden gelen IP/port/seri no) doldurulur.
    public bool SimulationMode { get; set; } = true;
}

public sealed class DeviceConfig
{
    public string SerialNo { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public int Port { get; set; }
}

public sealed class ListenConfig
{
    public int Port { get; set; } = 9595;
}
