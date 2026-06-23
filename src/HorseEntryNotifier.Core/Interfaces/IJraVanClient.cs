using HorseEntryNotifier.Core.Models;

namespace HorseEntryNotifier.Core.Interfaces;

public interface IJraVanClient
{
    Task<IReadOnlyList<RaceEntry>> GetRaceEntriesAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken);
}
