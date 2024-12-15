using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace h.Server.Infrastructure.Database;

public class AutoSetUpdatedAtDbSaveInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;

    public AutoSetUpdatedAtDbSaveInterceptor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if(dbContext is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Modified)
                continue;
            
            //// If has property
            //if (entry.Properties.Any(p => p.Metadata.Name == "UpdatedAt") == false)
            //    continue;

            // Update it
            // Todo: this might be a problem if the property doesnt exist. Ignore for now...
            entry.Property("UpdatedAt").CurrentValue = _timeProvider.GetUtcNow();
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
