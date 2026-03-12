# Delta Specification for Telegram Notifications (Deduplication)

## Purpose

Prevent duplicate notifications for the same Wallapop item per alert by tracking processed state and detecting changes. Introduce notification prefixes based on change type: (Nuevo) for new items, (Actualizacion) for non-price updates, (Bajada Precio) for price drops.

## ADDED Requirements

### Requirement: Per-Alert Item Deduplication

The system **MUST** send a Telegram notification for a Wallapop item associated with an alert **only if**:
- No prior notification exists for the (alert, item ID) pair, **OR**
- The item's `ModifiedAt` timestamp is newer than the last processed `ModifiedAt`, **OR**
- The item's price is lower than the previously stored price (price drop detection).

The system **MUST** update the stored `ModifiedAt` and price after processing a notification.

**Acceptance Criteria**:
- Unique notifications per (alert ID, Wallapop item ID).
- No notifications for unchanged items.

#### Scenario: New Item Notification
```
GIVEN an alert has no existing notification for a Wallapop item ID
WHEN the item is discovered in search results
THEN a Telegram notification is sent with prefix \"(Nuevo)\"
  AND the notification record is created with current `ModifiedAt` and price
```

#### Scenario: Unchanged Duplicate Item Skipped
```
GIVEN an alert has an existing notification for Wallapop item ID
  AND the item's `ModifiedAt` matches the stored `LastProcessedModifiedAt`
  AND the price matches the stored price
WHEN the same item is discovered again
THEN no Telegram notification is sent
```

#### Scenario: Item Update (No Price Change)
```
GIVEN an alert has an existing notification for item ID with stored `LastProcessedModifiedAt` and price P
WHEN the item is discovered with `ModifiedAt` > stored `LastProcessedModifiedAt` and same price P
THEN a Telegram notification is sent with prefix \"(Actualizacion)\"
  AND the stored `LastProcessedModifiedAt` and price are updated
```

#### Scenario: Price Drop
```
GIVEN an alert has an existing notification with stored price P
WHEN the item is discovered with price < P (regardless of `ModifiedAt`)
THEN a Telegram notification is sent with prefix \"(Bajada Precio)\"
  AND the stored price and `LastProcessedModifiedAt` are updated
```

#### Scenario: Same Item on Multiple Alerts
```
GIVEN two alerts A1 and A2 monitoring overlapping items
  AND item X changes
WHEN search runs for A1 and A2
THEN A1 receives notification for X independently of A2
  AND A2 receives notification for X independently of A1
```

### Requirement: Efficient State Lookup

The system **SHOULD** retrieve processed item state efficiently (e.g., recent notifications per alert, indexed queries) to avoid performance degradation during searches.

### Requirement: Data Persistence for Deduplication

The system **SHALL** persist per-notification state including:
- `AlertId` (unique identifier for the alert)
- `WallapopItemId` (item's ID from Wallapop)
- `LastProcessedModifiedAt` (Unix milliseconds timestamp)
- `StoredPrice` (decimal price at last process)

With a **unique constraint/index** on (`AlertId`, `WallapopItemId`) to prevent duplicates.

**Note**: API endpoints remain unchanged; deduplication handled internally in job flows.

## Job Flow

```
1. ItemSearcher runs periodic search for alert.
2. For each matching item:
   - Query notification state for (alertId, itemId).
   - If qualifies (new/changed), create notification with prefix.
   - Publish to Telegram.
3. Update state transactionally.
```

## Non-Functional Requirements

- **Performance**: Queries **MUST** complete in &lt; 100ms per item batch.
- **Reliability**: Deduplication **MUST** survive job restarts (DB-backed).
- **Scalability**: Support 1000s of alerts without degradation.

## Coverage
- Happy paths: Covered.
- Edge cases: Multiple alerts, price drops, updates.
- Error states: Implicit (e.g., invalid timestamps handled by logic).
