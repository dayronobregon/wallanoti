# Proposal - duplicated-notification-codex

## Intent
Fix duplicate Telegram notifications by introducing item price-aware deduplication logic in alert item processing.

## Scope
- Replace ID-only cache semantics with item ID -> cached price semantics.
- Preserve compatibility with legacy cache payload (`List<string>` IDs).
- Apply price-drop notification rule with explicit title suffix `(Baja de Precio)`.

## Why this change
Current behavior cannot distinguish valid repeat notifications (price drops) from undesired duplicates because it only tracks item IDs.

## Affected modules/packages
- Alerts application flow (`ItemSearcher`)
- Alert search tests (`ItemSearcherTest`)

## Rollback plan
- Revert `ItemSearcher` and `ItemSearcherTest` changes in a single commit.
- Legacy cache format compatibility is read-only; rollback does not require data migration.
