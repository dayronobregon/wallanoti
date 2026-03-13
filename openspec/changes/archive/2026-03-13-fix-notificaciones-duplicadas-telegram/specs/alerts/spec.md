# Alerts Specification (Duplicate Prevention)

## Purpose

This specification defines the complete behavior for processing Wallapop items during alert searches, including persistence of ProcessedItems and detection of new items, updates (ModifiedAt changes), and price drops to avoid duplicate Telegram notifications.

## Requirements

### Requirement: ProcessedItem Persistence

The system **SHALL** persist a `ProcessedItem` record for each unique `AlertId` + `Wallapop ItemId` combination. Each record **SHALL** store `LastModifiedAt` (from Wallapop `ModifiedAt`) and `LastPrice` (from Wallapop `CurrentPrice`).

The record **SHALL** be updated after every fetch with the latest values.

#### Scenario: Persist new ProcessedItem (Happy Path)

- GIVEN an active alert and a newly fetched Wallapop item with no existing ProcessedItem
- WHEN `ItemSearcher` processes the item
- THEN create and persist ProcessedItem with current `ModifiedAt` and `CurrentPrice`
- AND publish `NewItemsFoundEvent` with `changeType: \"new\"`

#### Escenario: Persistir nuevo ProcessedItem (Happy Path - Español)

- SI un alerta activa y un ítem de Wallapop recién fetched sin ProcessedItem existente
- CUANDO `ItemSearcher` procesa el ítem
- ENTONCES crear y persistir ProcessedItem con `ModifiedAt` y `CurrentPrice` actuales
- Y publicar `NewItemsFoundEvent` con `changeType: \"new\"`

### Requirement: New Item Detection

The system **SHALL** classify an item as **new** when no ProcessedItem record exists for the alert-item pair.

#### Scenario: New item notification

- GIVEN no ProcessedItem for alert A and item I
- WHEN item I is fetched (CreatedAt >= alert.CreatedAt)
- THEN `changeType = \"new\"`
- AND notification sent with \"Nuevo:\" prefix

### Requirement: Item Update Detection

The system **SHALL** detect an **update** when `item.ModifiedAt > processed.LastModifiedAt`.

#### Scenario: Update without price change

- GIVEN ProcessedItem with LastModifiedAt = \"2024-01-01T10:00\", LastPrice = 100€
- WHEN fetched item has ModifiedAt = \"2024-01-01T11:00\", CurrentPrice = 100€
- THEN `changeType = \"update\"`
- AND update ProcessedItem record
- AND notification with \"Actualización:\" prefix

#### Escenario: Actualización sin cambio de precio (Español)

- DADO ProcessedItem con LastModifiedAt = \"2024-01-01T10:00\", LastPrice = 100€
- CUANDO ítem fetched tiene ModifiedAt = \"2024-01-01T11:00\", CurrentPrice = 100€
- ENTONCES `changeType = \"update\"`
- Y actualizar ProcessedItem
- Y notificación con \"Actualización:\" prefijo

### Requirement: Price Drop Detection

The system **SHALL** detect a **price drop** when `item.CurrentPrice < processed.LastPrice`, independently of ModifiedAt.

#### Scenario: Price drop (no ModifiedAt change)

- GIVEN LastPrice = 100€, LastModifiedAt any
- WHEN CurrentPrice = 90€
- THEN `changeType = \"price_drop\"`
- AND update record with new price
- AND notification with \"Bajada de Precio:\" prefix

#### Scenario: Price drop with ModifiedAt change

- GIVEN LastPrice = 100€, ModifiedAt old
- WHEN CurrentPrice = 90€ AND newer ModifiedAt
- THEN `changeType = \"price_drop\"` (priority over update)
- AND notification \"Bajada de Precio:\"

### Requirement: No Notification on No Change

The system **SHALL NOT** send notification if `ModifiedAt <= LastModifiedAt` AND `CurrentPrice >= LastPrice`.

#### Scenario: No changes

- GIVEN LastModifiedAt = \"2024-01-01T12:00\", LastPrice = 100€
- WHEN item has ModifiedAt = \"2024-01-01T11:00\", CurrentPrice = 100€
- THEN no event published
- AND update ProcessedItem if fetched (but no notify)

## Edge Cases

### Scenario: False ModifiedAt (same ModifiedAt, price drop)

- GIVEN LastModifiedAt = \"2024-01-01T10:00\", LastPrice = 100€
- WHEN ModifiedAt = \"2024-01-01T10:00\" (false/no change), CurrentPrice = 90€
- THEN detect price drop, notify \"Bajada de Precio:\"
- AND update LastPrice to 90€

### Scenario: Multi-alerts for same item

- GIVEN two alerts A1, A2 for same search
- WHEN item I updates
- THEN A1 ProcessedItem updated and notifies for A1
- AND A2 ProcessedItem updated and notifies for A2 (independent)

### Scenario: Price increase

- GIVEN LastPrice = 100€
- WHEN CurrentPrice = 110€
- THEN no notification (only drops)
- AND update LastPrice to 110€