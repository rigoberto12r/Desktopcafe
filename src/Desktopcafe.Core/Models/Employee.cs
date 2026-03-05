using Desktopcafe.Core.Enums;

namespace Desktopcafe.Core.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Cashier;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
