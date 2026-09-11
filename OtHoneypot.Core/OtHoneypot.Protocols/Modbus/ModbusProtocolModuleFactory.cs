using System.Collections.Generic;
using OtHoneypot.Core.Interfaces;
using OtHoneypot.Core.Protocols;
using Serilog;

namespace OtHoneypot.Service.Factories;

public class ModbusProtocolModuleFactory : IProtocolModuleFactory
{
    private readonly ILogger _logger;
    private readonly IDataService _dataService;

    public ModbusProtocolModuleFactory(
        ILogger logger,
        IDataService dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    public IEnumerable<IProtocolModule> CreateModules(
        Configuration configuration)
    {
        if (configuration.Modbus == null)
            yield break;

        foreach (var modbusConfiguration in configuration.Modbus)
        {
            if (!modbusConfiguration.Enabled)
                continue;

            yield return new Modbus(
                _logger,
                _dataService,
                modbusConfiguration);
        }
    }
}