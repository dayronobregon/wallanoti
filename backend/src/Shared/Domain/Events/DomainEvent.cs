namespace Wallanoti.Src.Shared.Domain.Events;

public abstract class DomainEvent
{
    public string EventId { get; init; } = string.Empty;
    public string OccurredOn { get; init; } = string.Empty;

    public DomainEvent()
    {
    }

    protected DomainEvent(string eventId, string occurredOn)
    {
        EventId = eventId;
        OccurredOn = occurredOn;
    }

    public abstract string EventName();
}
