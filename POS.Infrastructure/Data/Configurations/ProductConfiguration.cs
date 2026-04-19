using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.MasterSku).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(300);
        builder.Property(p => p.Brand).HasMaxLength(150);
        builder.Property(p => p.TaxCategory).HasConversion<string>().IsRequired();

        builder.HasIndex(p => p.MasterSku).IsUnique();
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.CategoryId);

        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
