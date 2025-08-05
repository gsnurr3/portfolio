using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class LogDbContextFactory : IDesignTimeDbContextFactory<LogDbContext>
{
    public LogDbContext CreateDbContext(string[] args)
    {
        string conn = @"
                    Server=localhost;
                    Database=PortfolioApp_Dev;
                    Trusted_Connection=True;
                    TrustServerCertificate=True;";

        // string conn = "Server=tcp:portfolio-sql-server-84.database.windows.net,1433;" +
        //                             "Database=portfolio-db;" +
        //                             "Authentication=Active Directory Default;";

        var opt = new DbContextOptionsBuilder<LogDbContext>()
            .UseSqlServer(conn)
            .Options;

        return new LogDbContext(opt);
    }
}