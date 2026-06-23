using HorseEntryNotifier.Core.Interfaces;
using HorseEntryNotifier.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HorseEntryNotifier.Infrastructure.Persistence;

public sealed class EfNotificationRepository(AppDbContext dbContext) : INotificationRepository
{
    public Task<NotificationRecord?> GetByKeyAsync(
        string notificationKey,
        CancellationToken cancellationToken) =>
        dbContext.Notifications.AsNoTracking().SingleOrDefaultAsync(
            notification => notification.NotificationKey == notificationKey,
            cancellationToken);

    public Task AddAsync(NotificationRecord notification, CancellationToken cancellationToken) =>
        dbContext.Notifications.AddAsync(notification, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
