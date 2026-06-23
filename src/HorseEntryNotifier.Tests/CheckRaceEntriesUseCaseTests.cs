using HorseEntryNotifier.Core.Interfaces;
using HorseEntryNotifier.Core.Models;
using HorseEntryNotifier.Core.Services;
using HorseEntryNotifier.Core.UseCases;

namespace HorseEntryNotifier.Tests;

public sealed class CheckRaceEntriesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DoesNotNotifySameHorseAndRaceTwice()
    {
        var entry = TestData.RaceEntry();
        var notifications = new InMemoryNotificationRepository();
        notifications.Seed(CreateInitialRecord(entry));
        var notifier = new RecordingNotifier();
        var useCase = CreateUseCase(entry, notifications, notifier);

        var result = await useCase.ExecuteAsync(
            new CheckRaceEntriesRequest(entry.RaceDate, entry.RaceDate, false, true),
            CancellationToken.None);

        Assert.Equal(1, result.DuplicateCount);
        Assert.Empty(notifier.Messages);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesJockeyUpdateCandidateWhenJockeyChanges()
    {
        var entry = TestData.RaceEntry(jockeyId: "new-jockey", jockeyName: "新騎手");
        var initial = CreateInitialRecord(entry);
        initial.JockeyId = "old-jockey";
        initial.JockeyName = "旧騎手";
        var notifications = new InMemoryNotificationRepository();
        notifications.Seed(initial);
        var useCase = CreateUseCase(entry, notifications, new RecordingNotifier());

        var result = await useCase.ExecuteAsync(
            new CheckRaceEntriesRequest(entry.RaceDate, entry.RaceDate, true, true),
            CancellationToken.None);

        var outcome = Assert.Single(result.Outcomes);
        Assert.Equal(NotificationDisposition.Preview, outcome.Disposition);
        Assert.StartsWith("update-jockey:", outcome.NotificationKey);
        Assert.Equal("update-jockey", outcome.Message?.NotificationType);
    }

    private static CheckRaceEntriesUseCase CreateUseCase(
        RaceEntry entry,
        InMemoryNotificationRepository notifications,
        RecordingNotifier notifier)
    {
        var normalizer = new HorseNameNormalizer();
        var horse = TestData.RegisteredHorse();
        horse.NormalizedHorseName = normalizer.Normalize(horse.HorseName);
        var messageFactory = new NotificationMessageFactory(new StubUrlBuilder(), FixedClock.Instance);

        return new CheckRaceEntriesUseCase(
            new InMemoryHorseRepository(horse),
            notifications,
            new StubJraVanClient(entry),
            notifier,
            new RaceEntryMatcher(normalizer),
            messageFactory,
            FixedClock.Instance);
    }

    private static NotificationRecord CreateInitialRecord(RaceEntry entry) => new()
    {
        NotificationKey = NotificationKeyFactory.Entry(entry),
        HorseId = entry.HorseId,
        HorseName = entry.HorseName,
        RaceId = entry.RaceId,
        RaceDate = entry.RaceDate,
        RaceName = entry.RaceName,
        JockeyId = entry.JockeyId,
        JockeyName = entry.JockeyName,
        FrameNumber = entry.FrameNumber,
        HorseNumber = entry.HorseNumber,
        NotificationType = "entry",
        SentAt = FixedClock.Instance.Now,
        PayloadHash = NotificationPayloadHasher.Compute(entry)
    };

    private sealed class InMemoryHorseRepository(RegisteredHorse horse) : IHorseRepository
    {
        public Task<RegisteredHorse?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken) =>
            Task.FromResult<RegisteredHorse?>(horse.NormalizedHorseName == normalizedName ? horse : null);

        public Task<IReadOnlyList<RegisteredHorse>> ListAsync(bool enabledOnly, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<RegisteredHorse>>([horse]);

        public Task AddAsync(RegisteredHorse value, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryNotificationRepository : INotificationRepository
    {
        private readonly Dictionary<string, NotificationRecord> records = new(StringComparer.Ordinal);

        public void Seed(NotificationRecord notification) => records[notification.NotificationKey] = notification;

        public Task<NotificationRecord?> GetByKeyAsync(string notificationKey, CancellationToken cancellationToken) =>
            Task.FromResult(records.GetValueOrDefault(notificationKey));

        public Task AddAsync(NotificationRecord notification, CancellationToken cancellationToken)
        {
            records.Add(notification.NotificationKey, notification);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class StubJraVanClient(RaceEntry entry) : IJraVanClient
    {
        public Task<IReadOnlyList<RaceEntry>> GetRaceEntriesAsync(
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<RaceEntry>>([entry]);
    }

    private sealed class RecordingNotifier : IDiscordNotifier
    {
        public List<NotificationMessage> Messages { get; } = [];

        public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class StubUrlBuilder : IRaceUrlBuilder
    {
        public string BuildRaceUrl(RaceEntry entry) => $"https://example.test/{entry.RaceId}";
    }
}
