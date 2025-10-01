using System.Text.Json;
using WallapopNotification.Alerts._1_Domain.Models;
using WallapopNotification.Shared._1_Domain.Events;

namespace WallapopNotification.Alerts._1_Domain;

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

    public override string ToJson()
    {
        var obj = new
        {
            userId = UserId,
            items = Items
        };

        return JsonSerializer.Serialize(obj);
    }

    public override DomainEvent FromPrimitives(string eventId, string occurredOn, string data)
    {
        var document = JsonDocument.Parse(data);
        var root = document.RootElement;

        var userId = root.GetProperty("userId").GetInt64();
        var items = root.GetProperty("items").Deserialize<List<Item>>();

        return new NewItemsFoundEvent(eventId, occurredOn, userId, items);
    }
}