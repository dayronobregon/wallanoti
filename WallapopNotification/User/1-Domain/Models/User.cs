using WallapopNotification.Alert._1_Domain;
using WallapopNotification.Alert._1_Domain.Models;
using WallapopNotification.Shared._1_Domain.Events;
using WallapopNotification.User._1_Domain.Events;

namespace WallapopNotification.User._1_Domain.Models;

public sealed class User : AggregateRoot
{
    public long Id { get; private set; }
    public string? UserName { get; private set; }

    public List<AlertEntity>? Alerts { get; private set; }

    public User(long id, string? userName, List<AlertEntity>? alerts = null)
    {
        Id = id;
        UserName = userName;
        Alerts = alerts;
    }

    public User()
    {
    }

    public static User CreateUser(long id, string? userName)
    {
        var user = new User(id, userName);

        user.Record(new UserCreatedDomainEvent(user.Id, user.UserName));

        return user;
    }
}