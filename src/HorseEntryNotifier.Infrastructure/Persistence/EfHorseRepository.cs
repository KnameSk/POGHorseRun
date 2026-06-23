using HorseEntryNotifier.Core.Interfaces;
using HorseEntryNotifier.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HorseEntryNotifier.Infrastructure.Persistence;

public sealed class EfHorseRepository(AppDbContext dbContext) : IHorseRepository
{
    public Task<RegisteredHorse?> GetByNormalizedNameAsync(
        string normalizedName,
        CancellationToken cancellationToken) =>
        dbContext.RegisteredHorses.SingleOrDefaultAsync(
            horse => horse.NormalizedHorseName == normalizedName,
            cancellationToken);

    public async Task<IReadOnlyList<RegisteredHorse>> ListAsync(
        bool enabledOnly,
        CancellationToken cancellationToken)
    {
        var query = dbContext.RegisteredHorses.AsNoTracking();
        if (enabledOnly)
        {
            query = query.Where(horse => horse.Enabled);
        }

        return await query.OrderBy(horse => horse.HorseName).ToListAsync(cancellationToken);
    }

    public Task AddAsync(RegisteredHorse horse, CancellationToken cancellationToken) =>
        dbContext.RegisteredHorses.AddAsync(horse, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
