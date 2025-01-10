using WallapopNotification.Shared._1_Domain.Events;

namespace WallapopNotification.Alert._1_Domain.Models;

public sealed class Item : AggregateRoot
{
    public required string Id { get; init; }
    public required string WallapopUserId { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; }
    public int CategoryId { get; set; }
    public required Price Price { get; set; }
    public List<string>? Images { get; set; }
    public bool Reserved { get; set; }
    public Location Location { get; set; }
    public bool Shipping { get; set; }
    public bool Favorited { get; set; }
    public required string WebSlug { get; set; }
    public required long CreatedAt { get; set; }
    public required long ModifiedAt { get; set; }
}