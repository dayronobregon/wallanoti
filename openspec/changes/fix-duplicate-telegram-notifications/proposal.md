# Proposal: Fix Duplicate Telegram Notifications

## Intent

Prevent duplicate Telegram notifications for the same Wallapop item by persisting processed item state per alert in the database. Introduce notification prefixes: (Nuevo) for new items, (Actualizacion) for updates, (Bajada Precio) for price drops, determined by comparing item.ModifiedAt and price against last processed state.

## Scope

### In Scope
- Add `AlertId` (Guid) and `WallapopItemId` (string) to `Notification` domain entity and EF entity.
- Add `LastProcessedModifiedAt` (long, unix ms) and `StoredPrice` (decimal?) to `Notification`.
- Implement deduplication and change detection logic in `ItemSearcher` or `SaveNotificationOnNewItemsFound` handler using DB queries.
- Update `Notification.FormattedString()` or title to include prefixes based on change type.
- Create EF migration for schema changes and unique index on (AlertId, WallapopItemId).
- Make handlers idempotent (ignore if no changes).

### Out of Scope
- Changes to web/SignalR notifications.
- Frontend updates.
- Support for item deletion or other advanced states.

## Approach

Follow Clean Architecture + DDD + Hexagonal:
- Enhance `Notification` aggregate to include AlertId, WallapopItemId, LastProcessedModifiedAt, StoredPrice.
- In `Alert.NewSearch(items)`: for each item, raise `ItemProcessedEvent` or directly create/update Notification via repo in handler.
- `SaveNotificationOnNewItemsFound` handler: for each item in event:
  - Query `INotificationRepository.GetByAlertAndItemId(alertId, itemId)` (new query).
  - If none or item.ModifiedAt > existing.LastProcessedModifiedAt:
    - Detect change: new, price drop (if new price < stored), update.
    - Create new `Notification` with prefixed title, updated fields.
- Replace Redis ID cache with DB query for last processed items per alert (e.g., TOP 100 recent).
- Optional: Redis TTL set for alert recent processed item IDs to speed up.
- Ensure transactional: use unique constraint to prevent duplicates.
- Publish `NotificationCreatedEvent` only on actual new/updated notifs.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `backend/src/Alerts/Domain/Models/Alert.cs` | Modified | Enhance `NewSearch` to pass AlertId in event. |
| `backend/src/Alerts/Domain/NewItemsFoundEvent.cs` | Modified | Add AlertId if not, Items. |
| `backend/src/Alerts/Application/SearchNewItems/ItemSearcher.cs` | Modified | Remove Redis cache, fetch all recent items, filter changed/new vs DB. |
| `backend/src/Notifications/Domain/Notification.cs` | New fields | Add AlertId, WallapopItemId, LastProcessedModifiedAt, StoredPrice; update Create/FormattedString for prefixes. |
| `backend/src/Notifications/Application/SaveOnNewItemsFound/SaveNotificationOnNewItemsFound.cs` | Modified | Add dedup/change logic with repo query. |
| `backend/src/Shared/Infrastructure/Persistence/EntityFramework/EntityModels/NotificationEntity.cs` | Modified | Add fields. |
| `backend/src/Notifications/Infrastructure/Persistence/NotificationRepository.cs` | New methods | GetByAlertAndItemId, perhaps GetRecentByAlertId. |
| DB schema | Migration | Add columns, unique index (AlertId, WallapopItemId). |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Performance degradation from DB queries per item | Medium | Limit query to recent notifications (e.g., last 7 days), add indexes; fallback Redis TTL cache. |
| Migration downtime on large DB | Low | Run online migration, backfill optional. |
| Breaking existing notifications (no AlertId) | Low | Nullable fields, handle in queries/formatting. |
| Complex change detection logic | Medium | TDD with unit tests for scenarios; start with new items only. |

## Rollback Plan

1. Revert EF migration: `dotnet ef database update 0`.
2. Remove new fields/code from Notification/ItemSearcher/SaveNotificationOnNewItemsFound.
3. Revert Redis cache changes.
4. No data loss as new fields nullable.

## Dependencies

- None.

## Success Criteria

- [ ] No duplicate notifications in integration tests simulating multiple searches.
- [ ] Prefixes appear correctly: (Nuevo) for first, (Actualizacion)/(Bajada Precio) on changes.
- [ ] Tests pass: dotnet test.
- [ ] Lint/typecheck clean.
