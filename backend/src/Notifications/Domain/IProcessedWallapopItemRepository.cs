namespace Wallanoti.Src.Notifications.Domain;

public interface IProcessedWallapopItemRepository
{
    Task<ProcessedWallapopItem?> GetByAlertAndItemAsync(Guid alertId, string itemId, CancellationToken cancellationToken = default);
    Task UpsertAsync(ProcessedWallapopItem item, CancellationToken cancellationToken = default);
}