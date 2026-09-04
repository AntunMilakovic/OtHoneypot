using OtHoneypot.Core.Enums;
using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Core.Data;

public class DataTemplate : IDataTemplate
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DataType { get; set; }

    public Simulation Simulation { get; set; }

    // public DataTemplate(int id, string name, string description, string dataType, float minValue, float maxValue, int refreshRate, SimulationType simulationType, float? valueChangeRate = null)
    // {
    //     Id = id;
    //     Name = name;
    //     Description = description;
    //     DataType = dataType;
    //     Simulation = new Simulation(simulationType, minValue, maxValue, refreshRate, valueChangeRate);
    // }
}