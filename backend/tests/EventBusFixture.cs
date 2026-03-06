using Microsoft.Extensions.DependencyInjection;
using Moq;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Tests;

public sealed class EventBusFixture : IAsyncDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private bool _disposed;

    public IEventBus EventBus { get; }
    public IServiceProvider ServiceProvider => _serviceProvider;

    public EventBusFixture()
    {
        var services = new ServiceCollection();

        var eventBusMock = new Mock<IEventBus>();
        eventBusMock.Setup(x => x.Publish(It.IsAny<List<DomainEvent>>()))
            .Returns(Task.CompletedTask);

        services.AddSingleton<IEventBus>(_ => eventBusMock.Object);

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
