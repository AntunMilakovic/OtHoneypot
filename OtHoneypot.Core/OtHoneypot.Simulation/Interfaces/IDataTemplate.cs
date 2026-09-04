using OtHoneypot.Core.Data;

namespace OtHoneypot.Core.Interfaces;

public interface IDataTemplate
{
    int Id { get; set; }
    string Name { get; set; }
    string Description { get; set; }
    string DataType { get; set; }
    
    /// <summary>
    /// SimulationType property to specify the type of simulation for the data template.
    /// It can simulate data changes based on the specified simulation type (e.g., Static, Dynamic, etc.).
    /// </summary>
    Simulation Simulation { get; set; } 
}