using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WashDelivery.Domain.Entities;

namespace WashDelivery.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.OrderId)
            .IsRequired(false);

        builder.Property(i => i.DraftOrderId)
            .IsRequired(false);

        builder.Property(i => i.ServiceId)
            .IsRequired();

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Price)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(i => i.Weight)
            .HasPrecision(10, 3);

        builder.Property(i => i.IsExtra)
            .IsRequired();

        builder.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.DraftOrder)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.DraftOrderId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 