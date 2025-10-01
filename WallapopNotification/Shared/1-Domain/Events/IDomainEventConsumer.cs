namespace WallapopNotification.Shared._1_Domain.Events;

public interface IDomainEventConsumer
{
    public Task Consume();
}