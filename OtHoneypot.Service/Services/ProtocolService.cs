using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Service;

public class ProtocolService : BackgroundService
{
    private readonly ILogger<ProtocolService> _logger;
    private readonly List<IProtocolModule> _protocolModules;

    List<Task> _runningProtocolModules = new List<Task>();

    public ProtocolService(ILogger<ProtocolService> logger, List<IProtocolModule> protocolModules)
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

    private async Task StartProtocolModuleAsync(IProtocolModule module, CancellationToken stoppingToken)
    {
        try
        {
            await module.StartAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error starting protocol module {module.Name}");
        }
    }
}