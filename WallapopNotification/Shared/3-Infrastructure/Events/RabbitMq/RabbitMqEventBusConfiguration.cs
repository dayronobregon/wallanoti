using RabbitMQ.Client;

namespace WallapopNotification.Shared._3_Infrastructure.Events.RabbitMq;

public sealed class RabbitMqEventBusConfiguration
{
    private readonly RabbitMqConnection _connection;

    private readonly string _domainEventExchange;
    private readonly DomainEventSubscribersInformation _domainEventSubscribersInformation;

    public RabbitMqEventBusConfiguration(
        DomainEventSubscribersInformation domainEventSubscribersInformation,
        RabbitMqConnection connection, 
        string domainEventExchange = "domain_events")
    {
        _domainEventSubscribersInformation = domainEventSubscribersInformation;
        _connection = connection;
        _domainEventExchange = domainEventExchange;
    }

    public async Task Configure()
    {
        var channel = await _connection.Channel();

        var retryDomainEventExchange = RabbitMqExchangeNameFormatter.Retry(_domainEventExchange);
        var deadLetterDomainEventExchange = RabbitMqExchangeNameFormatter.DeadLetter(_domainEventExchange);

        await channel.ExchangeDeclareAsync(_domainEventExchange, ExchangeType.Topic);
        await channel.ExchangeDeclareAsync(retryDomainEventExchange, ExchangeType.Topic);
        await channel.ExchangeDeclareAsync(deadLetterDomainEventExchange, ExchangeType.Topic);

        foreach (var subscriberInformation in _domainEventSubscribersInformation.All())
        {
            var domainEventsQueueName = RabbitMqQueueNameFormatter.Format(subscriberInformation);
            var retryQueueName = RabbitMqQueueNameFormatter.FormatRetry(subscriberInformation);
            var deadLetterQueueName = RabbitMqQueueNameFormatter.FormatDeadLetter(subscriberInformation);
            var subscribedEvent = EventNameSubscribed(subscriberInformation);

            var queue = await channel.QueueDeclareAsync(domainEventsQueueName,
                true,
                false,
                false);

            var retryQueue = await channel.QueueDeclareAsync(retryQueueName, true,
                false,
                false,
                RetryQueueArguments(_domainEventExchange, domainEventsQueueName));

            var deadLetterQueue = await channel.QueueDeclareAsync(deadLetterQueueName, true,
                false,
                false);

            await channel.QueueBindAsync(queue, _domainEventExchange, domainEventsQueueName);
            await channel.QueueBindAsync(retryQueue, retryDomainEventExchange, domainEventsQueueName);
            await channel.QueueBindAsync(deadLetterQueue, deadLetterDomainEventExchange, domainEventsQueueName);

            await channel.QueueBindAsync(queue, _domainEventExchange, subscribedEvent);
        }
    }

    private static IDictionary<string, object?> RetryQueueArguments(string domainEventExchange,
        string domainEventQueue)
    {
        return new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", domainEventExchange },
            { "x-dead-letter-routing-key", domainEventQueue },
            { "x-message-ttl", 1000 }
        };
    }

    private static string EventNameSubscribed(DomainEventSubscriberInformation subscriberInformation)
    {
        dynamic domainEvent = Activator.CreateInstance(subscriberInformation.SubscribedEvent);
        return domainEvent.EventName();
    }
}