using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.Method).HasConversion<string>().IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().IsRequired();
        builder.Property(p => p.Amount).HasPrecision(12, 2).IsRequired();
        builder.Property(p => p.AmountTendered).HasPrecision(12, 2);
        builder.Property(p => p.ChangeGiven).HasPrecision(12, 2);
        builder.Property(p => p.GatewayRef).HasMaxLength(100);
        builder.Property(p => p.GatewayResponse).HasColumnType("jsonb");

        builder.HasIndex(p => p.TransactionId);
        builder.HasIndex(p => p.GatewayRef);

        builder.HasOne(p => p.Transaction)
               .WithMany(t => t.Payments)
               .HasForeignKey(p => p.TransactionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.GiftCard)
               .WithMany(g => g.Payments)
               .HasForeignKey(p => p.GiftCardId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
