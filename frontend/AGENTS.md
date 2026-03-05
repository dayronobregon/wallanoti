# Frontend Developer Guide - Wallanoti

Vue 3 + TypeScript SPA with Vite

## Build, Test & Run Commands

```bash
# Install dependencies
npm install

# Run dev server (port 3000)
npm run dev

# Type check
npm run type-check

# Build for production (runs type-check + build)
npm run build

# Build only (no type check)
npm run build-only

# Preview production build
npm run preview

# Generate API client from OpenAPI spec
npm run generate:api
```

**Note:** Frontend uses Rolldown-based Vite (experimental performance improvement).

## Architecture

### Vue 3 Composition API Structure

```
src/
├── api/                # OpenAPI-generated client (DO NOT EDIT MANUALLY)
├── components/         # Reusable Vue components
├── composables/        # Composition API logic (e.g., useApiClient)
├── stores/             # Pinia stores with persistence
├── views/              # Page-level components
├── router/             # Vue Router configuration
└── services/           # Business logic services
```

## Recommended Skills

**`tdd`** when:
- Implementing new features or bug fixes
- Writing component tests
- Following red-green-refactor cycle

## Code Style Guidelines

### Naming Conventions

- **Components:** PascalCase filenames (`AlertBadges.vue`, `HeaderView.vue`)
- **Composables:** camelCase with `use` prefix (`useApiClient.ts`, `useAuthStore`)
- **Stores:** camelCase (`auth.ts`)
- **Variables/Functions:** camelCase (`loadAlertCounter`, `isAuthenticated`)
- **Constants:** UPPER_SNAKE_CASE or camelCase for config

### Imports

- Use `@/` alias for `src/` directory (`import { useAuthStore } from '@/stores/auth'`)
- Group imports: Vue imports first, then third-party, then local
- Use named imports for clarity

### Types

- Always use TypeScript strict mode
- Import types with `type` keyword: `import type { User } from '@/api'`
- Define component props with `defineProps<Props>()` TypeScript syntax
- Define emits with `defineEmits<{ eventName: [arg: type] }>()`
- Use `.value` for refs explicitly

### Component Style

- Use **Composition API** with `<script setup lang="ts">`
- Organize script: imports → types → composables → refs → computed → functions → lifecycle hooks
- Use `ref()` for reactive primitives, `reactive()` for objects sparingly
- Prefer `ref()` for better TypeScript inference

### Error Handling

- Use try-catch blocks for async API calls
- Log errors to console with `console.error()`
- Show user-friendly alerts for errors (`alert()` or custom toast component)
- Never let unhandled promise rejections occur

### API Client

- Use `useApiClient()` for unauthenticated requests
- Use `useAuthenticatedApiClient()` for authenticated requests
- API client is auto-generated from OpenAPI spec (don't edit `src/api/` manually)
- Regenerate with `npm run generate:api` after backend API changes

### State Management

- Use **Pinia** stores for global state
- Enable persistence with `{ persist: true }` for auth state
- Use computed properties for derived state

## Common Patterns

### API Call with Error Handling

```typescript
async function handleCreateAlert() {
  try {
    creatingAlert.value = true
    await apiClient.alert.postAlert(newAlertName.value.trim(), newAlertUrl.value.trim())
    emit('alertUpdated')
    console.log('Alerta creada correctamente')
  } catch (error) {
    console.error('Error al crear la alerta:', error)
    alert('Error al crear la alerta. Por favor, intenta de nuevo.')
  } finally {
    creatingAlert.value = false
  }
}
```

## Important Notes

- **API Client:** Auto-generated from OpenAPI spec (don't edit `src/api/` manually)
- **Regenerate Client:** Run `npm run generate:api` after backend API changes
- **Base URL:** Configured via `VITE_API_BASE_URL` env var, proxied in dev mode
- **SignalR:** Used for real-time web notifications
- **Auth:** JWT tokens stored in Pinia with persistence
- **Build Tool:** Using Rolldown-based Vite (experimental performance improvement)
