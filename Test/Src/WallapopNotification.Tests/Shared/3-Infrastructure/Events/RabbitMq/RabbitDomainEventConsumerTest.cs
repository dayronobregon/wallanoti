using Microsoft.Extensions.DependencyInjection;
using Moq;
using WallapopNotification.Shared._3_Infrastructure.Events.RabbitMq;

namespace WallapopNotification.Tests.Shared._3_Infrastructure.Events.RabbitMq;

public class RabbitDomainEventConsumerTest : TestBase
{
    private readonly RabbitDomainEventConsumer _sut;


    public RabbitDomainEventConsumerTest(EventBusFixture fixture) : base(fixture)
    {
        _sut = fixture.ServiceProvider.GetRequiredService<RabbitDomainEventConsumer>();
    }

    [Fact]
    public async Task StartAsync_ShouldStartConsumingMessages()
    {
    }
}