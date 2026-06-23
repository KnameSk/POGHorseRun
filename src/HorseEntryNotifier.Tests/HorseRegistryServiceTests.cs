using HorseEntryNotifier.Core.Interfaces;
using HorseEntryNotifier.Core.Models;
using HorseEntryNotifier.Core.Services;
using HorseEntryNotifier.Core.UseCases;

namespace HorseEntryNotifier.Tests;

public sealed class HorseRegistryServiceTests
{
    [Fact]
    public async Task AddAsync_SavesNominatorSeparatelyFromMemo()
    {
        var repository = new InMemoryHorseRepository();
        var service = new HorseRegistryService(repository, new HorseNameNormalizer(), FixedClock.Instance);

        var result = await service.AddAsync(
            "アステリアドンナ",
            "牝2",
            "田中",
            CancellationToken.None);

        Assert.True(result.Created);
        Assert.Equal("牝2", result.Horse.Memo);
        Assert.Equal("田中", result.Horse.NominatorName);
    }

    [Fact]
    public async Task SetNominatorAsync_UpdatesNominatorWithoutChangingMemo()
    {
        var horse = TestData.RegisteredHorse("アステリアドンナ");
        horse.Memo = "牝2";
        horse.NominatorName = null;
        var repository = new InMemoryHorseRepository(horse);
        var service = new HorseRegistryService(repository, new HorseNameNormalizer(), FixedClock.Instance);

        var updated = await service.SetNominatorAsync(
            "アステリアドンナ",
            " 佐藤 ",
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("牝2", updated.Memo);
        Assert.Equal("佐藤", updated.NominatorName);
    }

    private sealed class InMemoryHorseRepository(params RegisteredHorse[] initial) : IHorseRepository
    {
        private readonly List<RegisteredHorse> horses = [.. initial];

        public Task<RegisteredHorse?> GetByNormalizedNameAsync(
            string normalizedName,
            CancellationToken cancellationToken) =>
            Task.FromResult(horses.SingleOrDefault(horse => horse.NormalizedHorseName == normalizedName));

        public Task<IReadOnlyList<RegisteredHorse>> ListAsync(
            bool enabledOnly,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<RegisteredHorse>>(
                horses.Where(horse => !enabledOnly || horse.Enabled).ToArray());

        public Task AddAsync(RegisteredHorse horse, CancellationToken cancellationToken)
        {
            horses.Add(horse);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
