using MassTransit;
using Moq;
using Wallanoti.Src.Shared.Infrastructure.Events.MassTransit;
using Wallanoti.Src.Users.Domain.Events;
using Wallanoti.Src.Users.Domain.ValueObjects;

namespace Wallanoti.Tests.Shared._3_Infrastructure.Events.RabbitMq;

public class DomainEventConsumerAdapterTest
{
    [Fact]
    public async Task Consume_ShouldForwardTypedMessageToDomainHandler()
    {
        var @event = new UserLoggedInDomainEvent(5, new VerificationCode("123456"));
        var handler = new RecordingUserLoggedInHandler();
        var sut = new DomainEventConsumerAdapter<UserLoggedInDomainEvent, RecordingUserLoggedInHandler>(handler);

        var context = new Mock<ConsumeContext<UserLoggedInDomainEvent>>();
        context.SetupGet(x => x.Message).Returns(@event);

        await sut.Consume(context.Object);

        Assert.Same(@event, handler.ReceivedEvent);
    }

    private sealed class RecordingUserLoggedInHandler : Wallanoti.Src.Shared.Domain.Events.IDomainEventHandler<UserLoggedInDomainEvent>
    {
        public UserLoggedInDomainEvent? ReceivedEvent { get; private set; }

        public Task Handle(UserLoggedInDomainEvent @event)
        {
            ReceivedEvent = @event;
            return Task.CompletedTask;
        }
    }
}
