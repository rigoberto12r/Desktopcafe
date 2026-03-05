using Desktopcafe.Core.Enums;
using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Desktopcafe.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Desktopcafe.Data.Tests;

public class RepositoryTests : IDisposable
{
    private readonly AppDbContext _db;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        SeedTestData();
    }

    private void SeedTestData()
    {
        _db.Computers.AddRange(
            new Computer { Id = 1, Name = "PC-01", Status = ComputerStatus.Available },
            new Computer { Id = 2, Name = "PC-02", Status = ComputerStatus.InUse },
            new Computer { Id = 3, Name = "PC-03", Status = ComputerStatus.Maintenance }
        );

        _db.Employees.Add(new Employee
        {
            Id = 1, Name = "Admin", Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("test"), Role = UserRole.Admin
        });

        _db.Sessions.Add(new Session
        {
            Id = 1, ComputerId = 2, EmployeeId = 1,
            SessionType = SessionType.Prepaid, Status = SessionStatus.Active,
            RatePerHour = 20, StartTime = DateTime.Now.AddMinutes(-30),
            PlannedEnd = DateTime.Now.AddMinutes(30)
        });

        _db.Products.AddRange(
            new Product { Id = 1, Name = "Coca-Cola", Category = "Bebidas", Price = 15, Stock = 50, IsActive = true },
            new Product { Id = 2, Name = "Papas", Category = "Snacks", Price = 18, Stock = 30, IsActive = true },
            new Product { Id = 3, Name = "Agua", Category = "Bebidas", Price = 10, Stock = 0, MinStock = 20, IsActive = true }
        );

        _db.SaveChanges();
        _db.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SessionRepository_GetActiveSessions_ReturnsOnlyActive()
    {
        var repo = new SessionRepository(_db);
        var sessions = await repo.GetActiveSessionsAsync();
        sessions.Should().HaveCount(1);
        sessions[0].ComputerId.Should().Be(2);
    }

    [Fact]
    public async Task SessionRepository_GetActiveForComputer_ReturnsCorrectSession()
    {
        var repo = new SessionRepository(_db);
        var session = await repo.GetActiveSessionForComputerAsync(2);
        session.Should().NotBeNull();
        session!.ComputerId.Should().Be(2);

        var noSession = await repo.GetActiveSessionForComputerAsync(1);
        noSession.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetAll_ReturnsAllComputers()
    {
        var repo = new Repository<Computer>(_db);
        var computers = await repo.GetAllAsync();
        computers.Should().HaveCount(3);
    }

    [Fact]
    public async Task Repository_Add_InsertsEntity()
    {
        var repo = new Repository<Computer>(_db);
        var pc = new Computer { Name = "PC-04", Status = ComputerStatus.Available };
        await repo.AddAsync(pc);

        var all = await repo.GetAllAsync();
        all.Should().HaveCount(4);
    }

    [Fact]
    public async Task SaleRepository_CreateSale_DeductsStock()
    {
        var repo = new SaleRepository(_db);

        var sale = new Sale
        {
            EmployeeId = 1,
            Total = 33m,
            PaymentMethod = PaymentMethod.Cash,
            Items = new List<SaleItem>
            {
                new() { ProductId = 1, Quantity = 2, UnitPrice = 15, Subtotal = 30 },
                new() { ProductId = 2, Quantity = 1, UnitPrice = 18, Subtotal = 18 }
            }
        };

        await repo.CreateSaleAsync(sale);

        var product1 = await _db.Products.FindAsync(1);
        product1!.Stock.Should().Be(48); // 50 - 2

        var product2 = await _db.Products.FindAsync(2);
        product2!.Stock.Should().Be(29); // 30 - 1
    }

    public void Dispose() => _db.Dispose();
}
