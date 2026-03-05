using System.Text.Json;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Notifications.Domain;

public sealed class NotificationCreatedEvent : DomainEvent
{
    public Notification Notification { get; }

    public NotificationCreatedEvent()
    {
    }

    public NotificationCreatedEvent(string eventId, string occurredOn, Notification notification) : base(eventId,
        occurredOn)
    {
        Notification = notification;
    }

    public override string EventName() => "notification.created";


    public override string ToJson()
    {
        return JsonSerializer.Serialize(Notification);
    }

    public override DomainEvent FromPrimitives(string eventId, string occurredOn, string data)
    {
        var notificationElement = JsonSerializer.Deserialize<Notification>(data);

        return new NotificationCreatedEvent(eventId, occurredOn, notificationElement);
    }
}