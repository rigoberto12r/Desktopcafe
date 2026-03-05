using Desktopcafe.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Desktopcafe.Data.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Category).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Price).HasColumnType("decimal(10,2)");
        builder.Property(p => p.Cost).HasColumnType("decimal(10,2)");
        builder.Property(p => p.ImagePath).HasMaxLength(260);
        builder.Property(p => p.Barcode).HasMaxLength(50);

        builder.HasIndex(p => new { p.Category, p.Name }).HasDatabaseName("IX_Products_Category_Name");
    }
}
