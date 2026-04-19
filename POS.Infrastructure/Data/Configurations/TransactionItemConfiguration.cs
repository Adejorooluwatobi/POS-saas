using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class TransactionItemConfiguration : IEntityTypeConfiguration<TransactionItem>
{
    public void Configure(EntityTypeBuilder<TransactionItem> builder)
    {
        builder.ToTable("TransactionItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.Quantity).HasPrecision(10, 3).IsRequired();
        builder.Property(i => i.UnitPrice).HasPrecision(12, 2).IsRequired();
        builder.Property(i => i.OriginalPrice).HasPrecision(12, 2).IsRequired();
        builder.Property(i => i.DiscountAmount).HasPrecision(12, 2);
        builder.Property(i => i.TaxRate).HasPrecision(5, 4);
        builder.Property(i => i.TaxAmount).HasPrecision(12, 2);
        builder.Property(i => i.LineTotal).HasPrecision(12, 2).IsRequired();

        builder.HasIndex(i => i.TransactionId);
        builder.HasIndex(i => i.VariantId);

        builder.HasOne(i => i.Transaction)
               .WithMany(t => t.Items)
               .HasForeignKey(i => i.TransactionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Variant)
               .WithMany(v => v.TransactionItems)
               .HasForeignKey(i => i.VariantId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
