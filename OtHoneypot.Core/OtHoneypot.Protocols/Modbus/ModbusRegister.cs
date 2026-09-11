namespace OtHoneypot.Core.Protocols;

public class ModbusRegister
{
    public string Name { get; set; } = string.Empty;
    public ModbusRegisterType Type { get; set; }
    public ushort Address { get; set; }
    public ModbusDataType DataType { get; set; }
    public bool Writable { get; set; }
    public int DataTemplateId { get; set; }
}
