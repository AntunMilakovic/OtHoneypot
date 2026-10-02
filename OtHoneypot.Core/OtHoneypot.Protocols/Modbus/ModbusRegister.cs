namespace OtHoneypot.Core.Protocols;

/// <summary>
/// Configured on slave side. Used to define the registers that will be available for reading by the master.
/// </summary>
public class ModbusRegister
{
    public string Name { get; set; } = "Register";

    public ModbusRegisterType Type { get; set; }

    public ushort Address { get; set; }

    public ModbusDataType DataType { get; set; }

    // public object Value { get; set; }

    // public bool Writable { get; set; }

    public int DataTemplateId { get; set; }
}