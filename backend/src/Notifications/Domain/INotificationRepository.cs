using Wallanoti.Src.Notifications.Domain.Models;

namespace Wallanoti.Src.Notifications.Domain;

public interface INotificationRepository
{
    public Task SaveAsync(Notification notification);
    public Task AddRangeAsync(IEnumerable<Notification> notifications);
    public Task<IEnumerable<Notification>?> ByUserId(long userId, CancellationToken cancellationToken);
    public Task<IReadOnlyDictionary<string, LastNotifiedItemSnapshot>> GetLatestByUserAndUrls(
        long userId,
        IReadOnlyCollection<string> urls,
        CancellationToken cancellationToken = default);
}
