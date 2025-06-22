using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt);

                entity.OwnsOne(e => e.ShippingAddress, address =>
                {
                    address.Property(a => a.Street).IsRequired();
                    address.Property(a => a.City).IsRequired();
                    address.Property(a => a.State).IsRequired();
                    address.Property(a => a.Country).IsRequired();
                    address.Property(a => a.ZipCode).IsRequired();
                });

                entity.HasMany(e => e.Items)
                    .WithOne(i => i.Order)
                    .HasForeignKey(i => i.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductId).IsRequired();
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            });
        }
    }
} 