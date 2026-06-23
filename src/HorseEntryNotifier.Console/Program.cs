using HorseEntryNotifier.ConsoleApp.Commands;
using HorseEntryNotifier.Core.Interfaces;
using HorseEntryNotifier.Core.Services;
using HorseEntryNotifier.Core.UseCases;
using HorseEntryNotifier.Infrastructure.Configuration;
using HorseEntryNotifier.Infrastructure.Discord;
using HorseEntryNotifier.Infrastructure.JraVan;
using HorseEntryNotifier.Infrastructure.Persistence;
using HorseEntryNotifier.Infrastructure.RaceUrl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile(
    Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
    optional: false,
    reloadOnChange: false);
builder.Configuration.AddUserSecrets<Program>(optional: true);
builder.Configuration.AddEnvironmentVariables();

var appOptions = builder.Configuration.GetSection("App").Get<AppOptions>() ?? new AppOptions();
var discordOptions = builder.Configuration.GetSection("Discord").Get<DiscordOptions>() ?? new DiscordOptions();
var jraVanOptions = builder.Configuration.GetSection("JraVan").Get<JraVanOptions>() ?? new JraVanOptions();
var raceUrlOptions = builder.Configuration.GetSection("RaceUrl").Get<RaceUrlOptions>() ?? new RaceUrlOptions();
var databaseOptions = builder.Configuration.GetSection("Database").Get<DatabaseOptions>() ?? new DatabaseOptions();

discordOptions.WebhookUrl = Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL")
                            ?? discordOptions.WebhookUrl;
jraVanOptions.ServiceKey = Environment.GetEnvironmentVariable("JRAVAN_SERVICE_KEY")
                           ?? jraVanOptions.ServiceKey;

builder.Services.AddSingleton(appOptions);
builder.Services.AddSingleton(discordOptions);
builder.Services.AddSingleton(jraVanOptions);
builder.Services.AddSingleton(raceUrlOptions);
builder.Services.AddSingleton(databaseOptions);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(databaseOptions.ConnectionString));
builder.Services.AddScoped<DatabaseInitializer>();
builder.Services.AddScoped<IHorseRepository, EfHorseRepository>();
builder.Services.AddScoped<INotificationRepository, EfNotificationRepository>();

builder.Services.AddSingleton<HorseNameNormalizer>();
builder.Services.AddSingleton<RaceEntryMatcher>();
builder.Services.AddSingleton<IRaceUrlBuilder, TemplateRaceUrlBuilder>();
builder.Services.AddSingleton<IClock>(new ConfiguredClock(ResolveTimeZone(appOptions.TimeZone)));
builder.Services.AddSingleton<NotificationMessageFactory>();
builder.Services.AddSingleton<DiscordPayloadBuilder>();
builder.Services.AddHttpClient<IDiscordNotifier, DiscordWebhookNotifier>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

if (jraVanOptions.UseMock)
{
    builder.Services.AddSingleton<IJraVanClient, MockJraVanClient>();
}
else
{
    builder.Services.AddSingleton<IJraVanClient, JraVanClient>();
}

builder.Services.AddScoped<HorseRegistryService>();
builder.Services.AddScoped<CheckRaceEntriesUseCase>();
builder.Services.AddScoped<CommandRunner>();

using var host = builder.Build();
using var scope = host.Services.CreateScope();

try
{
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>()
        .InitializeAsync(CancellationToken.None);
    return await scope.ServiceProvider.GetRequiredService<CommandRunner>()
        .RunAsync(args, CancellationToken.None);
}
catch (Exception exception)
{
    scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("Program")
        .LogError(exception, "コマンドの実行に失敗しました。");
    System.Console.Error.WriteLine($"エラー: {exception.Message}");
    return 1;
}

static TimeZoneInfo ResolveTimeZone(string configuredId)
{
    foreach (var id in new[] { configuredId, "Asia/Tokyo", "Tokyo Standard Time" })
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            // 次の互換IDを試します。
        }
        catch (InvalidTimeZoneException)
        {
            // 次の互換IDを試します。
        }
    }

    return TimeZoneInfo.CreateCustomTimeZone(
        "Japan Standard Time",
        TimeSpan.FromHours(9),
        "Japan Standard Time",
        "Japan Standard Time");
}

public partial class Program;
