using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Shared.Infrastructure.Events.MassTransit;

namespace Wallanoti.Tests;

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

        var services = new ServiceCollection();

        services.AddLogging();

        var rabbitMqConfig = config.GetSection("RabbitMq").Get<RabbitMqConfig>()
            ?? throw new InvalidOperationException("RabbitMq config missing in test settings.");

        services.AddMassTransitEventBus(rabbitMqConfig);
        services.AddScoped<IEventBus, MassTransitEventBus>();

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