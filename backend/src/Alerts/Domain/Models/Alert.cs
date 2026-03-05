using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Src.Alerts.Domain.Models;

public sealed class Alert : AggregateRoot
{
    public Guid Id { get; }
    public long UserId { get; }
    public string Name { get; }
    public Url Url { get; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; private set; }

    public Alert()
    {
    }

    private Alert(Guid id, long userId, string name, string url)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Url = Url.Create(url);
        CreatedAt = TimeProvider.System.GetUtcNow().UtcDateTime;
        UpdatedAt = TimeProvider.System.GetUtcNow().UtcDateTime;
        IsActive = true;
    }

    public Alert(Guid id, long userId, string name, string url, DateTime createdAt, DateTime? updatedAt,
        bool isActive = true)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Url = Url.Create(url);
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        IsActive = isActive;
    }

    public static Alert Create(long userId, string name, string url)
    {
        var alert = new Alert(Guid.NewGuid(), userId, name, url);

        return alert;
    }

    public string GetCacheKey() => Id.ToString();

    public void NewSearch(List<Item>? wallapopItems)
    {
        if (wallapopItems is not null && wallapopItems.Count > 0)
        {
            Record(new NewItemsFoundEvent(Id.ToString(), TimeProvider.System.GetUtcNow().ToString(), UserId,
                wallapopItems));
        }
    }

    public void Touch()
    {
        UpdatedAt = TimeProvider.System.GetUtcNow().UtcDateTime;
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }
}