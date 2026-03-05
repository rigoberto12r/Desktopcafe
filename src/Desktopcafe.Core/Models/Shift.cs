namespace Desktopcafe.Core.Models;

public class Shift
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }
    public decimal CashStart { get; set; }
    public decimal? CashEnd { get; set; }
    public string? Notes { get; set; }

    public Employee Employee { get; set; } = null!;
}
