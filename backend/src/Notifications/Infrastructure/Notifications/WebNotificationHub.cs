using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Wallanoti.Src.Notifications.Domain;

namespace Wallanoti.Src.Notifications.Infrastructure.Notifications;

public interface IWebNotificationClient
{
    Task ReceiveNotification(Notification notification);
}

[Authorize]
public sealed class WebNotificationHub : Hub<IWebNotificationClient>
{
    public const string HubName = "/hub/notifications";

    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }
    
    
}