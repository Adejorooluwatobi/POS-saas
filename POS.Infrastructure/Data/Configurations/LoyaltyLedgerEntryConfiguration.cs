using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class LoyaltyLedgerEntryConfiguration : IEntityTypeConfiguration<LoyaltyLedgerEntry>
{
    public void Configure(EntityTypeBuilder<LoyaltyLedgerEntry> builder)
    {
        builder.ToTable("LoyaltyLedger");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(l => l.Reason).IsRequired().HasMaxLength(100);

        builder.HasIndex(l => l.CustomerId);
        builder.HasIndex(l => l.TransactionId);

        builder.HasOne(l => l.Customer)
               .WithMany(c => c.LoyaltyLedger)
               .HasForeignKey(l => l.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Transaction)
               .WithMany()
               .HasForeignKey(l => l.TransactionId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
