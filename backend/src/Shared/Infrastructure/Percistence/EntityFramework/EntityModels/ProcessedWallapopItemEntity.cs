namespace Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.EntityModels;

public sealed class ProcessedWallapopItemEntity
{
    public required Guid Id { get; set; }
    public required Guid AlertId { get; set; }
    public required string WallapopItemId { get; set; }
    public required DateTime ProcessedAtUtc { get; set; }
    public decimal StoredPrice { get; set; }
    public long LastWallapopModifiedUtc { get; set; }
}