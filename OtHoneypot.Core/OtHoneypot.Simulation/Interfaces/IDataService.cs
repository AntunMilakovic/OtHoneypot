using System.Collections.Generic;

namespace OtHoneypot.Core.Interfaces;
public interface IDataService
{
    /// <summary>
    /// Gets the generated data.
    /// </summary>
    /// <returns>List of generated data items</returns>
    List<IData> GetGeneratedData(List<int> dataTemplateIds);
    List<IData> GetGeneratedData(List<string> dataTemplateNames);
}