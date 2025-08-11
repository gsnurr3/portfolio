using RESTfulAPI.Persistence;

namespace RESTfulAPI.Infrastructure.HostedServices
{
    // Wakes up the database in prod, so first call upon app startup doesn't fail. 
    public sealed class DbWarmupService : IHostedService
    {
        private readonly IServiceProvider _sp;
        public DbWarmupService(IServiceProvider sp) => _sp = sp;

        public async Task StartAsync(CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logDb = scope.ServiceProvider.GetRequiredService<LogDbContext>();

            try
            {
                await appDb.Database.CanConnectAsync(ct);
                await logDb.Database.CanConnectAsync(ct);
            }
            catch (Exception ex)
            {
                scope.ServiceProvider.GetRequiredService<ILogger<DbWarmupService>>()
                     .LogWarning(ex, "DB warm-up failed (continuing).");
            }
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
