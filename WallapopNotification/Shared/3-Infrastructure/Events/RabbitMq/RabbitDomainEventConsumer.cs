using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WallapopNotification.Shared._1_Domain;
using WallapopNotification.Shared._1_Domain.Events;
using WallapopNotification.Users._2_Application.NotifyUser;

namespace WallapopNotification.Shared._3_Infrastructure.Events.RabbitMq;

public sealed class RabbitDomainEventConsumer : IDomainEventConsumer
{
    private const int MaxRetries = 2;
    private const string HeaderRedelivery = "redelivery_count";
    private readonly RabbitMqConnection _connection;

    private readonly Dictionary<string, object> _domainEventSubscribers = new();
    private DomainEventSubscribersInformation _information;
    private readonly IServiceProvider _serviceProvider;

    public RabbitDomainEventConsumer(
        IServiceProvider serviceProvider,
        DomainEventSubscribersInformation information,
        RabbitMqConnection connection)
    {
        _information = information;
        _connection = connection;
        _serviceProvider = serviceProvider;
    }

    public Task Consume()
    {
        _information.RabbitMqFormattedNames().ForEach(queue => _ = ConsumeMessages(queue));
        return Task.CompletedTask;
    }

    private async Task ConsumeMessages(string queue, ushort prefetchCount = 10)
    {
        var scope = _serviceProvider.CreateScope();
        
        var channel = await _connection.Channel();

        await DeclareQueue(channel, queue);

        await channel.BasicQosAsync(0, prefetchCount, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            //eventId:1, occurredOn:2, data:{"userId":3,"items":[{"id":4,"title":5,"price":6,"url":7,"imageUrl":8}]}
            var message = Encoding.UTF8.GetString(ea.Body.Span);

            var evenType = _information.GetSubscribedEvent(queue);

            var instance = (DomainEvent)Activator.CreateInstance(evenType);

            var document = JsonDocument.Parse(message);
            var root = document.RootElement;

            var eventId = root.GetProperty("eventId").GetString();

            var domainEvent = instance?.FromPrimitives(eventId,
                root.GetProperty("occurredOn").GetString(),
                root.GetProperty("data").ToString());

            var subscriber = _domainEventSubscribers.TryGetValue(queue, out var value)
                ? value
                : SubscribeFor(queue, scope);

            try
            {
                await ((IDomainEventHandler)subscriber).Handle(domainEvent);
            }
            catch
            {
                if (instance != null) HandleConsumptionError(ea, instance, queue);
            }

            await channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        var consumerId = await channel.BasicConsumeAsync(queue, false, consumer);
    }

    private object SubscribeFor(string queue, IServiceScope scope)
    {
        var t = _information.GetSubscriberClass(queue);
        var subscriber = scope.ServiceProvider.GetRequiredService(t);
        
        _domainEventSubscribers.Add(queue, subscriber);
        
        return subscriber;
    }

    public void WithSubscribersInformation(DomainEventSubscribersInformation information)
    {
        _information = information;
    }

    private static async Task DeclareQueue(IChannel channel, string queue)
    {
        await channel.QueueDeclareAsync(queue,
            true,
            false,
            false
        );
    }

    private void HandleConsumptionError(BasicDeliverEventArgs ea, DomainEvent @event, string queue)
    {
        if (HasBeenRedeliveredTooMuch(ea.BasicProperties.Headers))
            SendToDeadLetter(ea, queue);
        else
            SendToRetry(ea, queue);
    }

    private bool HasBeenRedeliveredTooMuch(IDictionary<string, object> headers)
    {
        return (int)(headers[HeaderRedelivery] ?? 0) >= MaxRetries;
    }

    private async Task SendToRetry(BasicDeliverEventArgs ea, string queue)
    {
        await SendMessageTo(RabbitMqExchangeNameFormatter.Retry("domain_events"), ea, queue);
    }

    private async Task SendToDeadLetter(BasicDeliverEventArgs ea, string queue)
    {
        await SendMessageTo(RabbitMqExchangeNameFormatter.DeadLetter("domain_events"), ea, queue);
    }

    private async Task SendMessageTo(string exchange, BasicDeliverEventArgs ea, string queue)
    {
        var channel = await _connection.Channel();
        await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic);

        var body = ea.Body;
        var properties = ea.BasicProperties;
        var headers = ea.BasicProperties.Headers;
        headers[HeaderRedelivery] = (int)headers[HeaderRedelivery] + 1;
        // properties.Headers = headers;

        await channel.BasicPublishAsync(exchange,
            queue,
            body);
    }
}