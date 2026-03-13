## Exploration: fix-notificaciones-duplicadas-telegram

### Current State
- Scheduled `AlertSearcher` triggers `ItemSearcher.Execute()` periodically.
- Fetches 40 newest Wallapop items per active alert URL.
- Caches *all* fetched item IDs per alert in Redis (key=alert.Id).
- Notifies only items with `CreatedAt >= alert.CreatedAt` && not in cache.
- Publishes `NewItemsFoundEvent` → consumers create `Notification` → `NotificationCreatedEvent` → Telegram send.
- `Notification.FormattedString()` detects price drops if `PreviousPrice > CurrentPrice` (from Wallapop).
- No check for updates: ignores `ModifiedAt`; only new creations.

### Affected Areas
- `backend/src/Alerts/Application/SearchNewItems/ItemSearcher.cs` — core search/cache logic.
- `backend/src/Alerts/Infrastructure/Percistence/Wallapop/WallapopResponse.cs` & `Item.cs` — `ModifiedAt`, `PreviousPrice`.
- `backend/api/ScheduledTasks/AlertSearcher.cs` — entry point.
- `backend/src/Notifications/Application/SaveOnNewItemsFound/SaveNotificationOnNewItemsFound.cs` — event handler.
- `backend/src/Notifications/Domain/Notification.cs` — formatting/titles.
- Redis cache keys per alert.

### Approaches
1. **Per-alert item tracking in DB** — Store `ProcessedItem` (itemId, alertId, lastModifiedAt, lastPrice) in Postgres.
   - Pros: Persistent, handles updates/price drops; query diffs.
   - Cons: DB growth; perf with indexes.
   - Effort: Medium (new entity/repo/handler).

2. **Enhance Redis cache** — Store JSON `{itemId: {modifiedAt, price}}` per alert; compare on fetch.
   - Pros: Fast, no DB bloat.
   - Cons: Volatile (TTL needed); harder queries.
   - Effort: Low.

3. **Global processed items** — Single cache/DB for all users (by itemId).
   - Pros: Prevents cross-alert dups.
   - Cons: May miss user-specific updates.
   - Effort: Low.

### Recommendation
DB per-alert `ProcessedItem` (extend current DDD): Track last seen `ModifiedAt`/price; notify on change with title prefix (Nuevo/Actualizacion/Bajada Precio). Use `ModifiedAt` for updates. Global dedup optional.

### Risks
- DB perf on high-volume alerts (mitigate indexes/sharding).
- Cache/DB sync races (use transactions).
- Wallapop API changes (ModifiedAt semantics).
- Backward compat: migrate existing caches.

### Ready for Proposal
Yes — proceed to spec design distinguishing new/update/price-drop notifications.