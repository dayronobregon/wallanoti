using WallapopNotification.Shared._1_Domain.Events;
using WallapopNotification.Shared._1_Domain.ValueObject;

namespace WallapopNotification.Alerts._1_Domain.Models;

public sealed class Alert : AggregateRoot
{
    public Guid Id { get; }
    public long UserId { get; }
    public string Name { get; }
    public Url Url { get; }
    public DateTime CreatedAt { get; }
    public DateTime? LastSearch { get; set; }


    private Alert(Guid id, long userId, string name, string url)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Url = Url.Create(url);
        CreatedAt = DateTime.UtcNow;
    }

    public Alert(Guid id, long userId, string name, string url, DateTime createdAt, DateTime? lastSearch)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Url = Url.Create(url);
        CreatedAt = createdAt;
        LastSearch = lastSearch;
    }

    public Alert()
    {
    }

    public static Alert Create(long userId, string name, string url)
    {
        var alert = new Alert(Guid.NewGuid(), userId, name, url);

        return alert;
    }

    public string GetCacheKey() => Id.ToString();

    public void NewSearch(List<Item>? wallapopItems)
    {
        Touch();

        if (wallapopItems is not null && wallapopItems.Count > 0)
        {
            Record(new NewItemsFoundEvent(Guid.NewGuid().ToString(), TimeProvider.System.GetUtcNow().ToString(), UserId,
                wallapopItems));
        }
    }
    
    public void Touch()
    {
        LastSearch = DateTime.UtcNow;
    }
}