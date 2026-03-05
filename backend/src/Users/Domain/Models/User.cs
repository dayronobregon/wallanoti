using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Users.Domain.Events;
using Wallanoti.Src.Users.Domain.ValueObjects;

namespace Wallanoti.Src.Users.Domain.Models;

public sealed class User : AggregateRoot
{
    public long Id { get; private set; }
    public string UserName { get; private set; }
    public VerificationCode? VerificationCode { get; private set; }

    public User()
    {
    }

    public User(long id, string userName)
    {
        Id = id;
        UserName = userName;
    }

    public static User NewUser(long userId, string userName)
    {
        var user = new User(userId, userName);

        //TODO notificar la creacion de usuarios a admins
        //user.Record(new UserCreatedDomainEvent(user.Id, DateTimeOffset.UtcNow.ToString(), user.UserName));

        return user;
    }

    public void Login()
    {
        VerificationCode = VerificationCode.Random();

        Record(new UserLoggedInDomainEvent(Id, VerificationCode));
    }

    public bool Verify(string code)
    {
        return VerificationCode?.Verify(code) == true;
    }
}