using WallapopNotification.Alert._1_Domain.Events;
using WallapopNotification.Shared._1_Domain.Events;

namespace WallapopNotification.Alert._1_Domain.Models;

public sealed class AlertEntity : AggregateRoot
{
    public Guid Id { get; private set; }
    public long UserId { get; private set; }
    public string Name { get; private set; }
    public string Url { get; private set; }
    public List<string>? LastSearch { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SearchOn { get; private set; }

    private AlertEntity(Guid id, long userId, string name, string url, DateTime? searchOn = null)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Url = url;
        SearchOn = searchOn;
        CreatedAt = DateTime.Now;
    }

    public AlertEntity()
    {
    }

    public static AlertEntity Create(long userId, string name, string url)
    {
        if (!ValidUrl(url))
        {
            throw new ArgumentException("Invalid url");
        }

        var alert = new AlertEntity(Guid.NewGuid(), userId, name, url);

        return alert;
    }

    public static bool ValidUrl(string url)
    {
        return url.Contains("https://es.wallapop.com/");
    }

    public bool ItemAlreadyFound(string wallapopItemId)
    {
        return LastSearch?.Exists(item => item == wallapopItemId) ?? false;
    }

    public void NewSearch(List<Item> wallapopItems)
    {
        var newFoundItems = (
                from item in wallapopItems
                where (DateTimeOffset.FromUnixTimeMilliseconds(item.CreatedAt).DateTime.CompareTo(CreatedAt) >= 0 ||
                       DateTimeOffset.FromUnixTimeMilliseconds(item.ModifiedAt).DateTime.CompareTo(CreatedAt) >= 0) &&
                      !ItemAlreadyFound(item.Id)
                select item)
            .ToList();

        LastSearch ??= [];
        LastSearch.AddRange(newFoundItems.Select(item => item.Id).ToList());

        SearchOn = DateTime.Now;

        if (newFoundItems.Count == 0)
        {
            return;
        }

        Record(new NewSearchEvent(UserId, newFoundItems));
    }
}