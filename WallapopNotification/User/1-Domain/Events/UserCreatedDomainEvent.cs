using System.Text.Json;
using WallapopNotification.Shared._1_Domain.Events;

namespace WallapopNotification.User._1_Domain.Events;

public sealed class UserCreatedDomainEvent : DomainEvent
{
    public long Id { get; }
    public string? UserName { get; }

    public UserCreatedDomainEvent()
    {
    }

    public UserCreatedDomainEvent(long id, string? userName)
    {
        Id = id;
        UserName = userName;
    }

    public override string EventName()
    {
        return "user.usercreated";
    }

    public override string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    public override DomainEvent FromJson(string json)
    {
        throw new NotImplementedException();
    }
}