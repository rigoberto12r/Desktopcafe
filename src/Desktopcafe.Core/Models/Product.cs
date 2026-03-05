namespace Desktopcafe.Core.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public int Stock { get; set; }
    public int MinStock { get; set; }
    public string? ImagePath { get; set; }
    public string? Barcode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}
