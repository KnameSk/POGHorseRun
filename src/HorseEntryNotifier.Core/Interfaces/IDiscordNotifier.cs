using HorseEntryNotifier.Core.Models;

namespace HorseEntryNotifier.Core.Interfaces;

public interface IDiscordNotifier
{
    Task SendAsync(NotificationMessage message, CancellationToken cancellationToken);
}
