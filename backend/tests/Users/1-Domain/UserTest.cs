using Wallanoti.Src.Users.Domain.Events;
using Wallanoti.Src.Users.Domain.Models;

namespace Wallanoti.Tests.Users._1_Domain;

public class UserTest
{
    [Fact]
    public void Login_ShouldGenerateVerificationCodeAndEvent()
    {
        var user = User.NewUser(1, "user");

        user.Login();

        Assert.NotNull(user.VerificationCode);

        var events = user.PullDomainEvents();
        var loginEvent = Assert.Single(events.OfType<UserLoggedInDomainEvent>());
        Assert.Equal(user.Id, loginEvent.TelegramUserId);
        Assert.Equal(user.VerificationCode!.Value, loginEvent.VerificationCode.Value);
    }

    [Fact]
    public void Verify_ShouldReturnFalseWhenCodeNotGenerated()
    {
        var user = User.NewUser(2, "user2");

        Assert.False(user.Verify("000000"));
    }
}
