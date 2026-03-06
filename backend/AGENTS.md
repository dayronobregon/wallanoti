# Wallanoti API - AI Agent Ruleset

> **Skills Reference**: For detailed patterns, use these skills:
> - [`clean-ddd-hexagonal`](../../../../Users/dayronrey/.config/opencode/skills/clean-ddd-hexagonal/SKILL.md) -
    Hexagonal
    Architecture, DDD, CQRS for backend design
> - [`tdd`](../../../../Users/dayronrey/.config/opencode/skills/tdd/SKILL.md) - Test-Driven Development practices and
    techniques

.NET 8 API with Clean Architecture + DDD + CQRS + Event-Driven Architecture

### Auto-invoke Skills

When performing these actions, ALWAYS invoke the corresponding skill FIRST:

| Action                     | Skill                 |
|----------------------------|-----------------------|
| Implementing a new feature | `clean-ddd-hexagonal` |
| Refactoring existing code  | `clean-ddd-hexagonal` |
| Writing tests              | `tdd`                 |
| Fixing bugs                | `tdd`                 |

**Note:** Tests are located in `Test/Src/WallapopNotification.Tests/`

## Architecture

### Layer Responsibilities

- The `/src` folder contains the **core system layers**
- The `/api` folder is **only the delivery mechanism**
- The `/test` folder contains all unit and integration tests, organized by layer

### Clean Architecture + DDD Structure

The backend follows **Hexagonal Architecture** with clear separation:

### Key Patterns

- **CQRS:** Commands and Queries separated using MediatR
- **Event-Driven:** Domain events published via RabbitMQ (`IEventBus`)
- **Aggregate Roots:** Extend `AggregateRoot` base class
- **Value Objects:** Immutable types like `Price`, `Url`, `VerificationCode`
- **Repository Pattern:** Interfaces in Domain, implementations in Infrastructure

## Important Notes

- **Domain Events:** Always publish domain events after persistence (`await _eventBus.Publish(...)`)
- **EF Migrations:** Use `dotnet ef database update --project WallapopNotification` for schema changes
- **RabbitMQ:** Event bus for async communication between bounded contexts
- **Scheduled Tasks:** Uses Coravel for periodic alert searches
- **JWT Auth:** Token-based authentication for API endpoints
- **SignalR:** Used for real-time web notifications (see `WebNotificationHub`)

## Build, Test & Run Commands

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run all tests
dotnet test

# Run a single test by fully qualified name
dotnet test --filter "FullyQualifiedName=WallapopNotification.Tests.Alerts._2_Application.SearchNewItems.ItemSearcherTest.METHOD"

# Run all tests in a specific class
dotnet test --filter "FullyQualifiedName~ItemSearcherTest"

# Run tests with verbosity
dotnet test --verbosity detailed

# Build specific project
dotnet build WallapopNotification/WallapopNotification.csproj

# Run API locally
dotnet run --project Apps/Api

# Apply EF migrations
dotnet ef database update --project WallapopNotification
```