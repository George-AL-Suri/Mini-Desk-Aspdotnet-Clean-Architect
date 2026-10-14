using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MiniDesk.Infrastructure.Persistence;

namespace MiniDesk.Infrastructure.Auth;

internal class RefreshTokenCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<RefreshTokenCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var limit = DateTime.UtcNow.AddDays(-30);
                var deleted = await db.RefreshTokens
                    .Where(t => t.ExpiresAt < limit)
                    .ExecuteDeleteAsync(ct);

                logger.LogInformation("Refresh token cleanup removed {Count} rows.", deleted);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Refresh token cleanup failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(ct));
    }
}