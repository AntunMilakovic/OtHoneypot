namespace OtHoneypot.Core.Protocols;

public class ModbusPollDefinition
{
    public string Name { get; set; }

    public ModbusRegisterType RegisterType { get; set; }

    public ushort StartAddress { get; set; }

    public ushort Count { get; set; }

    public int PollIntervalMs { get; set; }
}