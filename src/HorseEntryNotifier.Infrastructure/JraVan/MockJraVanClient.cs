using System.Text.Json;
using HorseEntryNotifier.Core.Interfaces;
using HorseEntryNotifier.Core.Models;
using HorseEntryNotifier.Infrastructure.Configuration;

namespace HorseEntryNotifier.Infrastructure.JraVan;

public sealed class MockJraVanClient(JraVanOptions options) : IJraVanClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<RaceEntry>> GetRaceEntriesAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        var path = ResolvePath(options.SampleDataPath);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"モック出馬表が見つかりません。JraVan:SampleDataPath を確認してください: {path}",
                path);
        }

        await using var stream = File.OpenRead(path);
        var entries = await JsonSerializer.DeserializeAsync<List<RaceEntry>>(
            stream,
            SerializerOptions,
            cancellationToken) ?? [];

        return entries.Where(entry => entry.RaceDate >= from && entry.RaceDate <= to).ToArray();
    }

    private static string ResolvePath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        var outputPath = Path.GetFullPath(configuredPath, AppContext.BaseDirectory);
        if (File.Exists(outputPath))
        {
            return outputPath;
        }

        return Path.GetFullPath(configuredPath, Directory.GetCurrentDirectory());
    }
}
