using Desktopcafe.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Desktopcafe.Data.Configuration;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.RatePerHour).HasColumnType("decimal(10,2)");
        builder.Property(s => s.TotalCharge).HasColumnType("decimal(10,2)");
        builder.Property(s => s.SessionType).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Notes).HasMaxLength(500);

        builder.HasOne(s => s.Computer).WithMany(c => c.Sessions).HasForeignKey(s => s.ComputerId);
        builder.HasOne(s => s.Client).WithMany(c => c.Sessions).HasForeignKey(s => s.ClientId);
        builder.HasOne(s => s.Employee).WithMany(e => e.Sessions).HasForeignKey(s => s.EmployeeId);

        builder.HasIndex(s => new { s.Status, s.ComputerId }).HasDatabaseName("IX_Sessions_Active");
        builder.HasIndex(s => s.CreatedAt).HasDatabaseName("IX_Sessions_Date");
    }
}
