using HorseEntryNotifier.Core.Interfaces;
using HorseEntryNotifier.Core.Models;
using HorseEntryNotifier.Core.Services;

namespace HorseEntryNotifier.Core.UseCases;

public sealed class HorseRegistryService(
    IHorseRepository repository,
    HorseNameNormalizer normalizer,
    IClock clock)
{
    public async Task<(RegisteredHorse Horse, bool Created)> AddAsync(
        string horseName,
        string? memo,
        CancellationToken cancellationToken)
    {
        var displayName = horseName.Trim();
        var normalizedName = normalizer.Normalize(displayName);
        var existing = await repository.GetByNormalizedNameAsync(normalizedName, cancellationToken);

        if (existing is not null)
        {
            existing.HorseName = displayName;
            existing.Memo = memo ?? existing.Memo;
            existing.Enabled = true;
            existing.UpdatedAt = clock.Now;
            await repository.SaveChangesAsync(cancellationToken);
            return (existing, false);
        }

        var horse = new RegisteredHorse
        {
            HorseName = displayName,
            NormalizedHorseName = normalizedName,
            Memo = memo,
            Enabled = true,
            CreatedAt = clock.Now,
            UpdatedAt = clock.Now
        };

        await repository.AddAsync(horse, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return (horse, true);
    }

    public async Task<bool> RemoveAsync(string horseName, CancellationToken cancellationToken)
    {
        var normalizedName = normalizer.Normalize(horseName);
        var horse = await repository.GetByNormalizedNameAsync(normalizedName, cancellationToken);
        if (horse is null || !horse.Enabled)
        {
            return false;
        }

        horse.Enabled = false;
        horse.UpdatedAt = clock.Now;
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<IReadOnlyList<RegisteredHorse>> ListAsync(CancellationToken cancellationToken) =>
        repository.ListAsync(enabledOnly: false, cancellationToken);
}
