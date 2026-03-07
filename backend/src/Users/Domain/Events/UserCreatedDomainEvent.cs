using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Users.Domain.Events;

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
}
