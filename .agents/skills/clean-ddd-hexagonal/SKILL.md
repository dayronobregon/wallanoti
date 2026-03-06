---
name: clean-ddd-hexagonal
description: Proactively apply when designing APIs, microservices, or scalable backend structure. Triggers on DDD, Clean Architecture, Hexagonal, entities, value objects, domain events, CQRS, event sourcing, repository pattern, use cases, onion architecture, outbox pattern, aggregate root, anti-corruption layer. Use when working with domain models, aggregates, repositories, or bounded contexts. Clean Architecture + DDD + Hexagonal patterns for backend services, language-agnostic (Go, Rust, Python, TypeScript, Java, C#).
---

# Clean Architecture + DDD + Hexagonal

Backend architecture combining DDD tactical patterns, Clean Architecture dependency rules, and Hexagonal boundaries for maintainable, testable systems.

## Canonical Defaults (Use These)

- **DDD structure:** vertical slices grouped by **aggregate**.
- **CQRS flow:** commands/queries go through **Mediator** (no controller-to-handler direct calls).
- **Framework dependencies:** `DbContext`, ORM models (`Eloquent`, etc.), and DB context objects are allowed **only** in `src/{aggregate}/infrastructure/**`.
- **Scope of this file:** concise operating rules only. Detailed examples live in `references/`.

## When to Use (and When NOT to)

| Use When | Skip When |
|----------|-----------|
| Complex business domain with many rules | Simple CRUD, few business rules |
| Long-lived system (years of maintenance) | Prototype, MVP, throwaway code |
| Team of 5+ developers | Solo developer or small team (1-2) |
| Multiple entry points (API, CLI, events) | Single entry point, simple API |
| Need to swap infrastructure (DB, broker) | Fixed infrastructure, unlikely to change |
| High test coverage required | Quick scripts, internal tools |

**Start simple. Evolve complexity only when needed.** Most systems don't need full CQRS or Event Sourcing.

## CRITICAL: The Dependency Rule

Dependencies point **inward only**. Outer layers depend on inner layers, never the reverse.

```
Infrastructure → Application → Domain
 (implementations) (use cases/commands/queries) (core)
```

**Violations to catch:**
- Domain importing database/HTTP libraries
- Controllers calling repositories directly (bypassing use cases)
- Entities depending on application services

**Design validation:** "Create your application to work without either a UI or a database" — Alistair Cockburn. If you can run your domain logic from tests with no infrastructure, your boundaries are correct.

## Quick Decision Trees

See [references/CHEATSHEET.md](references/CHEATSHEET.md) for all trees. Keep only this policy:

- One aggregate per transaction.
- Cross-aggregate consistency via domain events (eventual consistency).

## Directory Structure

```
project/
├── src/                        # class libraries (domain + application + infrastructure impl)
│   ├── {aggregate}/
│   │   ├── domain/             # entities + value objects + events + repository interface
│   │   ├── application/        # commands + queries + handlers + use case interfaces
│   │   └── infrastructure/     # implementations grouped by concern (persistence, messaging, etc.)
│   └── shared/
│       ├── domain/
│       ├── application/
│       └── infrastructure/
└── apps/                       # transport/framework apps (same level as src)
   ├── Api/                    # controllers, routes, grpc, graphql
   └── ...
```

**Required slice shape:** `{aggregate}` is the root folder, with `domain/application/infrastructure` inside each aggregate.

For concrete examples, see [references/LAYERS.md](references/LAYERS.md) and [references/HEXAGONAL.md](references/HEXAGONAL.md).

## DDD Building Blocks

| Pattern | Purpose | Layer | Key Rule |
|---------|---------|-------|----------|
| **Entity** | Identity + behavior | Domain | Equality by ID |
| **Value Object** | Immutable data | Domain | Equality by value, no setters |
| **Aggregate** | Consistency boundary | Domain | Only root is referenced externally |
| **Domain Event** | Record of change | Domain | Past tense naming (`OrderPlaced`) |
| **Repository** | Persistence abstraction | Domain (interface) | Per aggregate, not per table |
| **Domain Service** | Stateless logic | Domain | When logic doesn't fit an entity |
| **Application Service** | Orchestration | Application | Coordinates domain + infra |

CQRS command/query handlers are application concerns and are invoked through Mediator.

## Anti-Patterns (CRITICAL)

| Anti-Pattern | Problem | Fix |
|--------------|---------|-----|
| **Anemic Domain Model** | Entities are data bags, logic in services | Move behavior INTO entities |
| **Repository per Entity** | Breaks aggregate boundaries | One repository per AGGREGATE |
| **Leaking Infrastructure** | Domain imports DB/HTTP libs | Domain has ZERO external deps |
| **Framework ORM Outside Infrastructure** | `DbContext`/`Eloquent` in domain/application/apps creates tight coupling | Keep `DbContext`/`Eloquent` only in `src/{aggregate}/infrastructure/**` |
| **Skipping Application Layer** | Controllers/transport calling repositories directly | Always go through application commands/queries |
| **Premature CQRS** | Adding complexity before needed | Start with simple read/write, evolve |
| **Cross-Aggregate TX** | Multiple aggregates in one transaction | Use domain events for consistency |
| **Bypassing Mediator** | Tight coupling to handlers | Route all commands/queries via mediator |

See detailed anti-patterns and examples in [references/CHEATSHEET.md](references/CHEATSHEET.md) and [references/DDD-TACTICAL.md](references/DDD-TACTICAL.md).

## Implementation Order

1. **Discover the Domain** — Event Storming, conversations with domain experts
2. **Model the Domain** — Entities, value objects, aggregates (no infra)
3. **Define Interfaces** — Repository and external service contracts
4. **Implement Use Cases** — Application services coordinating domain
5. **Add Infrastructure Implementations last** — Database, messaging, external service implementations
6. **Add Apps last** — API/CLI/gRPC transport in `apps/`

**DDD is collaborative.** Modeling sessions with domain experts are as important as the code patterns.

## Reference Documentation

| File | Purpose |
|------|---------|
| [references/LAYERS.md](references/LAYERS.md) | Complete layer specifications |
| [references/DDD-STRATEGIC.md](references/DDD-STRATEGIC.md) | Bounded contexts, context mapping |
| [references/DDD-TACTICAL.md](references/DDD-TACTICAL.md) | Entities, value objects, aggregates (pseudocode) |
| [references/HEXAGONAL.md](references/HEXAGONAL.md) | Boundaries, interfaces, implementations |
| [references/CQRS-EVENTS.md](references/CQRS-EVENTS.md) | Command/query separation, events |
| [references/CHEATSHEET.md](references/CHEATSHEET.md) | Quick decision guide |

## Sources

### Primary Sources
- [The Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) — Robert C. Martin (2012)
- [Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/) — Alistair Cockburn (2005)
- [Domain-Driven Design: The Blue Book](https://www.domainlanguage.com/ddd/blue-book/) — Eric Evans (2003)
- [Implementing Domain-Driven Design](https://openlibrary.org/works/OL17392277W) — Vaughn Vernon (2013)

### Pattern References
- [CQRS](https://martinfowler.com/bliki/CQRS.html) — Martin Fowler
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html) — Martin Fowler
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html) — Martin Fowler (PoEAA)
- [Unit of Work](https://martinfowler.com/eaaCatalog/unitOfWork.html) — Martin Fowler (PoEAA)
- [Bounded Context](https://martinfowler.com/bliki/BoundedContext.html) — Martin Fowler
- [Transactional Outbox](https://microservices.io/patterns/data/transactional-outbox.html) — microservices.io
- [Effective Aggregate Design](https://www.dddcommunity.org/library/vernon_2011/) — Vaughn Vernon

### Implementation Guides
- [Microsoft: DDD + CQRS Microservices](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)
- [Domain Events](https://udidahan.com/2009/06/14/domain-events-salvation/) — Udi Dahan
