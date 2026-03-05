using Desktopcafe.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Desktopcafe.Data.Configuration;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Total).HasColumnType("decimal(10,2)");
        builder.Property(s => s.AmountPaid).HasColumnType("decimal(10,2)");
        builder.Property(s => s.Change).HasColumnType("decimal(10,2)");
        builder.Property(s => s.PaymentMethod).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(s => s.Employee).WithMany(e => e.Sales).HasForeignKey(s => s.EmployeeId);
        builder.HasOne(s => s.Client).WithMany(c => c.Sales).HasForeignKey(s => s.ClientId);
        builder.HasOne(s => s.Session).WithMany(ses => ses.Sales).HasForeignKey(s => s.SessionId);

        builder.HasIndex(s => new { s.CreatedAt, s.EmployeeId }).HasDatabaseName("IX_Sales_Date_Employee");
    }
}

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.HasKey(si => si.Id);
        builder.Property(si => si.UnitPrice).HasColumnType("decimal(10,2)");
        builder.Property(si => si.Subtotal).HasColumnType("decimal(10,2)");

        builder.HasOne(si => si.Sale).WithMany(s => s.Items).HasForeignKey(si => si.SaleId);
        builder.HasOne(si => si.Product).WithMany(p => p.SaleItems).HasForeignKey(si => si.ProductId);
    }
}
