using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class InventoryOrderItemConfiguration : IEntityTypeConfiguration<InventoryOrderItem>
{
    public void Configure(EntityTypeBuilder<InventoryOrderItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.InventoryOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Variant)
            .WithMany(x => x.InventoryOrderItems)
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.InventoryOrderId);
        builder.HasIndex(x => x.VariantId);
    }
}
