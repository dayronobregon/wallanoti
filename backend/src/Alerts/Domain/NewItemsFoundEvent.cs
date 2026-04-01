using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Alerts.Domain;

public sealed class NewItemsFoundEvent : DomainEvent
{
    public Guid AlertId { get; init; }
    public long UserId { get; init; }
    public IEnumerable<Item>? Items { get; init; }
    public IEnumerable<LabeledAlertItem>? LabeledItems { get; init; }

    public NewItemsFoundEvent()
    {
    }

    public NewItemsFoundEvent(string eventId, string occurredOn, Guid alertId, long userId,
        IEnumerable<LabeledAlertItem> labeledItems)
        : base(eventId, occurredOn)
    {
        AlertId = alertId;
        UserId = userId;
        LabeledItems = labeledItems;
        Items = labeledItems.Select(x => x.Item).ToList();
    }

    public override string EventName() => "alert.items-found";
}
