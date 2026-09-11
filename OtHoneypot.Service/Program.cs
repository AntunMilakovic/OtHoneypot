using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OtHoneypot.Core.Data;
using OtHoneypot.Core.Interfaces;
using OtHoneypot.Service;
using OtHoneypot.Service.Factories;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services);
});

builder.Services.Configure<Configuration>(
    builder.Configuration.GetSection("Honeypot"));

var dataTemplates = builder.Configuration
    .GetSection("Honeypot:DataTemplates")
    .Get<List<DataTemplate>>()
    ?? new List<DataTemplate>();

builder.Services.AddSingleton(dataTemplates);


// DataService
builder.Services.AddSingleton<DataService>();

builder.Services.AddSingleton<IDataService>(sp =>
    sp.GetRequiredService<DataService>());

builder.Services.AddHostedService(sp =>
    sp.GetRequiredService<DataService>());


// Protocol factories
builder.Services.AddSingleton<
    IProtocolModuleFactory,
    ModbusProtocolModuleFactory>();

builder.Services.AddSingleton<
    IProtocolServiceFactory,
    ProtocolServiceFactory>();


// Protocol manager
builder.Services.AddHostedService<ProtocolService>();


var host = builder.Build();

await host.RunAsync();