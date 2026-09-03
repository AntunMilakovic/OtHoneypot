using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Core.Data;

public class DataTemplate : IDataTemplate
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DataType { get; set; }
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public int RefreshRate { get; set; }

    public DataTemplate(int id, string name, string description, string dataType, float minValue, float maxValue, int refreshRate)
    {
        Id = id;
        Name = name;
        Description = description;
        DataType = dataType;
        MinValue = minValue;
        MaxValue = maxValue;
        RefreshRate = refreshRate;
    }
}