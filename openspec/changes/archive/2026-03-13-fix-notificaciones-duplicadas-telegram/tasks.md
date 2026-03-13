# Tasks: Fix Duplicate Telegram Notifications

## Phase 1: Domain (Effort Total: S | Dependencies: None)

- [ ] 1.1 Write unit tests for ProcessedItem entity (validation, equality on composite key AlertId+ItemId). Effort: S, deps: none
- [x] 1.1 Write unit tests for ProcessedItem entity (validation, equality on composite key AlertId+ItemId). Effort: S, deps: none
- [x] 1.3 Write unit tests for Notification.FormattedString() with changeType prefixes (new/update/price_drop scenarios from specs/notifications). Effort: S, deps: none
- [x] 1.4 Update `backend/src/Notifications/Domain/Notification.cs` to include ChangeType prop and prefix logic (Nuevo/Actualización/Bajada de Precio). Effort: S, deps: 1.3
- [x] 1.5 Create `backend/src/Alerts/Domain/Repositories/IProcessedItemRepository.cs` (GetByAlertAndItemAsync, UpsertAsync). Effort: S, deps: 1.2

## Phase 2: Application (Effort Total: M | Dependencies: Phase 1)

- [x] 2.1 Write unit/integration tests for ItemSearcherCommandHandler change detection (mock repo; cover all specs/alerts scenarios: new/update/price_drop/no-change/priority). Failing tests for Redis removal. Effort: M, deps: 1.1-1.5
- [x] 2.2 Modify `backend/src/Alerts/Application/Commands/SearchNewItems/ItemSearcherCommandHandler.cs`: inject repo; for each item: get existing or new ProcessedItem; detect changeType (new if none; price_drop if cheaper; update if ModifiedAt newer); if changed publish NewItemsFoundEvent with changeType; always upsert repo; remove all Redis cache logic. Effort: M, deps: 2.1 (TDD), Phase 1
- [ ] 2.3 Update NewItemsFoundEvent to include ChangeType (if not already; propagate to notification handlers). Effort: S, deps: 2.2

## Phase 3: Infrastructure (Effort Total: M | Dependencies: Phase 1)

- [x] 3.1 Write unit tests for ProcessedItemRepository (EF queries, upsert). Effort: S, deps: 1.5
- [x] 3.2 Create `backend/src/Alerts/Infrastructure/Persistence/Configurations/ProcessedItemConfiguration.cs` (composite key, indexes: (AlertId,ItemId) unique; LastModifiedAt). Effort: S, deps: 3.1
- [x] 3.3 Create `backend/src/Alerts/Infrastructure/Persistence/Repositories/ProcessedItemRepository.cs` (EF impl of IProcessedItemRepository). Effort: M, deps: 3.1, 3.2 (TDD)

## Phase 4: Tests (Effort Total: L | Dependencies: Phases 1-3)

- [ ] 4.1 Write integration tests for full ItemSearcher flow (DB-backed repo; cover all specs/alerts scenarios incl. multi-alerts, edge cases). Effort: L, deps: Phase 3
- [ ] 4.2 Write integration tests for event → notification → Telegram (mock; prefixes correct). Effort: M, deps: Phase 2
- [ ] 4.3 Run `dotnet test` in backend; fix any failures. Ensure 100% coverage on new logic. Effort: S, deps: 4.1-4.2

## Phase 5: Migration (Effort Total: S | Dependencies: Phase 3)

- [ ] 5.1 Generate EF migration: `dotnet ef migrations add AddProcessedItems --project backend/api`. Review SQL (table, indexes). Effort: S, deps: 3.2
- [ ] 5.2 Update DB: `dotnet ef database update`. Verify table schema. Effort: S, deps: 5.1
- [ ] 5.3 (Optional) Drop Redis cache keys for alerts if existing; assess if Redis service droppable (check usages). Effort: S, deps: Phase 2