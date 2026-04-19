using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Depth).IsRequired().HasDefaultValue(0);

        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => c.ParentId);

        builder.HasOne(c => c.Parent)
               .WithMany(c => c.Children)
               .HasForeignKey(c => c.ParentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
