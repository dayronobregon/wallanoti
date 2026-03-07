using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Alerts.Domain;

public sealed class NewItemsFoundEvent : DomainEvent
{
    public long UserId { get; init; }
    public IEnumerable<Item>? Items { get; init; }

    public NewItemsFoundEvent()
    {
        
    }
    
    public NewItemsFoundEvent(string eventId, string occurredOn, long userId, IEnumerable<Item>? items) : base(eventId,
        occurredOn)
    {
        UserId = userId;
        Items = items;
    }

    public override string EventName() => "alert.items-found";
}
