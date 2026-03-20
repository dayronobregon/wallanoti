# Explore - duplicated-notification-codex

## Problem
Telegram notifications are duplicated for already processed Wallapop items.

## Current Behavior Discovery
- `ItemSearcher` currently reads cache as `List<string>` (item IDs only).
- Already processed items are skipped only by ID presence.
- Cache does not store historical price, so the system cannot decide if a repeat notification is justified by a price drop.
- Notification title is passed through as-is from Wallapop item title.

## Business Constraints
- Identity must be based on `Item.Id`.
- Price-drop condition must be `Item.Price.CurrentPrice < previous cached Item price`.
- If price drops, notify again and append `(Baja de Precio)` to title.
- If processed and no price drop, do nothing.
- If never processed, notify normally.
- On price drop, cache must be updated with the current price.

## Affected Module
- `backend/src/Alerts/Application/SearchNewItems/ItemSearcher.cs`
- `backend/tests/Alerts/2-Application/SearchNewItems/ItemSearcherTest.cs`
