using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class PricingRuleConfiguration : IEntityTypeConfiguration<PricingRule>
{
    public void Configure(EntityTypeBuilder<PricingRule> builder)
    {
        builder.ToTable("PricingRules");
        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(pr => pr.SalePrice).HasPrecision(12, 2).IsRequired();
        
        builder.HasIndex(pr => pr.VariantId);
        builder.HasIndex(pr => pr.StoreId);

        builder.HasOne(pr => pr.Variant)
               .WithMany(v => v.PricingRules)
               .HasForeignKey(pr => pr.VariantId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
