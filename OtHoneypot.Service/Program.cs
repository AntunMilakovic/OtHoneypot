using OtHoneypot.Service;
using Microsoft.Extensions.Configuration;
using Serilog;
using OtHoneypot.Core.Interfaces;
using OtHoneypot.Core.Data;
using OtHoneypot.Core.Protocols;

var builder = Host.CreateApplicationBuilder(args);
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

builder.Services.Configure<Configuration>(configuration.GetSection("Honeypot"));

var modbusConfigurations =
    configuration
        .GetSection("Protocols:Modbus")
        .Get<List<ModbusConfiguration>>();

// builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton(configuration.GetSection("Honeypot:DataTemplates").Get<List<DataTemplate>>());
builder.Services.AddSingleton<DataService>();
// builder.Services.Configure<ProtocolService>(configuration.GetSection("Mod"));
// builder.Services.AddHostedService<DataService>();
builder.Services.AddHostedService<ProtocolService>();

var host = builder.Build();
host.Run();
