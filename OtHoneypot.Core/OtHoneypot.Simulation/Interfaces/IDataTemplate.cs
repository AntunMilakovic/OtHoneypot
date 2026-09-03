namespace OtHoneypot.Core.Interfaces;

public interface IDataTemplate
{
    string Id { get; set; }
    string Name { get; set; }
    string Description { get; set; }
    string DataType { get; set; }
    float MinValue { get; set; }
    float MaxValue { get; set; }
    DateTime RefreshRate { get; set; }
    float Value { get; set; }
    
}