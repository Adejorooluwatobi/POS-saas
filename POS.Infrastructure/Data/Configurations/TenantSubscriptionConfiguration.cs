using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("TenantSubscriptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.Plan).HasConversion<string>().IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().IsRequired();
        builder.Property(s => s.BillingCycle).HasConversion<string>().IsRequired();
        builder.Property(s => s.MonthlyPrice).HasPrecision(10, 2);

        builder.Ignore(s => s.IsExpired);

        builder.HasIndex(s => s.TenantId).IsUnique();
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.CurrentPeriodEnd);
    }
}
