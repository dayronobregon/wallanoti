# Wallanoti - Agent Guide

Developer guide for coding agents working in the Wallanoti codebase.

## Project Overview

**Wallanoti** is a Telegram bot that sends real-time notifications when new items matching your search criteria appear
on Wallapop (Spanish second-hand marketplace). No more manually checking every 5 minutes - the bot monitors searches and
notifies you instantly.

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

## Available Skills

| Skill                 | Description                                                        | Location                                          |
|-----------------------|--------------------------------------------------------------------|---------------------------------------------------|
| `clean-ddd-hexagonal` | Clean Architecture, DDD, Hexagonal patterns for backend design     | [clean-ddd-hexagonal](skills/clean-ddd-hexagonal) |
| `tdd`                 | Test-Driven Development practices and techniques                   | [tdd](skills/tdd)                                 |
| `git-commit`          | Best practices for writing clear and effective git commit messages | [git-commit](skills/git-commit)                   |
| `pull-request`        | Best practices for writing clear and effective pull request        | [pull-request](skills/pull-request)               |

### Auto-invoke Skills

When performing these actions, ALWAYS invoke the corresponding skill FIRST:

| Action                     | Skill                 |
|----------------------------|-----------------------|
| Implementing a new feature | `clean-ddd-hexagonal` |
| Refactoring existing code  | `clean-ddd-hexagonal` |
| Writing tests              | `tdd`                 |
| Fixing bugs                | `tdd`                 |
| Writing commit messages    | `git-commit`          |
| Writing pull requests      | `pull-request`        |

## Detailed Documentation

For specific implementation guidelines, see:

- **Backend guidelines:** `backend/AGENTS.md`
- **Frontend guidelines:** `frontend/AGENTS.md`

## Quick Start

### Backend

```bash
cd backend/api
dotnet restore
dotnet build
dotnet run --project api
```

### Frontend

```bash
cd frontend
npm install
npm run dev
```

See the respective `AGENTS.md` files in each directory for complete command references and style guidelines.