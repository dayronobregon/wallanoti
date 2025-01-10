using System.Text;
using Azure.Messaging.ServiceBus;
using WallapopNotification.Shared._1_Domain.Events;

namespace WallapopNotification.Shared._3_Infraestrcture.Events.AzureServiceBus;

public sealed class AzureEventBus : IEventBus
{
    private readonly ServiceBusClient _client;

    public AzureEventBus(ServiceBusClient client)
    {
        _client = client;
    }

    public async Task Publish(List<DomainEvent>? domainEvent)
    {
        if (domainEvent == null)
        {
            return;
        }

        foreach (var @event in domainEvent)
        {
            await PublishEvent(@event);
        }
    }

    private async Task PublishEvent(DomainEvent domainEvent)
    {
        var sender = _client.CreateSender(domainEvent.EventName());

        var message = new ServiceBusMessage(domainEvent.ToJson());

        await sender.SendMessageAsync(message);
    }
}