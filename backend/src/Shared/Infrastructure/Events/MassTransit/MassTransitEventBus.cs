using MassTransit;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Shared.Infrastructure.Events.MassTransit;

/// <summary>
/// IEventBus implementation backed by MassTransit.
/// Publishes each concrete DomainEvent directly so MassTransit applies
/// native serialization and message-type routing.
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
        await publishEndpoint.Publish(domainEvent, domainEvent.GetType());
    }
}
