using Microsoft.Extensions.Configuration;
using Moq;
using Wallanoti.Src.Users.Application.Login;
using Wallanoti.Src.Users.Domain.Models;
using Wallanoti.Src.Users.Domain.Repositories;

namespace Wallanoti.Tests.Users._2_Application.Login;

public class VerifyQueryHandlerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly IConfiguration _configuration;
    private readonly VerifyQueryHandler _sut;

    public VerifyQueryHandlerTest()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "supersecretkeysupersecretkey1234",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:Issuer"] = "test-issuer"
            })
            .Build();

        _sut = new VerifyQueryHandler(_configuration, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenVerificationSucceeds_ReturnsJwtToken()
    {
        var user = User.NewUser(1, "user");
        user.Login();
        var code = user.VerificationCode!.Value;

        _userRepositoryMock.Setup(x => x.Search(user.UserName)).ReturnsAsync(user);

        var token = await _sut.Handle(new VerifyUserRequest(user.UserName, code), CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Handle_WhenVerificationFails_ReturnsNull()
    {
        var user = User.NewUser(2, "user2");
        user.Login();

        _userRepositoryMock.Setup(x => x.Search(user.UserName)).ReturnsAsync(user);

        var token = await _sut.Handle(new VerifyUserRequest(user.UserName, "000000"), CancellationToken.None);

        Assert.Null(token);
    }
}
