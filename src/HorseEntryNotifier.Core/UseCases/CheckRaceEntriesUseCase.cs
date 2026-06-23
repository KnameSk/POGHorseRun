using HorseEntryNotifier.Core.Interfaces;
using HorseEntryNotifier.Core.Models;
using HorseEntryNotifier.Core.Services;

namespace HorseEntryNotifier.Core.UseCases;

public sealed record CheckRaceEntriesRequest(
    DateOnly From,
    DateOnly To,
    bool DryRun,
    bool NotificationsEnabled);

public sealed class CheckRaceEntriesUseCase(
    IHorseRepository horseRepository,
    INotificationRepository notificationRepository,
    IJraVanClient jraVanClient,
    IDiscordNotifier discordNotifier,
    RaceEntryMatcher matcher,
    NotificationMessageFactory messageFactory,
    IClock clock)
{
    public async Task<CheckRaceEntriesResult> ExecuteAsync(
        CheckRaceEntriesRequest request,
        CancellationToken cancellationToken)
    {
        if (request.To < request.From)
        {
            throw new ArgumentException("終了日は開始日以降を指定してください。");
        }

        var horses = await horseRepository.ListAsync(enabledOnly: true, cancellationToken);
        var entries = await jraVanClient.GetRaceEntriesAsync(request.From, request.To, cancellationToken);
        var outcomes = new List<NotificationOutcome>();
        var matchCount = 0;

        foreach (var horse in horses)
        {
            foreach (var entry in entries.Where(entry => matcher.IsMatch(horse, entry)))
            {
                matchCount++;
                var candidate = await DetermineCandidateAsync(entry, cancellationToken);
                if (candidate is null)
                {
                    outcomes.Add(new NotificationOutcome(
                        NotificationDisposition.Duplicate,
                        NotificationKeyFactory.Entry(entry),
                        entry.HorseName,
                        null));
                    continue;
                }

                var message = messageFactory.Create(candidate.Value.Key, candidate.Value.Type, horse, entry);
                if (request.DryRun)
                {
                    outcomes.Add(new NotificationOutcome(
                        NotificationDisposition.Preview,
                        candidate.Value.Key,
                        entry.HorseName,
                        message));
                    continue;
                }

                if (!request.NotificationsEnabled)
                {
                    outcomes.Add(new NotificationOutcome(
                        NotificationDisposition.Disabled,
                        candidate.Value.Key,
                        entry.HorseName,
                        message));
                    continue;
                }

                await discordNotifier.SendAsync(message, cancellationToken);
                await notificationRepository.AddAsync(CreateRecord(candidate.Value, entry), cancellationToken);
                await notificationRepository.SaveChangesAsync(cancellationToken);
                outcomes.Add(new NotificationOutcome(
                    NotificationDisposition.Sent,
                    candidate.Value.Key,
                    entry.HorseName,
                    message));
            }
        }

        return new CheckRaceEntriesResult(horses.Count, entries.Count, matchCount, outcomes);
    }

    private async Task<(string Key, string Type)?> DetermineCandidateAsync(
        RaceEntry entry,
        CancellationToken cancellationToken)
    {
        var entryKey = NotificationKeyFactory.Entry(entry);
        var initial = await notificationRepository.GetByKeyAsync(entryKey, cancellationToken);
        if (initial is null)
        {
            return (entryKey, "entry");
        }

        if (HasJockeyChanged(initial, entry))
        {
            var key = NotificationKeyFactory.JockeyUpdate(entry);
            if (await notificationRepository.GetByKeyAsync(key, cancellationToken) is null)
            {
                return (key, "update-jockey");
            }
        }

        if (entry.HorseNumber is not null && entry.HorseNumber != initial.HorseNumber)
        {
            var key = NotificationKeyFactory.NumberUpdate(entry);
            if (await notificationRepository.GetByKeyAsync(key, cancellationToken) is null)
            {
                return (key, "update-number");
            }
        }

        return null;
    }

    private static bool HasJockeyChanged(NotificationRecord initial, RaceEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.JockeyId) || !string.IsNullOrWhiteSpace(initial.JockeyId))
        {
            return !string.Equals(initial.JockeyId, entry.JockeyId, StringComparison.Ordinal);
        }

        return !string.Equals(initial.JockeyName, entry.JockeyName, StringComparison.Ordinal);
    }

    private NotificationRecord CreateRecord((string Key, string Type) candidate, RaceEntry entry) => new()
    {
        NotificationKey = candidate.Key,
        HorseId = entry.HorseId,
        HorseName = entry.HorseName,
        RaceId = entry.RaceId,
        RaceDate = entry.RaceDate,
        RaceName = entry.RaceName,
        JockeyId = entry.JockeyId,
        JockeyName = entry.JockeyName,
        FrameNumber = entry.FrameNumber,
        HorseNumber = entry.HorseNumber,
        NotificationType = candidate.Type,
        SentAt = clock.Now,
        PayloadHash = NotificationPayloadHasher.Compute(entry)
    };
}
