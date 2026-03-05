using BCrypt.Net;
using Desktopcafe.Core.Enums;
using Desktopcafe.Core.Models;
using Desktopcafe.Shared;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Data.Seeding;

public static class InitialDataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();
        await db.ApplyPragmasAsync();

        if (await db.Employees.AnyAsync()) return;

        // Default admin
        db.Employees.Add(new Employee
        {
            Name = "Administrador",
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123", 12),
            Role = UserRole.Admin,
            IsActive = true
        });

        // Default rate config
        db.RateConfigs.Add(new RateConfig
        {
            Name = "Tarifa Normal",
            PricePerHour = Constants.Defaults.PricePerHour,
            PricePerHalfHour = Constants.Defaults.PricePerHalfHour,
            PricePer15Min = Constants.Defaults.PricePer15Min,
            IsDefault = true,
            StartHour = 0,
            EndHour = 24
        });

        // Default app configs
        var configs = new List<AppConfig>
        {
            new() { Key = Constants.ConfigKeys.CafeName, Value = Constants.Defaults.CafeName, Description = "Nombre del cafe" },
            new() { Key = Constants.ConfigKeys.PrintPriceBW, Value = Constants.Defaults.PrintPriceBW.ToString("F2"), Description = "Precio por pagina B/N" },
            new() { Key = Constants.ConfigKeys.PrintPriceColor, Value = Constants.Defaults.PrintPriceColor.ToString("F2"), Description = "Precio por pagina color" },
            new() { Key = Constants.ConfigKeys.WarningMinutes1, Value = Constants.Defaults.WarningMinutes1.ToString(), Description = "Primera alerta (minutos)" },
            new() { Key = Constants.ConfigKeys.WarningMinutes2, Value = Constants.Defaults.WarningMinutes2.ToString(), Description = "Segunda alerta (minutos)" },
            new() { Key = Constants.ConfigKeys.WarningMinutes3, Value = Constants.Defaults.WarningMinutes3.ToString(), Description = "Tercera alerta (minutos)" },
            new() { Key = Constants.ConfigKeys.LockScreenMessage, Value = Constants.Defaults.LockScreenMessage, Description = "Mensaje en pantalla de bloqueo" },
            new() { Key = Constants.ConfigKeys.AutoBackupEnabled, Value = "true", Description = "Backup automatico habilitado" },
            new() { Key = Constants.ConfigKeys.BackupIntervalHours, Value = "6", Description = "Intervalo de backup en horas" },
            new() { Key = Constants.ConfigKeys.PointsPerDollar, Value = "1", Description = "Puntos por dolar gastado" },
            new() { Key = Constants.ConfigKeys.InactivityLockMinutes, Value = "5", Description = "Minutos de inactividad antes de bloquear" },
        };
        db.AppConfigs.AddRange(configs);

        // Sample products
        var products = new List<Product>
        {
            new() { Name = "Coca-Cola", Category = Constants.ProductCategories.Drinks, Price = 15, Cost = 8, Stock = 50, MinStock = 10 },
            new() { Name = "Pepsi", Category = Constants.ProductCategories.Drinks, Price = 15, Cost = 8, Stock = 50, MinStock = 10 },
            new() { Name = "Agua 500ml", Category = Constants.ProductCategories.Drinks, Price = 10, Cost = 4, Stock = 100, MinStock = 20 },
            new() { Name = "Jugo de Naranja", Category = Constants.ProductCategories.Drinks, Price = 18, Cost = 10, Stock = 30, MinStock = 10 },
            new() { Name = "Cafe", Category = Constants.ProductCategories.Drinks, Price = 20, Cost = 5, Stock = 999, MinStock = 0 },
            new() { Name = "Papas Fritas", Category = Constants.ProductCategories.Snacks, Price = 18, Cost = 10, Stock = 40, MinStock = 10 },
            new() { Name = "Galletas", Category = Constants.ProductCategories.Snacks, Price = 12, Cost = 6, Stock = 30, MinStock = 10 },
            new() { Name = "Sandwich", Category = Constants.ProductCategories.Snacks, Price = 35, Cost = 15, Stock = 20, MinStock = 5 },
            new() { Name = "Hojas blancas (10)", Category = Constants.ProductCategories.Stationery, Price = 5, Cost = 2, Stock = 200, MinStock = 50 },
            new() { Name = "USB 16GB", Category = Constants.ProductCategories.Stationery, Price = 80, Cost = 45, Stock = 10, MinStock = 3 },
        };
        db.Products.AddRange(products);

        await db.SaveChangesAsync();
    }
}
