using Microsoft.AspNetCore.SignalR;
using Wallanoti.Src.Notifications.Domain;

namespace Wallanoti.Src.Notifications.Infrastructure.Notifications;

public sealed class WebNotificationSender : IWebNotificationSender
{
    private readonly IHubContext<WebNotificationHub, IWebNotificationClient> _hubContext;

    public WebNotificationSender(IHubContext<WebNotificationHub, IWebNotificationClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task Notify(Notification notification)
    {
        return _hubContext.Clients
            .User(notification.UserId.ToString())
            .ReceiveNotification(notification);
    }
}