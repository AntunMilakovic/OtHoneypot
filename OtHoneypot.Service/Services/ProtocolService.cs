using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OtHoneypot.Core.Interfaces;
using OtHoneypot.Core.Protocols;
using Serilog;

namespace OtHoneypot.Service;

public class ProtocolService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly List<IProtocolModule> _protocolModules;

    private readonly List<string> activeProtocols;

    private readonly IOptions<Configuration> _options;

    List<Task> _runningProtocolModules = new List<Task>();

    public ProtocolService(ILogger logger, IOptions<Configuration> options)
    {
        _logger = logger;
        _options = options;
        // this.activeProtocols = activeProtocols;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();

        foreach (var module in _options.Value.Modbus)
            tasks.Add(new Modbus(_logger, (ModbusConfiguration)module).StartAsync(stoppingToken));
        

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
            _logger.Error(ex, $"Error starting protocol module {module.Name}");
        }
    }
}