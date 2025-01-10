using WallapopNotification.Alert._1_Domain.Events;
using WallapopNotification.Shared._1_Domain.Events;
using WallapopNotification.User._1_Domain;
using WallapopNotification.User._1_Domain.Models;

namespace WallapopNotification.User._2_Application.NotifyUser;

public sealed class NotifyOnNewSearchEvent : DomainEventHandler<NewSearchEvent>
{
    private readonly INotificationSender _notificationSender;

    public NotifyOnNewSearchEvent(INotificationSender notificationSender)
    {
        _notificationSender = notificationSender;
    }

    protected override async Task Handle(NewSearchEvent newSearchEvent)
    {
        if (newSearchEvent.Items is null)
        {
            return;
        }

        foreach (var item in newSearchEvent.Items)
        {
            var notification = new Notification
            {
                ToUserId = newSearchEvent.UserId,
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