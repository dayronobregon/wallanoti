# 🐳 Docker Deployment Guide

Guía rápida para desplegar Wallanoti con Docker.

## 📖 Documentación Completa

Ver **[docker/README.md](docker/README.md)** para documentación detallada.

## ⚡ Quick Start

### 1. Configurar variables de entorno

```bash
cd docker
cp .env.example .env
# Edita .env con tus credenciales
```

### 2. Elegir modo de despliegue

#### Opción A: Solo recursos (desarrollo local)

```bash
cd docker
docker compose -f docker-compose.resources.yml up -d
```

#### Opción B: Stack completo (todo en Docker)

```bash
cd docker
docker compose -f docker-compose.yml up -d
```

## 📂 Archivos importantes

- **`docker/docker-compose.resources.yml`** - Solo recursos
- **`docker/docker-compose.apps.yml`** - Solo aplicaciones
- **`docker/docker-compose.yml`** - Stack completo (incluye resources + apps)
- **`docker/.env.example`** - Variables de entorno

## 🔍 Puertos expuestos

| Servicio | Puerto | URL                       |
|----------|--------|---------------------------|
| Frontend | 80     | http://localhost     |
| API      | 8080   | http://localhost:8080     |
| Postgres | 5432   | localhost:5432            |
| RabbitMQ | 15672  | http://localhost:15672    |
| Redis    | 6379   | localhost:6379            |
