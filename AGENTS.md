# Wallanoti - Agent Guide

Developer guide for coding agents working in the Wallanoti codebase.

## Project Overview

**Wallanoti** is a Telegram bot that sends real-time notifications when new items matching your search criteria appear on Wallapop (Spanish second-hand marketplace). No more manually checking every 5 minutes - the bot monitors searches and notifies you instantly.

## Monorepo Structure

This is a **monorepo** with two main parts:
- **`backend/`** - .NET 8 C# API with Clean Architecture + DDD + CQRS
- **`frontend/`** - Vue 3 + TypeScript SPA with Vite

## Technology Stack

### Backend
- **.NET 8** with ASP.NET Core
- **Clean Architecture** + **DDD** + **CQRS** + **Event-Driven Architecture**
- **Entity Framework Core** with PostgreSQL
- **RabbitMQ** for Event Bus (async communication)
- **Redis** for distributed caching
- **SignalR** for real-time web notifications
- **Telegram.Bot** for Telegram integration
- **MediatR** for CQRS implementation
- **Coravel** for scheduled tasks

### Frontend
- **Vue 3** with TypeScript
- **Vite** (Rolldown-based) for build tooling
- **Pinia** for state management with persistence
- **TailwindCSS** for styling
- **OpenAPI-generated client** for type-safe API calls
- **SignalR** for real-time notifications

## Recommended Skills

When working on this project, consider invoking these skills:

### `clean-ddd-hexagonal`
Use for backend architecture work involving:
- Clean Architecture principles
- Domain-Driven Design (DDD) patterns
- Hexagonal Architecture boundaries
- Entities, Value Objects, Aggregates
- Domain Events and CQRS
- Repository Pattern

### `tdd`
Use for test-driven development:
- Red-green-refactor cycle
- Writing tests before implementation
- Integration tests
- Behavior verification through public interfaces

## Domain Model

The backend is organized into **Bounded Contexts** following DDD:

- **Alerts** - Search alert management (create, activate, deactivate, delete)
- **Notifications** - Notification creation and delivery (Telegram, Web)
- **Users** - User management and authentication (JWT-based)
- **AlertCounter** - Statistics and counters for alerts

## Detailed Documentation

For specific implementation guidelines, see:

- **Backend guidelines:** `backend/AGENTS.md`
- **Frontend guidelines:** `frontend/AGENTS.md`

## Additional Resources

- **Backend README:** `backend/README.md`
- **Architecture Documentation:** `backend/docs/ARCHITECTURE.md`
- **C4 Diagrams:** `backend/docs/c4-diagrams/`
- **Docker Compose:** `backend/compose.yaml`

## Quick Start

### Backend
```bash
cd backend
dotnet restore
dotnet build
dotnet run --project Apps/Api
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

See the respective `AGENTS.md` files in each directory for complete command references and style guidelines.
