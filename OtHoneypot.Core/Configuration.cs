using System.Collections.Generic;
using OtHoneypot.Core.Data;
using OtHoneypot.Core.Interfaces;
using OtHoneypot.Core.Protocols;

namespace OtHoneypot.Service;

public class Configuration
{
    public List<ModbusConfiguration> Modbus { get; set; } = [];

    public List<DataTemplate> DataTemplates { get; set; } = [];

    // Future:
    // public List<Dnp3Configuration> Dnp3 { get; set; } = [];
    // public List<S7Configuration> S7 { get; set; } = [];
}