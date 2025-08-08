using Microsoft.EntityFrameworkCore;
using RESTfulAPI.Domain.Entities;

namespace RESTfulAPI.Persistence;

public partial class LogDbContext : DbContext
{
    public LogDbContext(DbContextOptions<LogDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<RequestLog> RequestLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RequestLog>(entity =>
        {
            entity.HasIndex(e => e.RequestTime, "IX_RequestLogs_Errors")
                .IsDescending()
                .HasFilter("([StatusCode]>=(500))");

            entity.Property(e => e.RequestDate).HasComputedColumnSql("(CONVERT([date],[RequestTime]))", true);
            entity.Property(e => e.RequestId).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.RequestTime).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ServerName).HasDefaultValueSql("(host_name())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
