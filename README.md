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

Este proyecto está desarrollado con **.NET** y sigue principios de:

- **Arquitectura Hexagonal**
- **Domain-Driven Design (DDD)**

Para la gestión de eventos de dominio, he utilizado **Azure Service Bus** como **Event Bus asíncrono** (aunque aún estoy
evaluando si es lo más adecuado 🤔).

---

## 📦 **Puesta en marcha**

1. **Clona el repositorio:**
   ```bash
   git clone https://github.com/dayronobregon/wallanoti.git
   ```

2. **Configura las variables de entorno:**
    - `TelegramBotToken` → Tu clave de tu bot de telegram.
    - `ServiceBusConnection` → La cadena de conexión a tu servicio de Azure Service Bus.
    - `MySqlConnection` → La cadena de conexión a tu base de datos MySQL.

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

