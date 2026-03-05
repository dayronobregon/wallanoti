using Microsoft.EntityFrameworkCore;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Domain.ValueObjects;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.EntityModels;

namespace Wallanoti.Src.Notifications.Infrastructure.Percistence;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly WallanotiDbContext _dbContext;

    public NotificationRepository(WallanotiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveAsync(Notification notification)
    {
        var notificationToAdd = new NotificationEntity
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Description = notification.Description,
            CurrentPrice = notification.Price.CurrentPrice,
            PreviousPrice = notification.Price.PreviousPrice,
            Images = notification.Images,
            Location = notification.Location,
            Url = notification.Url.Value,
            CreatedAt = notification.CreatedAt
        };

        _dbContext.Notifications.Add(notificationToAdd);

        return _dbContext.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Notification> notifications)
    {
        var notificationToAdd = notifications.Select(x => new NotificationEntity
        {
            Id = x.Id,
            UserId = x.UserId,
            Title = x.Title,
            Description = x.Description,
            CurrentPrice = x.Price.CurrentPrice,
            PreviousPrice = x.Price.PreviousPrice,
            Images = x.Images,
            Location = x.Location,
            Url = x.Url.Value,
            CreatedAt = x.CreatedAt
        });

        _dbContext.Notifications.AddRange(notificationToAdd);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Notification>?> ByUserId(long userId, CancellationToken cancellationToken)
    {
        var notifications = await _dbContext.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            //TODO paginar
            .Take(100)
            .Select(x => new Notification(
                x.Id,
                x.UserId,
                x.Title,
                x.Description,
                x.Location,
                Price.Create(x.CurrentPrice, x.PreviousPrice),
                Url.Create(x.Url),
                x.CreatedAt,
                x.Images))
            .ToListAsync(cancellationToken);

        return notifications;
    }
}