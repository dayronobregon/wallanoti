using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Tests;

public class TestBase : IClassFixture<EventBusFixture>
{
    protected readonly EventBusFixture Fixture;
    protected IEventBus EventBus => Fixture.EventBus;

    public TestBase(EventBusFixture fixture)
    {
        Fixture = fixture;
    }
}