namespace HorseEntryNotifier.Core.Models;

public sealed record RaceEntry
{
    public required string RaceId { get; init; }
    public required DateOnly RaceDate { get; init; }
    public required string RacecourseCode { get; init; }
    public required string RacecourseName { get; init; }
    public required int RaceNumber { get; init; }
    public required string RaceName { get; init; }
    public required string RaceClass { get; init; }
    public required string TrackType { get; init; }
    public required int Distance { get; init; }
    public TimeOnly? StartTime { get; init; }
    public required string HorseId { get; init; }
    public required string HorseName { get; init; }
    public string? JockeyId { get; init; }
    public string? JockeyName { get; init; }
    public int? FrameNumber { get; init; }
    public int? HorseNumber { get; init; }
    public string? RawData { get; init; }
}
