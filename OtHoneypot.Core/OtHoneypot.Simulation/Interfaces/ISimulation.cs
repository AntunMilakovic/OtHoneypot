using OtHoneypot.Core.Enums;

namespace OtHoneypot.Core.Interfaces;

 public interface ISimulation
{
    float MinValue { get; set; }
    float MaxValue { get; set; }

    /// <summary>
    /// Gets or sets the refresh rate in seconds for the data template.
    /// </summary>
    int RefreshRate { get; set; }  

    float? ValueChangeRate { get; set; }
    SimulationType Type { get; set; }
}