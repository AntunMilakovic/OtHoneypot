using System.Collections.Generic;
using OtHoneypot.Core.Protocols;

namespace OtHoneypot.Core.Interfaces;

public interface IModbusConfiguration : IProtocolConfiguration
{
    byte UnitId { get; set; }
    List<ModbusDevice> Devices { get; set; }
    List<ModbusRegister> Registers { get; set; }
    List<ModbusCommandMapping> CommandMappings { get; set; }
}
