namespace Desktopcafe.Core.Models;

public class RateConfig
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerHour { get; set; }
    public decimal PricePerHalfHour { get; set; }
    public decimal PricePer15Min { get; set; }
    public bool IsWeekend { get; set; }
    public bool IsNightRate { get; set; }
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public bool IsDefault { get; set; }
}
