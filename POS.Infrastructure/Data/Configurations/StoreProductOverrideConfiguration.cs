using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class StoreProductOverrideConfiguration : IEntityTypeConfiguration<StoreProductOverride>
{
    public void Configure(EntityTypeBuilder<StoreProductOverride> builder)
    {
        builder.HasKey(o => o.Id);

        // Enforce uniqueness for (StoreId, ProductId)
        builder.HasIndex(o => new { o.StoreId, o.ProductId })
            .IsUnique();

        builder.HasOne(o => o.Product)
            .WithMany(p => p.StoreOverrides)
            .HasForeignKey(o => o.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Store)
            .WithMany()
            .HasForeignKey(o => o.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Tenant)
            .WithMany()
            .HasForeignKey(o => o.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
