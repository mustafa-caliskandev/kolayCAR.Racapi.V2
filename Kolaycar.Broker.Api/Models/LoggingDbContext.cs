using KolayCAR.Broker.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace KolayCAR.Broker.API.Models
{
    public class LoggingDbContext : DbContext
    {
        public DbSet<Brokerlog> BrokerLogs { get; set; }

        public LoggingDbContext(DbContextOptions<LoggingDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Brokerlog>(entity =>
            {
                entity.ToTable("BROKERLOG");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Content).HasColumnName("CONTENT");

                entity.Property(e => e.Logdate)
                    .HasColumnName("LOGDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Logkey).HasColumnName("LOGKEY");

                entity.Property(e => e.Logtype).HasColumnName("LOGTYPE");

                entity.Property(e => e.Logtypekey)
                    .IsRequired()
                    .HasColumnName("LOGTYPEKEY");
            });
        }
    }
}
