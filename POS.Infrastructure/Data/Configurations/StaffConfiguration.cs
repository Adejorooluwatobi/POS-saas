using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;
using POS.Domain.Enums;

namespace POS.Infrastructure.Data.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staff");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.TenantId).IsRequired();
        builder.Property(s => s.SystemRole).HasConversion<string>().IsRequired();
        builder.Property(s => s.EmployeeNo).IsRequired().HasMaxLength(30);
        builder.Property(s => s.Email).IsRequired().HasMaxLength(200);
        builder.Property(s => s.PinHash).IsRequired().HasMaxLength(255);
        builder.Property(s => s.PasswordHash).HasMaxLength(255);
        builder.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.LastName).IsRequired().HasMaxLength(100);

        builder.Ignore(s => s.FullName);

        builder.HasIndex(s => s.EmployeeNo).IsUnique();
        builder.HasIndex(s => s.Email).IsUnique();
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => new { s.TenantId, s.SystemRole });

        builder.HasOne(s => s.Tenant)
               .WithMany(t => t.Staff)
               .HasForeignKey(s => s.TenantId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Store)
               .WithMany(st => st.Staff)
               .HasForeignKey(s => s.StoreId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Role)
               .WithMany(r => r.StaffMembers)
               .HasForeignKey(s => s.RoleId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(new Staff
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            SystemRole = SystemRole.SuperAdmin,
            EmployeeNo = "SYS-ADM",
            Email = "admin@retailos.com",
            FirstName = "System",
            LastName = "Admin",
            PasswordHash = "AQAAAAIAAYagAAAAEFvn4SFv5/GxAt2+VZlbkxht0TV1NDZsRCBiFRT61dFmGmAWgtnSSrTT1q6iQElF1Q==",
            PinHash = "AQAAAAIAAYagAAAAEFvn4SFv5/GxAt2+VZlbkxht0TV1NDZsRCBiFRT61dFmGmAWgtnSSrTT1q6iQElF1Q==",
            HiredAt = DateOnly.FromDateTime(new DateTime(2026, 1, 1)),
            IsActive = true,
            CreatedAt = DateTimeOffset.Parse("2026-01-01T00:00:00Z")
        });
    }
}
