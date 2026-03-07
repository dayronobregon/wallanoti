using MassTransit;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Shared.Infrastructure.Events.MassTransit;

/// <summary>
/// Generic MassTransit consumer that bridges IConsumer&lt;TEvent&gt; to the existing
/// IDomainEventHandler&lt;TEvent&gt; without requiring changes to any handler.
///
/// Each concrete pairing (TEvent + THandler) results in one registered consumer
/// with its own dedicated queue following the AGENTS.md naming convention.
/// </summary>
public sealed class DomainEventConsumerAdapter<TEvent, THandler> : IConsumer<TEvent>
    where TEvent : DomainEvent
    where THandler : class, IDomainEventHandler<TEvent>
{
    private readonly THandler _handler;

    public DomainEventConsumerAdapter(THandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        await _handler.Handle(context.Message);
    }
}
