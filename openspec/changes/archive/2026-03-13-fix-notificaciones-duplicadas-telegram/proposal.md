# Proposal: Fix Duplicate Telegram Notifications

## Intent

Users receive duplicate notifications for the same Wallapop item across multiple alert searches or repeated fetches without proper change detection. This change introduces per-alert ProcessedItem tracking to detect and notify only on new items, updates (ModifiedAt change), or price drops, using prefixes: **Nuevo:**, **Actualización:**, **Bajada de Precio:**.

## Scope

### In Scope
- New DDD entity `ProcessedItem` (composite key: AlertId + ItemId, fields: LastModifiedAt, LastPrice).
- Update `ItemSearcher` logic: query repo for existing processed items, detect changes, notify if new/changed, update records.
- Enhance `Notification` formatting with change-type prefixes.
- EF Core migration for `ProcessedItems` table with indexes.
- Comprehensive tests (unit/integration).

### Out of Scope
- Cross-user/alert global deduplication.
- Item deletion handling.
- Frontend changes.

## Approach

Follow Clean Architecture + DDD + Hexagonal:

1. **Domain Layer** (`src/Alerts/Domain`): `ProcessedItem` entity/aggregate root.
2. **Application Layer** (`src/Alerts/Application`): Update `ItemSearcherCommandHandler` to use `IProcessedItemRepository` for get/update.
3. **Infrastructure** (`src/Alerts/Infrastructure/Persistence`): EF impl of repo.
4. Leverage existing `NewItemsFoundEvent` → notification handlers; add change-type to event payload.
5. Detect changes:
   - New: No record.
   - Update: `item.ModifiedAt > processed.LastModifiedAt`.
   - Price Drop: `item.Price < processed.LastPrice`.
6. Prefixes in `Notification.FormattedString()` based on change type.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `backend/src/Alerts/Domain/Entities/ProcessedItem.cs` | New | DDD entity. |
| `backend/src/Alerts/Domain/Repositories/IProcessedItemRepository.cs` | New | Interface. |
| `backend/src/Alerts/Application/Commands/SearchNewItems/ItemSearcherCommandHandler.cs` | Modified | Change detection + repo calls. |
| `backend/src/Alerts/Infrastructure/Persistence/Repositories/ProcessedItemRepository.cs` | New | EF impl. |
| `backend/src/Alerts/Infrastructure/Persistence/Configurations/ProcessedItemConfiguration.cs` | New | EF config. |
| `backend/src/Notifications/Domain/Notification.cs` | Modified | Prefix logic. |
| Migrations | New | Table + indexes (AlertId+ItemId unique). |
| `test/Alerts/...` | New/Mod | Tests. |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| DB performance on high-volume | Medium | Composite indexes; monitor query perf. |
| Concurrent update races | Low | Aggregate concurrency token; tx isolation. |
| Data growth | Low | Scheduled cleanup job for stale items. |
| Wallapop API `ModifiedAt` changes | Low | Integration tests; monitor. |
| Migration conflicts | Low | Backup DB before migrate. |

## Rollback Plan

1. Revert EF migration: `dotnet ef database update PreviousMigration`.
2. Remove new files/code changes.
3. Clear any inserted `ProcessedItem` data (if any).

## Dependencies

None.

## Success Criteria

- [ ] Integration tests: No duplicates on repeated searches; correct prefixes.
- [ ] Unit tests: Change detection logic 100% covered.
- [ ] `dotnet test` passes fully.
- [ ] No perf regression in `ItemSearcher` (benchmark if needed).