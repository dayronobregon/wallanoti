using Microsoft.EntityFrameworkCore;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.EntityModels;

namespace Wallanoti.Src.Notifications.Infrastructure.Persistence;

public class ProcessedWallapopItemRepository : IProcessedWallapopItemRepository
{
    private readonly WallanotiDbContext _context;

    public ProcessedWallapopItemRepository(WallanotiDbContext context)
    {
        _context = context;
    }

    public async Task<ProcessedWallapopItem?> GetByAlertAndItemAsync(Guid alertId, string itemId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.ProcessedWallapopItems
            .FirstOrDefaultAsync(e => e.AlertId == alertId && e.WallapopItemId == itemId, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    public async Task UpsertAsync(ProcessedWallapopItem item, CancellationToken cancellationToken = default)
    {
        var existingEntity = await _context.ProcessedWallapopItems
            .FirstOrDefaultAsync(e => e.AlertId == item.AlertId && e.WallapopItemId == item.WallapopItemId, cancellationToken);

        ProcessedWallapopItemEntity entity;

        if (existingEntity is null)
        {
            entity = MapToNewEntity(item);
            _context.ProcessedWallapopItems.Add(entity);
        }
        else
        {
            MapToExistingEntity(item, existingEntity);
            entity = existingEntity;
            _context.ProcessedWallapopItems.Update(entity);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static ProcessedWallapopItem MapToDomain(ProcessedWallapopItemEntity entity) =>
        new(entity.Id, entity.AlertId, entity.WallapopItemId, entity.ProcessedAtUtc, entity.StoredPrice, entity.LastWallapopModifiedUtc);

    private static ProcessedWallapopItemEntity MapToNewEntity(ProcessedWallapopItem item) =>
        new()
        {
            Id = item.Id,
            AlertId = item.AlertId,
            WallapopItemId = item.WallapopItemId,
            ProcessedAtUtc = item.ProcessedAtUtc,
            StoredPrice = item.StoredPrice,
            LastWallapopModifiedUtc = item.LastWallapopModifiedUtc
        };

    private static void MapToExistingEntity(ProcessedWallapopItem item, ProcessedWallapopItemEntity entity)
    {
        entity.Id = item.Id;
        entity.AlertId = item.AlertId;
        entity.WallapopItemId = item.WallapopItemId;
        entity.ProcessedAtUtc = item.ProcessedAtUtc;
        entity.StoredPrice = item.StoredPrice;
        entity.LastWallapopModifiedUtc = item.LastWallapopModifiedUtc;
    }
}