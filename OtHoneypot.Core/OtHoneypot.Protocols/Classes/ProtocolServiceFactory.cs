using System.Collections.Generic;
using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Service.Factories;

public class ProtocolServiceFactory : IProtocolServiceFactory
{
    private readonly IEnumerable<IProtocolModuleFactory> _factories;

    public ProtocolServiceFactory(
        IEnumerable<IProtocolModuleFactory> factories)
    {
        _factories = factories;
    }

    public IEnumerable<IProtocolModule> CreateModules(
        Configuration configuration)
    {
        foreach (var factory in _factories)
        {
            foreach (var module in factory.CreateModules(configuration))
            {
                yield return module;
            }
        }
    }
}