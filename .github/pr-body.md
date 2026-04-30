## Summary

- **New endpoint**: `POST /api/Alert/from-natural-language` accepts natural language queries
- **NER integration**: Extracts entities (keywords, brand, price, category, location) via HuggingFace Inference API
- **URL builder**: Constructs valid Wallapop search URLs from extracted entities
- **Synchronous**: User waits for the complete creation process

## User Flow

1. User types "quiero una iphone por menos de 300 euros" in the frontend
2. BE calls NER service to extract: `iphone` (keyword), `Apple` (brand), `300` (max price)
3. URL built: `https://es.wallapop.com/search?keywords=iphone+apple&max_price=300`
4. Alert saved to database

## Backend Changes

| Layer | Files |
|-------|-------|
| Domain | `NerEntities.cs`, `INerService.cs`, `IWallapopUrlBuilder.cs` |
| Application | `CreateAlertFromNaturalLanguageCommand/Handler/Validator` |
| Infrastructure | `HuggingFaceNerService.cs`, `WallapopUrlBuilder.cs`, `NerServiceOptions.cs` |
| API | `AlertController.cs` (new endpoint) |

## Frontend Changes

- `AlertService.postAlertFromNaturalLanguage()` - new API method
- `HomeView.createAlertWithAI()` - wired to new endpoint with loading state

## Testing

- **140 tests passing** (Strict TDD enforced)
- Unit tests for: NerEntities, WallapopUrlBuilder, HuggingFaceNerService
- Handler tests: happy path, keywords-only, zero keywords, timeout
- Controller tests: 201, 400, 422, 503 responses

## Scenarios Verified

| Scenario | Status |
|----------|--------|
| Happy path (all entities extracted) | ✅ |
| Keywords-only extraction | ✅ |
| Zero keywords → 422 with extractedEntities | ✅ |
| Empty query → 400 | ✅ |
| Too short query → 400 | ✅ |
| NER timeout → 503 with Retry-After | ✅ |
| URL validation (es.wallapop.com) | ✅ |

## Risks

- NER API dependency (503 on timeout)
- URL structure fragile (covered by integration tests)