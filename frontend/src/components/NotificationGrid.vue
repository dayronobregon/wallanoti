<template>
  <section class="pt-4">
    <!-- Loading state -->
    <div v-if="loading" class="bg-white/60 backdrop-blur-sm rounded-2xl shadow-lg p-10">
      <div class="flex items-center justify-center space-x-3">
        <div class="w-6 h-6 border-3 border-indigo-500 border-t-transparent rounded-full animate-spin"></div>
        <p class="text-gray-700 font-medium">Cargando notificaciones...</p>
      </div>
    </div>

    <!-- Sort controls -->
    <div v-else-if="!hideSortControls && notifications.length > 0" class="mb-6 flex justify-center">
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

    <!-- Notifications grid -->
    <div v-if="!loading && notifications.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div
          v-for="notification in sortedNotifications"
          :key="notification.id"
          class="group bg-white/60 backdrop-blur-sm border border-white/80 rounded-xl shadow-md p-6 hover:shadow-xl transition-all duration-300 cursor-pointer relative overflow-hidden"
          :class="notification.id && newNotificationIds.has(notification.id) ? 'animate-fade-highlight' : ''"
      >
        <!-- Highlight overlay for new notifications -->
        <div
            v-if="notification.id && newNotificationIds.has(notification.id)"
            class="absolute inset-0 bg-gradient-to-br from-yellow-200/80 to-orange-200/80 animate-fade-out pointer-events-none"
        ></div>

        <!-- Notification content -->
        <div class="relative z-10">
          <h3 class="text-lg font-semibold text-gray-800 mb-3">{{ notification.title }}</h3>

          <div class="relative mb-3">
            <div
                @click="copyToClipboard(notification.url||'')"
                class="px-3 py-2 bg-gray-100/70 rounded-lg border border-gray-200/50 overflow-x-auto cursor-pointer hover:bg-gray-200/70 transition-colors group/copy"
                title="Click para copiar"
            >
              <p class="text-gray-600 text-sm whitespace-nowrap">{{ notification.url || 'Sin URL' }}</p>
              <!-- Copy indicator -->
              <div class="absolute top-1/2 right-2 -translate-y-1/2 opacity-0 group-hover/copy:opacity-100 transition-opacity">
                <svg class="w-4 h-4 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
                </svg>
              </div>
            </div>
          </div>

          <!-- Images gallery - Horizontal scroll at the bottom -->
          <div v-if="notification.images && notification.images.length > 0">
            <div class="overflow-x-auto flex gap-2 pb-2 scrollbar-thin-horizontal scrollbar-thumb-gray-300 scrollbar-track-gray-100">
              <img
                  v-for="(image, index) in notification.images"
                  :key="index"
                  :src="image"
                  @click="notification.id && openImageZoom(notification.id, index)"
                  class="h-24 w-auto flex-shrink-0 object-cover rounded-lg cursor-pointer hover:ring-2 hover:ring-indigo-400 transition-all"
                  :alt="`Imagen ${index + 1}`"
              />
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- No notifications state -->
    <div v-else class="bg-white/60 backdrop-blur-sm rounded-2xl shadow-lg p-12 text-center">
      <svg class="w-16 h-16 mx-auto text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/>
      </svg>
      <p class="text-gray-600 text-lg">No tienes notificaciones</p>
    </div>

    <!-- Image Zoom Modal -->
    <Teleport to="body">
      <div
          v-if="galleryOpen"
          @click="closeGallery"
          class="fixed inset-0 z-50 bg-black/60 flex items-center justify-center p-4"
      >
        <!-- Zoom container -->
        <div
            @click.stop
            class="bg-white rounded-lg shadow-2xl overflow-hidden max-w-[90vw] max-h-[90vh]"
        >
          <!-- Close button -->
          <button
              @click="closeGallery"
              class="absolute top-2 right-2 bg-black/50 text-white hover:bg-black/70 rounded-full p-1.5 transition-colors z-10"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>

          <!-- Previous button -->
          <button
              v-if="galleryImages.length > 1"
              @click.stop="previousImage"
              class="absolute left-2 top-1/2 -translate-y-1/2 bg-black/50 text-white hover:bg-black/70 rounded-full p-2 transition-colors"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
            </svg>
          </button>

          <!-- Image -->
          <img
              :src="galleryImages[currentImageIndex]"
              class="max-w-full max-h-[60vh] object-contain"
              :alt="`Imagen ${currentImageIndex + 1}`"
          />

          <!-- Next button -->
          <button
              v-if="galleryImages.length > 1"
              @click.stop="nextImage"
              class="absolute right-2 top-1/2 -translate-y-1/2 bg-black/50 text-white hover:bg-black/70 rounded-full p-2 transition-colors"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
            </svg>
          </button>

          <!-- Image counter -->
          <div class="absolute bottom-2 left-1/2 -translate-x-1/2 bg-black/70 text-white px-3 py-1 rounded-full text-sm">
            {{ currentImageIndex + 1 }} / {{ galleryImages.length }}
          </div>
        </div>
      </div>
    </Teleport>
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import type { NotificationResponse } from '@/api'

interface Props {
  notifications: NotificationResponse[]
  loading: boolean
  newNotificationIds: Set<string>
  hideSortControls?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  hideSortControls: false
})

// Sort state
const sortOrder = ref<'asc' | 'desc'>('desc')

// Computed sorted notifications
const sortedNotifications = computed(() => {
  const sorted = [...props.notifications]

  sorted.sort((a, b) => {
    const dateA = a.createdAt ? new Date(a.createdAt).getTime() : 0
    const dateB = b.createdAt ? new Date(b.createdAt).getTime() : 0

    if (sortOrder.value === 'desc') {
      return dateB - dateA // Más recientes primero
    } else {
      return dateA - dateB // Más antiguos primero
    }
  })

  return sorted
})

// Gallery state
const galleryOpen = ref(false)
const galleryImages = ref<string[]>([])
const currentImageIndex = ref(0)

async function copyToClipboard(text: string | undefined) {
  if (!text) return

  try {
    await navigator.clipboard.writeText(text)
    console.log('URL copiada al portapapeles')
  } catch (error) {
    console.error('Error al copiar:', error)
  }
}

// Gallery functions
function openImageZoom(notificationId: string, imageIndex: number) {
  const notification = props.notifications.find(n => n.id === notificationId)
  if (!notification || !notification.images) return

  galleryImages.value = notification.images || []
  currentImageIndex.value = imageIndex
  galleryOpen.value = true
}

function closeGallery() {
  galleryOpen.value = false
  galleryImages.value = []
  currentImageIndex.value = 0
}

function nextImage() {
  currentImageIndex.value = (currentImageIndex.value + 1) % galleryImages.value.length
}

function previousImage() {
  currentImageIndex.value = currentImageIndex.value === 0
      ? galleryImages.value.length - 1
      : currentImageIndex.value - 1
}

// Keyboard navigation for gallery
function handleKeydown(event: KeyboardEvent) {
  if (!galleryOpen.value) return

  if (event.key === 'ArrowRight') {
    nextImage()
  } else if (event.key === 'ArrowLeft') {
    previousImage()
  } else if (event.key === 'Escape') {
    closeGallery()
  }
}

onMounted(() => {
  document.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleKeydown)
})
</script>

<style scoped>
@keyframes fade-out {
  0% {
    opacity: 0.8;
  }
  100% {
    opacity: 0;
  }
}

.animate-fade-out {
  animation: fade-out 10s ease-out forwards;
}

/* Custom scrollbar - Horizontal */
.scrollbar-thin-horizontal::-webkit-scrollbar {
  height: 6px;
}

.scrollbar-thin-horizontal::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.scrollbar-thin-horizontal::-webkit-scrollbar-thumb {
  background: #d1d5db;
  border-radius: 3px;
}

.scrollbar-thin-horizontal::-webkit-scrollbar-thumb:hover {
  background: #9ca3af;
}
</style>

