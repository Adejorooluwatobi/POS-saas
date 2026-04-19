using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class TillSessionConfiguration : IEntityTypeConfiguration<TillSession>
{
    public void Configure(EntityTypeBuilder<TillSession> builder)
    {
        builder.ToTable("TillSessions");
        builder.HasKey(ts => ts.Id);
        builder.Property(ts => ts.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(ts => ts.OpeningFloat).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(ts => ts.ClosingCash).HasPrecision(12, 2);
        builder.Property(ts => ts.ExpectedCash).HasPrecision(12, 2);
        builder.Property(ts => ts.Variance).HasPrecision(12, 2);
        builder.Property(ts => ts.Status).HasConversion<string>().IsRequired();

        builder.HasIndex(ts => ts.TerminalId);
        builder.HasIndex(ts => ts.StaffId);
        builder.HasIndex(ts => ts.Status);

        builder.HasOne(ts => ts.Terminal)
               .WithMany(t => t.TillSessions)
               .HasForeignKey(ts => ts.TerminalId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ts => ts.Staff)
               .WithMany(s => s.TillSessions)
               .HasForeignKey(ts => ts.StaffId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
