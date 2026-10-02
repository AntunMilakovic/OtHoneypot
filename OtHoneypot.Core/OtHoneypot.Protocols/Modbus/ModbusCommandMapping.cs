using OtHoneypot.Core.Enums;

namespace OtHoneypot.Core.Protocols;

/// <summary>
/// Represents a mapping between a Modbus command and a data template. 
/// This allows for the simulation of specific Modbus commands and their associated data templates.
/// Configured on slave side.
/// </summary>
public class ModbusCommandMapping
{
    public string Name { get; set; } = string.Empty;
    public ModbusRegisterType RegisterType { get; set; } = ModbusRegisterType.HoldingRegister;
    public ushort Address { get; set; }
    public ushort Value { get; set; }
    public int DataTemplateId { get; set; }
    public SimulationCommand Command { get; set; }
}