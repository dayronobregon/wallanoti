# 🎮 Wallapop Notification Bot

## 📋 Descripción

¿Eres fan de las consolas retro de Nintendo y te frustras buscando una **Nintendo 64** en Wallapop?
Seguro que te ha pasado esto:

💬 "Hay 45 nuevos artículos que coinciden con tu búsqueda"
👉 Corres a mirar… ¡y ya están reservados! 🤯

El problema es que **Wallapop no te notifica en tiempo real**. Así que, **manos a la obra**.

Puedes probarlo con el bot de Telegram [**@WallapopNotificationBot**](https://t.me/wallapop_notification_bot)

---

## 💡 **¿Qué es Wallapop Notification Bot?**

Este es un **bot de Telegram** que te envía **notificaciones en tiempo real** cuando aparecen nuevos artículos en
Wallapop que coinciden con los términos de búsqueda que configures. 🎯

Olvídate de revisar manualmente cada 5 minutos. **El bot lo hace por ti y te avisa al instante.**

---

## 🧑‍💻 **Tecnologías utilizadas**

Este proyecto está desarrollado con **.NET 8** y sigue principios de:

- **Clean Architecture (Arquitectura Hexagonal)**
- **Domain-Driven Design (DDD)**
- **CQRS (Command Query Responsibility Segregation)**
- **Event-Driven Architecture**

### Stack Tecnológico:
- **.NET 8** con ASP.NET Core
- **Entity Framework Core** con PostgreSQL
- **RabbitMQ** para Event Bus asíncrono
- **Redis** para caché distribuido
- **SignalR** para notificaciones web en tiempo real
- **Telegram.Bot** para integración con Telegram
- **MediatR** para implementar CQRS
- **Coravel** para tareas programadas

### 📐 Arquitectura
El sistema está organizado en **Bounded Contexts** siguiendo DDD:
- **Alerts**: Gestión de alertas de búsqueda
- **Notifications**: Creación y envío de notificaciones
- **Users**: Gestión de usuarios y autenticación
- **AlertCounter**: Estadísticas y contadores

**📚 [Ver documentación completa de arquitectura](docs/ARCHITECTURE.md)**  
**🎨 [Ver diagramas C4 completos (Mermaid)](docs/c4-diagrams/complete-c4-diagram.md)** ⭐ NUEVO  
**📁 [Más diagramas (PlantUML, Draw.io)](docs/c4-diagrams/)**

---

## 📦 **Puesta en marcha**

### Requisitos previos
- .NET 8 SDK
- Docker y Docker Compose
- PostgreSQL (o usar Docker)
- Redis (o usar Docker)
- RabbitMQ (o usar Docker)

### 1. **Clona el repositorio:**
   ```bash
   git clone https://github.com/dayronobregon/wallanoti.git
   cd wallanoti/backend
   ```

### 2. **Configura las variables de entorno:**
Crea un archivo `.env` o configura en `appsettings.json`:

```bash
# Telegram
TelegramBotToken=your_telegram_bot_token

# Database
ConnectionStrings__Postgres=Host=localhost;Database=wallanoti;Username=postgres;Password=yourpassword

# Cache
ConnectionStrings__Redis=localhost:6379

# Message Broker
RabbitMq__HostName=localhost
RabbitMq__Port=5672
RabbitMq__UserName=guest
RabbitMq__Password=guest
RabbitMq__VirtualHost=/

# JWT
Jwt__Secret=your_secret_key_here
Jwt__Issuer=Wallanoti
Jwt__Audience=WallanotiUsers
```

### 3. **Ejecuta con Docker Compose:**
   ```bash
   docker-compose -f compose.yaml up -d
   ```

### 4. **O ejecuta localmente:**
   ```bash
   # Restaurar dependencias
   dotnet restore
   
   # Ejecutar migraciones
   dotnet ef database update --project WallapopNotification
   
   # Ejecutar API
   dotnet run --project Apps/Api
   ```

---

## 📚 **Cómo funciona**

1. Configuras los **términos de búsqueda** que quieres monitorear en Wallapop.
2. El bot se conecta a Wallapop y busca nuevos artículos en tiempo real.
3. Cuando aparece un nuevo artículo, el bot te envía una **notificación por Telegram**. 📩

---

## 🤝 **Contribuciones**

¡Las contribuciones son **bienvenidas**! 🎉
Si tienes alguna idea o mejora, **abre un PR** o **crea un issue**.

---

## 📧 **Contacto**

Si tienes dudas o sugerencias, ¡no dudes en escribirme!

👤 **Autor:** Dayron Rey Obregón Colina

📧 **Email:** wallanoti@gmail.com

---

¡Gracias por pasarte por aquí y feliz caza de reliquias retro! 🎮

