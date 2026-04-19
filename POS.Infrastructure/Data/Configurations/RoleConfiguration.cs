using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Permissions)
               .HasColumnType("jsonb")
               .HasDefaultValueSql("'{}'::jsonb");

        builder.HasIndex(r => r.Name).IsUnique();
    }
}
