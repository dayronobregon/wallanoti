using WallapopNotification.User._1_Domain.Models;

namespace WallapopNotification.User._1_Domain;

public interface INotificationSender
{
    Task Notify( Notification notification);
}