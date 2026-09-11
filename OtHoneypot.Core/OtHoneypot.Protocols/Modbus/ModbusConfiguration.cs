using System.Collections.Generic;
using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Core.Protocols;

public class ModbusConfiguration : IModbusConfiguration
{
    public List<ModbusDevice> Devices { get; set; } = new();
    public List<ModbusRegister> Registers { get; set; } = new();
    public List<ModbusCommandMapping> CommandMappings { get; set; } = new();
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