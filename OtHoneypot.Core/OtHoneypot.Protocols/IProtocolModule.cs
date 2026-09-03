namespace OtHoneypot.Core.Interfaces;
public interface IProtocolModule
{
    string ProtocolName { get; }

    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}