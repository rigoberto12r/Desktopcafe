using Desktopcafe.Core.Enums;

namespace Desktopcafe.Core.Models;

public class Session
{
    public int Id { get; set; }
    public int ComputerId { get; set; }
    public int? ClientId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }
    public DateTime? PlannedEnd { get; set; }
    public SessionType SessionType { get; set; }
    public decimal RatePerHour { get; set; }
    public decimal TotalCharge { get; set; }
    public bool IsPaused { get; set; }
    public DateTime? PausedAt { get; set; }
    public TimeSpan PausedDuration { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Computer Computer { get; set; } = null!;
    public Client? Client { get; set; }
    public Employee Employee { get; set; } = null!;
    public ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
