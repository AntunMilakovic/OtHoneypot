using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Core.Protocols;

public class Modbus : IProtocolModule
{
    public string ProtocolName => "Modbus";

    public ModbusRole Role { get; set; }

    public Modbus()
    {

    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Implementation for starting the Modbus protocol module
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Implementation for stopping the Modbus protocol module
        return Task.CompletedTask;
    }
}

public enum ModbusRole
{
    Master = 0,
    Slave = 1,
}
