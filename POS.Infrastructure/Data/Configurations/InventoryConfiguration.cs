using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventories");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.HasIndex(i => new { i.VariantId, i.StoreId }).IsUnique();

        builder.Ignore(i => i.QuantityAvailable);

        builder.HasOne(i => i.Variant)
               .WithMany(v => v.Inventories)
               .HasForeignKey(i => i.VariantId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Store)
               .WithMany(s => s.Inventories)
               .HasForeignKey(i => i.StoreId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
