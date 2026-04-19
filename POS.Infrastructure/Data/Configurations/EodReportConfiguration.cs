using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class EodReportConfiguration : IEntityTypeConfiguration<EodReport>
{
    public void Configure(EntityTypeBuilder<EodReport> builder)
    {
        builder.ToTable("EodReports");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.TotalSales).HasPrecision(12, 2);
        builder.Property(e => e.TotalReturns).HasPrecision(12, 2);
        builder.Property(e => e.NetSales).HasPrecision(12, 2);
        builder.Property(e => e.TotalTax).HasPrecision(12, 2);
        builder.Property(e => e.TotalDiscounts).HasPrecision(12, 2);
        builder.Property(e => e.CashCollected).HasPrecision(12, 2);
        builder.Property(e => e.CardCollected).HasPrecision(12, 2);
        builder.Property(e => e.OtherCollected).HasPrecision(12, 2);

        builder.HasIndex(e => new { e.StoreId, e.ReportDate });
        builder.HasIndex(e => new { e.StoreId, e.SessionId, e.ReportDate }).IsUnique();
    }
}
