using Microsoft.EntityFrameworkCore;

public sealed class LogDbContext : DbContext
{
    public LogDbContext(DbContextOptions<LogDbContext> o) : base(o) { }

    public DbSet<AppLog> AppLogs => Set<AppLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("log");
        builder.Entity<AppLog>()
                .ToTable("AppLogs");
    }
}