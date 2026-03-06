using MassTransit;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Shared.Infrastructure.Events.MassTransit;

/// <summary>
/// IEventBus implementation backed by MassTransit.
/// Converts each DomainEvent into a DomainEventEnvelope and publishes it using
/// MassTransit's default RabbitMQ conventions.
/// </summary>
public sealed class MassTransitEventBus(IPublishEndpoint publishEndpoint) : IEventBus
{
    public async Task Publish(List<DomainEvent>? domainEvents)
    {
        if (domainEvents is null || domainEvents.Count == 0)
            return;

        foreach (var domainEvent in domainEvents)
            await PublishEvent(domainEvent);
    }

    private async Task PublishEvent(DomainEvent domainEvent)
    {
        var envelope = new DomainEventEnvelope
        {
            EventId = domainEvent.EventId,
            OccurredOn = domainEvent.OccurredOn,
            EventType = domainEvent.GetType().AssemblyQualifiedName!,
            Data = domainEvent.ToJson()
        };

        await publishEndpoint.Publish(envelope);
    }
}
