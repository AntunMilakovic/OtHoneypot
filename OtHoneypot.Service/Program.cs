// // using OtHoneypot.Service;
// // using Microsoft.Extensions.Configuration;
// // using Serilog;
// // using OtHoneypot.Core.Interfaces;
// // using OtHoneypot.Core.Data;
// // using OtHoneypot.Core.Protocols;
// // using System.Runtime.Serialization;
// // using System.Collections.Generic;
// // using Microsoft.Extensions.Hosting;
// // using Microsoft.Extensions.Logging;
// // using Microsoft.Extensions.DependencyInjection;

// // var builder = Host.CreateApplicationBuilder(args);
// // var configuration = new ConfigurationBuilder()
// //     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
// //     .Build();

// // // Log.Logger = new LoggerConfiguration()
// // //     .ReadFrom.Configuration(configuration)
// // //     .CreateLogger();

// // builder.Logging.ClearProviders();

// // builder.Services.AddSerilog((services, loggerConfiguration) =>
// // {
// //     loggerConfiguration
// //         .ReadFrom.Configuration(builder.Configuration)
// //         .ReadFrom.Services(services);
// // });

// // builder.Services.Configure<Configuration>(configuration.GetSection("Honeypot"));
// // var turnedOnProtocols = configuration.GetSection("Honeypot:ActiveProtocols");
// // // if (turnedOnProtocols.GetChildren().Any(c => c.Value.ToLower() == "modbus"))
// // //     builder.Services.AddSingleton(configuration.GetSection("Honeypot:Modbus").Get<List<ModbusConfiguration>>());

// // // builder.Services.AddHostedService<Worker>();
// // builder.Services.AddSingleton(configuration.GetSection("Honeypot:DataTemplates").Get<List<DataTemplate>>());

// // // builder.Services.AddSingleton<DataService>();
// // // builder.Services.Configure<ProtocolService>(configuration.GetSection("Mod"));
// // builder.Services.AddSingleton<DataService>();
// // // builder.Services.AddSingleton<Modbus>();
// // builder.Services.AddSingleton<ProtocolService>();

// // var host = builder.Build();
// // host.Run();

// using System.Collections.Generic;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using OtHoneypot.Core.Data;
// using OtHoneypot.Core.Interfaces;
// using OtHoneypot.Service;
// using Serilog;

// var builder = Host.CreateApplicationBuilder(args);

// builder.Logging.ClearProviders();

// builder.Services.AddSerilog((services, loggerConfiguration) =>
// {
//     loggerConfiguration
//         .ReadFrom.Configuration(builder.Configuration)
//         .ReadFrom.Services(services);
// });


// // CONFIGURATION
// builder.Services.Configure<Configuration>(
//     builder.Configuration.GetSection("Honeypot"));

// var dataTemplates = builder.Configuration
//     .GetSection("Honeypot:DataTemplates")
//     .Get<List<DataTemplate>>()
//     ?? new List<DataTemplate>();

// builder.Services.AddSingleton(dataTemplates);


// // DATA SERVICE
// builder.Services.AddSingleton<DataService>();

// builder.Services.AddSingleton<IDataService>(sp =>
//     sp.GetRequiredService<DataService>());

// builder.Services.AddHostedService(sp =>
//     sp.GetRequiredService<DataService>());


// // PROTOCOL SERVICE
// builder.Services.AddHostedService<ProtocolService>();


// var host = builder.Build();

// await host.RunAsync();

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