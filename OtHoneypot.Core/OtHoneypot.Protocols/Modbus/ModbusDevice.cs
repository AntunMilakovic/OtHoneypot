using System.Collections.Generic;

namespace OtHoneypot.Core.Protocols;

public class ModbusDevice
{
    public string Name { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 502;
    public byte UnitId { get; set; } = 1;
    public int PollIntervalMs { get; set; }
    public List<ModbusPollDefinition> Polls { get; set; } = new();
}
