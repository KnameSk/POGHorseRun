using HorseEntryNotifier.Infrastructure.Configuration;
using HorseEntryNotifier.Infrastructure.RaceUrl;

namespace HorseEntryNotifier.Tests;

public sealed class RaceUrlBuilderTests
{
    [Fact]
    public void BuildRaceUrl_ReplacesConfiguredPlaceholders()
    {
        var options = new RaceUrlOptions
        {
            Template = "https://example.test/race/{raceDate}/{racecourseCode}/{raceNumber}/{raceId}",
            FallbackUrl = "https://www.jra.go.jp/JRADB/accessD.html"
        };
        var builder = new TemplateRaceUrlBuilder(options);

        var result = builder.BuildRaceUrl(TestData.RaceEntry());

        Assert.Equal("https://example.test/race/20260627/03/05/race-001", result);
    }

    [Fact]
    public void BuildRaceUrl_UsesOfficialFallbackWhenTemplateIsEmpty()
    {
        var builder = new TemplateRaceUrlBuilder(new RaceUrlOptions());

        Assert.Equal(
            "https://www.jra.go.jp/JRADB/accessD.html",
            builder.BuildRaceUrl(TestData.RaceEntry()).TrimEnd('/'));
    }
}
