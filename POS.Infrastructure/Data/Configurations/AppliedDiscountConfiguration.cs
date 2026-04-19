using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class AppliedDiscountConfiguration : IEntityTypeConfiguration<AppliedDiscount>
{
    public void Configure(EntityTypeBuilder<AppliedDiscount> builder)
    {
        builder.ToTable("AppliedDiscounts");
        builder.HasKey(ad => ad.Id);
        builder.Property(ad => ad.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(ad => ad.Type).HasConversion<string>().IsRequired();
        builder.Property(ad => ad.Amount).HasPrecision(12, 2).IsRequired();

        builder.HasIndex(ad => ad.TransactionId);
        builder.HasIndex(ad => ad.ItemId);

        builder.HasOne(ad => ad.Transaction)
               .WithMany(t => t.AppliedDiscounts)
               .HasForeignKey(ad => ad.TransactionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ad => ad.Item)
               .WithMany(i => i.AppliedDiscounts)
               .HasForeignKey(ad => ad.ItemId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ad => ad.Promotion)
               .WithMany(p => p.AppliedDiscounts)
               .HasForeignKey(ad => ad.PromotionId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ad => ad.Coupon)
               .WithMany(c => c.AppliedDiscounts)
               .HasForeignKey(ad => ad.CouponId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ad => ad.AppliedBy)
               .WithMany()
               .HasForeignKey(ad => ad.AppliedById)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
