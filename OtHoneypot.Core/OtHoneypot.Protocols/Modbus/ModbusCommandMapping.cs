using OtHoneypot.Core.Enums;

namespace OtHoneypot.Core.Protocols;

public class ModbusCommandMapping
{
    public string Name { get; set; } = string.Empty;
    public ModbusRegisterType RegisterType { get; set; } = ModbusRegisterType.HoldingRegister;
    public ushort Address { get; set; }
    public ushort Value { get; set; }
    public int DataTemplateId { get; set; }
    public SimulationCommand Command { get; set; }
}
