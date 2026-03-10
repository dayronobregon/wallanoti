# Docker Deployment

Este directorio contiene la configuración Docker para desplegar Wallanoti.

## 📁 Estructura

```
docker/
├── docker-compose.resources.yml  # Solo recursos (postgres, rabbitmq, redis)
├── docker-compose.apps.yml       # Solo aplicaciones (api, frontend)
├── docker-compose.yml            # Incluye resources + apps (stack completo)
├── .env.example                  # Variables de entorno de ejemplo
└── initdb/                       # Scripts de inicialización de PostgreSQL
    └── *.sql
```

## 🚀 Uso

### 1. Configurar variables de entorno

```bash
cd docker
cp .env.example .env
# Edita .env con tus credenciales y tokens
```

### 2. Solo recursos (desarrollo local)

Levanta solo PostgreSQL, RabbitMQ y Redis.

```bash
docker compose -f docker-compose.resources.yml up -d
```

### 3. Stack completo (desarrollo con Docker)

Levanta recursos + API + frontend (sin duplicación de código).

```bash
docker compose -f docker-compose.yml up -d
```

## 🔍 Servicios

### Recursos

| Servicio  | Puerto | URL Admin                    |
|-----------|--------|------------------------------|
| Postgres  | 5432   | -                            |
| RabbitMQ  | 5672   | http://localhost:15672       |
| Redis     | 6379   | -                            |

**RabbitMQ Management**: usuario/contraseña según `.env` (default: `admin`/`admin`)

### Aplicaciones

| Servicio | Puerto | URL                   |
|----------|--------|-----------------------|
| API      | 8080   | http://localhost:8080 |
| Frontend | 80     | http://localhost |

## 🛠️ Comandos útiles

```bash
# Ver logs
docker compose -f docker-compose.yml logs -f

# Ver logs de un servicio específico
docker compose -f docker-compose.yml logs -f api

# Parar servicios
docker compose -f docker-compose.yml down

# Parar y eliminar volúmenes (⚠️ elimina datos)
docker compose -f docker-compose.yml down -v

# Reconstruir imágenes
docker compose -f docker-compose.yml build

# Reconstruir y levantar
docker compose -f docker-compose.yml up -d --build
```

## 🔄 Flujo de trabajo recomendado

### Desarrollo local (backend + frontend en tu máquina)

```bash
cd docker
docker compose -f docker-compose.resources.yml up -d

# En otra terminal
cd ../backend/api
dotnet run

# En otra terminal
cd ../frontend
npm run dev
```

### Desarrollo con Docker (todo en contenedores)

```bash
cd docker
docker compose -f docker-compose.yml up -d
```

## 📝 Notas

- `docker-compose.yml` usa `include` para reutilizar `docker-compose.resources.yml` y `docker-compose.apps.yml`.
- Para asegurar el mismo nombre de proyecto, se recomienda `COMPOSE_PROJECT_NAME=wallanoti` en `.env`.
- `initdb/` se monta completo en Postgres para ejecutar todos los `.sql`.
- El frontend se sirve con nginx y hace proxy de `/api/*` y `/hub/*` al backend.
- La configuración por defecto usa credenciales de desarrollo. **Cambia las credenciales en producción**.
