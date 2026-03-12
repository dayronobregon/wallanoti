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
    public DateTime? LastSearchedAt { get; set; }
    public bool IsActive { get; private set; }

    public Alert()
    {
    }

    public Alert(Guid id, long userId, string name, string url, DateTime createdAt, DateTime? updatedAt = null,
        bool isActive = true, DateTime? lastSearchedAt = null)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Url = Url.Create(url);
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        IsActive = isActive;
        LastSearchedAt = lastSearchedAt;
    }

    public static Alert Create(long userId, string name, string url, DateTime createdAt)
    {
        var alert = new Alert(Guid.NewGuid(), userId, name, url, createdAt, null);

        return alert;
    }

    public string GetCacheKey() => Id.ToString();

    public void NewSearch(List<Item>? wallapopItems, DateTime eventTimestamp, DateTime searchTimestamp)
    {
        if (wallapopItems is not null && wallapopItems.Count > 0)
        {
            Record(new NewItemsFoundEvent(Guid.NewGuid().ToString(), eventTimestamp.ToString("o"), Id,
                UserId,
                wallapopItems));

            RecordSearch(searchTimestamp);
        }
    }

    public void RecordSearch(DateTime timestamp)
    {
        LastSearchedAt = timestamp;
    }

    private void Touch(DateTime timestamp)
    {
        UpdatedAt = timestamp;
    }

    public void Deactivate(DateTime timestamp)
    {
        IsActive = false;
        Touch(timestamp);
    }

    public void Activate(DateTime timestamp)
    {
        IsActive = true;
        Touch(timestamp);
    }
}