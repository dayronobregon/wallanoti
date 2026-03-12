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

    private Alert(Guid id, long userId, string name, string url)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Url = Url.Create(url);
        CreatedAt = Wallanoti.Src.Shared.Domain.AppTime.Current.GetUtcNow().UtcDateTime;
        UpdatedAt = Wallanoti.Src.Shared.Domain.AppTime.Current.GetUtcNow().UtcDateTime;
        IsActive = true;
    }

    public Alert(Guid id, long userId, string name, string url, DateTime createdAt, DateTime? updatedAt,
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
            Record(new NewItemsFoundEvent(Guid.NewGuid().ToString(), Wallanoti.Src.Shared.Domain.AppTime.Current.GetUtcNow().ToString("o"), Id,
                UserId,
                wallapopItems));
            
            RecordSearch();
        }
    }

    public void RecordSearch()
    {
        LastSearchedAt = Wallanoti.Src.Shared.Domain.AppTime.Current.GetUtcNow().UtcDateTime;
    }

    private void Touch()
    {
        UpdatedAt = Wallanoti.Src.Shared.Domain.AppTime.Current.GetUtcNow().UtcDateTime;
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
