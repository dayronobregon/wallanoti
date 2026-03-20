# Spec - duplicated-notification-codex

## Requirement 1: Item identity and processing decision
The system MUST evaluate each eligible Wallapop item by `Item.Id` against cached processed items.

### Scenario 1.1 - Never processed item
Given an item whose `Item.Id` is not present in cache
When the item is processed
Then the system SHALL include it for notification with its original title
And the cache SHALL store its current price for future comparisons.

### Scenario 1.2 - Previously processed item without price decrease
Given an item whose `Item.Id` exists in cache with previous price `P`
When `Item.Price.CurrentPrice` is greater than or equal to `P`
Then the system SHALL NOT include it for notification.

### Scenario 1.3 - Previously processed item with price decrease
Given an item whose `Item.Id` exists in cache with previous price `P`
When `Item.Price.CurrentPrice < P`
Then the system SHALL include it for notification
And the item title SHALL append ` (Baja de Precio)` exactly once
And the cache SHALL update the item price to `Item.Price.CurrentPrice`.

## Requirement 2: Legacy cache compatibility
The system SHOULD support legacy cache entries stored as ID-only lists.

### Scenario 2.1 - Legacy cache payload
Given cached data in legacy `List<string>` format
When an item ID is present in that legacy cache
Then the system SHALL treat it as already processed
And once current price is available, the system SHALL persist the new cache payload in ID->price format.
