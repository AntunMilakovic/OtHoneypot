using OtHoneypot.Service;
using Microsoft.Extensions.Configuration;
using Serilog;
using OtHoneypot.Core.Interfaces;
using OtHoneypot.Core.Data;
using OtHoneypot.Core.Protocols;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Log.Logger = new LoggerConfiguration()
//     .ReadFrom.Configuration(configuration)
//     .CreateLogger();

builder.Logging.ClearProviders();

builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services);
});

builder.Services.Configure<Configuration>(configuration.GetSection("Honeypot"));
var turnedOnProtocols = configuration.GetSection("Honeypot:ActiveProtocols");
// if (turnedOnProtocols.GetChildren().Any(c => c.Value.ToLower() == "modbus"))
//     builder.Services.AddSingleton(configuration.GetSection("Honeypot:Modbus").Get<List<ModbusConfiguration>>());

// builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton(configuration.GetSection("Honeypot:DataTemplates").Get<List<DataTemplate>>());

// builder.Services.AddSingleton<DataService>();
// builder.Services.Configure<ProtocolService>(configuration.GetSection("Mod"));
builder.Services.AddHostedService<DataService>();
// builder.Services.AddSingleton<Modbus>();
builder.Services.AddHostedService<ProtocolService>();

var host = builder.Build();
host.Run();