namespace WallapopNotification.Shared._1_Domain.Events;

public interface IEventBus
{
    public Task Publish(List<DomainEvent>? domainEvents);
}