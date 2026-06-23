using HorseEntryNotifier.Core.Models;

namespace HorseEntryNotifier.Core.Interfaces;

public interface INotificationRepository
{
    Task<NotificationRecord?> GetByKeyAsync(string notificationKey, CancellationToken cancellationToken);
    Task AddAsync(NotificationRecord notification, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
