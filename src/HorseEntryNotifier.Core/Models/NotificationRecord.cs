namespace HorseEntryNotifier.Core.Models;

public sealed class NotificationRecord
{
    public long Id { get; set; }
    public required string NotificationKey { get; set; }
    public required string HorseId { get; set; }
    public required string HorseName { get; set; }
    public required string RaceId { get; set; }
    public DateOnly RaceDate { get; set; }
    public required string RaceName { get; set; }
    public string? JockeyId { get; set; }
    public string? JockeyName { get; set; }
    public int? FrameNumber { get; set; }
    public int? HorseNumber { get; set; }
    public required string NotificationType { get; set; }
    public DateTimeOffset SentAt { get; set; }
    public required string PayloadHash { get; set; }
}
