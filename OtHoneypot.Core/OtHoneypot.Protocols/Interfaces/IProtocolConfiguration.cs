using System.Collections.Generic;

namespace OtHoneypot.Core.Protocols;

public interface IProtocolConfiguration
{
    public string Name { get; set; }
    public string Description { get; set; }

    public bool Enabled { get; set; }

    public ProtocolType Type { get; set; }
    public ProtocolRole Role { get; set; }

    public string BindAddress { get; set; }
    public int Port { get; set; }

    public List<string> AllowedIPs { get; set; }

    public int ReadingInterval { get; set; }
}