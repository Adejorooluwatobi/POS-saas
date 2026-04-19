using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.ReceiptNumber).IsRequired().HasMaxLength(30);
        builder.Property(t => t.Type).HasConversion<string>().IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().IsRequired();
        builder.Property(t => t.Subtotal).HasPrecision(12, 2);
        builder.Property(t => t.DiscountTotal).HasPrecision(12, 2);
        builder.Property(t => t.TaxTotal).HasPrecision(12, 2);
        builder.Property(t => t.GrandTotal).HasPrecision(12, 2);
        builder.Property(t => t.AmountPaid).HasPrecision(12, 2);
        builder.Property(t => t.ChangeGiven).HasPrecision(12, 2);

        builder.HasIndex(t => t.ReceiptNumber).IsUnique();
        builder.HasIndex(t => new { t.StoreId, t.CreatedAt });
        builder.HasIndex(t => t.CashierId);
        builder.HasIndex(t => t.CustomerId);
        builder.HasIndex(t => t.Status);

        builder.HasOne(t => t.Session)
               .WithMany(s => s.Transactions)
               .HasForeignKey(t => t.SessionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Store)
               .WithMany(s => s.Transactions)
               .HasForeignKey(t => t.StoreId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Cashier)
               .WithMany(s => s.Transactions)
               .HasForeignKey(t => t.CashierId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Customer)
               .WithMany(c => c.Transactions)
               .HasForeignKey(t => t.CustomerId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.VoidRef)
               .WithMany()
               .HasForeignKey(t => t.VoidRefId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
