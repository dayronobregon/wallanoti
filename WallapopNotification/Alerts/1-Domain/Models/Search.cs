namespace WallapopNotification.Alerts._1_Domain.Models;

public sealed class Search
{
    public required Guid AlertId { get; init; }
    public List<string>? ItemIds { get; private set; }
    public required DateOnly Date { get; init; }

    public static Search NewSearch(Guid alertId, List<Item>? items)
    {
        return new Search
        {
            AlertId = alertId,
            ItemIds = items?.Select(x => x.Id).ToList(),
            Date = DateOnly.FromDateTime(DateTime.Now)
        };
    }

    public void Update(IEnumerable<Item>? items)
    {
        if (items is null) return;

        ItemIds ??= [];
        ItemIds?.AddRange(items.Select(x => x.Id).ToList());
    }
}