using HorseEntryNotifier.Infrastructure.Configuration;
using HorseEntryNotifier.Infrastructure.JraVan;

namespace HorseEntryNotifier.Tests;

public sealed class MockJraVanClientTests
{
    [Fact]
    public async Task GetRaceEntriesAsync_LoadsAndFiltersSampleData()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "sample-data", "race_entries_sample.json");
        var client = new MockJraVanClient(new JraVanOptions { SampleDataPath = path });

        var entries = await client.GetRaceEntriesAsync(
            new DateOnly(2026, 6, 27),
            new DateOnly(2026, 6, 27),
            CancellationToken.None);

        var entry = Assert.Single(entries);
        Assert.Equal("ジェットシェヴロン", entry.HorseName);
        Assert.Equal(7, entry.HorseNumber);
    }
}
