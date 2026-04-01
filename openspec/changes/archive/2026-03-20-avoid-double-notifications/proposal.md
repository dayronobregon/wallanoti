# Proposal: Avoid Double Notifications and Re-notify on Price Drops

## Intent
Remove duplicate notifications for already-notified Wallapop items and allow re-notification only when the item price drops versus the last notified price. Keep `ItemSearcher` responsible for selecting eligible items, while notification formatting and delivery remain in `NewItemsFoundEvent` subscribers.

## Scope

### In Scope
- Move duplicate/price-drop eligibility to deterministic persistence-backed checks (not cache-only behavior).
- Extend notification read capabilities to retrieve last notified snapshot by user and item URL/slug in batch.
- Update `ItemSearcher` logic and tests to emit only first-time items and price-drop items.
- Preserve subscriber responsibilities (`SaveNotificationOnNewItemsFound`, push/web senders) without moving formatting/sending into search.

### Out of Scope
- Redesigning Telegram/web notification templates or delivery channels.
- Full historical dedup migration for old malformed records.
- Broad Wallapop ingestion changes unrelated to eligibility decisions.

## Approach
Implement a persistence-backed eligibility step inside `ItemSearcher`:
1. Fetch Wallapop candidates as today.
2. Build candidate URLs/slugs and query a new repository method for latest notification state per candidate.
3. Mark item eligible if never notified, or if `currentPrice < lastNotifiedCurrentPrice`.
4. For price-drop re-notifications, set item price context so downstream formatting can show a drop message.
5. Emit `NewItemsFoundEvent` only with eligible items; subscribers continue persisting/sending.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `backend/src/Alerts/Application/SearchNewItems/ItemSearcher.cs` | Modified | Replace cache-only dedup with persistence-backed eligibility and price-drop rule. |
| `backend/src/Notifications/Domain/INotificationRepository.cs` | Modified | Add query contract for latest notified snapshots by user + item URLs. |
| `backend/src/Notifications/Infrastructure/Percistence/NotificationRepository.cs` | Modified | Implement efficient batch query and projection for eligibility checks. |
| `backend/tests/Alerts/2-Application/SearchNewItems/ItemSearcherTest.cs` | Modified | Add red/green scenarios for duplicate suppression and price-drop re-notification. |
| `backend/tests/Notifications/2-Application/SaveOnNewItemsFound/SaveNotificationOnNewItemsFoundTest.cs` | Modified (if needed) | Ensure subscriber behavior remains stable with filtered event items. |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Batched lookup becomes slow on large candidate sets | Medium | Use single query with grouping by URL and proper DB index strategy. |
| Price comparison edge cases (null prices, currency assumptions) | Medium | Define explicit null-handling rule and cover with tests. |
| Behavioral drift if cache and persistence rules diverge | Medium | Make persistence decision authoritative and simplify cache role. |

## Rollback Plan
Revert `ItemSearcher` eligibility to current cache-based filtering and remove new notification lookup method usage. Keep schema/query additions behind reversible migration or non-breaking repository changes so rollback only requires code revert and redeploy.

## Dependencies
- Existing notifications table quality (URL and current price must be present for prior notifications).
- Test fixtures for Wallapop items and notification history in backend unit tests.

## Success Criteria
- [ ] Previously notified item with unchanged or higher price MUST NOT produce a new notification event.
- [ ] Previously notified item with lower current price MUST produce a new notification event.
- [ ] First-time item MUST produce a new notification event.
- [ ] Formatting/sending logic remains in `NewItemsFoundEvent` subscribers, not in `ItemSearcher`.
- [ ] Automated tests cover all three eligibility paths and pass.