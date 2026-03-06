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

## RabbitMQ / MassTransit Event Bus

> **MANDATORY** — All topology MUST follow this structure exactly. Any deviation is not allowed.

The event bus is implemented with **MassTransit** (RabbitMQ transport, v8).

### Exchanges

The system uses **1 main exchange**. Retry and dead-letter are handled by MassTransit internally — do **not** create `retry-domain-events` or `dead-letter-domain-events` manually.

| Exchange        | Type  | Purpose                                         |
|-----------------|-------|-------------------------------------------------|
| `domain-events` | topic | Main exchange — all queues and bindings go here |

#### Error handling (managed by MassTransit)

| Mechanism          | How                                                                                                                                   |
|--------------------|---------------------------------------------------------------------------------------------------------------------------------------|
| Immediate retry    | `UseMessageRetry` — 2 attempts with 1 s interval, per endpoint                                                                       |
| Delayed redelivery | `UseDelayedRedelivery` — 3 attempts at 5 s / 15 s / 30 s (requires `rabbitmq_delayed_message_exchange` plugin)                       |
| Dead-letter        | MassTransit auto-creates `[queue]_error` queues after all retries are exhausted                                                       |

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

| Queue                                                           | Routing key            |
|-----------------------------------------------------------------|------------------------|
| `wallanoti.notifications.save_notification_on_new_items_found` | `alert.items-found`    |
| `wallanoti.notifications.notify_on_notification_created_push`  | `notification.created` |
| `wallanoti.notifications.notify_on_notification_created_web`   | `notification.created` |
| `wallanoti.notifications.notify_on_user_logged_in_push`        | `user.logged-in`       |
| `wallanoti.alert_counter.increment_on_new_items_found`         | `alert.items-found`    |

---

### Rules Summary

1. **One queue per consumer** — never share a queue between two consumers.
2. **Queue name** = `[bounded_context].[module].[consumer_name_snake_case]`
3. **Routing key** = domain event name (e.g. `alert.items-found`)
4. **Never publish directly to a queue** — always publish to `domain-events` with the correct routing key.
5. **Do not create** `retry-domain-events` or `dead-letter-domain-events` exchanges — MassTransit handles retry and error queues automatically.
6. **To add a new consumer**: register it in `MassTransitBusConfiguration.cs` — add `x.AddConsumer<>()` and `cfg.ReceiveEndpoint(...)` with the correct queue name and routing key.

---

## Important Notes

- **Domain Events:** Always publish domain events after persistence (`await _eventBus.Publish(...)`)
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
