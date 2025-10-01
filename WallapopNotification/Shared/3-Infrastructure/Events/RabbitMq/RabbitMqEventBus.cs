using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using WallapopNotification.Shared._1_Domain.Events;

namespace WallapopNotification.Shared._3_Infrastructure.Events.RabbitMq;

public sealed class RabbitMqEventBus(RabbitMqConnection connection, string exchangeName = "domain_events") : IEventBus
{
    public async Task Publish(List<DomainEvent>? domainEvents)
    {
        if (domainEvents == null || domainEvents.Count == 0)
        {
            return;
        }

        foreach (var @event in domainEvents)
        {
            await PublishEvent(@event);
        }
    }


    private async Task PublishEvent(DomainEvent @event)
    {
        var channel = await connection.Channel();
        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic);

        var body = Encoding.UTF8.GetBytes(DomainEventSerializer.Serialize(@event));
        var properties = new BasicProperties
        {
            MessageId = @event.EventId,
            ContentEncoding = Encoding.UTF8.ToString(),
            ContentType = "application/json",
        };

        await channel.BasicPublishAsync(exchangeName,
            @event.EventName(),
            true,
            properties,
            body);
    }
}