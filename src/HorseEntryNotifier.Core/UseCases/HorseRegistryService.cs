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
        string? nominatorName,
        CancellationToken cancellationToken)
    {
        var displayName = horseName.Trim();
        var normalizedName = normalizer.Normalize(displayName);
        var existing = await repository.GetByNormalizedNameAsync(normalizedName, cancellationToken);

        if (existing is not null)
        {
            existing.HorseName = displayName;
            existing.Memo = memo ?? existing.Memo;
            existing.NominatorName = NormalizeOptional(nominatorName) ?? existing.NominatorName;
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
            NominatorName = NormalizeOptional(nominatorName),
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

    public async Task<RegisteredHorse?> SetNominatorAsync(
        string horseName,
        string nominatorName,
        CancellationToken cancellationToken)
    {
        var normalizedName = normalizer.Normalize(horseName);
        var horse = await repository.GetByNormalizedNameAsync(normalizedName, cancellationToken);
        if (horse is null)
        {
            return null;
        }

        horse.NominatorName = NormalizeOptional(nominatorName)
            ?? throw new ArgumentException("指名者名を入力してください。", nameof(nominatorName));
        horse.UpdatedAt = clock.Now;
        await repository.SaveChangesAsync(cancellationToken);
        return horse;
    }

    public Task<IReadOnlyList<RegisteredHorse>> ListAsync(CancellationToken cancellationToken) =>
        repository.ListAsync(enabledOnly: false, cancellationToken);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
