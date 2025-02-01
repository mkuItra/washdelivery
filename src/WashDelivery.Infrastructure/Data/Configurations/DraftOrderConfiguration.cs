using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WashDelivery.Domain.Entities;

namespace WashDelivery.Infrastructure.Data.Configurations;

public class DraftOrderConfiguration : IEntityTypeConfiguration<DraftOrder>
{
    public void Configure(EntityTypeBuilder<DraftOrder> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.LastModified)
            .IsRequired();

        builder.Property(o => o.ExpiresAt);

        builder.OwnsOne(o => o.PickupAddress);
        builder.OwnsOne(o => o.DeliveryAddress);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.DraftOrder)
            .HasForeignKey(i => i.DraftOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 