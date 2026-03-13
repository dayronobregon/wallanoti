# Notifications Specification (Prefixes for Changes)

## Purpose

This specification defines how notifications are formatted with Spanish prefixes indicating the type of change detected in Wallapop items.

## Requirements

### Requirement: Change Type Prefixes

The system **SHALL** prefix the notification title with one of the following based on `changeType` from `NewItemsFoundEvent`:

- `changeType == \"new\"`: \"**Nuevo:** \"
- `changeType == \"update\"`: \"**Actualización:** \"
- `changeType == \"price_drop\"`: \"**Bajada de Precio:** \"

#### Scenario: New item prefix (English)

- GIVEN `changeType = \"new\"`
- WHEN `Notification.FormattedString()` is called
- THEN title starts with \"**Nuevo:** \"

#### Escenario: Prefijo para nuevo ítem (Español)

- SI `changeType = \"new\"`
- CUANDO `Notification.FormattedString()` es llamada
- ENTONCES título comienza con \"**Nuevo:** \"

#### Scenario: Update prefix

- GIVEN `changeType = \"update\"`
- WHEN formatting
- THEN \"**Actualización:** \"

#### Escenario: Prefijo de actualización

- SI `changeType = \"update\"`
- CUANDO formateando
- ENTONCES \"**Actualización:** \"

#### Scenario: Price drop prefix

- GIVEN `changeType = \"price_drop\"`
- WHEN formatting
- THEN \"**Bajada de Precio:** \"

#### Escenario: Prefijo bajada de precio

- SI `changeType = \"price_drop\"`
- CUANDO formateando
- ENTONCES \"**Bajada de Precio:** \"

### Requirement: Priority for Multiple Changes

If both update and price drop conditions met, **SHALL** use \"price_drop\" prefix.

#### Scenario: Priority price drop

- GIVEN conditions for both update and price_drop
- WHEN determining prefix
- THEN use \"**Bajada de Precio:** \"