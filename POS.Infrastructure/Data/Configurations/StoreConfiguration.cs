using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.TenantId).IsRequired();
        builder.Property(s => s.Code).IsRequired().HasMaxLength(10);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Address).IsRequired();
        builder.Property(s => s.City).IsRequired().HasMaxLength(100);
        builder.Property(s => s.State).HasMaxLength(100);
        builder.Property(s => s.Country).IsRequired().HasMaxLength(100).HasDefaultValue("Nigeria");
        builder.Property(s => s.Phone).HasMaxLength(30);
        builder.Property(s => s.Timezone).IsRequired().HasMaxLength(60).HasDefaultValue("Africa/Lagos");

        builder.HasIndex(s => s.Code).IsUnique();
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => new { s.TenantId, s.IsActive });

        builder.HasOne(s => s.Tenant)
               .WithMany(t => t.Stores)
               .HasForeignKey(s => s.TenantId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
