using Desktopcafe.Core.Enums;
using Desktopcafe.Core.Models;
using FluentAssertions;
using Xunit;

namespace Desktopcafe.Core.Tests;

public class ModelTests
{
    [Fact]
    public void Computer_DefaultStatus_IsOffline()
    {
        var computer = new Computer();
        computer.Status.Should().Be(ComputerStatus.Offline);
    }

    [Fact]
    public void Session_DefaultStatus_IsActive()
    {
        var session = new Session();
        session.Status.Should().Be(SessionStatus.Active);
        session.IsPaused.Should().BeFalse();
    }

    [Fact]
    public void Client_DefaultValues_AreCorrect()
    {
        var client = new Client();
        client.Balance.Should().Be(0);
        client.Points.Should().Be(0);
        client.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Product_DefaultValues_AreCorrect()
    {
        var product = new Product();
        product.IsActive.Should().BeTrue();
        product.Stock.Should().Be(0);
    }

    [Fact]
    public void Employee_DefaultRole_IsCashier()
    {
        var employee = new Employee();
        employee.Role.Should().Be(UserRole.Cashier);
        employee.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Sale_CalculatesItemsCorrectly()
    {
        var sale = new Sale();
        sale.Items.Add(new SaleItem { Quantity = 2, UnitPrice = 15m, Subtotal = 30m });
        sale.Items.Add(new SaleItem { Quantity = 1, UnitPrice = 20m, Subtotal = 20m });
        sale.Total = sale.Items.Sum(i => i.Subtotal);

        sale.Total.Should().Be(50m);
        sale.Items.Should().HaveCount(2);
    }

    [Fact]
    public void PrintJob_DefaultStatus_IsPending()
    {
        var job = new PrintJob();
        job.Status.Should().Be(PrintJobStatus.Pending);
    }

    [Fact]
    public void WiFiVoucher_DefaultValues()
    {
        var voucher = new WiFiVoucher { Code = "ABC123", DurationMinutes = 60 };
        voucher.IsUsed.Should().BeFalse();
        voucher.UsedAt.Should().BeNull();
    }
}
