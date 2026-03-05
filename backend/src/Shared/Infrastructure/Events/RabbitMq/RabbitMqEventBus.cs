using System.Text;
using RabbitMQ.Client;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Shared.Infrastructure.Events.RabbitMq;

public sealed class RabbitMqEventBus(RabbitMqConnection connection, string exchangeName = "domain_events") : IEventBus
{
    private const string HeaderReDelivery = "redelivery_count";

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
            Headers = new Dictionary<string, object?>
            {
                { HeaderReDelivery, 0 }
            }
        };


        await channel.BasicPublishAsync(exchangeName,
            @event.EventName(),
            true,
            properties,
            body);
    }
}