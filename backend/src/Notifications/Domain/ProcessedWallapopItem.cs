using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Notifications.Domain;

public sealed class ProcessedWallapopItem(
    Guid id,
    Guid alertId,
    string wallapopItemId,
    DateTime processedAtUtc,
    decimal storedPrice,
    long lastWallapopModifiedUtc) : AggregateRoot
{
    public Guid Id { get; } = id;
    public Guid AlertId { get; } = alertId;
    public string WallapopItemId { get; } = wallapopItemId;
    public DateTime ProcessedAtUtc { get; } = processedAtUtc;
    public decimal StoredPrice { get; } = storedPrice;
    public long LastWallapopModifiedUtc { get; } = lastWallapopModifiedUtc;
}