namespace Wallanoti.Src.Notifications.Domain;

public interface IWebNotificationSender
{
    public Task Notify(Notification notification);
}