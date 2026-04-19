using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(a => a.Action).HasConversion<string>().IsRequired();
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityId).IsRequired().HasMaxLength(100);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.Changes).HasColumnType("jsonb");

        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => a.CreatedAt).IsDescending();
        builder.HasIndex(a => new { a.TenantId, a.EntityType, a.EntityId });
        builder.HasIndex(a => a.UserId);

        builder.HasOne(a => a.Tenant)
               .WithMany()
               .HasForeignKey(a => a.TenantId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.User)
               .WithMany(s => s.AuditLogs)
               .HasForeignKey(a => a.UserId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
