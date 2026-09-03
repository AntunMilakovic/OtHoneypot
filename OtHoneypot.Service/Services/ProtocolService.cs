using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Service;

public class ProtocolService : BackgroundService
{
    private readonly ILogger<ProtocolService> _logger;
    private readonly IEnumerable<IProtocolModule> _protocolModules;

    public ProtocolService(ILogger<ProtocolService> logger, IEnumerable<IProtocolModule> protocolModules)
    {
        _logger = logger;
        _protocolModules = protocolModules;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();

        foreach (var module in _protocolModules)
        {
            tasks.Add(module.StartAsync(stoppingToken));
        }

        await Task.WhenAll(tasks);
    }
}