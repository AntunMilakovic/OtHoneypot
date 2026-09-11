using System.Collections.Generic;
using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Service.Factories;

public interface IProtocolServiceFactory
{
    IEnumerable<IProtocolModule> CreateModules(
        Configuration configuration);
}