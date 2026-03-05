namespace Wallanoti.Src.Notifications.Domain;

public interface IPushNotificationSender
{
    public Task Notify(Notification notification);

    public Task Notify(long userId, string message);
}