# Tasks: Avoid Double Notifications and Re-notify on Price Drops

## Phase 1: Foundation and Contracts

- [x] 1.1 Create failing contract test coverage in `backend/tests/Alerts/2-Application/SearchNewItems/ItemSearcherTest.cs` for first-time items, duplicate suppression, and price-drop re-notify scenarios (RED).
- [x] 1.2 Add `LastNotifiedItemSnapshot` read model in `backend/src/Notifications/Domain/Models/LastNotifiedItemSnapshot.cs` and wire namespace/usings where needed (GREEN).
- [x] 1.3 Extend `backend/src/Notifications/Domain/INotificationRepository.cs` with `GetLatestByUserAndUrls(...)` returning URL-keyed latest snapshots, then update mocks/stubs in impacted tests to compile (GREEN).
- [x] 1.4 Refactor duplicated test arrangement in `backend/tests/Alerts/2-Application/SearchNewItems/ItemSearcherTest.cs` into focused builders/helpers without changing assertions (REFACTOR).

## Phase 2: Persistence-backed Eligibility Implementation

- [x] 2.1 Implement `GetLatestByUserAndUrls(...)` in `backend/src/Notifications/Infrastructure/Percistence/NotificationRepository.cs` using one grouped query per `(UserId, Url)` that returns the latest notification snapshot.
- [x] 2.2 Add/adjust index configuration in `backend/src/Shared/Infrastructure/Percistence/EntityFramework/Configurations/NotificationConfiguration.cs` for `(UserId, Url, CreatedAt)` lookup performance.
- [x] 2.3 Inject `INotificationRepository` into `backend/src/Alerts/Application/SearchNewItems/ItemSearcher.cs`, derive candidate URLs from `WebSlug`, and apply eligibility rule: first-time OR strict price drop vs last notified price.
- [x] 2.4 Preserve existing boundaries in `backend/src/Alerts/Application/SearchNewItems/ItemSearcher.cs`: emit `NewItemsFoundEvent` with eligible items only and keep formatting/sending out of `ItemSearcher`.
- [x] 2.5 Handle null-price and non-drop cases in `backend/src/Alerts/Application/SearchNewItems/ItemSearcher.cs` as non-eligible for re-notify, keeping behavior explicit and test-covered.

## Phase 3: Integration and Subscriber Stability

- [x] 3.1 Add/adjust repository behavior tests in `backend/tests/Notifications/3-Infrastructure/Notifications/NotificationRepositoryTest.cs` to prove one latest snapshot per URL/user and no cross-user leakage (RED->GREEN).
- [x] 3.2 Verify `backend/tests/Notifications/2-Application/SaveOnNewItemsFound/SaveNotificationOnNewItemsFoundTest.cs` still passes with filtered event payloads; add focused assertion only if a gap exists.
- [x] 3.3 Ensure `backend/tests/Alerts/2-Application/SearchNewItems/ItemSearcherTest.cs` keeps regression coverage for Wallapop failure owner notification behavior while new eligibility logic is active.

## Phase 4: Verification and Refactor

- [x] 4.1 Run targeted backend tests for changed areas (`ItemSearcherTest`, `SaveNotificationOnNewItemsFoundTest`, and `NotificationRepositoryTest`) and make them pass end-to-end.
- [x] 4.2 Run full backend test suite in `backend/tests` and fix any fallout caused by the new repository contract.
- [x] 4.3 Refactor `backend/src/Alerts/Application/SearchNewItems/ItemSearcher.cs` and `backend/src/Notifications/Infrastructure/Percistence/NotificationRepository.cs` for readability (method extraction/naming) with no behavior changes.
- [x] 4.4 Confirm implementation satisfies all scenarios in `openspec/changes/avoid-double-notifications/specs/item-notification-eligibility/spec.md` and record any non-critical follow-ups for optional URL normalization/index rollout.
