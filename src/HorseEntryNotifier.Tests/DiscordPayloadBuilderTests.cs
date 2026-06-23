using System.Text.Json;
using HorseEntryNotifier.Core.Models;
using HorseEntryNotifier.Infrastructure.Configuration;
using HorseEntryNotifier.Infrastructure.Discord;

namespace HorseEntryNotifier.Tests;

public sealed class DiscordPayloadBuilderTests
{
    [Fact]
    public void BuildJson_CreatesEmbedWithMentionAndRaceInformation()
    {
        var builder = new DiscordPayloadBuilder(new DiscordOptions
        {
            Username = "テスト通知",
            MentionRoleId = "12345",
            AvatarUrl = "https://example.test/avatar.png"
        });
        var message = new NotificationMessage(
            "entry:race-001:horse-001",
            "entry",
            "🐴 出走決定通知",
            "ジェットシェヴロン が出走予定です！",
            TestData.RaceEntry(),
            "https://example.test/race/race-001",
            FixedClock.Instance.Now,
            "田中",
            "POG指名馬");

        using var json = JsonDocument.Parse(builder.BuildJson(message));

        Assert.Equal("<@&12345>", json.RootElement.GetProperty("content").GetString());
        Assert.Equal("テスト通知", json.RootElement.GetProperty("username").GetString());
        Assert.Equal(
            "https://example.test/avatar.png",
            json.RootElement.GetProperty("avatar_url").GetString());
        var embed = json.RootElement.GetProperty("embeds")[0];
        Assert.Equal("🐴 出走決定通知", embed.GetProperty("title").GetString());
        Assert.Equal("https://example.test/race/race-001", embed.GetProperty("url").GetString());
        Assert.Contains(
            embed.GetProperty("fields").EnumerateArray(),
            field => field.GetProperty("value").GetString() == "5R 2歳新馬");
        Assert.Contains(
            embed.GetProperty("fields").EnumerateArray(),
            field => field.GetProperty("name").GetString() == "🎯 指名者" &&
                     field.GetProperty("value").GetString() == "田中");
    }
}
