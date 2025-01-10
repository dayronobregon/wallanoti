using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using WallapopNotification.Shared._1_Domain.Events;

namespace WallapopNotification.Shared._3_Infraestrcture.Events.AzureServiceBus;

public sealed class DomainEventConsumer : IDomainEventConsumer
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<string, object> _domainEventHandlers = new();

    public DomainEventConsumer(ServiceBusClient serviceBusClient, IServiceProvider serviceProvider)
    {
        _serviceBusClient = serviceBusClient;
        _serviceProvider = serviceProvider;
    }


    public async Task Consume()
    {
        //obtener todas EventName de todas las clases que hereden de DomainEvent
        var events = typeof(DomainEvent).Assembly.GetTypes()
            .Where(x => x.IsSubclassOf(typeof(DomainEvent)) && !x.IsAbstract)
            .Select(x => Activator.CreateInstance(x) as DomainEvent);

        foreach (var @event in events)
        {
            await ConsumeMessage(@event.EventName());
        }
    }

    private async Task ConsumeMessage(string domainEventName)
    {
        var processor = _serviceBusClient.CreateProcessor(domainEventName, "S1");

        // configure the message and error handler to use
        processor.ProcessMessageAsync += args => MessageHandler(args, domainEventName);
        processor.ProcessErrorAsync += args =>
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        };

        // start processing
        await processor.StartProcessingAsync();
    }

    private async Task MessageHandler(ProcessMessageEventArgs arg, string domainEventName)
    {
        Console.WriteLine("Received message:");
        var body = arg.Message.Body.ToString();
        Console.WriteLine(body);

        var handler = GetHandler(domainEventName);

        var message = Encoding.UTF8.GetString(arg.Message.Body);

        var eventClass = typeof(DomainEvent).Assembly.GetTypes()
            .Where(x => x.IsSubclassOf(typeof(DomainEvent)) && !x.IsAbstract)
            .Select(x => Activator.CreateInstance(x) as DomainEvent)
            .First(x => x?.EventName() == domainEventName);

        var domainEvent = eventClass.FromJson(message);

        await handler.Handle(domainEvent);
    }


    private IDomainEventHandler GetHandler(string domainEventName)
    {
        if (_domainEventHandlers.ContainsKey(domainEventName) == false)
        {
            RegisterHandler(domainEventName);
        }

        return (IDomainEventHandler)_domainEventHandlers[domainEventName];
    }

    private void RegisterHandler(string domainEventName)
    {
        var eventClass = typeof(DomainEvent).Assembly.GetTypes()
            .Where(x => x.IsSubclassOf(typeof(DomainEvent)) && !x.IsAbstract)
            .Select(x => Activator.CreateInstance(x) as DomainEvent)
            .First(x => x?.EventName() == domainEventName);

        using var scope = _serviceProvider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        var handlerType = typeof(DomainEventHandler<>).MakeGenericType(eventClass.GetType());
        var handler = scopedProvider.GetRequiredService(handlerType);

        _domainEventHandlers.Add(domainEventName, handler);
    }
}