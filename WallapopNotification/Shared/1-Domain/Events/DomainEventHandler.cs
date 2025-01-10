using MediatR;

namespace WallapopNotification.Shared._1_Domain.Events;

public abstract class DomainEventHandler<T> : IDomainEventHandler where T : DomainEvent
{
    async Task IDomainEventHandler.Handle(DomainEvent @event)
    {
        if (@event is T msg)
            await Handle(msg);
    }

    protected abstract Task Handle(T @event);
}

public interface IDomainEventHandler
{
    public Task Handle(DomainEvent @event);
}