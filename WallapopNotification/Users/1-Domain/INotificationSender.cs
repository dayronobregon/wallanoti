using WallapopNotification.Users._1_Domain.Models;

namespace WallapopNotification.Users._1_Domain;

public interface INotificationSender
{
    Task Notify( Notification notification);
}