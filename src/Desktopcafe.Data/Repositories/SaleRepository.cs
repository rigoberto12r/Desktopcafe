using Desktopcafe.Core.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Desktopcafe.Data.Repositories;

public class SaleRepository : Repository<Sale>
{
    public SaleRepository(AppDbContext db) : base(db) { }

    public async Task<List<Sale>> GetSalesForDateAsync(DateTime date)
    {
        try
        {
            return await Db.Sales
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .Include(s => s.Employee)
                .Include(s => s.Client)
                .Where(s => s.CreatedAt.Date == date.Date)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get sales for {Date}", date);
            throw;
        }
    }

    public async Task<decimal> GetTotalForDateAsync(DateTime date)
    {
        try
        {
            return await Db.Sales
                .Where(s => s.CreatedAt.Date == date.Date)
                .SumAsync(s => s.Total);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get sales total for {Date}", date);
            throw;
        }
    }

    public async Task<List<Sale>> GetClientSalesAsync(int clientId)
    {
        try
        {
            return await Db.Sales
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .Where(s => s.ClientId == clientId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(50)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get sales for client {ClientId}", clientId);
            throw;
        }
    }

    public async Task<Sale> CreateSaleAsync(Sale sale)
    {
        try
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
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create sale with {ItemCount} items", sale.Items.Count);
            throw;
        }
    }
}
