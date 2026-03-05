namespace Desktopcafe.Core.Models;

public class Client
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public int Points { get; set; }
    public string? AvatarPath { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
