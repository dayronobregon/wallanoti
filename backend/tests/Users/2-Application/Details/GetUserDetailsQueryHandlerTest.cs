using Moq;
using Wallanoti.Src.Shared.Domain;
using Wallanoti.Src.Users.Application.Details;
using Wallanoti.Src.Users.Domain.Models;
using Wallanoti.Src.Users.Domain.Repositories;

namespace Wallanoti.Tests.Users._2_Application.Details;

public class GetUserDetailsQueryHandlerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly UserContext _userContext = new();
    private readonly GetUserDetailsQueryHandler _sut;

    public GetUserDetailsQueryHandlerTest()
    {
        _sut = new GetUserDetailsQueryHandler(_userRepositoryMock.Object, _userContext);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ReturnsUserDetailsResponse()
    {
        var user = new User(1, "username");
        _userContext.SetUser("token", user.Id, user.UserName);
        _userRepositoryMock.Setup(x => x.Find(user.Id)).ReturnsAsync(user);

        var result = await _sut.Handle(new GetUserDetailsQuery(), CancellationToken.None);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.UserName, result.UserName);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.Handle(new GetUserDetailsQuery(), CancellationToken.None));

        _userRepositoryMock.Verify(x => x.Find(It.IsAny<long>()), Times.Never);
    }
}
