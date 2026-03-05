using Desktopcafe.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Desktopcafe.Data.Configuration;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Phone).HasMaxLength(20);
        builder.Property(c => c.Email).HasMaxLength(100);
        builder.Property(c => c.Code).HasMaxLength(10).IsRequired();
        builder.Property(c => c.Balance).HasColumnType("decimal(10,2)");
        builder.Property(c => c.AvatarPath).HasMaxLength(260);

        builder.HasIndex(c => c.Code).IsUnique().HasDatabaseName("IX_Clients_Code");
        builder.HasIndex(c => c.Name).HasDatabaseName("IX_Clients_Name");
    }
}
