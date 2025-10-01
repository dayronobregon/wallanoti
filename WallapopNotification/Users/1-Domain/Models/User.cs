using WallapopNotification.Alerts._1_Domain.Models;
using WallapopNotification.Shared._1_Domain.Events;
using WallapopNotification.Users._1_Domain.Events;

namespace WallapopNotification.Users._1_Domain.Models;

public sealed class User : AggregateRoot
{
    public long Id { get; private set; }
    public string? UserName { get; private set; }

    public User(long id, string? userName)
    {
        Id = id;
        UserName = userName;
    }

    public User()
    {
    }

    public static User CreateUser(long id, string? userName)
    {
        var user = new User(id, userName);

        //user.Record(new UserCreatedDomainEvent(user.Id, user.UserName));

        return user;
    }
}