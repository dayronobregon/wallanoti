using Wallanoti.Src.Shared.Domain;

namespace Wallanoti.Src.Shared.Infrastructure.Events.RabbitMq;

public class DomainEventSubscriberInformation
{
    public readonly Type SubscriberClass;

    public Type SubscribedEvent { get; }

    private string? ContextName
    {
        get
        {
            var nameParts = SubscriberClass.FullName?.Split(".");
            return nameParts?[0];
        }
    }

    private string? ModuleName
    {
        get
        {
            var nameParts = SubscriberClass.FullName?.Split(".");
            return nameParts?[1];
        }
    }

    private string? ClassName
    {
        get
        {
            var nameParts = SubscriberClass.FullName?.Split(".");
            return nameParts?[^1];
        }
    }

    public DomainEventSubscriberInformation(Type subscriberClass, Type subscribedEvent)
    {
        SubscribedEvent = subscribedEvent;
        SubscriberClass = subscriberClass;
    }

    public string FormatRabbitMqQueueName()
    {
        return $"{ContextName?.ToSnake()}.{ModuleName?.ToSnake()}.{ClassName?.ToSnake()}";
    }
}