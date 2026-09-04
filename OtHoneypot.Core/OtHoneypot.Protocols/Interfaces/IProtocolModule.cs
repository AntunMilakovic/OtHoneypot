using OtHoneypot.Core.Protocols;

namespace OtHoneypot.Core.Interfaces;

public interface IProtocolModule
{

    string Name { get; }
    ProtocolType Type { get; }
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}