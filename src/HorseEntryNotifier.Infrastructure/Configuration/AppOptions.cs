namespace HorseEntryNotifier.Infrastructure.Configuration;

public sealed class AppOptions
{
    public string TimeZone { get; set; } = "Asia/Tokyo";
    public int DefaultCheckDaysAhead { get; set; } = 14;
    public bool DryRun { get; set; }
}

public sealed class DiscordOptions
{
    public bool Enabled { get; set; } = true;
    public string WebhookUrl { get; set; } = string.Empty;
    public string Username { get; set; } = "競馬出走通知";
    public string AvatarUrl { get; set; } = string.Empty;
    public string MentionRoleId { get; set; } = string.Empty;
}

public sealed class JraVanOptions
{
    public bool UseMock { get; set; } = true;
    public string SampleDataPath { get; set; } = "sample-data/race_entries_sample.json";
    public string DataSpecNotesPath { get; set; } = "docs/jravan/";
    public string ServiceKey { get; set; } = string.Empty;
}

public sealed class RaceUrlOptions
{
    public string Template { get; set; } = string.Empty;
    public string FallbackUrl { get; set; } = "https://www.jra.go.jp/JRADB/accessD.html";
}

public sealed class DatabaseOptions
{
    public string ConnectionString { get; set; } = "Data Source=horse-entry-notifier.db";
}
