using System;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Shared.Domain.ValueObjects;
using Wallanoti.Src.Alerts.Domain;

namespace Wallanoti.Src.Alerts.Domain.Entities;

public sealed class ProcessedItem : AggregateRoot
{
    public Guid AlertId { get; private set; }
    public string ItemId { get; private set; } = null!;
    public long LastModifiedAtMs { get; private set; }
    public Price LastPrice { get; private set; } = null!;
    public byte[]? RowVersion { get; set; } // concurrency token

    private ProcessedItem() { }

    public ProcessedItem(Guid alertId, string itemId, long lastModifiedAtMs, Price lastPrice)
    {
        AlertId = alertId == Guid.Empty ? throw new ArgumentException("AlertId cannot be empty", nameof(alertId)) : alertId;
        ItemId = string.IsNullOrWhiteSpace(itemId) ? throw new ArgumentException("ItemId cannot be empty", nameof(itemId)) : itemId;
        LastModifiedAtMs = lastModifiedAtMs;
        LastPrice = lastPrice ?? throw new ArgumentNullException(nameof(lastPrice));
    }

    public ChangeType DetectChange(Item item)
    {
        if (item.Price is null || LastPrice is null) return ChangeType.None;

        bool isPriceDrop = item.Price.CurrentPrice < LastPrice.CurrentPrice;
        bool isUpdate = item.ModifiedAt > LastModifiedAtMs;

        if (isPriceDrop) return ChangeType.PriceDrop;
        if (isUpdate) return ChangeType.Update;
        return ChangeType.None;
    }

    public void UpdateFrom(Item item)
    {
        var changeType = DetectChange(item);
        if (changeType == ChangeType.None) return;

        LastModifiedAtMs = item.ModifiedAt;
        if (item.Price is not null)
        {
            LastPrice = Price.Create(item.Price.CurrentPrice, LastPrice.PreviousPrice);
        }

        Record(new ItemChangesFoundEvent(
            Guid.NewGuid().ToString(),
            DateTime.UtcNow.ToString("O"),
            AlertId,
            ItemId,
            changeType,
            item
        ));
    }

    public override bool Equals(object? obj)
    {
        return obj is ProcessedItem other && AlertId == other.AlertId && ItemId == other.ItemId;
    }

    public static bool operator ==(ProcessedItem? left, ProcessedItem? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ProcessedItem? left, ProcessedItem? right)
    {
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AlertId, ItemId);
    }
}