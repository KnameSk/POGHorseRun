using HorseEntryNotifier.Core.Models;

namespace HorseEntryNotifier.Core.Services;

public sealed class RaceEntryMatcher(HorseNameNormalizer normalizer)
{
    public bool IsMatch(RegisteredHorse horse, RaceEntry entry) =>
        horse.Enabled && horse.NormalizedHorseName == normalizer.Normalize(entry.HorseName);
}
