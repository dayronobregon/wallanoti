namespace Wallanoti.Tests.Shared._3_Infrastructure.Events.RabbitMq;

public class DomainEventConsumerTest : TestBase
{
    public DomainEventConsumerTest(EventBusFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Consumer_ShouldBeRegisteredViaMassTransit()
    {
        // MassTransit manages consumer lifecycle — this test is a placeholder.
        // Integration tests for message consumption can be added using
        // MassTransit's built-in test harness (AddMassTransitTestHarness).
        await Task.CompletedTask;
    }
}