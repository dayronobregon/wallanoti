namespace WallapopNotification.Shared._3_Infrastructure.Events.RabbitMq;

public class DomainEventSubscribersInformation
{
    private readonly Dictionary<Type, DomainEventSubscriberInformation> _information;

    public DomainEventSubscribersInformation(Dictionary<Type, DomainEventSubscriberInformation> information)
    {
        _information = information;
    }

    public Dictionary<Type, DomainEventSubscriberInformation>.ValueCollection All()
    {
        return _information.Values;
    }

    public List<string> RabbitMqFormattedNames()
    {
        return _information.Values.Select(x => x.FormatRabbitMqQueueName()).ToList();
    }

    public Type GetSubscriberClass(string queueName)
    {
        return _information.Values.First(x => x.FormatRabbitMqQueueName() == queueName).SubscriberClass;
    }

    public Type GetSubscribedEvent(string queueName)
    {
        return _information.Values.First(x => x.FormatRabbitMqQueueName() == queueName).SubscribedEvent;
    }
}