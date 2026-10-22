namespace OtHoneypot.Core.Protocols;

public class ModbusWrite
{
    public string Name { get; set; } = string.Empty;

    public ModbusRegisterType RegisterType { get; set; } = ModbusRegisterType.HoldingRegister;

    /// <summary>
    /// Address on plc to change value
    /// </summary>
    public ushort Address { get; set; }

    /// <summary>
    /// Value to write on plc's register
    /// </summary>
    public ushort Value { get; set; }

    /// <summary>
    /// Determines how the sending command process will be triggered. Scheduled trigger and value depended starts of process are supported.
    /// </summary>
    public WriteType Type { get; set; } = WriteType.Scheduled;

    /// <summary>
    /// Determines type of depended register on PLC.
    /// </summary>
    public ModbusRegisterType DependedRegisterType { get; set; } 

    /// <summary>
    /// Address of depended register on PLC, which value will be monitored so the value depeded start of process can be triggered.
    /// </summary>
    public ushort? DependedRegisterAddress {get; set;}

    /// <summary>
    /// If register value is bigger than set value, the process of sending command is triggered.
    /// </summary>
    public float? BiggerThanValue { get; set; }

    /// <summary>
    /// If register value is smaller than set value, the process of sending command is triggered.
    /// </summary>
    public float? SmallerThanValue { get; set; }

    /// <summary>
    /// Initial delay from service startup.
    /// Number of seconds.
    /// </summary>
    public int? InitialDelayS { get; set; }

    /// <summary>
    /// Interval between writes. A value of zero executes the write only once.
    /// Number of seconds.
    /// </summary>
    public int? RepeatEveryS { get; set; }

    public override bool Equals(object? obj)
    {
        var item = obj as ModbusWrite;

        if (item == null)
            return false;
        
        if(item.Address != this.Address)
            return false;
        
        if(item.Name != this.Name)
            return false;
        
        if(item.DependedRegisterAddress != this.DependedRegisterAddress)
            return false;
        
        if(item.Value != this.Value)
            return false;
        return base.Equals(obj);
    }
}
