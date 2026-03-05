# Backend Developer Guide - Wallanoti

.NET 8 API with Clean Architecture + DDD + CQRS + Event-Driven Architecture

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

**Note:** Tests are located in `Test/Src/WallapopNotification.Tests/`

## Architecture

### Clean Architecture + DDD Structure

The backend follows **Hexagonal Architecture** with clear separation:

```
WallapopNotification/
├── [BoundedContext]/
│   ├── Domain/              # Entities, Value Objects, Domain Events, Interfaces
│   ├── Application/         # Use Cases, Command/Query Handlers (MediatR)
│   └── Infrastructure/      # Repositories, External Services, Persistence
└── Shared/                  # Cross-cutting concerns
```

### Bounded Contexts

- **Alerts** - Search alert management
- **Notifications** - Notification creation and delivery
- **Users** - User management and authentication
- **AlertCounter** - Statistics and counters

### Key Patterns

- **CQRS:** Commands and Queries separated using MediatR
- **Event-Driven:** Domain events published via RabbitMQ (`IEventBus`)
- **Aggregate Roots:** Extend `AggregateRoot` base class
- **Value Objects:** Immutable types like `Price`, `Url`, `VerificationCode`
- **Repository Pattern:** Interfaces in Domain, implementations in Infrastructure

## Recommended Skills

When working on the backend, consider invoking:

**`clean-ddd-hexagonal`** when:
- Designing new aggregates or bounded contexts
- Implementing repositories or domain events
- Structuring use cases with CQRS
- Working with entities, value objects, or domain models

**`tdd`** when:
- Implementing new features or bug fixes
- Following red-green-refactor cycle
- Writing integration tests

## Code Style Guidelines

### Naming Conventions

- **Classes/Interfaces:** PascalCase (`AlertController`, `IAlertRepository`)
- **Methods/Properties:** PascalCase (`GetAlerts`, `CreatedAt`)
- **Private fields:** `_camelCase` with underscore prefix (`_mediator`, `_repository`)
- **Local variables/parameters:** camelCase (`alertId`, `cancellationToken`)
- **Constants:** PascalCase (`MaxRetries`)

### Imports/Usings

- Implicit usings enabled globally (`<ImplicitUsings>enable</ImplicitUsings>`)
- Group usings: System namespaces first, then third-party, then project namespaces
- No unused usings

### Types

- **Nullable reference types enabled** (`<Nullable>enable</Nullable>`)
- Use `?` for nullable types explicitly (`DateTime?`, `string?`)
- Prefer `sealed` classes when not designed for inheritance
- Use `record` types for immutable DTOs and Commands/Queries

### Error Handling

- Throw domain-specific exceptions (e.g., `UserNotFoundException`)
- Use try-catch in Controllers/Application layer, not Domain
- Return proper HTTP status codes in controllers (201, 202, 404, etc.)

### Commands & Queries

- Commands: `CreateAlertCommand`, `DeactivateAlertCommand`
- Handlers: `AlertCommandHandler`, `GetAlertsByUserIdHandler`
- Always implement `IRequest` or `IRequest<TResponse>` from MediatR

### Testing

- Use **xUnit** with `[Fact]` and `[Theory]` attributes
- Use **Moq** for mocking dependencies
- Use **Bogus** for test data generation (see `AlertFaker`, `ItemFaker`)
- Inherit from `TestBase` for shared fixture setup
- Name pattern: `MethodName_Scenario_ExpectedResult` or descriptive test names

## Common Patterns

### Creating a New Command Handler

```csharp
// Command
public record CreateAlertCommand(long UserId, string AlertName, string AlertUrl) : IRequest;

// Handler
public sealed class AlertCommandHandler : IRequestHandler<CreateAlertCommand>
{
    private readonly IEventBus _eventBus;
    private readonly IAlertRepository _repository;

    public AlertCommandHandler(IEventBus eventBus, IAlertRepository repository)
    {
        _eventBus = eventBus;
        _repository = repository;
    }

    public async Task Handle(CreateAlertCommand command, CancellationToken cancellationToken)
    {
        var alert = Alert.Create(command.UserId, command.AlertName, command.AlertUrl);
        await _repository.Add(alert);
        await _eventBus.Publish(alert.PullDomainEvents());
    }
}
```

## Important Notes

- **Domain Events:** Always publish domain events after persistence (`await _eventBus.Publish(...)`)
- **EF Migrations:** Use `dotnet ef database update --project WallapopNotification` for schema changes
- **RabbitMQ:** Event bus for async communication between bounded contexts
- **Scheduled Tasks:** Uses Coravel for periodic alert searches
- **JWT Auth:** Token-based authentication for API endpoints
- **SignalR:** Used for real-time web notifications (see `WebNotificationHub`)

## References

- Architecture Docs: `docs/ARCHITECTURE.md`
- C4 Diagrams: `docs/c4-diagrams/`
- Docker Compose: `compose.yaml`
