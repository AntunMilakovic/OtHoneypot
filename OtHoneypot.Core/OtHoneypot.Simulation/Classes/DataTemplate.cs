using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Core.Data;

public class DataTemplate : IDataTemplate
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DataType { get; set; }
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public DateTime RefreshRate { get; set; }
    public float Value { get; set; }
}