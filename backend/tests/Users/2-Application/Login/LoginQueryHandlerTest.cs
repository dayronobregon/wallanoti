using Moq;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Users.Application.Login;
using Wallanoti.Src.Users.Domain.Models;
using Wallanoti.Src.Users.Domain.Repositories;

namespace Wallanoti.Tests.Users._2_Application.Login;

public class LoginQueryHandlerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IEventBus> _eventBusMock = new();
    private readonly LoginQueryHandler _sut;

    public LoginQueryHandlerTest()
    {
        _eventBusMock.Setup(x => x.Publish(It.IsAny<List<DomainEvent>>()))
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.Save(It.IsAny<User>())).Returns(Task.CompletedTask);
        _sut = new LoginQueryHandler(_userRepositoryMock.Object, _eventBusMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNull()
    {
        _userRepositoryMock.Setup(x => x.Search("missing")).ReturnsAsync((User?)null);

        var result = await _sut.Handle(new LoginUserRequest("missing"), CancellationToken.None);

        Assert.Null(result);
        _userRepositoryMock.Verify(x => x.Save(It.IsAny<User>()), Times.Never);
        _eventBusMock.Verify(x => x.Publish(It.IsAny<List<DomainEvent>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserExists_LogsInAndPublishesEvents()
    {
        var user = User.NewUser(10, "username");
        _userRepositoryMock.Setup(x => x.Search(user.UserName)).ReturnsAsync(user);

        var result = await _sut.Handle(new LoginUserRequest(user.UserName), CancellationToken.None);

        Assert.Equal(user.UserName, result);
        Assert.NotNull(user.VerificationCode);

        _userRepositoryMock.Verify(x => x.Save(user), Times.Once);
        _eventBusMock.Verify(x => x.Publish(It.Is<List<DomainEvent>>(events =>
            events.Any(e => e is Wallanoti.Src.Users.Domain.Events.UserLoggedInDomainEvent))), Times.Once);
    }
}
