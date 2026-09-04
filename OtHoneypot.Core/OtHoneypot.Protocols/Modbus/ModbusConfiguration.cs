namespace OtHoneypot.Core.Protocols;

public class ModbusConfiguration : ProtocolConfiguration
{
    public List<ModbusDevice> Devices { get; set; }

    public List<ModbusRegister> Registers { get; set; }
}
// public class ModbusConfiguration
// {
//     public ModbusMasterConfiguration Master { get; set; }
//     public ModbusSlaveConfiguration Slave { get; set; }
// }

public class ModbusMasterConfiguration
{
    public List<ModbusDevice> Devices { get; set; }
}

public class ModbusSlaveConfiguration
{
    public byte UnitId { get; set; }

    public List<ModbusRegister> Registers { get; set; }
}