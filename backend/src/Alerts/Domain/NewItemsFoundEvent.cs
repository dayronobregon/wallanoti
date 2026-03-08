using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Alerts.Domain;

public sealed class NewItemsFoundEvent : DomainEvent
{
    public Guid AlertId { get; init; }
    public long UserId { get; init; }
    public IEnumerable<Item>? Items { get; init; }

    public NewItemsFoundEvent()
    {
        
    }
    
    public NewItemsFoundEvent(string eventId, string occurredOn, Guid alertId, long userId, IEnumerable<Item>? items)
        : base(eventId, occurredOn)
    {
        AlertId = alertId;
        UserId = userId;
        Items = items;
    }

    public override string EventName() => "alert.items-found";
}
