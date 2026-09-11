using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;

namespace OtHoneypot.Core.Interfaces;
public interface IDataService
{
    /// <summary>
    /// Gets the generated data.
    /// </summary>
    /// <returns>List of generated data items</returns>
    List<IData> GetGeneratedDatas(List<int> dataTemplateIds);
    List<IData> GetGeneratedDatas(List<string> dataTemplateNames);

    bool GetGeneratedData(int dataTemplateId, out IData data);
    bool GetGeneratedData(string dataTemplateName, out IData data);
}