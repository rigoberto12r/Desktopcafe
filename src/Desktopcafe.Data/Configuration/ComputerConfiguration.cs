using Desktopcafe.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Desktopcafe.Data.Configuration;

public class ComputerConfiguration : IEntityTypeConfiguration<Computer>
{
    public void Configure(EntityTypeBuilder<Computer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(50).IsRequired();
        builder.Property(c => c.IpAddress).HasMaxLength(45);
        builder.Property(c => c.MacAddress).HasMaxLength(17);
        builder.Property(c => c.Zone).HasMaxLength(50);
        builder.Property(c => c.Specs).HasMaxLength(1000);
        builder.Property(c => c.MaintenanceNotes).HasMaxLength(500);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(c => c.Status).HasDatabaseName("IX_Computers_Status");
        builder.HasIndex(c => c.MacAddress).IsUnique().HasDatabaseName("IX_Computers_Mac");
    }
}
