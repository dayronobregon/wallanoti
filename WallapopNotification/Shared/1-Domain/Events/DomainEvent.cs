using System.Text.Json;

namespace WallapopNotification.Shared._1_Domain.Events;

public abstract class DomainEvent
{
    public string EventId { get; }
    public string OccurredOn { get; }

    public DomainEvent()
    {
    }

    protected DomainEvent(string eventId, string occurredOn)
    {
        EventId = eventId;
        OccurredOn = occurredOn;
    }

    public abstract string EventName();

    public abstract string ToJson();

    public abstract DomainEvent FromPrimitives(string eventId, string occurredOn, string data);
}