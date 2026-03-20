## Exploration: avoid-double-notifications

### Current State
`ItemSearcher` fetches Wallapop items and filters with two rules: item created after alert creation and item ID not in distributed cache. When matches exist, it raises `NewItemsFoundEvent`; subscribers create and send notifications. This means duplicate suppression depends on cache state, and historical notification data is not consulted. Price-drop re-notification is not implemented as a business rule.

### Affected Areas
- `backend/src/Alerts/Application/SearchNewItems/ItemSearcher.cs` — current eligibility logic and cache-based duplicate check live here.
- `backend/src/Alerts/Domain/Models/Item.cs` — item payload used by event subscribers (contains `Price`, `Id`, `WebSlug`).
- `backend/src/Notifications/Application/SaveOnNewItemsFound/SaveNotificationOnNewItemsFound.cs` — consumes `NewItemsFoundEvent` and persists notifications.
- `backend/src/Notifications/Domain/INotificationRepository.cs` — candidate place to expose historical lookup for already-notified items.
- `backend/src/Notifications/Infrastructure/Percistence/NotificationRepository.cs` — can implement batch lookup by `(userId, itemUrl)` and last notified price.
- `backend/tests/Alerts/2-Application/SearchNewItems/ItemSearcherTest.cs` — needs scenarios for duplicate suppression and price-drop re-eligibility.

### Approaches
1. **Cache-only dedup with extra keys** — extend cache entries with prices and compare on each run.
   - Pros: low schema impact; fast reads.
   - Cons: volatile state (eviction/restart issues), weak source of truth, hard to audit.
   - Effort: Medium.

2. **Persistent dedup from notifications history (recommended)** — ItemSearcher asks persistence for last notified snapshot per item URL/user and decides eligibility:
   - first time seen => notify
   - seen before and current price lower than last notified current price => notify again (price drop)
   - otherwise => skip
   - Pros: deterministic across restarts, aligns with business rule, easy to reason/test.
   - Cons: requires new repository query and tests; possible query optimization needed.
   - Effort: Medium.

### Recommendation
Use persistent dedup based on notification history, keeping ItemSearcher focused on eligibility only. Keep subscriber responsibilities unchanged (persist/format/send). Reuse existing `Notification` persistence as source of truth, adding a batched lookup for candidate URLs by user.

### Risks
- N+1 database access if lookup is per item instead of batched query.
- Ambiguity between Wallapop `Price.PreviousPrice` and last-notified price if not normalized in eligibility step.
- Existing cache behavior may conflict with new rule if both mechanisms stay active without clear precedence.

### Ready for Proposal
Yes — enough evidence exists to define scope and constraints.