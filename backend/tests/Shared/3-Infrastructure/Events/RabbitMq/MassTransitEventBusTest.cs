using MassTransit;
using Moq;
using Wallanoti.Src.Shared.Infrastructure.Events.MassTransit;
using Wallanoti.Src.Users.Domain.Events;
using Wallanoti.Src.Users.Domain.ValueObjects;

namespace Wallanoti.Tests.Shared._3_Infrastructure.Events.RabbitMq;

public class MassTransitEventBusTest
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly MassTransitEventBus _sut;

    public MassTransitEventBusTest()
    {
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<Type>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new MassTransitEventBus(_publishEndpointMock.Object);
    }

    [Fact]
    public async Task Publish_WhenEventsListIsNull_ShouldNotPublishAnyMessage()
    {
        await _sut.Publish(null);

        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<object>(), It.IsAny<Type>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Publish_WhenEventsListIsEmpty_ShouldNotPublishAnyMessage()
    {
        await _sut.Publish([]);

        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<object>(), It.IsAny<Type>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Publish_ShouldPublishConcreteDomainEventTypeWithoutEnvelope()
    {
        var @event = new UserLoggedInDomainEvent(5, new VerificationCode("123456"));

        await _sut.Publish([@event]);

        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<object>(message => ReferenceEquals(message, @event)),
                typeof(UserLoggedInDomainEvent),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
