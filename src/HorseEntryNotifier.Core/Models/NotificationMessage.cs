namespace HorseEntryNotifier.Core.Models;

public sealed record NotificationMessage(
    string NotificationKey,
    string NotificationType,
    string Title,
    string Description,
    RaceEntry Entry,
    string RaceUrl,
    DateTimeOffset NotificationTime,
    string? NominatorName,
    string? Memo);

public enum NotificationDisposition
{
    Sent,
    Preview,
    Disabled,
    Duplicate
}

public sealed record NotificationOutcome(
    NotificationDisposition Disposition,
    string NotificationKey,
    string HorseName,
    NotificationMessage? Message);

public sealed record CheckRaceEntriesResult(
    int RegisteredHorseCount,
    int RaceEntryCount,
    int MatchCount,
    IReadOnlyList<NotificationOutcome> Outcomes)
{
    public int SentCount => Outcomes.Count(x => x.Disposition == NotificationDisposition.Sent);
    public int PreviewCount => Outcomes.Count(x => x.Disposition == NotificationDisposition.Preview);
    public int DuplicateCount => Outcomes.Count(x => x.Disposition == NotificationDisposition.Duplicate);
}
