namespace HorseEntryNotifier.Core.Interfaces;

public interface IClock
{
    DateTimeOffset Now { get; }
}
