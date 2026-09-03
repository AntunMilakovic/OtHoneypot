using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Core.Data;

public class Data : IData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DataType { get; set; }
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public DateTime RefreshRate { get; set; }
    public float Value { get; set; }
    public DateTime Timestamp { get; set; }

    public Data(string id, string name, string description, string dataType, float minValue, float maxValue, DateTime refreshRate, float value)
    {
        Id = id;
        Name = name;
        Description = description;
        DataType = dataType;
        MinValue = minValue;
        MaxValue = maxValue;
        RefreshRate = refreshRate;
        Value = value;
    }
    public void ReplaceData(float newValue, DateTime newTimestamp)
    {
        Value = newValue;
        Timestamp = newTimestamp;
    }
}