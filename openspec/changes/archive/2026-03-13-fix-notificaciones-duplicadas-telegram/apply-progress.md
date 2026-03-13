## Apply Progress: Domain Phase

### Completed
- 1.1 ProcessedItem unit tests (validation, equality, DetectChange/UpdateFrom scenarios)
- 1.2 ProcessedItem aggregate (ChangeType enum, DetectChange/UpdateFrom, concurrency RowVersion)
- ItemChangesFoundEvent domain event

Tests: 14/14 pass
dotnet build: pass

### Files Created
- src/Alerts/Domain/ChangeType.cs
- src/Alerts/Domain/Entities/ProcessedItem.cs
- src/Alerts/Domain/ItemChangesFoundEvent.cs
- tests/Alerts/1-Domain/ProcessedItemTest.cs

Ready for repo interface and application changes.