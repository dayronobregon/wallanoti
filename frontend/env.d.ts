/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string
  readonly VITE_SIGNALR_URL?: string
  // Optional endpoint templates; supported placeholders: {username}, {code}
  readonly VITE_API_LOGIN_PATH?: string
  readonly VITE_API_VERIFY_PATH?: string
  // Optional Telegram bot handle (with or without leading @)
  readonly VITE_TELEGRAM_BOT?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
