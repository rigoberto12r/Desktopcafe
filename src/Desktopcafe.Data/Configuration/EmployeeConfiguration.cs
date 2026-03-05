using Desktopcafe.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Desktopcafe.Data.Configuration;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Username).HasMaxLength(50).IsRequired();
        builder.Property(e => e.PasswordHash).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(e => e.Username).IsUnique().HasDatabaseName("IX_Employees_Username");
    }
}
