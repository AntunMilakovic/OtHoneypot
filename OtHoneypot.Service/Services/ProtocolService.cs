using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OtHoneypot.Core.Interfaces;
using OtHoneypot.Service.Factories;
using Serilog;

namespace OtHoneypot.Service;

public class ProtocolService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IOptions<Configuration> _options;
    private readonly IProtocolServiceFactory _protocolFactory;

    public ProtocolService(
        ILogger logger,
        IOptions<Configuration> options,
        IProtocolServiceFactory protocolFactory)
    {
        _logger = logger;
        _options = options;
        _protocolFactory = protocolFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var modules =
            _protocolFactory.CreateModules(_options.Value);

        var tasks = modules
            .Select(module =>
                StartProtocolModuleAsync(
                    module,
                    stoppingToken))
            .ToList();

        await Task.WhenAll(tasks);
    }

    private async Task StartProtocolModuleAsync(
        IProtocolModule module,
        CancellationToken stoppingToken)
    {
        try
        {
            _logger.Information(
                "Starting protocol module {ProtocolName} ({ProtocolType})",
                module.Name,
                module.Type);

            await module.StartAsync(stoppingToken);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "Error running protocol module {ProtocolName}",
                module.Name);
        }
    }
}