using h.Server.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Infrastructure;

/// <summary>
/// Also used as the startup pipeline for the application.
/// (The App.MapXYZ or App.UseXYZ)
/// </summary>
public static class RequestPipeline
{
    public static async Task TryMigrateDbAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await db.Database.MigrateAsync();
        }
    }
}
