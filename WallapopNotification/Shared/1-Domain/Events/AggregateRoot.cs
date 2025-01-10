namespace WallapopNotification.Shared._1_Domain.Events;

public abstract class AggregateRoot
{
    private List<DomainEvent> _domainEvents = [];

    public List<DomainEvent> PullDomainEvents()
    {
        var events = _domainEvents;
        
        _domainEvents= [];

        return events;
    }

    protected void Record(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}