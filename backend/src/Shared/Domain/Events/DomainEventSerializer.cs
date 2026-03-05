using System.Text.Json;

namespace Wallanoti.Src.Shared.Domain.Events;

public sealed class DomainEventSerializer
{
    public static DomainEvent Deserialize(string @event)
    {
        var document = JsonDocument.Parse(@event);

        var root = document.RootElement;
        var data = root.GetProperty("data");

        return JsonSerializer.Deserialize<DomainEvent>(@event.ToString());
    }

    public static string Serialize<T>(T domainEvent) where T : DomainEvent
    {
        var obj = new
        {
            eventId = domainEvent.EventId,
            occurredOn = domainEvent.OccurredOn,
            data = domainEvent.ToJson()
        };

        return JsonSerializer.Serialize(obj);
    }
}