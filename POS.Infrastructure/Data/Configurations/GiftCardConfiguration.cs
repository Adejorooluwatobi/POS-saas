using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class GiftCardConfiguration : IEntityTypeConfiguration<GiftCard>
{
    public void Configure(EntityTypeBuilder<GiftCard> builder)
    {
        builder.ToTable("GiftCards");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(g => g.TenantId).IsRequired();
        builder.Property(g => g.CardNumber).IsRequired().HasMaxLength(50);
        builder.Property(g => g.PinHash).HasMaxLength(255);
        builder.Property(g => g.Balance).HasPrecision(12, 2).IsRequired();
        builder.Property(g => g.InitialValue).HasPrecision(12, 2).IsRequired();

        builder.HasIndex(g => g.TenantId);
        builder.HasIndex(g => new { g.TenantId, g.CardNumber }).IsUnique();

        builder.HasOne(g => g.Tenant)
               .WithMany(t => t.GiftCards)
               .HasForeignKey(g => g.TenantId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
