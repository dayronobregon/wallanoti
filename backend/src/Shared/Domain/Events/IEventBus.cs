namespace Wallanoti.Src.Shared.Domain.Events;

public interface IEventBus
{
    public Task Publish(List<DomainEvent>? domainEvents);
}