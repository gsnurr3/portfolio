using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RESTfulAPI.Persistence.DbContextFactory
{
    public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        string conn = @"
                Server=localhost;
                Database=PortfolioApp_Dev;
                Trusted_Connection=True;
                TrustServerCertificate=True;";

        public AppDbContext CreateDbContext(string[] args)
        {
            var opt = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(conn)
                .Options;

            return new AppDbContext(opt);
        }
    }
}

