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
    private List<DataTemplate> _dataTemplates { get; set; }

    private Dictionary<int, DataTemplate> DataTemplateIdToTemplateMap { get; set; }
    private List<Data> _generatedDatas { get; set; }
    private Dictionary<int, Data> _templateIdToDataMap { get; set; }

    private object _lock = new object();

    public DataService(ILogger logger, List<DataTemplate> dataTemplates)
    {
        _logger = logger;
        _dataTemplates = dataTemplates;
        DataTemplateIdToTemplateMap = _dataTemplates.ToDictionary(t => t.Id, t => t);

        _generatedDatas = GenerateData(_dataTemplates);
        _templateIdToDataMap = _generatedDatas.ToDictionary(d => d.TemplateId, d => d);
    }

    public List<IData> GetGeneratedData(List<int> dataTemplateIds)
    {
        lock (_lock)
            return _generatedDatas.Where(d => dataTemplateIds.Contains(d.TemplateId)).ToList<IData>();
    }

    public List<IData> GetGeneratedData(List<string> dataTemplateNames)
    {
        lock (_lock)
            return _generatedDatas.Where(d => dataTemplateNames.Contains(d.Name)).ToList<IData>();
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            RefreshData(_dataTemplates);
            await Task.Delay(1000, stoppingToken);
        }
    }

    private List<Data> GenerateData(List<DataTemplate> dataTemplates)
    {
        var datas = new List<Data>();

        foreach (var template in dataTemplates)
        {
            float value;
            switch(template.Simulation.Type)
            {
                case SimulationType.Static:
                case SimulationType.Random:
                case SimulationType.RandomWalk:
                    value = template.Simulation.MinValue + (float)new Random().NextDouble() * (template.Simulation.MaxValue - template.Simulation.MinValue);
                    break;
                default:
                    _logger.Warning("Simulation type {type} is not implemented.", template.Simulation.Type);
                    throw new NotImplementedException($"Simulation type {template.Simulation.Type} is not implemented.");   
            }
            var data = new Data(template.Name, template.Id, value, DateTime.UtcNow);
            datas.Add(data);
        }

        return datas;
    }

    private void RefreshData(List<DataTemplate> dataTemplates)
    {
        var dataToRefresh = GetDataIdsWhichNeedsRefresh();
        lock (_lock)
        {
            foreach (var templateId in dataToRefresh)
            {
                var data = _templateIdToDataMap[templateId];
                var template = DataTemplateIdToTemplateMap[templateId];
                var value = template.Simulation.MinValue + (float)new Random().NextDouble() * (template.Simulation.MaxValue - template.Simulation.MinValue);
                data.ReplaceData(value, DateTime.UtcNow);
                _templateIdToDataMap[templateId] = data;
            }
            _generatedDatas = _templateIdToDataMap.Values.ToList();
        }
        if(dataToRefresh.Count > 0)
            _logger.Information("Refreshed {count} data items at: {time}", dataToRefresh.Count, DateTimeOffset.Now);
    }

    private List<int> GetDataIdsWhichNeedsRefresh()
    {
        var dataIdsToRefresh = new List<int>();
        foreach (var data in _generatedDatas)
        {
            var template = _dataTemplates.FirstOrDefault(t => t.Id == data.TemplateId);
            if (data.Timestamp.AddSeconds(template.Simulation.RefreshRate) <= DateTime.UtcNow)
                dataIdsToRefresh.Add(data.TemplateId);

        }
        return dataIdsToRefresh;
    }
}