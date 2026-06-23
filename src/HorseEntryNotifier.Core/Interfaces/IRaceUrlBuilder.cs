using HorseEntryNotifier.Core.Models;

namespace HorseEntryNotifier.Core.Interfaces;

public interface IRaceUrlBuilder
{
    string BuildRaceUrl(RaceEntry entry);
}
