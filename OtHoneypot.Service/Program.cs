using OtHoneypot.Core.Data;
using OtHoneypot.Service;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<DataService>();

var host = builder.Build();
host.Run();
