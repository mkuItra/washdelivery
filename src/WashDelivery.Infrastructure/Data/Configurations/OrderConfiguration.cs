using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WashDelivery.Domain.Entities;

namespace WashDelivery.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.LaundryId)
            .IsRequired(false);

        builder.Property(o => o.CourierId)
            .IsRequired(false);

        builder.Property(o => o.Status)
            .IsRequired();

        builder.Property(o => o.PickupTime)
            .IsRequired();

        builder.Property(o => o.LeaveAtDoor)
            .IsRequired();

        builder.Property(o => o.CourierInstructions)
            .IsRequired(false);

        builder.Property(o => o.FinalPrice)
            .HasPrecision(10, 2);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.OwnsOne(o => o.PickupAddress);
        builder.OwnsOne(o => o.DeliveryAddress);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.StatusHistory)
            .WithOne(h => h.Order)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 