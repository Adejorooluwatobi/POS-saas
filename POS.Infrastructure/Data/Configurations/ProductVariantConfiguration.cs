using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(v => v.Barcode).IsRequired().HasMaxLength(100);
        builder.Property(v => v.Sku).IsRequired().HasMaxLength(100);
        builder.Property(v => v.UnitOfMeasure).IsRequired().HasMaxLength(30).HasDefaultValue("Each");
        builder.Property(v => v.BasePrice).HasPrecision(12, 2).IsRequired();
        builder.Property(v => v.CostPrice).HasPrecision(12, 2);
        builder.Property(v => v.WeightGrams).HasPrecision(10, 3);
        builder.Property(v => v.LowStockThreshold).HasDefaultValue(10);
        
        builder.Property(v => v.IsBaseUnit).HasDefaultValue(true);
        builder.Property(v => v.ConversionFactor).HasPrecision(12, 4).HasDefaultValue(1m);

        // JSONB column
        builder.Property(v => v.Attributes)
               .HasColumnType("jsonb")
               .HasDefaultValueSql("'{}'::jsonb");

        builder.HasIndex(v => v.Barcode).IsUnique();
        builder.HasIndex(v => v.Sku).IsUnique();
        builder.HasIndex(v => v.ProductId);

        builder.HasOne(v => v.Product)
               .WithMany(p => p.Variants)
               .HasForeignKey(v => v.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.BaseVariant)
               .WithMany()
               .HasForeignKey(v => v.BaseVariantId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.BaseVariantId);
    }
}
