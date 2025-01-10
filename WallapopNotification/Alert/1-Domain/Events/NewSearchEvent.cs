using System.Text.Json;
using WallapopNotification.Alert._1_Domain.Models;
using WallapopNotification.Shared._1_Domain.Events;

namespace WallapopNotification.Alert._1_Domain.Events;

public sealed class NewSearchEvent : DomainEvent
{
    public long UserId { get; set; }
    public IEnumerable<Item>? Items { get; set; }

    public NewSearchEvent()
    {
    }

    public NewSearchEvent(long userId, IEnumerable<Item> items)
    {
        UserId = userId;
        Items = items;
    }

    public override string EventName() => "alert.newsearch";

    public override string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    public override DomainEvent FromJson(string json)
    {
        return JsonSerializer.Deserialize<NewSearchEvent>(json);
    }
}