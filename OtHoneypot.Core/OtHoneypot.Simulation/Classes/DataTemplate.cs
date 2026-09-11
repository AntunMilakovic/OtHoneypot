using OtHoneypot.Core.Enums;
using OtHoneypot.Core.Interfaces;

namespace OtHoneypot.Core.Data;

public class DataTemplate
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DataType { get; set; }

    public Simulation Simulation { get; set; }

}