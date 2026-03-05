using Desktopcafe.Core.Enums;

namespace Desktopcafe.Core.Models;

public class Reservation
{
    public int Id { get; set; }
    public int ComputerId { get; set; }
    public int? ClientId { get; set; }
    public DateTime StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Computer Computer { get; set; } = null!;
    public Client? Client { get; set; }
}
