<template>
  <div class="min-h-screen bg-indigo-50 overflow-x-hidden">
    <!-- Header Navbar -->
    <HeaderView />

    <!-- Main content - Centered Alert Management -->
    <div ref="alertManagementSection" class="min-h-screen flex flex-col items-center justify-center px-4 sm:px-6 lg:px-8 pt-20 max-w-full">
      <div class="w-full max-w-4xl space-y-8">
        <!-- AI Input - Central Focus -->
        <div class="space-y-3">
          <div class="relative">
            <div class="absolute inset-y-0 left-0 flex items-center pl-6 pointer-events-none">
              <span class="text-4xl">🤖</span>
            </div>
            <input
                v-model="alertDescription"
                @keyup.enter="createAlertWithAI"
                @focus="stopTyping"
                @blur="startTyping"
                type="text"
                :placeholder="currentPlaceholder"
                class="w-full pl-20 pr-6 py-6 text-lg border-2 border-gray-300/50 bg-white/80 backdrop-blur-sm rounded-2xl focus:ring-4 focus:ring-indigo-500/30 focus:border-indigo-500 placeholder-gray-400 shadow-xl transition-all duration-300"
            />
          </div>
        </div>

        <!-- Alert Badges Section -->
        <AlertBadges
            :alerts="alerts"
            :loading-alerts="loadingAlerts"
            @alert-created="fetchAlerts"
            @alert-deleted="handleAlertDeleted"
            @alert-updated="fetchAlerts"
        />
      </div>

      <!-- Click to view notifications button -->
      <button
          v-if="!notificationsLoaded"
          @click="loadNotifications"
          class="fixed bottom-8 left-1/2 -translate-x-1/2 flex flex-col items-center space-y-3 z-40 transition-transform duration-300 hover:scale-110 cursor-pointer group"
      >
        <span class="text-gray-600 font-medium text-lg group-hover:text-indigo-600 transition-colors">
          Ver las notificaciones
        </span>
        <svg class="w-8 h-8 text-indigo-500 group-hover:text-indigo-600 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 14l-7 7m0 0l-7-7m7 7V3"/>
        </svg>
      </button>
    </div>

    <!-- Notifications Section - Loaded on scroll -->
    <div
        ref="notificationsSection"
        v-if="notificationsLoaded"
        class="fixed inset-0 top-16 bg-gradient-to-b from-indigo-50 to-indigo-100 flex flex-col"
    >
      <!-- Fixed Controls Header -->
      <div class="sticky top-0 z-30 bg-indigo-50/95 backdrop-blur-sm border-b border-indigo-100 py-4 px-4 sm:px-6 lg:px-8">
        <div class="max-w-7xl mx-auto">
          <!-- Back to Alert Management Button -->
          <div class="flex justify-center mb-4">
            <button
                @click="scrollToAlertManagement"
                class="flex items-center space-x-2 text-gray-600 hover:text-indigo-600 transition-colors group"
            >
              <svg class="w-6 h-6 transform transition-transform group-hover:-translate-y-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 10l-7 7m0 0l7 7m-7-7v18"/>
              </svg>
              <span class="font-medium">Ir arriba para gestionar las alertas</span>
            </button>
          </div>

          <!-- Sort controls -->
          <div v-if="notifications.length > 0" class="flex justify-center">
            <div class="bg-white/70 backdrop-blur-sm rounded-full shadow-lg p-1 flex items-center gap-1">
              <button
                  @click="sortOrder = 'desc'"
                  :class="[
                    'px-4 py-2 rounded-full text-sm font-medium transition-all duration-200 flex items-center gap-2',
                    sortOrder === 'desc'
                      ? 'bg-indigo-500 text-white shadow-md'
                      : 'text-gray-600 hover:text-indigo-500'
                  ]"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
                </svg>
                Más recientes
              </button>
              <button
                  @click="sortOrder = 'asc'"
                  :class="[
                    'px-4 py-2 rounded-full text-sm font-medium transition-all duration-200 flex items-center gap-2',
                    sortOrder === 'asc'
                      ? 'bg-indigo-500 text-white shadow-md'
                      : 'text-gray-600 hover:text-indigo-500'
                  ]"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7"/>
                </svg>
                Más antiguos
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Scrollable Notifications Content -->
      <div class="flex-1 overflow-y-auto px-4 sm:px-6 lg:px-8 py-8">
        <div class="max-w-7xl mx-auto">
          <NotificationGrid
              :notifications="sortedNotifications"
              :loading="loading"
              :new-notification-ids="newNotificationIds"
              :hide-sort-controls="true"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useAuthenticatedApiClient } from '@/composables/useApiClient'
import type { NotificationResponse, GetAlertsByUserIdResponse } from "@/api"
import HeaderView from './HeaderView.vue'
import NotificationGrid from '@/components/NotificationGrid.vue'
import AlertBadges from '@/components/AlertBadges.vue'
import * as signalR from "@microsoft/signalr"
import { useAuthStore } from "@/stores/auth.ts"

const apiClient = useAuthenticatedApiClient()

const notifications = ref<NotificationResponse[]>([])
const alerts = ref<GetAlertsByUserIdResponse[]>([])
const loading = ref(false)
const loadingAlerts = ref(false)
const newNotificationIds = ref<Set<string>>(new Set())
const notificationsLoaded = ref(false)
const alertDescription = ref('')
const currentPlaceholder = ref('')
const isTyping = ref(true)
const notificationsSection = ref<HTMLElement | null>(null)
const alertManagementSection = ref<HTMLElement | null>(null)
const sortOrder = ref<'asc' | 'desc'>('desc')
const configuredSignalRBaseUrl = import.meta.env.VITE_SIGNALR_URL
const configuredApiBaseUrl = import.meta.env.VITE_API_BASE_URL

const signalRBaseUrl = (() => {
  if (configuredSignalRBaseUrl) {
    return configuredSignalRBaseUrl
  }

  if (configuredApiBaseUrl) {
    try {
      return new URL(configuredApiBaseUrl, window.location.origin).origin
    } catch {
      console.warn('[SignalR] Invalid VITE_API_BASE_URL. Falling back to window.location.origin.')
    }
  } else {
    console.warn('[SignalR] Neither VITE_SIGNALR_URL nor VITE_API_BASE_URL is configured. Falling back to window.location.origin.')
  }

  return window.location.origin
})()

let signalRConnection: signalR.HubConnection | null = null

// Computed property for sorted notifications
const sortedNotifications = computed(() => {
  const sorted = [...notifications.value]
  if (sortOrder.value === 'desc') {
    return sorted.sort((a, b) => {
      const dateA = a.createdAt ? new Date(a.createdAt).getTime() : 0
      const dateB = b.createdAt ? new Date(b.createdAt).getTime() : 0
      return dateB - dateA
    })
  } else {
    return sorted.sort((a, b) => {
      const dateA = a.createdAt ? new Date(a.createdAt).getTime() : 0
      const dateB = b.createdAt ? new Date(b.createdAt).getTime() : 0
      return dateA - dateB
    })
  }
})

// AI Input placeholder animation
const examples = [
  '¿Qué quieres buscar?',
  'Monitor gaming 27 pulgadas menos de 300€',
  'iPhone 15 Pro reacondicionado',
  'Portátil para programar con 16GB RAM'
]

let currentExampleIndex = 0
let currentCharIndex = 0
let typingInterval: number | null = null
let isDeleting = false

function typeText() {
  if (!isTyping.value) return

  const currentText = examples[currentExampleIndex]
  if (!currentText) return

  if (!isDeleting) {
    if (currentCharIndex < currentText.length) {
      currentPlaceholder.value = currentText.substring(0, currentCharIndex + 1)
      currentCharIndex++
      typingInterval = window.setTimeout(typeText, 80)
    } else {
      typingInterval = window.setTimeout(() => {
        isDeleting = true
        typeText()
      }, 2000)
    }
  } else {
    if (currentCharIndex > 0) {
      currentCharIndex--
      currentPlaceholder.value = currentText.substring(0, currentCharIndex)
      typingInterval = window.setTimeout(typeText, 30)
    } else {
      isDeleting = false
      currentExampleIndex = (currentExampleIndex + 1) % examples.length
      typingInterval = window.setTimeout(typeText, 200)
    }
  }
}

function stopTyping() {
  isTyping.value = false
  if (typingInterval) {
    clearTimeout(typingInterval)
    typingInterval = null
  }
}

function startTyping() {
  if (!alertDescription.value) {
    isTyping.value = true
    typeText()
  }
}

function handleAlertDeleted(alertId: string) {
  alerts.value = alerts.value.filter(a => a.id !== alertId)
}

async function fetchNotifications() {
  if (notificationsLoaded.value) return // Don't fetch again

  try {
    loading.value = true
    notifications.value = await apiClient.notification.getNotification()
    notificationsLoaded.value = true
  } catch (error) {
    console.error('Error al obtener notificaciones:', error)
  } finally {
    loading.value = false
  }
}

async function fetchAlerts() {
  try {
    loadingAlerts.value = true
    alerts.value = await apiClient.alert.getAlert()
  } catch (error) {
    console.error('Error al obtener alertas:', error)
  } finally {
    loadingAlerts.value = false
  }
}

async function createAlertWithAI() {
  if (!alertDescription.value.trim()) return

  // TODO: Implement AI alert creation
  console.log('Crear alerta con IA:', alertDescription.value)
  alertDescription.value = ''
}

function loadNotifications() {
  console.log('🔔 Loading notifications...')
  fetchNotifications()
  startSignalR()

  // Scroll to notifications section after a short delay to ensure DOM is updated
  setTimeout(() => {
    notificationsSection.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }, 100)
}

function scrollToAlertManagement() {
  alertManagementSection.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

function startSignalR() {
  if (signalRConnection) return // Already connected

  const authStore = useAuthStore()

  const accessTokenFactory = async () => {
    let token = authStore.bearerToken
    console.log("Access Token Factory called, token:", token)
    return token || ''
  }

  const hubUrl = `${signalRBaseUrl.replace(/\/$/, '')}/hub/notifications`

  signalRConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {accessTokenFactory})
      .configureLogging(signalR.LogLevel.Information)
      .withAutomaticReconnect()
      .build()

  async function start() {
    try {
      console.log("Starting SignalR connection...")
      await signalRConnection!.start()
      console.log("SignalR Connected.")
    } catch (err) {
      console.log(err)
      setTimeout(start, 5000)
    }
  }

  signalRConnection.onclose(async () => {
    await start()
  })

  signalRConnection.on("ReceiveNotification", (notification: any) => {
    console.log("SignalR: Received notification:", notification)
    notifications.value.unshift(notification)
    if (notification.id) {
      newNotificationIds.value.add(notification.id)
      setTimeout(() => {
        newNotificationIds.value.delete(notification.id)
      }, 10000)
    }
  })

  start()
}

onMounted(() => {
  const authStore = useAuthStore()

  // Only fetch alerts on mount, not notifications
  fetchAlerts()
  authStore.getUser()

  // Start typing animation
  typeText()
})

onUnmounted(() => {
  if (typingInterval) {
    clearTimeout(typingInterval)
  }

  // Disconnect SignalR
  if (signalRConnection) {
    signalRConnection.stop()
  }
})
</script>
