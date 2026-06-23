using System.Security.Cryptography;
using System.Text;
using HorseEntryNotifier.Core.Models;

namespace HorseEntryNotifier.Core.Services;

public static class NotificationPayloadHasher
{
    public static string Compute(RaceEntry entry)
    {
        var value = string.Join('|',
            entry.RaceId,
            entry.HorseId,
            entry.JockeyId,
            entry.JockeyName,
            entry.FrameNumber,
            entry.HorseNumber,
            entry.StartTime,
            entry.RaceName,
            entry.RaceClass);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}
