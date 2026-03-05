// composables/useTelegram.ts
export function TelegramService() {
    const getBotHandle = () => {
        return import.meta.env.VITE_TELEGRAM_BOT || '@tu_bot'
    }

    const getBotUrl = () => {
        const handle = getBotHandle()
        return `https://t.me/${handle.replace('@', '')}`
    }

    return {
        getBotUrl,
        getBotHandle
    }
}
