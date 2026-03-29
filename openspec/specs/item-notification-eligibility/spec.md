# Item Notification Eligibility Specification

## Purpose

Define deterministic rules for selecting items that are eligible for new notifications while preventing duplicate notifications and allowing re-notification on valid price drops.

## Requirements

### Requirement: Detect Previously Processed Items by Item Identifier

The system MUST determine prior processing state by `Item.Id` for each candidate item.

#### Scenario: First-time item is treated as not previously processed

- GIVEN a candidate item with an `Item.Id` that does not exist in processing cache/history
- WHEN eligibility is evaluated
- THEN the item is treated as first-time
- AND the item is eligible for inclusion in the new-items result

#### Scenario: Existing item is treated as previously processed

- GIVEN a candidate item with an `Item.Id` that already exists in processing cache/history
- WHEN eligibility is evaluated
- THEN the item is treated as previously processed

### Requirement: Re-notify Only on Current Price Drop

For previously processed items, the system MUST mark an item as eligible only when `Item.Price.CurrentPrice` is strictly lower than the previously cached price for that same `Item.Id`.

#### Scenario: Previously processed item has a lower current price

- GIVEN a candidate item with an `Item.Id` that was processed before
- AND the previously cached price is greater than the current item price
- WHEN eligibility is evaluated
- THEN the item is eligible for inclusion in the new-items result

#### Scenario: Previously processed item has equal current price

- GIVEN a candidate item with an `Item.Id` that was processed before
- AND the previously cached price is equal to the current item price
- WHEN eligibility is evaluated
- THEN the item is not eligible for inclusion in the new-items result

#### Scenario: Previously processed item has higher current price

- GIVEN a candidate item with an `Item.Id` that was processed before
- AND the previously cached price is lower than the current item price
- WHEN eligibility is evaluated
- THEN the item is not eligible for inclusion in the new-items result

### Requirement: Persist Latest Price After Eligible Price-Drop Re-notification

When a previously processed item becomes eligible due to a price drop, the system MUST persist the latest current price in cache/history for that `Item.Id`.

#### Scenario: Cache/history is updated on price-drop eligibility

- GIVEN a previously processed item becomes eligible because current price is lower than cached price
- WHEN the item is included in the new-items result
- THEN the latest `Item.Price.CurrentPrice` is persisted in cache/history for that `Item.Id`

### Requirement: Do Nothing for Same Item Without Price Drop

The system MUST perform no notification eligibility action for a previously processed item when no price drop is detected.

#### Scenario: Same item without drop is ignored

- GIVEN a previously processed item with current price equal to or greater than cached price
- WHEN eligibility is evaluated
- THEN the item is excluded from the new-items result
- AND no new notification flow is triggered for that item

### Requirement: Keep ItemSearcher Focused on Eligibility

`ItemSearcher` MUST only identify and emit eligible items and MUST NOT perform channel-specific notification formatting.

#### Scenario: ItemSearcher emits eligibility-only payload

- GIVEN a set of candidate items with mixed eligibility outcomes
- WHEN `ItemSearcher` completes evaluation
- THEN the emitted new-items payload contains only eligible items
- AND no channel-specific formatting logic is executed in `ItemSearcher`

### Requirement: Keep Channel Delivery in Event Subscribers

Subscribers to `NewItemsFoundEvent` MUST remain responsible for channel-specific formatting and sending.

#### Scenario: Subscribers handle formatting and sending responsibilities

- GIVEN `NewItemsFoundEvent` is emitted with eligible items
- WHEN subscriber handlers process the event
- THEN each subscriber performs its own channel-specific formatting and sending behavior
- AND eligibility computation is not moved into subscriber formatting logic
