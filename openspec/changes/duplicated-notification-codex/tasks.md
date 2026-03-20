# Tasks - duplicated-notification-codex

## 1. Exploration and proposal
- [x] 1.1 Inspect current duplicate filtering and cache format in `ItemSearcher`.
- [x] 1.2 Confirm notification creation path and title handling.
- [x] 1.3 Produce proposal/spec/design artifacts under openspec.

## 2. Implementation
- [x] 2.1 Add price-aware cache model (`Item.Id` -> cached price) in `ItemSearcher`.
- [x] 2.2 Implement decision logic for never-processed, processed-no-drop, processed-drop.
- [x] 2.3 Append ` (Baja de Precio)` to dropped-price notification titles.
- [x] 2.4 Add legacy cache compatibility for old ID-list payloads.
- [x] 2.5 Ensure cache price is updated after processing.

## 3. Testing and verification
- [x] 3.1 Add/adjust tests for cache payload shape and baseline flow.
- [x] 3.2 Add test for price-drop notification suffix behavior.
- [x] 3.3 Add test for legacy cache compatibility.
- [x] 3.4 Run targeted test suite for affected modules.
