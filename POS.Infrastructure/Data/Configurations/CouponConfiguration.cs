using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);

        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => c.PromotionId);

        builder.HasOne(c => c.Promotion)
               .WithMany(p => p.Coupons)
               .HasForeignKey(c => c.PromotionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
