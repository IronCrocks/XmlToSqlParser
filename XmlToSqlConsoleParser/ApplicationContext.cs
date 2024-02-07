using Microsoft.EntityFrameworkCore;

namespace XmlToSqlConsoleParser
{
    internal class ApplicationContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=1.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Order>()
                .HasMany(c => c.Products)
                .WithMany(s => s.Orders)
                .UsingEntity<OrderItem>(
                   j => j
                    .HasOne(pt => pt.Product)
                    .WithMany(t => t.OrderItems)
                    .HasForeignKey(pt => pt.ProductId),
                j => j
                    .HasOne(pt => pt.Order)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(pt => pt.OrderId),
                j =>
                {
                    j.Property(pt => pt.Count).HasDefaultValue(1);
                    j.HasKey(t => t.Id);
                    j.ToTable("OrderItems");
                });
        }
    }
}
