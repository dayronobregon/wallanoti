# Wallanoti API - AI Agent Ruleset

> **Skills Reference**: For detailed patterns, use these skills:
> - [clean-ddd-hexagonal](../.agents/skills/clean-ddd-hexagonal) -
    Hexagonal Architecture, DDD, CQRS for backend design
> - [tdd](../.agents/skills/tdd) - Test-Driven Development practices and
    techniques

.NET 8 API with Clean Architecture + DDD + CQRS + Event-Driven Architecture

---

## 🔴 TDD is Mandatory — No Exceptions

**Before writing any production code**, tests MUST be written first. This is non-negotiable.

### The Red-Green-Refactor cycle applies to ALL work:

1. 🔴 **Red** — Write a failing test that describes the expected behavior
2. 🟢 **Green** — Write the minimum production code to make it pass
3. 🔵 **Refactor** — Clean up while keeping tests green

> If there is no failing test, there is no implementation. Always invoke the `tdd` skill before starting any task.

---

### Auto-invoke Skills

When performing these actions, ALWAYS invoke the corresponding skill FIRST:

| Action                     | Skill(s)                            |
|----------------------------|-------------------------------------|
| Implementing a new feature | `tdd` → then `clean-ddd-hexagonal` |
| Refactoring existing code  | `tdd` → then `clean-ddd-hexagonal` |
| Writing tests              | `tdd`                               |
| Fixing bugs                | `tdd` (reproduce bug as a failing test first) |

> **Order matters:** Always invoke `tdd` before `clean-ddd-hexagonal`. Tests come before implementation.

**Note:** Tests are located in `test/`

---

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

---

## RabbitMQ / MassTransit Event Bus

> **MANDATORY** — All topology MUST follow this structure exactly. Any deviation is not allowed.

The event bus is implemented with **MassTransit** (RabbitMQ transport, v8).

#### Error handling

On failure, MassTransit automatically moves the message to `[queue]_error`.

---

### Queue Convention

**One queue per consumer.** A consumer is identified by its bounded context, module, and handler name.

#### Queue naming pattern

```
[bounded_context].[module].[consumer_name_snake_case]
```

**Example:**

```
wallanoti.users.notify_on_new_search
```

---

### Binding Convention

Each queue is bound to `domain-events` using a routing key equal to the domain event name.

#### Routing key pattern

```
[aggregate].[event_name_snake_case]
```

**Example:**

```
alert.items-found
```

#### Current topology

| Queue                                                          | Routing key            |
|----------------------------------------------------------------|------------------------|
| `wallanoti.notifications.save_notification_on_new_items_found` | `alert.items-found`    |
| `wallanoti.notifications.notify_on_notification_created_push`  | `notification.created` |
| `wallanoti.notifications.notify_on_notification_created_web`   | `notification.created` |
| `wallanoti.notifications.notify_on_user_logged_in_push`        | `user.logged-in`       |
| `wallanoti.alert_counter.increment_on_new_items_found`         | `alert.items-found`    |

---

### Rules Summary

1. **Queue name** = `[bounded_context].[module].[consumer_name_snake_case]`
2. **Routing key** = domain event name (e.g. `alert.items-found`)

---

## Important Notes

- **Domain Events:** Always publish domain events after persistence (`await _eventBus.Publish(...)`)
- **RabbitMQ:** Event bus for async communication between bounded contexts
- **Scheduled Tasks:** Uses Coravel for periodic alert searches
- **JWT Auth:** Token-based authentication for API endpoints
- **SignalR:** Used for real-time web notifications (see `WebNotificationHub`)

---

## Build, Test & Run Commands

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run all tests
dotnet test

# Run a single test by fully qualified name
dotnet test --filter "FullyQualifiedName=Tests.Alerts._2_Application.SearchNewItems.ItemSearcherTest.METHOD"

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