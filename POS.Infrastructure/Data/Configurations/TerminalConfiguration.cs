using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class TerminalConfiguration : IEntityTypeConfiguration<Terminal>
{
    public void Configure(EntityTypeBuilder<Terminal> builder)
    {
        builder.ToTable("Terminals");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.TerminalCode).IsRequired().HasMaxLength(30);
        builder.Property(t => t.Label).HasMaxLength(100);
        builder.Property(t => t.IpAddress).HasMaxLength(45);
        builder.Property(t => t.Status).HasConversion<string>().IsRequired();

        builder.HasIndex(t => t.TerminalCode).IsUnique();
        builder.HasIndex(t => t.StoreId);

        builder.HasOne(t => t.Store)
               .WithMany(s => s.Terminals)
               .HasForeignKey(t => t.StoreId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
