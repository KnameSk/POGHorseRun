using HorseEntryNotifier.Core.Interfaces;

namespace HorseEntryNotifier.Core.Services;

public sealed class ConfiguredClock(TimeZoneInfo timeZone) : IClock
{
    public DateTimeOffset Now => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
}
