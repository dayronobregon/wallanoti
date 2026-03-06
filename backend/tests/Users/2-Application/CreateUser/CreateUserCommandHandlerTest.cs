using Moq;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Users.Application.CreateUser;
using Wallanoti.Src.Users.Domain.Models;
using Wallanoti.Src.Users.Domain.Repositories;

namespace Wallanoti.Tests.Users._2_Application.CreateUser;

public class CreateUserCommandHandlerTest
{
    private readonly Mock<IEventBus> _eventBusMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly CreateUserCommandHandler _sut;

    public CreateUserCommandHandlerTest()
    {
        _eventBusMock.Setup(x => x.Publish(It.IsAny<List<DomainEvent>>()))
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.Add(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _sut = new CreateUserCommandHandler(_eventBusMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_AddsAndPublishesEvents()
    {
        var command = new CreateUserCommand(1, "username");

        _userRepositoryMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);

        await _sut.Handle(command, CancellationToken.None);

        _userRepositoryMock.Verify(x => x.Add(It.Is<User>(u => u.Id == command.Id && u.UserName == command.UserName)),
            Times.Once);
        _eventBusMock.Verify(x => x.Publish(It.IsAny<List<DomainEvent>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_DoesNotAddAgain()
    {
        var command = new CreateUserCommand(2, "existing");

        _userRepositoryMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(true);

        await _sut.Handle(command, CancellationToken.None);

        _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
        _eventBusMock.Verify(x => x.Publish(It.IsAny<List<DomainEvent>>()), Times.Once);
    }
}
