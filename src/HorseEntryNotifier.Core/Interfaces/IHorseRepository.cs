using HorseEntryNotifier.Core.Models;

namespace HorseEntryNotifier.Core.Interfaces;

public interface IHorseRepository
{
    Task<RegisteredHorse?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken);
    Task<IReadOnlyList<RegisteredHorse>> ListAsync(bool enabledOnly, CancellationToken cancellationToken);
    Task AddAsync(RegisteredHorse horse, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
