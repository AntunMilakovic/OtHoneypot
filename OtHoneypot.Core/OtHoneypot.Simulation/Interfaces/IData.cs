namespace OtHoneypot.Core.Interfaces;

public interface IData
{
    public string Id { get; set; }
    public float Value { get; set; }

    public string Name { get; set; }

    public DateTime Timestamp { get; set; }

    
}