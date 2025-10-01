namespace WallapopNotification.Shared._3_Infrastructure.Events.RabbitMq;

public sealed class RabbitMqConfig
{
    public required string UserName { get; init; }

    public required string Password { get; init; }

    public required string HostName { get; init; }

    public int Port { get; init; } = 5672;
}