using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class StockRequisitionConfiguration : IEntityTypeConfiguration<StockRequisition>
{
    public void Configure(EntityTypeBuilder<StockRequisition> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RequisitionNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RequestingStore)
            .WithMany()
            .HasForeignKey(x => x.RequestingStoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedBy)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RequisitionNumber).IsUnique();
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.RequestingStoreId);
    }
}
