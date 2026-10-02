using System.Collections.Generic;

namespace OtHoneypot.Core.Protocols;

/// <summary>
/// Configured on master side. Represents a Modbus device (slave) that can be polled for data or have scheduled writes performed on it.
/// </summary>
public class ModbusDevice
{
    public string Name { get; set; } = "PLC";

    public string IPAddress { get; set; } = "0.0.0.0";

    public int Port { get; set; } = 502;

    public byte UnitId { get; set; } = 1;

    /// <summary>
    /// In seconds
    /// </summary>
    public int PollIntervalS { get; set; }

    public List<ModbusPollDefinition> Polls { get; set; } = [];

    public List<ModbusScheduledWrite> ScheduledWrites { get; set; } = [];
}
