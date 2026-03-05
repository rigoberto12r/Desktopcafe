using Desktopcafe.Core.Enums;

namespace Desktopcafe.Core.Models;

public class Sale
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int? ClientId { get; set; }
    public int? SessionId { get; set; }
    public decimal Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal? AmountPaid { get; set; }
    public decimal? Change { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Employee Employee { get; set; } = null!;
    public Client? Client { get; set; }
    public Session? Session { get; set; }
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}
