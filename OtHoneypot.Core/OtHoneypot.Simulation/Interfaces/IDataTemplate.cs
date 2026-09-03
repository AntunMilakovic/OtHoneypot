namespace OtHoneypot.Core.Interfaces;

public interface IDataTemplate
{
    int Id { get; set; }
    string Name { get; set; }
    string Description { get; set; }
    string DataType { get; set; }
    float MinValue { get; set; }
    float MaxValue { get; set; }

    /// <summary>
    /// Gets or sets the refresh rate in seconds for the data template.
    /// </summary>
    int RefreshRate { get; set; }    
}