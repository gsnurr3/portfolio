using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RESTfulAPI.Persistence.DbContextFactory
{
    public sealed class LogDbContextFactory : IDesignTimeDbContextFactory<LogDbContext>
    {
        public LogDbContext CreateDbContext(string[] args)
        {
            string conn = @"
                    Server=localhost;
                    Database=PortfolioApp_Dev;
                    Trusted_Connection=True;
                    TrustServerCertificate=True;";

            var opt = new DbContextOptionsBuilder<LogDbContext>()
                .UseSqlServer(conn)
                .Options;

            return new LogDbContext(opt);
        }
    }
}
