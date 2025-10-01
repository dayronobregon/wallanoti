using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace WallapopNotification.Shared._3_Infrastructure.Events.RabbitMq;

public sealed class RabbitMqConnection
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqConnection> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqConnection(
        IOptions<RabbitMqConfig> rabbitMqConfig,
        ILogger<RabbitMqConnection> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var configParams = rabbitMqConfig.Value;

        _connectionFactory = new ConnectionFactory
        {
            HostName = configParams.HostName,
            UserName = configParams.UserName,
            Password = configParams.Password,
            Port = configParams.Port,
            VirtualHost = configParams.VirtualHost
        };
    }

    private async Task<IConnection> Connection()
    {
        if (_connection is { IsOpen: false } or null)
        {
            _connection = await _connectionFactory.CreateConnectionAsync();
        }

        return _connection;
    }

    public async Task<IChannel> Channel()
    {
        var connection = await Connection();

        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        _channel = await connection.CreateChannelAsync();

        return _channel;
    }
}