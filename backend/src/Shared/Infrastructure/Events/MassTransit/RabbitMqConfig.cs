namespace Wallanoti.Src.Shared.Infrastructure.Events.MassTransit;

/// <summary>
/// Configuration options for the RabbitMQ broker connection.
/// Bound from the "RabbitMq" section of appsettings.json.
/// </summary>
public sealed class RabbitMqConfig
{
    public required string UserName { get; init; }

    public required string Password { get; init; }

    public required string HostName { get; init; }

    public int Port { get; init; } = 5672;

    public string VirtualHost { get; init; } = "/";
}
