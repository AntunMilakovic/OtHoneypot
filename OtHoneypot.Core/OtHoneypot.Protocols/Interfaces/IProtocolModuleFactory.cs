using System.Collections.Generic;
using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Service.Factories;

public interface IProtocolModuleFactory
{
    IEnumerable<IProtocolModule> CreateModules(Configuration configuration);
}