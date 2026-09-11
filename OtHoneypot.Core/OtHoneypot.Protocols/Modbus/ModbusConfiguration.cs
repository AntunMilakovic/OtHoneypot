using System.Collections.Generic;
using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Core.Protocols;

public class ModbusConfiguration : IModbusConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public ProtocolType Type { get; set; }
    public ProtocolRole Role { get; set; }
    public string BindAddress { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 502;
    public byte UnitId { get; set; } = 1;
    public List<string> AllowedIPs { get; set; } = new();
    public int ReadingInterval { get; set; } = 1000;

    public List<ModbusDevice> Devices { get; set; } = new();
    public List<ModbusRegister> Registers { get; set; } = new();
    public List<ModbusCommandMapping> CommandMappings { get; set; } = new();
}
