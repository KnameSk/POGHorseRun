using System.Globalization;

namespace HorseEntryNotifier.ConsoleApp.Commands;

internal abstract record ConsoleCommand;
internal sealed record HorseAddCommand(string HorseName, string? Memo) : ConsoleCommand;
internal sealed record HorseRemoveCommand(string HorseName) : ConsoleCommand;
internal sealed record HorseListCommand : ConsoleCommand;
internal sealed record CheckCommand(DateOnly? From, DateOnly? To, bool DryRun) : ConsoleCommand;
internal sealed record HelpCommand : ConsoleCommand;

internal static class CommandLine
{
    public static ConsoleCommand Parse(string[] args)
    {
        if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
        {
            return new HelpCommand();
        }

        return args[0].ToLowerInvariant() switch
        {
            "horse" => ParseHorse(args),
            "check" => ParseCheck(args),
            _ => throw new ArgumentException($"不明なコマンドです: {args[0]}")
        };
    }

    private static ConsoleCommand ParseHorse(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("horse の後に add / remove / list を指定してください。");
        }

        return args[1].ToLowerInvariant() switch
        {
            "add" when args.Length >= 3 => new HorseAddCommand(args[2], ReadOption(args, "--memo")),
            "remove" when args.Length >= 3 => new HorseRemoveCommand(args[2]),
            "list" => new HorseListCommand(),
            _ => throw new ArgumentException("horse add <馬名> / horse remove <馬名> / horse list を使用してください。")
        };
    }

    private static ConsoleCommand ParseCheck(string[] args)
    {
        var from = ReadDateOption(args, "--from");
        var to = ReadDateOption(args, "--to");
        var dryRun = args.Any(value => value.Equals("--dry-run", StringComparison.OrdinalIgnoreCase));
        return new CheckCommand(from, to, dryRun);
    }

    private static string? ReadOption(string[] args, string option)
    {
        var index = Array.FindIndex(args, value => value.Equals(option, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return null;
        }

        if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException($"{option} の値を指定してください。");
        }

        return args[index + 1];
    }

    private static DateOnly? ReadDateOption(string[] args, string option)
    {
        var value = ReadOption(args, option);
        if (value is null)
        {
            return null;
        }

        if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            throw new ArgumentException($"{option} は yyyy-MM-dd 形式で指定してください: {value}");
        }

        return date;
    }
}
