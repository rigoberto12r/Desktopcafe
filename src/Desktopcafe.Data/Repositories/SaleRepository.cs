using Desktopcafe.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Data.Repositories;

public class SaleRepository : Repository<Sale>
{
    public SaleRepository(AppDbContext db) : base(db) { }

    public async Task<List<Sale>> GetSalesForDateAsync(DateTime date) =>
        await Db.Sales
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .Include(s => s.Employee)
            .Include(s => s.Client)
            .Where(s => s.CreatedAt.Date == date.Date)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

    public async Task<decimal> GetTotalForDateAsync(DateTime date) =>
        await Db.Sales
            .Where(s => s.CreatedAt.Date == date.Date)
            .SumAsync(s => s.Total);

    public async Task<List<Sale>> GetClientSalesAsync(int clientId) =>
        await Db.Sales
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .Where(s => s.ClientId == clientId)
            .OrderByDescending(s => s.CreatedAt)
            .Take(50)
            .ToListAsync();

    public async Task<Sale> CreateSaleAsync(Sale sale)
    {
        foreach (var item in sale.Items)
        {
            var product = await Db.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                product.Stock -= item.Quantity;
                Db.Entry(product).State = EntityState.Modified;
            }
        }

        await Db.Sales.AddAsync(sale);
        await Db.SaveChangesAsync();
        return sale;
    }
}
