using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.TenantId).IsRequired();
        builder.Property(c => c.LoyaltyCardNo).HasMaxLength(30);
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.Phone).HasMaxLength(30);
        builder.Property(c => c.FirstName).HasMaxLength(100);
        builder.Property(c => c.LastName).HasMaxLength(100);
        builder.Property(c => c.Tier).HasConversion<string>().IsRequired();

        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => new { c.TenantId, c.LoyaltyCardNo }).IsUnique();
        builder.HasIndex(c => new { c.TenantId, c.Email }).IsUnique();

        builder.HasOne(c => c.Tenant)
               .WithMany(t => t.Customers)
               .HasForeignKey(c => c.TenantId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
