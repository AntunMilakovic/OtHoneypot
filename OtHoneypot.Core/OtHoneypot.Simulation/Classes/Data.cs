using System;
using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Core.Data;

public class Data : IData
{
    public string Name { get; set; }
    public int TemplateId { get; set; }
    public float Value { get; set; }
    public DateTime Timestamp { get; set; }

    public Data(string name, int templateId, float value, DateTime timestamp)
    {
        Name = name;
        TemplateId = templateId;
        Value = value;
        Timestamp = timestamp;
    }

    public void ReplaceData(float newValue, DateTime newTimestamp)
    {
        Value = newValue;
        Timestamp = newTimestamp;
    }
}