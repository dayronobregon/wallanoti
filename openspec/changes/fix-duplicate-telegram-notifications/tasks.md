# Tasks: Fix Duplicate Telegram Notifications

## Overview
Actionable checklist derived from spec/design. Phases ordered by dependency. TDD cycle for implementation tasks (RED: failing test, GREEN: minimal impl, REFACTOR: cleanup). Effort estimates in hours (junior dev). Batches for sdd-apply.

## Phase 1: Database Infrastructure (Effort: 2h | Deps: None | Batch 1)
- [ ] 1.1 Create `backend/src/Shared/Infrastructure/Persistence/EntityFramework/EntityModels/ProcessedWallapopItemEntity.cs` (EF model with AlertId, WallapopItemId unique, LastModifiedAt, StoredPrice etc.)
- [ ] 1.2 Create `backend/src/Shared/Infrastructure/Persistence/EntityFramework/Configurations/ProcessedWallapopItemConfiguration.cs` (Fluent config, unique index, AlertId+ProcessedAt index)
- [ ] 1.3 Edit `backend/src/Shared/Infrastructure/Persistence/EntityFramework/WallanotiDbContext.cs`: add DbSet&lt;ProcessedWallapopItemEntity&gt;
- [ ] 1.4 Generate/apply migration: workdir=backend/api `dotnet ef migrations add AddProcessedWallapopItemsTable && dotnet ef database update`

## Phase 2: Domain Layer (Effort: 1.5h | Deps: Phase 1 | Batch 2)
- [ ] 2.1 TDD: Create unit tests `backend/tests/Notifications/1-Domain/ProcessedWallapopItemTest.cs` (entity validation, equality for dedup)
- [ ] 2.2 GREEN/REFACTOR: Create `backend/src/Notifications/Domain/ProcessedWallapopItem.cs` (AggregateRoot, props: AlertId, WallapopItemId, ProcessedAt, LastPrice, LastModifiedAt; methods: RequiresNotification?(Item), UpdateFrom(Item))
- [ ] 2.3 Create `backend/src/Notifications/Domain/IProcessedWallapopItemRepository.cs` (GetByAlertAndItemAsync(Guid alertId, string itemId), UpsertAsync(ProcessedWallapopItem))

## Phase 3: Application Services (Effort: 1h | Deps: Phase 2 | Batch 2)
- [ ] 3.1 TDD: Create `backend/tests/Notifications/2-Application/NotificationTitleResolverTest.cs` (prefix scenarios: Nuevo, Actualizacion, BajadaPrecio)
- [ ] 3.2 GREEN/REFACTOR: Create `backend/src/Notifications/Application/NotificationTitleResolver.cs` (Resolve(string title, ProcessedWallapopItem? existing, Item current) → prefixed title, ChangeType enum)

## Phase 4: Core Handler Logic (Effort: 3h | Deps: Phases 2-3 | Batch 3)
- [ ] 4.1 TDD: Extend `backend/tests/Notifications/2-Application/SaveOnNewItemsFound/SaveNotificationOnNewItemsFoundTest.cs` → ItemProcessorHandlerTest.cs (scenarios: new item, unchanged skip, update notify+update state, price drop notify+update)
- [ ] 4.2 GREEN/REFACTOR: Rename file/class `backend/src/Notifications/Application/SaveOnNewItemsFound/ItemProcessorHandler.cs` (handle NewItemsFoundEvent: for each item, get processed by alertId+itemId, if requiresNotification: resolve title, create Notification, collect; AddRange notifications, Publish events; UpsertRange processed items)
- [ ] 4.3 Update DI `backend/api/Extension/DependencyInjection/Infrastructure.cs`: register ItemProcessorHandler, update MassTransit consumer for NewItemsFoundEvent

## Phase 5: Infrastructure Repository (Effort: 1.5h | Deps: Phase 1,2 | Batch 4)
- [ ] 5.1 TDD: Create integration tests `backend/tests/Notifications/3-Infrastructure/ProcessedWallapopItemRepositoryTest.cs` (GetByAlertAndItem, Upsert, unique constraint)
- [ ] 5.2 GREEN/REFACTOR: Create `backend/src/Notifications/Infrastructure/Persistence/ProcessedWallapopItemRepository.cs` (EF impl of IProcessedWallapopItemRepository)

## Phase 6: Integrate Search Flow & Redis Cleanup (Effort: 1h | Deps: Phase 4 | Batch 5)
- [ ] 6.1 TDD: Extend `backend/tests/Alerts/2-Application/SearchNewItems/ItemSearcherTest.cs` (sends all recent without cache filter)
- [ ] 6.2 GREEN/REFACTOR: Edit `backend/src/Alerts/Application/SearchNewItems/ItemSearcher.cs`: remove IDistributedCache dep/inject, remove GetCachedItems/AlreadyFound/cache.Set (send all items with createdAt >= alert.CreatedAt to event)
- [ ] 6.3 Verify AlertCounter handler unchanged (scraped count)

## Phase 7: Full Verification (Effort: 2h | Deps: All | Batch 6)
- [ ] 7.1 Run all tests: `dotnet test`
- [ ] 7.2 Integration/E2E: Test full flow with PostgreSQL (Testcontainers): repeat searches, verify no dups, prefixes, state updates (scenarios from spec)
- [ ] 7.3 Lint/typecheck: `dotnet build`
- [ ] 7.4 Docs: Update README/AGENTS.md if needed

**Total Effort Est: 12h** | **Batches: 6 parallelizable phases**