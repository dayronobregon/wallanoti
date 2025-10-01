using WallapopNotification.Shared._1_Domain.Events;

namespace WallapopNotification.Tests;

public class TestBase : IClassFixture<EventBusFixture>
{
    protected readonly EventBusFixture Fixture;
    protected IEventBus EventBus => Fixture.EventBus;

    public TestBase(EventBusFixture fixture)
    {
        Fixture = fixture;
    }
}