using HorseEntryNotifier.Core.Interfaces;
using HorseEntryNotifier.Core.Models;

namespace HorseEntryNotifier.Core.Services;

public sealed class NotificationMessageFactory(IRaceUrlBuilder raceUrlBuilder, IClock clock)
{
    public NotificationMessage Create(
        string notificationKey,
        string notificationType,
        RegisteredHorse horse,
        RaceEntry entry)
    {
        var (title, description) = notificationType switch
        {
            "update-jockey" => ("🔄 騎手変更通知", $"{entry.HorseName} の騎手情報が更新されました。"),
            "update-number" => ("🔢 枠順・馬番更新通知", $"{entry.HorseName} の枠順・馬番が更新されました。"),
            _ => ("🐴 出走決定通知", $"{entry.HorseName} が出走予定です！")
        };

        return new NotificationMessage(
            notificationKey,
            notificationType,
            title,
            description,
            entry,
            raceUrlBuilder.BuildRaceUrl(entry),
            clock.Now,
            horse.NominatorName,
            horse.Memo);
    }
}
