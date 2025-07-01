using Microsoft.EntityFrameworkCore;

namespace PaymentManager.Models
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Prevenir duplicados
            modelBuilder.Entity<Payment>()
                .HasIndex(p => new { p.Reference, p.Category, p.Amount })
                .IsUnique()
                .HasDatabaseName("IX_Payment_Unique");

            // Configurar decimales
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);
        }
    }
}