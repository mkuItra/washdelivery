using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WashDelivery.Domain.Entities;

namespace WashDelivery.Infrastructure.Data.Configurations;

public class LaundryConfiguration : IEntityTypeConfiguration<Laundry>
{
    public void Configure(EntityTypeBuilder<Laundry> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.ContactEmail)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.ContactPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(l => l.Rating)
            .HasPrecision(3, 2);

        builder.OwnsOne(l => l.Address);

        builder.HasMany(l => l.Workers)
            .WithOne(u => u.Laundry)
            .HasForeignKey(u => u.LaundryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
} 