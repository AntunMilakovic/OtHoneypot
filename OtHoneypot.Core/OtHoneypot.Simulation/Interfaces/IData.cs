namespace OtHoneypot.Core.Interfaces;

public interface IData
{
    public float Value { get; set; }

    public string Name { get; set; }

    public int TemplateId { get; set; }

    public DateTime Timestamp { get; set; }

    
}