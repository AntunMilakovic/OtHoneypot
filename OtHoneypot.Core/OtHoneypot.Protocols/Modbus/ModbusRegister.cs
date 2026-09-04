namespace OtHoneypot.Core.Protocols;
public class ModbusRegister
{
    public string Name { get; set; }

    public ModbusRegisterType Type { get; set; }

    public ushort Address { get; set; }

    public string DataType { get; set; }

    public object Value { get; set; }

    public bool Writable { get; set; }
}