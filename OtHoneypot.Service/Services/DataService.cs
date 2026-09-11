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
    private const int ServiceLoopDelayMs = 250;

    private readonly ILogger _logger;
    private readonly Dictionary<int, DataTemplate> _templatesById;
    private readonly Dictionary<string, int> _templateIdsByName;
    private readonly Dictionary<int, Data> _dataByTemplateId;
    private readonly Dictionary<int, SimulationCommand> _simulationCommands;
    private readonly object _sync = new();
    private readonly Random _random = new();

    public DataService(ILogger logger, List<DataTemplate> dataTemplates)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ArgumentNullException.ThrowIfNull(dataTemplates);
        ValidateTemplates(dataTemplates);

        _templatesById = dataTemplates.ToDictionary(template => template.Id);
        _templateIdsByName = dataTemplates.ToDictionary(template => template.Name, template => template.Id, StringComparer.Ordinal);
        _dataByTemplateId = GenerateInitialData(dataTemplates);
        _simulationCommands = dataTemplates.ToDictionary(template => template.Id, _ => SimulationCommand.Hold);
    }

    public List<IData> GetGeneratedDatas(List<int> dataTemplateIds)
    {
        ArgumentNullException.ThrowIfNull(dataTemplateIds);
        var requestedIds = dataTemplateIds.ToHashSet();

        lock (_sync)
        {
            return _dataByTemplateId.Values
                .Where(data => requestedIds.Contains(data.TemplateId))
                .Select(CreateSnapshot)
                .Cast<IData>()
                .ToList();
        }
    }

    public List<IData> GetGeneratedDatas(List<string> dataTemplateNames)
    {
        ArgumentNullException.ThrowIfNull(dataTemplateNames);
        var requestedNames = dataTemplateNames.ToHashSet(StringComparer.Ordinal);

        lock (_sync)
        {
            return _dataByTemplateId.Values
                .Where(data => requestedNames.Contains(data.Name))
                .Select(CreateSnapshot)
                .Cast<IData>()
                .ToList();
        }
    }

    public bool GetGeneratedData(int dataTemplateId, out IData data)
    {
        lock (_sync)
        {
            if (_dataByTemplateId.TryGetValue(dataTemplateId, out var generatedData))
            {
                data = CreateSnapshot(generatedData);
                return true;
            }
        }

        data = null!;
        return false;
    }

    public bool GetGeneratedData(string dataTemplateName, out IData data)
    {
        if (string.IsNullOrWhiteSpace(dataTemplateName))
        {
            data = null!;
            return false;
        }

        lock (_sync)
        {
            if (_templateIdsByName.TryGetValue(dataTemplateName, out var templateId) &&
                _dataByTemplateId.TryGetValue(templateId, out var generatedData))
            {
                data = CreateSnapshot(generatedData);
                return true;
            }
        }

        data = null!;
        return false;
    }

    public bool ExecuteSimulationCommand(int dataTemplateId, SimulationCommand command)
    {
        lock (_sync)
        {
            if (!_templatesById.TryGetValue(dataTemplateId, out var template))
                return false;

            if (!_dataByTemplateId.TryGetValue(dataTemplateId, out var data))
                return false;

            if (command == SimulationCommand.Reset)
            {
                data.ReplaceData(GetInitialValue(template), DateTime.UtcNow);
                _simulationCommands[dataTemplateId] = SimulationCommand.Hold;
            }
            else
            {
                _simulationCommands[dataTemplateId] = command;
            }
        }

        _logger.Information(
            "Simulation command {Command} applied to DataTemplateId {DataTemplateId}",
            command,
            dataTemplateId);

        return true;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            RefreshData();
            await Task.Delay(ServiceLoopDelayMs, stoppingToken);
        }
    }

    private Dictionary<int, Data> GenerateInitialData(IEnumerable<DataTemplate> templates)
    {
        var now = DateTime.UtcNow;

        return templates.ToDictionary(
            template => template.Id,
            template => new Data(template.Name, template.Id, GetInitialValue(template), now));
    }

    private void RefreshData()
    {
        lock (_sync)
        {
            var now = DateTime.UtcNow;

            foreach (var (templateId, template) in _templatesById)
            {
                var data = _dataByTemplateId[templateId];
                var refreshInterval = TimeSpan.FromSeconds(template.Simulation.RefreshRate);

                if (now - data.Timestamp < refreshInterval)
                    continue;

                var nextValue = GetNextValue(template, data.Value);
                data.ReplaceData(nextValue, now);
            }
        }
    }

    private float GetInitialValue(DataTemplate template)
    {
        var simulation = template.Simulation;

        return simulation.Type switch
        {
            SimulationType.Static => simulation.MinValue,
            SimulationType.Random => NextRandom(simulation.MinValue, simulation.MaxValue),
            SimulationType.RandomWalk => NextRandom(simulation.MinValue, simulation.MaxValue),
            SimulationType.Dynamic => simulation.MinValue,
            _ => throw new NotSupportedException(
                $"Simulation type {simulation.Type} for template '{template.Name}' is not implemented.")
        };
    }

    private float GetNextValue(DataTemplate template, float currentValue)
    {
        var simulation = template.Simulation;
        var changeRate = simulation.ValueChangeRate ?? 1f;

        return simulation.Type switch
        {
            SimulationType.Static => currentValue,
            SimulationType.Random => NextRandom(simulation.MinValue, simulation.MaxValue),
            SimulationType.RandomWalk => Math.Clamp(
                currentValue + NextRandom(-changeRate, changeRate),
                simulation.MinValue,
                simulation.MaxValue),
            SimulationType.Dynamic => GetDynamicValue(
                template.Id,
                currentValue,
                changeRate,
                simulation.MinValue,
                simulation.MaxValue),
            _ => throw new NotSupportedException(
                $"Simulation type {simulation.Type} for template '{template.Name}' is not implemented.")
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
        if (minValue == maxValue)
            return minValue;

        return minValue + (float)_random.NextDouble() * (maxValue - minValue);
    }

    private static Data CreateSnapshot(Data source) =>
        new(source.Name, source.TemplateId, source.Value, source.Timestamp);

    private static void ValidateTemplates(IEnumerable<DataTemplate> templates)
    {
        var templateList = templates.ToList();

        var duplicateId = templateList
            .GroupBy(template => template.Id)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateId != null)
            throw new InvalidOperationException($"Duplicate DataTemplate Id detected: {duplicateId.Key}.");

        var duplicateName = templateList
            .Where(template => !string.IsNullOrWhiteSpace(template.Name))
            .GroupBy(template => template.Name, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateName != null)
            throw new InvalidOperationException($"Duplicate DataTemplate name detected: '{duplicateName.Key}'.");

        foreach (var template in templateList)
        {
            if (string.IsNullOrWhiteSpace(template.Name))
                throw new InvalidOperationException($"DataTemplate {template.Id} must have a name.");

            if (template.Simulation == null)
                throw new InvalidOperationException($"DataTemplate '{template.Name}' ({template.Id}) has no Simulation configuration.");

            if (template.Simulation.MinValue > template.Simulation.MaxValue)
            {
                throw new InvalidOperationException(
                    $"DataTemplate '{template.Name}' ({template.Id}) has MinValue {template.Simulation.MinValue} greater than MaxValue {template.Simulation.MaxValue}.");
            }

            if (template.Simulation.RefreshRate <= 0)
            {
                throw new InvalidOperationException(
                    $"DataTemplate '{template.Name}' ({template.Id}) must have RefreshRate greater than 0 seconds.");
            }

            if (template.Simulation.ValueChangeRate < 0)
            {
                throw new InvalidOperationException(
                    $"DataTemplate '{template.Name}' ({template.Id}) cannot have a negative ValueChangeRate.");
            }
        }
    }
}
