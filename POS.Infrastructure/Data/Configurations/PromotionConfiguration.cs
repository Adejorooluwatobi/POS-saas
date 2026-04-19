using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.TenantId).IsRequired();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Type).HasConversion<string>().IsRequired();
        builder.Property(p => p.Scope).HasConversion<string>().IsRequired();
        builder.Property(p => p.Value).HasPrecision(10, 4).IsRequired();
        builder.Property(p => p.MinPurchase).HasPrecision(12, 2);
        builder.Property(p => p.MaxDiscount).HasPrecision(12, 2);

        builder.Property(p => p.Conditions)
               .HasColumnType("jsonb");

        builder.HasIndex(p => p.TenantId);
        builder.HasIndex(p => new { p.TenantId, p.IsActive, p.StartsAt, p.EndsAt });

        builder.HasOne(p => p.Tenant)
               .WithMany(t => t.Promotions)
               .HasForeignKey(p => p.TenantId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
