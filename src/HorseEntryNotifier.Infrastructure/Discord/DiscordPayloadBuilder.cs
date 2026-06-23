using System.Text.Json;
using System.Text.Json.Serialization;
using HorseEntryNotifier.Core.Models;
using HorseEntryNotifier.Infrastructure.Configuration;

namespace HorseEntryNotifier.Infrastructure.Discord;

public sealed class DiscordPayloadBuilder(DiscordOptions options)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string BuildJson(NotificationMessage message)
    {
        var entry = message.Entry;
        var number = entry.HorseNumber is null
            ? "未確定"
            : entry.FrameNumber is null
                ? $"{entry.HorseNumber}番"
                : $"{entry.FrameNumber}枠{entry.HorseNumber}番";

        var fields = new[]
        {
            new Field("📅 日付", entry.RaceDate.ToString("yyyy/MM/dd"), true),
            new Field("🏟 競馬場", entry.RacecourseName, true),
            new Field("🏁 レース", $"{entry.RaceNumber}R {entry.RaceName}", false),
            new Field("🕒 発走", entry.StartTime?.ToString("HH:mm") ?? "未定", true),
            new Field("🛣 条件", $"{entry.RaceClass} / {entry.TrackType}{entry.Distance}m", true),
            new Field("👤 騎手", entry.JockeyName ?? "未定", true),
            new Field("🔢 枠番・馬番", number, true),
            new Field("🎯 指名者", message.NominatorName ?? "未設定", true),
            new Field("📝 メモ", message.Memo ?? "なし", false)
        };

        var payload = new Payload(
            string.IsNullOrWhiteSpace(options.MentionRoleId) ? null : $"<@&{options.MentionRoleId}>",
            options.Username,
            string.IsNullOrWhiteSpace(options.AvatarUrl) ? null : options.AvatarUrl,
            [new Embed(
                message.Title,
                message.Description,
                message.RaceUrl,
                message.NotificationType == "entry" ? 0x2ECC71 : 0xF1C40F,
                fields,
                new Footer($"通知日時: {message.NotificationTime:yyyy/MM/dd HH:mm:ss zzz}"),
                message.NotificationTime)]);

        return JsonSerializer.Serialize(payload, SerializerOptions);
    }

    private sealed record Payload(
        string? Content,
        string Username,
        [property: JsonPropertyName("avatar_url")] string? AvatarUrl,
        Embed[] Embeds);
    private sealed record Embed(
        string Title,
        string Description,
        string Url,
        int Color,
        Field[] Fields,
        Footer Footer,
        DateTimeOffset Timestamp);
    private sealed record Field(string Name, string Value, bool Inline);
    private sealed record Footer(string Text);
}
