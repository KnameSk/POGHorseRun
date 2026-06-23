using HorseEntryNotifier.Core.Services;

namespace HorseEntryNotifier.Tests;

public sealed class RaceEntryMatcherTests
{
    [Fact]
    public void IsMatch_MatchesRegisteredHorseAfterNormalization()
    {
        var normalizer = new HorseNameNormalizer();
        var matcher = new RaceEntryMatcher(normalizer);
        var horse = TestData.RegisteredHorse();
        horse.NormalizedHorseName = normalizer.Normalize("ジェットシェヴロン");

        Assert.True(matcher.IsMatch(horse, TestData.RaceEntry(" ジェット　シェヴロン ")));
    }
}
