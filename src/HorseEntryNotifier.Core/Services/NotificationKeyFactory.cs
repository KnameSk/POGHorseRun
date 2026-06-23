using HorseEntryNotifier.Core.Models;

namespace HorseEntryNotifier.Core.Services;

public static class NotificationKeyFactory
{
    public static string Entry(RaceEntry entry) => $"entry:{entry.RaceId}:{entry.HorseId}";

    public static string JockeyUpdate(RaceEntry entry)
    {
        var jockeyKey = string.IsNullOrWhiteSpace(entry.JockeyId)
            ? entry.JockeyName ?? "unknown"
            : entry.JockeyId;
        return $"update-jockey:{entry.RaceId}:{entry.HorseId}:{jockeyKey}";
    }

    public static string NumberUpdate(RaceEntry entry) =>
        $"update-number:{entry.RaceId}:{entry.HorseId}:{entry.HorseNumber}";
}
