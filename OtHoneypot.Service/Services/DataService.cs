using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OtHoneypot.Core.Data;
using OtHoneypot.Core.Enums;
using OtHoneypot.Core.Interfaces;
using Serilog;

namespace OtHoneypot.Service;

public class DataService : BackgroundService, IDataService
{
    private readonly ILogger _logger;
    private readonly List<DataTemplate> _dataTemplates;
    private readonly Dictionary<int, DataTemplate> _dataTemplateIdToTemplateMap;
    private readonly Dictionary<int, Data> _templateIdToDataMap;
    private readonly Dictionary<int, SimulationCommand> _simulationCommands = new();
    private readonly object _lock = new();
    private readonly Random _random = new();

    public DataService(ILogger logger, List<DataTemplate> dataTemplates)
    {
        _logger = logger;
        _dataTemplates = dataTemplates;
        _dataTemplateIdToTemplateMap = _dataTemplates.ToDictionary(t => t.Id, t => t);
        _templateIdToDataMap = GenerateData(_dataTemplates).ToDictionary(d => d.TemplateId, d => d);

        foreach (var template in _dataTemplates)
            _simulationCommands[template.Id] = SimulationCommand.Hold;
    }

    public List<IData> GetGeneratedDatas(List<int> dataTemplateIds)
    {
        lock (_lock)
            return _templateIdToDataMap.Values.Where(d => dataTemplateIds.Contains(d.TemplateId)).Cast<IData>().ToList();
    }

    public List<IData> GetGeneratedDatas(List<string> dataTemplateNames)
    {
        lock (_lock)
            return _templateIdToDataMap.Values.Where(d => dataTemplateNames.Contains(d.Name)).Cast<IData>().ToList();
    }

    public bool GetGeneratedData(int dataTemplateId, out IData data)
    {
        lock (_lock)
        {
            if (_templateIdToDataMap.TryGetValue(dataTemplateId, out var generatedData))
            {
                data = generatedData;
                return true;
            }
        }

        data = null!;
        return false;
    }

    public bool GetGeneratedData(string dataTemplateName, out IData data)
    {
        lock (_lock)
        {
            var generatedData = _templateIdToDataMap.Values.FirstOrDefault(d => d.Name == dataTemplateName);
            if (generatedData != null)
            {
                data = generatedData;
                return true;
            }
        }

        data = null!;
        return false;
    }

    public bool ExecuteSimulationCommand(int dataTemplateId, SimulationCommand command)
    {
        lock (_lock)
        {
            if (!_dataTemplateIdToTemplateMap.ContainsKey(dataTemplateId))
                return false;

            _simulationCommands[dataTemplateId] = command;

            if (command == SimulationCommand.Reset)
            {
                var template = _dataTemplateIdToTemplateMap[dataTemplateId];
                var data = _templateIdToDataMap[dataTemplateId];
                data.ReplaceData(GetInitialValue(template), DateTime.UtcNow);
                _simulationCommands[dataTemplateId] = SimulationCommand.Hold;
            }
        }

        _logger.Information("Simulation command {Command} applied to DataTemplateId {DataTemplateId}", command, dataTemplateId);
        return true;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            RefreshData();
            await Task.Delay(250, stoppingToken);
        }
    }

    private List<Data> GenerateData(List<DataTemplate> dataTemplates)
    {
        var datas = new List<Data>();
        foreach (var template in dataTemplates)
            datas.Add(new Data(template.Name, template.Id, GetInitialValue(template), DateTime.UtcNow));

        return datas;
    }

    private float GetInitialValue(DataTemplate template)
    {
        return template.Simulation.Type switch
        {
            SimulationType.Static => template.Simulation.MinValue,
            SimulationType.Random => NextRandom(template.Simulation.MinValue, template.Simulation.MaxValue),
            SimulationType.RandomWalk => NextRandom(template.Simulation.MinValue, template.Simulation.MaxValue),
            SimulationType.Dynamic => template.Simulation.MinValue,
            _ => throw new NotImplementedException($"Simulation type {template.Simulation.Type} is not implemented.")
        };
    }

    private void RefreshData()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            foreach (var pair in _templateIdToDataMap)
            {
                var data = pair.Value;
                var template = _dataTemplateIdToTemplateMap[pair.Key];
                var refreshRate = Math.Max(1, template.Simulation.RefreshRate);

                if (data.Timestamp.AddSeconds(refreshRate) > now)
                    continue;

                var currentValue = Convert.ToSingle(data.Value);
                var nextValue = GetNextValue(template, currentValue);
                data.ReplaceData(nextValue, now);
            }
        }
    }

    private float GetNextValue(DataTemplate template, float currentValue)
    {
        var simulation = template.Simulation;
        var changeRate = Math.Abs(simulation.ValueChangeRate ?? 1f);

        return simulation.Type switch
        {
            SimulationType.Static => currentValue,
            SimulationType.Random => NextRandom(simulation.MinValue, simulation.MaxValue),
            SimulationType.RandomWalk => Math.Clamp(currentValue + NextRandom(-changeRate, changeRate), simulation.MinValue, simulation.MaxValue),
            SimulationType.Dynamic => GetDynamicValue(template.Id, currentValue, changeRate, simulation.MinValue, simulation.MaxValue),
            _ => throw new NotImplementedException($"Simulation type {simulation.Type} is not implemented.")
        };
    }

    private float GetDynamicValue(int templateId, float currentValue, float changeRate, float minValue, float maxValue)
    {
        var command = _simulationCommands.GetValueOrDefault(templateId, SimulationCommand.Hold);
        return command switch
        {
            SimulationCommand.Increase => Math.Clamp(currentValue + changeRate, minValue, maxValue),
            SimulationCommand.Decrease => Math.Clamp(currentValue - changeRate, minValue, maxValue),
            _ => currentValue
        };
    }

    private float NextRandom(float minValue, float maxValue)
    {
        if (maxValue <= minValue)
            return minValue;

        return minValue + (float)_random.NextDouble() * (maxValue - minValue);
    }
}
