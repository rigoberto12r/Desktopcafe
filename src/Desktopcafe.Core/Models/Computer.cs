using Desktopcafe.Core.Enums;

namespace Desktopcafe.Core.Models;

public class Computer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public ComputerStatus Status { get; set; } = ComputerStatus.Offline;
    public string Zone { get; set; } = string.Empty;
    public bool IsGaming { get; set; }
    public string? Specs { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    public bool IsOnline { get; set; }
    public string? MaintenanceNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
