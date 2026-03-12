# Design: Fix Duplicate Telegram Notifications

## Technical Approach

Introduce a dedicated `ProcessedWallapopItem` domain entity to track the last processed state of Wallapop items per alert. Modify the existing `SaveNotificationOnNewItemsFound` handler (renamed to `ItemProcessorHandler`) to query for existing processed items, compare changes, conditionally create `Notification` with prefixed titles via `NotificationTitleResolver`, upsert processed state, following Clean Architecture layers: Domain (entity), Application (handler, resolver service), Infrastructure (repository, EF). Add DB migration for new table with unique composite index on `AlertId + WallapopItemId`.

## Architecture Decisions

### Decision: Introduce separate `ProcessedWallapopItem` entity

**Choice**: New aggregate/entity `ProcessedWallapopItem` (Id, AlertId, WallapopItemId, ProcessedAt, LastTitle, LastPrice, LastModifiedAt).
**Alternatives considered**: Enhance `Notification` entity with tracking fields (as initially proposed).
**Rationale**: Single Responsibility Principle—`Notification` remains immutable sent message; `ProcessedWallapopItem` handles mutable tracking state. Avoids bloating `Notification` aggregate, enables efficient queries (e.g., recent per alert).

### Decision: `ItemProcessorHandler` for upsert+compare logic

**Choice**: Rename/modify `SaveNotificationOnNewItemsFound` to `ItemProcessorHandler` consuming `NewItemsFoundEvent`, performing dedup/compare in batch.
**Alternatives considered**: New handler before/after existing; separate event for processed items.
**Rationale**: Minimizes topology changes (reuse existing queue `wallanoti.notifications.save_notification_on_new_items_found`); batch AddRange/Upsert for perf; AlertCounter handler remains unchanged for scraped count.

### Decision: `NotificationTitleResolver` service

**Choice**: Application service injecting into handler, resolves prefixed title based on change type (Nuevo/Actualizacion/Bajada Precio).
**Alternatives considered**: Logic inline in handler/domain.
**Rationale**: Separation of concerns; testable; aligns with CQRS (app layer orchestration).

### Decision: PostgreSQL table with indexes

**Choice**: New `ProcessedWallapopItems` table; UNIQUE(AlertId, WallapopItemId); INDEX(AlertId, ProcessedAt DESC) for recent queries.
**Alternatives considered**: Redis for recent TTL cache.
**Rationale**: Persistent, transactional (EF UnitOfWork); indexes ensure perf; fallback to Redis later if needed.

## Data Flow

```
Wallapop Scrape (ItemSearcher) → Alert.NewSearch(items) → NewItemsFoundEvent (RabbitMQ)
          ↓
ItemProcessorHandler (Notifications)
  ├─ repo.GetByAlertItem(AlertId, ItemId) ×N
  ├─ if changed: titleResolver.Resolve() → Notification.Create() → notifRepo.AddRange()
  │  └─ Publish NotificationCreatedEvent → TelegramSender.Notify()
  └─ processedRepo.UpsertRange(new ProcessedWallapopItem ×N)
          ↓ (parallel)
AlertCounter.IncrementOnNewItemsFound (scraped count)
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `backend/src/Notifications/Domain/ProcessedWallapopItem.cs` | Create | Domain entity extending AggregateRoot. |
| `backend/src/Notifications/Domain/IProcessedWallapopItemRepository.cs` | Create | Repository interface (GetByAlertAndItemAsync, UpsertAsync). |
| `backend/src/Notifications/Application/NotificationTitleResolver.cs` | Create | Service for prefixing titles based on change detection. |
| `backend/src/Notifications/Application/SaveOnNewItemsFound/ItemProcessorHandler.cs` | Rename/Modify | Add dedup logic, inject repo/resolver; conditional Notification creation. |
| `backend/src/Notifications/Infrastructure/Persistence/ProcessedWallapopItemRepository.cs` | Create | EF implementation. |
| `backend/src/Shared/Infrastructure/Persistence/EntityFramework/EntityModels/ProcessedWallapopItemEntity.cs` | Create | EF entity model. |
| `backend/src/Shared/Infrastructure/Persistence/EntityFramework/Configurations/ProcessedWallapopItemConfiguration.cs` | Create | Fluent config with indexes. |
| `backend/src/Shared/Infrastructure/Persistence/EntityFramework/WallanotiDbContext.cs` | Modify | DbSet&lt;ProcessedWallapopItemEntity&gt;. |
| EF Migrations/.../AddProcessedWallapopItem.cs | Create | Schema changes. |

## Interfaces / Contracts

```csharp
// Domain
public interface IProcessedWallapopItemRepository
{
    Task&lt;ProcessedWallapopItem?&gt; GetByAlertAndItemAsync(Guid alertId, string itemId);
    Task UpsertAsync(ProcessedWallapopItem item);
    // Optional: Task&lt;IReadOnlyList&lt;ProcessedWallapopItem&gt;&gt; GetRecentByAlertAsync(Guid alertId, int limit = 100);
}

// Application
public class NotificationTitleResolver
{
    public string Resolve(string baseTitle, ChangeType changeType) { /* (Nuevo) etc. */ }
}
public enum ChangeType { None, Nuevo, Actualizacion, BajadaPrecio }
```

## Testing Strategy

| Layer | What to Test | Approach |
|-------|--------------|----------|
| Unit | Domain: entity creation/equality; Resolver: prefix scenarios; Handler: conditional logic (mocks) | xUnit in `test/Notifications/1-Domain/`, `test/Notifications/2-Application/` |
| Integration | Repo: CRUD EF ops; Handler end-to-end (in-memory DB) | `test/Notifications/3-Infrastructure/` |
| E2E | Full flow: scrape → event → no dups on repeat, prefixes on changes | Integration tests with Testcontainers/PostgreSQL |

## Migration / Rollout

EF migration: `dotnet ef migrations add AddProcessedWallapopItemTable && dotnet ef database update`.

Sketch (Up()):
```csharp
builder.CreateTable("ProcessedWallapopItems",
  c => {
    c.Id = uuid primary;
    c.AlertId = uuid not null;
    c.WallapopItemId = text not null;
    c.ProcessedAt = timestamptz not null;
    c.LastTitle = text;
    c.LastPrice = numeric;
    c.LastModifiedAt = bigint not null;
    unique(AlertId, WallapopItemId);
  });
builder.CreateIndex("IX_ProcessedWallapopItems_AlertId_ProcessedAt", "ProcessedWallapopItems", "AlertId", "ProcessedAt DESC");
```
Online migration (no downtime). No backfill needed (start fresh tracking). Feature flag not required.

## Open Questions

- None