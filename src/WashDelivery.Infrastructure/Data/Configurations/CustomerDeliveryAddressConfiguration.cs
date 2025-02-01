using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WashDelivery.Domain.Entities;

namespace WashDelivery.Infrastructure.Data.Configurations;

public class CustomerDeliveryAddressConfiguration : IEntityTypeConfiguration<CustomerDeliveryAddress>
{
    public void Configure(EntityTypeBuilder<CustomerDeliveryAddress> builder)
    {
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Name).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Street).IsRequired().HasMaxLength(200);
        builder.Property(a => a.BuildingNumber).IsRequired().HasMaxLength(10);
        builder.Property(a => a.ApartmentNumber).HasMaxLength(10);
        builder.Property(a => a.City).IsRequired().HasMaxLength(100);
        builder.Property(a => a.PostalCode).IsRequired().HasMaxLength(10);
        builder.Property(a => a.AdditionalInstructions).HasMaxLength(500);
        
        builder.HasOne<Customer>()
            .WithMany(c => c.Addresses)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 