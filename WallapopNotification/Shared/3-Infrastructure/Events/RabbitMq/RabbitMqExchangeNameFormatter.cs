namespace WallapopNotification.Shared._3_Infrastructure.Events.RabbitMq;

public static class RabbitMqExchangeNameFormatter
{
    public static string Retry(string exchangeName)
    {
        return $"retry-{exchangeName}";
    }

    public static string DeadLetter(string exchangeName)
    {
        return $"dead_letter-{exchangeName}";
    }
}