using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WashDelivery.Domain.Entities;

namespace WashDelivery.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public DbSet<Laundry> Laundries { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderHistory> OrderHistory { get; set; }
        public DbSet<LaundryWorker> LaundryWorkers { get; set; }
        public DbSet<LaundryManager> LaundryManagers { get; set; }
        public DbSet<Courier> Couriers { get; set; }
        public DbSet<CustomerDeliveryAddress> CustomerDeliveryAddresses { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; } = null!;
        public DbSet<DraftOrder> DraftOrders { get; set; } = null!;
        public DbSet<LaundryOrderView> LaundryOrderViews { get; set; }
        public DbSet<RejectedOrder> RejectedOrders { get; set; }
        public DbSet<CourierCompletedOrder> CourierCompletedOrders { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<User>("User")
                .HasValue<Customer>("Customer")
                .HasValue<Courier>("Courier")
                .HasValue<LaundryWorker>("LaundryWorker")
                .HasValue<LaundryManager>("LaundryManager");

            builder.Entity<Customer>()
                .ToTable("AspNetUsers");

            builder.Entity<CustomerDeliveryAddress>(b =>
            {
                b.HasOne<Customer>()
                 .WithMany(c => c.Addresses)
                 .HasForeignKey(a => a.CustomerId)
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Cascade);

                b.Property(a => a.Id).ValueGeneratedOnAdd();
                b.UsePropertyAccessMode(PropertyAccessMode.Property);
            });

            builder.Entity<LaundryOrderView>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Laundry)
                    .WithMany()
                    .HasForeignKey(e => e.LaundryId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasIndex(e => new { e.OrderId, e.LaundryId }).IsUnique();
            });

            builder.Entity<CourierCompletedOrder>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Courier)
                    .WithMany()
                    .HasForeignKey(e => e.CourierId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.CompletedAt)
                    .IsRequired();
            });

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
