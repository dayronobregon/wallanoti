namespace Wallanoti.Src.Shared.Domain.Events;

public interface IDomainEventConsumer
{
    public Task Consume();
}