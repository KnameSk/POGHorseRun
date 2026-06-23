using Microsoft.EntityFrameworkCore;

namespace HorseEntryNotifier.Infrastructure.Persistence;

public sealed class DatabaseInitializer(AppDbContext dbContext)
{
    public Task InitializeAsync(CancellationToken cancellationToken) =>
        dbContext.Database.EnsureCreatedAsync(cancellationToken);
}
