using System.Collections.Generic;
using OtHoneypot.Core.Enums;

namespace OtHoneypot.Core.Interfaces;

public interface IDataService
{
    List<IData> GetGeneratedDatas(List<int> dataTemplateIds);
    List<IData> GetGeneratedDatas(List<string> dataTemplateNames);

    bool GetGeneratedData(int dataTemplateId, out IData data);
    bool GetGeneratedData(string dataTemplateName, out IData data);

    bool ExecuteSimulationCommand(int dataTemplateId, SimulationCommand command);
}
