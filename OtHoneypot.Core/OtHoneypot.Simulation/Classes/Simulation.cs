using OtHoneypot.Core.Enums;
using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Core.Data;

public class Simulation
{
    public float MinValue { get; set; }
    public float MaxValue { get; set; }

    /// <summary>
    /// Gets or sets the refresh rate in seconds for the data template.
    /// </summary>
    public int RefreshRate { get; set; }  

    public float? ValueChangeRate { get; set; }
    public SimulationType Type { get; set; }
}