## Verification Report

**Change**: avoid-double-notifications
**Version**: N/A

---

### Completeness

| Metric | Value |
|--------|-------|
| Tasks total | 16 |
| Tasks complete | 16 |
| Tasks incomplete | 0 |

All tasks in `openspec/changes/archive/2026-03-20-avoid-double-notifications/tasks.md` are marked complete.

---

### Build & Tests Execution

**Build**: ✅ Passed

```bash
dotnet build wallanoti.sln
Build succeeded.
0 Warning(s)
0 Error(s)
```

**Tests**: ✅ 102 passed / ❌ 0 failed / ⚠️ 0 skipped

```bash
# targeted verification tests
dotnet test tests/tests.csproj --filter "FullyQualifiedName~ItemSearcherTest|FullyQualifiedName~SaveNotificationOnNewItemsFoundTest|FullyQualifiedName~NotificationRepositoryTest"
Passed!  - Failed: 0, Passed: 9, Skipped: 0, Total: 9

# full regression
dotnet test tests/tests.csproj
Passed!  - Failed: 0, Passed: 93, Skipped: 0, Total: 93
```

**Coverage**: ➖ Not configured (`openspec/config.yaml` does not define `rules.verify.coverage_threshold`)

---

### Spec Compliance Matrix

| Requirement | Scenario | Test | Result |
|-------------|----------|------|--------|
| Detect Previously Processed Items by Item Identifier | First-time item is treated as not previously processed | `backend/tests/Alerts/2-Application/SearchNewItems/ItemSearcherTest.cs` > `Execute_EmitsOnlyFirstTimeAndPriceDropItems` | ✅ COMPLIANT |
| Detect Previously Processed Items by Item Identifier | Existing item is treated as previously processed | `backend/tests/Alerts/2-Application/SearchNewItems/ItemSearcherTest.cs` > `Execute_WhenItemsAreOnlyCached_DoesNotQueryHistoryAndDoesNotPublish` | ✅ COMPLIANT |
| Re-notify Only on Current Price Drop | Previously processed item has a lower current price | `backend/tests/Alerts/2-Application/SearchNewItems/ItemSearcherTest.cs` > `Execute_EmitsOnlyFirstTimeAndPriceDropItems` | ✅ COMPLIANT |
| Re-notify Only on Current Price Drop | Previously processed item has equal current price | `backend/tests/Alerts/2-Application/SearchNewItems/ItemSearcherTest.cs` > `Execute_EmitsOnlyFirstTimeAndPriceDropItems` | ✅ COMPLIANT |
| Re-notify Only on Current Price Drop | Previously processed item has higher current price | `backend/tests/Alerts/2-Application/SearchNewItems/ItemSearcherTest.cs` > `Execute_EmitsOnlyFirstTimeAndPriceDropItems` | ✅ COMPLIANT |
| Persist Latest Price After Eligible Price-Drop Re-notification | Cache/history is updated on price-drop eligibility | `backend/tests/Notifications/2-Application/SaveOnNewItemsFound/SaveNotificationOnNewItemsFoundTest.cs` > `Handle_WithPriceDropItem_ShouldPersistNotificationWithPriceDropTitleAndUpdatedPrice` | ✅ COMPLIANT |
| Do Nothing for Same Item Without Price Drop | Same item without drop is ignored | `backend/tests/Alerts/2-Application/SearchNewItems/ItemSearcherTest.cs` > `Execute_EmitsOnlyFirstTimeAndPriceDropItems` | ✅ COMPLIANT |
| Keep ItemSearcher Focused on Eligibility | ItemSearcher emits eligibility-only payload | `backend/tests/Alerts/2-Application/SearchNewItems/ItemSearcherTest.cs` > `Execute_EmitsOnlyFirstTimeAndPriceDropItems` | ✅ COMPLIANT |
| Keep Channel Delivery in Event Subscribers | Subscribers handle formatting and sending responsibilities | `backend/tests/Notifications/2-Application/SaveOnNewItemsFound/SaveNotificationOnNewItemsFoundTest.cs` > `Handle_WithItems_ShouldPersistNotificationsAndPublishEvents` | ✅ COMPLIANT |

**Compliance summary**: 9/9 scenarios compliant

---

### Correctness (Static — Structural Evidence)

| Requirement | Status | Notes |
|------------|--------|-------|
| Detect Previously Processed Items by Item Identifier | ✅ Implemented | `ItemSearcher` pre-filters by cached `Item.Id` and then applies history checks on remaining candidates. |
| Re-notify Only on Current Price Drop | ✅ Implemented | `ItemSearcher.IsEligible(...)` uses strict comparison (`item.Price.CurrentPrice < snapshot.LastNotifiedCurrentPrice`). |
| Persist Latest Price After Eligible Price-Drop Re-notification | ✅ Implemented | `SaveNotificationOnNewItemsFound` persists the lowered `Price` via `AddRangeAsync`, now with focused assertion on updated price value and title. |
| Do Nothing for Same Item Without Price Drop | ✅ Implemented | Equal/higher and null-price previously notified items are excluded before event emission. |
| Keep ItemSearcher Focused on Eligibility | ✅ Implemented | `ItemSearcher` filters and emits event payload only; no channel formatting logic is present. |
| Keep Channel Delivery in Event Subscribers | ✅ Implemented | Subscriber keeps notification creation, formatting, and publication responsibilities. |

---

### Coherence (Design)

| Decision | Followed? | Notes |
|----------|-----------|-------|
| Persistence-backed notification state as source of truth | ✅ Yes | `INotificationRepository.GetLatestByUserAndUrls(...)` is used by both `ItemSearcher` and notification saver flow. |
| Preserve service boundaries and event responsibilities | ✅ Yes | Eligibility stays in `ItemSearcher`; subscriber persists/sends. |
| URL lookup key + cache by `Item.Id` | ✅ Yes | URL is derived from slug for history lookup; cache remains keyed by wallapop item id. |
| Strict price-drop comparison (`<`) | ✅ Yes | Implemented in eligibility and reflected in tests. |
| File changes alignment | ✅ Yes | Designed files and focused test additions are present, including `NotificationRepositoryTest` and price-drop title behavior. |

---

### Issues Found

**CRITICAL** (must fix before archive):
- None.

**WARNING** (should fix):
- None.

**SUGGESTION** (nice to have):
- Align archived spec wording from "same `Item.Id` price history" to explicit URL-based persisted snapshot terminology to match the implemented contract.

---

### Verdict
PASS

Verification rerun is fully green: build passes, targeted and full tests pass, and all spec scenarios are behaviorally compliant.