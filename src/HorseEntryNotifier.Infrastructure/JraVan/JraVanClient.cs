using HorseEntryNotifier.Core.Interfaces;
using HorseEntryNotifier.Core.Models;
using HorseEntryNotifier.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;

namespace HorseEntryNotifier.Infrastructure.JraVan;

public sealed class JraVanClient(
    JraVanOptions options,
    ILogger<JraVanClient> logger) : IJraVanClient
{
    public Task<IReadOnlyList<RaceEntry>> GetRaceEntriesAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "JV-Link実AdapterはSDK仕様確認後に有効化します。仕様メモ: {NotesPath}",
            options.DataSpecNotesPath);

        throw new NotSupportedException(
            "JRA-VAN実連携はSDK仕様書に依存します。docs/jravan-implementation-notes.md を確認し、" +
            "JRA-VAN SDKを docs/jravan/ に配置してAdapterを実装してから JraVan:UseMock=false にしてください。");
    }
}
