namespace Desktopcafe.Core.Models;

public class WiFiVoucher
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public bool IsUsed { get; set; }
    public string? MacAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UsedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
