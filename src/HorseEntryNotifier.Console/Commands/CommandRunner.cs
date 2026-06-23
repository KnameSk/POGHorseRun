using HorseEntryNotifier.Core.Interfaces;
using HorseEntryNotifier.Core.Models;
using HorseEntryNotifier.Core.UseCases;
using HorseEntryNotifier.Infrastructure.Configuration;

namespace HorseEntryNotifier.ConsoleApp.Commands;

public sealed class CommandRunner(
    HorseRegistryService registryService,
    CheckRaceEntriesUseCase checkUseCase,
    AppOptions appOptions,
    DiscordOptions discordOptions,
    IClock clock)
{
    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        switch (CommandLine.Parse(args))
        {
            case HorseAddCommand add:
                await AddHorseAsync(add, cancellationToken);
                break;
            case HorseRemoveCommand remove:
                await RemoveHorseAsync(remove, cancellationToken);
                break;
            case HorseListCommand:
                await ListHorsesAsync(cancellationToken);
                break;
            case CheckCommand check:
                await CheckAsync(check, cancellationToken);
                break;
            case HelpCommand:
                PrintHelp();
                break;
        }

        return 0;
    }

    private async Task AddHorseAsync(HorseAddCommand command, CancellationToken cancellationToken)
    {
        var result = await registryService.AddAsync(command.HorseName, command.Memo, cancellationToken);
        var verb = result.Created ? "登録しました" : "登録内容を更新しました";
        System.Console.WriteLine($"{result.Horse.HorseName} を{verb}。");
    }

    private async Task RemoveHorseAsync(HorseRemoveCommand command, CancellationToken cancellationToken)
    {
        var removed = await registryService.RemoveAsync(command.HorseName, cancellationToken);
        System.Console.WriteLine(removed
            ? $"{command.HorseName} を通知対象から外しました。"
            : $"有効な登録が見つかりません: {command.HorseName}");
    }

    private async Task ListHorsesAsync(CancellationToken cancellationToken)
    {
        var horses = await registryService.ListAsync(cancellationToken);
        if (horses.Count == 0)
        {
            System.Console.WriteLine("登録馬はありません。");
            return;
        }

        foreach (var horse in horses)
        {
            var status = horse.Enabled ? "有効" : "無効";
            var memo = string.IsNullOrWhiteSpace(horse.Memo) ? string.Empty : $" / {horse.Memo}";
            System.Console.WriteLine($"[{status}] {horse.HorseName}{memo}");
        }
    }

    private async Task CheckAsync(CheckCommand command, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(clock.Now.DateTime);
        var from = command.From ?? today;
        var to = command.To ?? from.AddDays(appOptions.DefaultCheckDaysAhead);
        var dryRun = command.DryRun || appOptions.DryRun;

        var result = await checkUseCase.ExecuteAsync(
            new CheckRaceEntriesRequest(from, to, dryRun, discordOptions.Enabled),
            cancellationToken);

        foreach (var outcome in result.Outcomes.Where(item => item.Message is not null))
        {
            if (outcome.Disposition is NotificationDisposition.Preview or NotificationDisposition.Disabled)
            {
                System.Console.WriteLine(FormatMessage(outcome.Message!));
                System.Console.WriteLine();
            }
        }

        System.Console.WriteLine(
            $"チェック完了: 登録馬 {result.RegisteredHorseCount}頭 / 出馬表 {result.RaceEntryCount}件 / " +
            $"一致 {result.MatchCount}件 / 送信 {result.SentCount}件 / dry-run {result.PreviewCount}件 / " +
            $"重複除外 {result.DuplicateCount}件");

        if (!discordOptions.Enabled && !dryRun && result.MatchCount > 0)
        {
            System.Console.WriteLine("Discord通知は設定でOFFです。通知履歴は保存していません。");
        }
    }

    private static string FormatMessage(NotificationMessage message)
    {
        var entry = message.Entry;
        var number = entry.HorseNumber is null
            ? "未確定"
            : entry.FrameNumber is null
                ? $"{entry.HorseNumber}番"
                : $"{entry.FrameNumber}枠{entry.HorseNumber}番";

        return $"""
               {message.Title}

               {message.Description}

               📅 日付：{entry.RaceDate:yyyy/MM/dd}
               🏟 競馬場：{entry.RacecourseName}
               🏁 レース：{entry.RaceNumber}R {entry.RaceName}
               🏷 クラス：{entry.RaceClass}
               🕒 発走：{entry.StartTime?.ToString("HH:mm") ?? "未定"}
               🛣 条件：{entry.TrackType}{entry.Distance}m
               👤 騎手：{entry.JockeyName ?? "未定"}
               🔢 枠番/馬番：{number}

               🔗 レースURL：
               {message.RaceUrl}

               通知日時：{message.NotificationTime:yyyy/MM/dd HH:mm:ss zzz}
               """;
    }

    private static void PrintHelp()
    {
        System.Console.WriteLine("""
                                 競馬出走通知

                                   horse add <馬名> [--memo <メモ>]  馬を登録
                                   horse remove <馬名>               馬を通知対象から外す
                                   horse list                        登録馬を一覧表示
                                   check [--from yyyy-MM-dd] [--to yyyy-MM-dd] [--dry-run]
                                 """);
    }
}
