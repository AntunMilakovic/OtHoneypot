namespace OtHoneypot.Core.Protocols;

public class ModbusScheduledWrite
{
    public string Name { get; set; } = string.Empty;

    public ModbusRegisterType RegisterType { get; set; } = ModbusRegisterType.HoldingRegister;

    public ushort Address { get; set; }

    public ushort Value { get; set; }

    public int InitialDelayMs { get; set; }

    /// <summary>
    /// Interval between writes. A value of zero executes the write only once.
    /// </summary>
    public int RepeatEveryMs { get; set; }
}
