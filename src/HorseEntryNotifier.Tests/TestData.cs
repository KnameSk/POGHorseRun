using HorseEntryNotifier.Core.Interfaces;
using HorseEntryNotifier.Core.Models;

namespace HorseEntryNotifier.Tests;

internal static class TestData
{
    public static RaceEntry RaceEntry(
        string horseName = "ジェットシェヴロン",
        string jockeyId = "new-jockey",
        string jockeyName = "新騎手",
        int? horseNumber = 7) => new()
    {
        RaceId = "race-001",
        RaceDate = new DateOnly(2026, 6, 27),
        RacecourseCode = "03",
        RacecourseName = "福島",
        RaceNumber = 5,
        RaceName = "2歳新馬",
        RaceClass = "2歳新馬",
        TrackType = "芝",
        Distance = 1800,
        StartTime = new TimeOnly(12, 25),
        HorseId = "horse-001",
        HorseName = horseName,
        JockeyId = jockeyId,
        JockeyName = jockeyName,
        FrameNumber = horseNumber is null ? null : 4,
        HorseNumber = horseNumber,
        RawData = "test"
    };

    public static RegisteredHorse RegisteredHorse(string name = "ジェットシェヴロン") => new()
    {
        Id = 1,
        HorseName = name,
        NormalizedHorseName = name,
        NominatorName = "田中",
        Enabled = true,
        CreatedAt = FixedClock.Instance.Now,
        UpdatedAt = FixedClock.Instance.Now
    };
}

internal sealed class FixedClock : IClock
{
    public static FixedClock Instance { get; } = new();
    public DateTimeOffset Now { get; } = new(2026, 6, 22, 10, 30, 0, TimeSpan.FromHours(9));
}
