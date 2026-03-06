using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Shared.Infrastructure.Events.MassTransit;

/// <summary>
/// Generic MassTransit consumer that bridges IConsumer&lt;DomainEventEnvelope&gt; to
/// the existing IDomainEventHandler&lt;TEvent&gt; without requiring changes to any handler.
///
/// Each concrete pairing (TEvent + THandler) results in one registered consumer
/// with its own dedicated queue following the AGENTS.md naming convention.
/// </summary>
public sealed class DomainEventConsumerAdapter<TEvent, THandler> : IConsumer<DomainEventEnvelope>
    where TEvent : DomainEvent
    where THandler : class, IDomainEventHandler<TEvent>
{
    private readonly THandler _handler;

    public DomainEventConsumerAdapter(THandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<DomainEventEnvelope> context)
    {
        var envelope = context.Message;

        // Instantiate a temporary prototype to access the FromPrimitives factory.
        // This mirrors the existing DomainEventSerializer deserialization approach.
        var prototype = Activator.CreateInstance<TEvent>();
        var domainEvent = (TEvent)prototype.FromPrimitives(
            envelope.EventId,
            envelope.OccurredOn,
            envelope.Data
        );

        await _handler.Handle(domainEvent);
    }
}
