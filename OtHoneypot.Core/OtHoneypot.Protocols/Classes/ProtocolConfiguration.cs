namespace OtHoneypot.Core.Protocols;

public abstract class ProtocolConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public ProtocolType Type { get; set; }
    public ProtocolRole Role { get; set; }

    public string BindAddress { get; set; } = "0.0.0.0";
    public int Port { get; set; }

    public List<string> AllowedIPs { get; set; } = new();

    public int ReadingPeriod { get; set; }
}