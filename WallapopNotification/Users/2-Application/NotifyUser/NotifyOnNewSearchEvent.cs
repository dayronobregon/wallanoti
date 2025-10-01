using WallapopNotification.Alerts._1_Domain;
using WallapopNotification.Shared._1_Domain.Events;
using WallapopNotification.Users._1_Domain;
using WallapopNotification.Users._1_Domain.Models;

namespace WallapopNotification.Users._2_Application.NotifyUser;

public sealed class NotifyOnNewSearchEvent : IDomainEventHandler<NewItemsFoundEvent>
{
    private readonly INotificationSender _notificationSender;

    public NotifyOnNewSearchEvent(INotificationSender notificationSender)
    {
        _notificationSender = notificationSender;
    }

    public async Task Handle(NewItemsFoundEvent newItemsFoundEvent)
    {
        if (newItemsFoundEvent.Items is null)
        {
            return;
        }

        foreach (var item in newItemsFoundEvent.Items)
        {
            var notification = new Notification
            {
                ToUserId = newItemsFoundEvent.UserId,
                Title = item.Title,
                Description = item.Description,
                Price = new Price
                {
                    CurrentPrice = item.Price.CurrentPrice,
                    PreviousPrice = item.Price.PreviousPrice
                },
                Link = Link.CreateFromSlug(item.WebSlug),
                Images = item.Images,
                Location = item.Location.FullLocation
            };

            await _notificationSender.Notify(notification);
        }
    }
}