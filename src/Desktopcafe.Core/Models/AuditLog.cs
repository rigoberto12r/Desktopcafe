namespace Desktopcafe.Core.Models;

public class AuditLog
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Employee Employee { get; set; } = null!;
}
