using System.Globalization;
using HorseEntryNotifier.Core.Interfaces;
using HorseEntryNotifier.Core.Models;
using HorseEntryNotifier.Infrastructure.Configuration;

namespace HorseEntryNotifier.Infrastructure.RaceUrl;

public sealed class TemplateRaceUrlBuilder(RaceUrlOptions options) : IRaceUrlBuilder
{
    public string BuildRaceUrl(RaceEntry entry)
    {
        if (Uri.TryCreate(entry.RaceId, UriKind.Absolute, out var raceIdUri) &&
            IsHttp(raceIdUri))
        {
            return raceIdUri.ToString();
        }

        if (string.IsNullOrWhiteSpace(options.Template))
        {
            return SafeFallback();
        }

        var value = options.Template
            .Replace("{raceId}", entry.RaceId, StringComparison.OrdinalIgnoreCase)
            .Replace("{raceDate}", entry.RaceDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{raceDateIso}", entry.RaceDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{year}", entry.RaceDate.Year.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{racecourseCode}", entry.RacecourseCode, StringComparison.OrdinalIgnoreCase)
            .Replace("{raceNumber}", entry.RaceNumber.ToString("00", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{horseId}", entry.HorseId, StringComparison.OrdinalIgnoreCase);

        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && IsHttp(uri)
            ? uri.ToString()
            : SafeFallback();
    }

    private string SafeFallback() =>
        Uri.TryCreate(options.FallbackUrl, UriKind.Absolute, out var uri) && IsHttp(uri)
            ? uri.ToString()
            : "https://www.jra.go.jp/JRADB/accessD.html";

    private static bool IsHttp(Uri uri) =>
        string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
}
