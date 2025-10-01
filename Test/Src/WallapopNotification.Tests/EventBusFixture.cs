using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WallapopNotification.Shared._1_Domain.Events;
using WallapopNotification.Shared._3_Infrastructure.Events.RabbitMq;

namespace WallapopNotification.Tests;

public sealed class EventBusFixture : IAsyncDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private bool _disposed;

    public IEventBus EventBus { get; }
    public IServiceProvider ServiceProvider => _serviceProvider;

    public EventBusFixture()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", false, true)
            .AddEnvironmentVariables()
            .Build();
        ;

        var services = new ServiceCollection();

        // Configurar servicios para pruebas
        services.AddLogging();
        services.Configure<RabbitMqConfig>(config.GetSection("RabbitMq"));
        services.AddScoped<RabbitMqConnection>();
        services.AddScoped<RabbitMqEventBusConfiguration>();
        services.AddScoped<RabbitDomainEventConsumer>();

        services.AddScoped<IEventBus>(p =>
        {
            var connection = p.GetRequiredService<RabbitMqConnection>();
            return new RabbitMqEventBus(connection, "test_domain_events");
        });

        services.AddDomainEventSubscriberInformationService();

        _serviceProvider = services.BuildServiceProvider();
        EventBus = _serviceProvider.GetRequiredService<IEventBus>();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        await _serviceProvider.DisposeAsync();
        _disposed = true;
    }
}