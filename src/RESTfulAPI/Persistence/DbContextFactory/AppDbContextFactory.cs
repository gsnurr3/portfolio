using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    string conn = @"
                Server=localhost;
                Database=PortfolioApp_Dev;
                Trusted_Connection=True;
                TrustServerCertificate=True;";

    // string conn = "Server=tcp:portfolio-sql-server-84.database.windows.net,1433;" +
    //                             "Database=portfolio-db;" +
    //                             "Authentication=Active Directory Default;";

    public AppDbContext CreateDbContext(string[] args)
    {
        var opt = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(conn)
            .Options;

        return new AppDbContext(opt);
    }
}
