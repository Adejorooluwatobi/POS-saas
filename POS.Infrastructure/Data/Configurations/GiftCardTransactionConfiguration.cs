using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class GiftCardTransactionConfiguration : IEntityTypeConfiguration<GiftCardTransaction>
{
    public void Configure(EntityTypeBuilder<GiftCardTransaction> builder)
    {
        builder.ToTable("GiftCardTransactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.TenantId).IsRequired();
        builder.Property(t => t.GiftCardId).IsRequired();
        builder.Property(t => t.Type).IsRequired();
        builder.Property(t => t.Amount).HasPrecision(12, 2).IsRequired();
        builder.Property(t => t.BalanceBefore).HasPrecision(12, 2).IsRequired();
        builder.Property(t => t.BalanceAfter).HasPrecision(12, 2).IsRequired();
        builder.Property(t => t.Method).IsRequired();
        builder.Property(t => t.Reference).HasMaxLength(100);
        builder.Property(t => t.Notes).HasMaxLength(500);

        builder.HasIndex(t => t.TenantId);
        builder.HasIndex(t => t.GiftCardId);
        builder.HasIndex(t => t.CreatedAt);

        builder.HasOne(t => t.Tenant)
               .WithMany()
               .HasForeignKey(t => t.TenantId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.GiftCard)
               .WithMany(g => g.Transactions)
               .HasForeignKey(t => t.GiftCardId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Store)
               .WithMany()
               .HasForeignKey(t => t.StoreId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Staff)
               .WithMany()
               .HasForeignKey(t => t.StaffId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
