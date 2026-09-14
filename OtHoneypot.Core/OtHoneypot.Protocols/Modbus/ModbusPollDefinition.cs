namespace OtHoneypot.Core.Protocols;

public class ModbusPollDefinition
{
    public string Name { get; set; }

    public ModbusRegisterType RegisterType { get; set; }

    public ushort StartAddress { get; set; }

    public ushort Count { get; set; }

    public ModbusDataType DataType { get; set; } = ModbusDataType.UInt16;

    public ModbusByteOrder ByteOrder { get; set; } = ModbusByteOrder.ABCD;

    public int PollIntervalMs { get; set; }
}
