using OtHoneypot.Core.Data;
using OtHoneypot.Core.Interfaces;
using OtHoneypot.Core.Protocols;

namespace OtHoneypot.Service;

public class Configuration
{
    public List<IProtocolModule> Protocols { get; set; }
    public List<DataTemplate> DataTemplates { get; set; }
}