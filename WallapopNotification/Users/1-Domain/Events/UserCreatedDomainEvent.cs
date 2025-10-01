using System.Text.Json;
using WallapopNotification.Shared._1_Domain.Events;

namespace WallapopNotification.Users._1_Domain.Events;

public sealed class UserCreatedDomainEvent : DomainEvent
{
    public long Id { get; }
    public string? UserName { get; }


    public UserCreatedDomainEvent(string eventId, string ocurredOn, long id, string? userName) : base(eventId,
        ocurredOn)
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

    public override DomainEvent FromPrimitives(string eventId, string occurredOn, string data)
    {
        throw new NotImplementedException();
    }
}