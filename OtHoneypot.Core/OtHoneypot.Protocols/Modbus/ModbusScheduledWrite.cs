namespace OtHoneypot.Core.Protocols;

public class ModbusScheduledWrite
{
    public string Name { get; set; } = string.Empty;

    public ModbusRegisterType RegisterType { get; set; } = ModbusRegisterType.HoldingRegister;

    public ushort Address { get; set; }

    public ushort Value { get; set; }

    /// <summary>
    /// Initial delay from service startup.
    /// Number of seconds.
    /// </summary>
    public int InitialDelayS { get; set; }

    /// <summary>
    /// Interval between writes. A value of zero executes the write only once.
    /// Number of seconds.
    /// </summary>
    public int RepeatEveryS { get; set; }
}
