using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class StockRequisitionItemConfiguration : IEntityTypeConfiguration<StockRequisitionItem>
{
    public void Configure(EntityTypeBuilder<StockRequisitionItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Requisition)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.StockRequisitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Variant)
            .WithMany(x => x.StockRequisitionItems)
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.StockRequisitionId);
        builder.HasIndex(x => x.VariantId);
    }
}
