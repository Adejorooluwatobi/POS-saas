using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class InventoryOrderConfiguration : IEntityTypeConfiguration<InventoryOrder>
{
    public void Configure(EntityTypeBuilder<InventoryOrder> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourceStore)
            .WithMany()
            .HasForeignKey(x => x.SourceStoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DestinationStore)
            .WithMany()
            .HasForeignKey(x => x.DestinationStoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReceivedBy)
            .WithMany()
            .HasForeignKey(x => x.ReceivedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedBy)
            .WithMany()
            .HasForeignKey(x => x.ApprovedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ResolvedBy)
            .WithMany()
            .HasForeignKey(x => x.ResolvedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StockRequisition)
            .WithMany(x => x.FulfillmentOrders)
            .HasForeignKey(x => x.StockRequisitionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.SourceStoreId);
        builder.HasIndex(x => x.DestinationStoreId);
    }
}
