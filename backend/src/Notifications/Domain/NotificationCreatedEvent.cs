using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Notifications.Domain;

public sealed class NotificationCreatedEvent : DomainEvent
{
    public Notification Notification { get; init; } = null!;

    public NotificationCreatedEvent()
    {
    }

    public NotificationCreatedEvent(string eventId, string occurredOn, Notification notification) : base(eventId,
        occurredOn)
    {
        Notification = notification;
    }

    public override string EventName() => "notification.created";
}
