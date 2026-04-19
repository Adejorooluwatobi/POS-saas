using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.Slug).IsRequired().HasMaxLength(80);
        builder.Property(t => t.BusinessName).IsRequired().HasMaxLength(200);
        builder.Property(t => t.ContactEmail).IsRequired().HasMaxLength(200);
        builder.Property(t => t.ContactPhone).HasMaxLength(30);
        builder.Property(t => t.Country).IsRequired().HasMaxLength(100).HasDefaultValue("Nigeria");
        builder.Property(t => t.LogoUrl).HasMaxLength(500);

        builder.HasIndex(t => t.Slug).IsUnique();
        builder.HasIndex(t => t.ContactEmail).IsUnique();
        builder.HasIndex(t => t.IsActive);

        // 1:1 with TenantSubscription
        builder.HasOne(t => t.Subscription)
               .WithOne(s => s.Tenant)
               .HasForeignKey<TenantSubscription>(s => s.TenantId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(new Tenant
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Slug = "system",
            BusinessName = "RetailOS System",
            ContactEmail = "admin@retailos.com",
            Country = "Global",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
