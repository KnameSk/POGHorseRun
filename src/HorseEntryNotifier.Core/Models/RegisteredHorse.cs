namespace HorseEntryNotifier.Core.Models;

public sealed class RegisteredHorse
{
    public long Id { get; set; }
    public required string HorseName { get; set; }
    public required string NormalizedHorseName { get; set; }
    public string? Memo { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
