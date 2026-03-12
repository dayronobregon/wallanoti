namespace Wallanoti.Src.Alerts.Domain.Models;

public sealed class Search
{
    public required Guid AlertId { get; init; }
    public List<string>? ItemIds { get; private set; }
    public required DateOnly Date { get; init; }

    public static Search NewSearch(Guid alertId, List<Item>? items, TimeProvider timeProvider)
    {
        return new Search
        {
            AlertId = alertId,
            ItemIds = items?.Select(x => x.Id).ToList(),
            Date = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime)
        };
    }

    public void Update(IEnumerable<Item>? items)
    {
        if (items is null) return;

        ItemIds ??= [];
        ItemIds?.AddRange(items.Select(x => x.Id).ToList());
    }
}