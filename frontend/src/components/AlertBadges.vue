<template>
  <section class="space-y-4">

    <div class="flex flex-wrap gap-4">
      <!-- Loading alerts -->
      <div v-if="loadingAlerts" class="flex items-center space-x-2 px-5 py-3 bg-white/60 backdrop-blur-sm rounded-full shadow-sm">
        <div class="w-4 h-4 border-2 border-indigo-500 border-t-transparent rounded-full animate-spin"></div>
        <span class="text-base text-gray-600">Cargando alertas...</span>
      </div>

      <!-- Alert badges with menu -->
      <div
          v-for="alert in alerts"
          :key="alert.id"
          class="relative group"
      >
        <!-- Notification counter badge -->
        <div
            v-if="alertCounters[alert.id || ''] !== undefined"
            class="absolute -top-2 -left-2 w-7 h-7 bg-red-500 text-white rounded-full flex items-center justify-center text-xs font-bold shadow-lg z-10 border-2 border-white"
        >
          {{ alertCounters[alert.id || ''] }}
        </div>

        <!-- Loading counter indicator -->
        <div
            v-else-if="loadingCounters[alert.id || '']"
            class="absolute -top-2 -left-2 w-7 h-7 bg-gray-400 text-white rounded-full flex items-center justify-center shadow-lg z-10 border-2 border-white"
        >
          <div class="w-3 h-3 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
        </div>

        <div
            :class="[
              'px-6 py-3 rounded-full font-medium text-base transition-all duration-200 shadow-sm flex items-center gap-2',
              isAlertActive(alert)
                ? 'bg-indigo-500 text-white'
                : 'bg-white/60 backdrop-blur-sm text-gray-700 opacity-60'
            ]"
        >
          <span :class="!isAlertActive(alert) ? 'line-through' : ''">
            {{ alert.name || 'Sin nombre' }}
          </span>
          <span v-if="!isAlertActive(alert)" class="text-xs opacity-75">
            (Desactivada)
          </span>
        </div>

        <!-- Three dots menu button -->
        <button
            @click.stop="toggleMenu(alert.id || '')"
            :class="[
              'absolute -top-1 -right-1 w-6 h-6 rounded-full flex items-center justify-center transition-all duration-200 shadow-sm',
              isAlertActive(alert)
                ? 'bg-indigo-600 text-white hover:bg-indigo-700'
                : 'bg-white text-gray-600 hover:bg-gray-100'
            ]"
        >
          <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 16 16">
            <circle cx="2" cy="8" r="1.5"/>
            <circle cx="8" cy="8" r="1.5"/>
            <circle cx="14" cy="8" r="1.5"/>
          </svg>
        </button>

        <!-- Dropdown menu -->
        <div
            v-if="openMenuId === alert.id"
            class="absolute top-8 right-0 mt-1 w-40 bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-10"
        >
          <!-- Activar/Desactivar button -->
          <button
              v-if="!isAlertActive(alert)"
              @click.stop="handleActivateAlert(alert.id || '')"
              class="w-full px-4 py-2 text-left text-sm text-green-600 hover:bg-green-50 flex items-center gap-2"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
            Activar
          </button>
          <button
              v-else
              @click.stop="handleDeactivateAlert(alert.id || '')"
              class="w-full px-4 py-2 text-left text-sm text-gray-700 hover:bg-gray-100 flex items-center gap-2"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636"/>
            </svg>
            Desactivar
          </button>
          <button
              @click.stop="handleDeleteAlert(alert.id || '')"
              class="w-full px-4 py-2 text-left text-sm text-red-600 hover:bg-red-50 flex items-center gap-2"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
            </svg>
            Eliminar
          </button>
        </div>
      </div>


      <!-- Add manually button -->
      <button
          v-if="!loadingAlerts"
          @click="showCreateAlertModal = true"
          class="px-6 py-3 rounded-full font-medium text-base transition-all duration-200 shadow-sm hover:shadow-md flex items-center gap-2 bg-white/60 backdrop-blur-sm text-indigo-600 hover:bg-indigo-50/80 border-2 border-dashed border-indigo-300 hover:border-indigo-400"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
        </svg>
        <span>Agregar manualmente</span>
      </button>

      <!-- No alerts state -->
      <div v-if="!loadingAlerts && alerts.length === 0" class="text-base text-gray-500 px-5 py-3">
        No tienes alertas creadas
      </div>
    </div>

    <!-- Create Alert Modal -->
    <div
        v-if="showCreateAlertModal"
        class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 p-4"
        @click.self="showCreateAlertModal = false"
    >
      <div class="bg-white rounded-2xl shadow-2xl max-w-md w-full p-6 space-y-4">
        <div class="flex items-center justify-between">
          <h3 class="text-xl font-semibold text-gray-900">Crear alerta manualmente</h3>
          <button
              @click="showCreateAlertModal = false"
              class="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>

        <form @submit.prevent="handleCreateAlert" class="space-y-4">
          <div>
            <label for="alertName" class="block text-sm font-medium text-gray-700 mb-1">
              Nombre de la alerta
            </label>
            <input
                id="alertName"
                v-model="newAlertName"
                type="text"
                required
                placeholder="Ej: Nuevo producto en stock"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
            />
          </div>

          <div>
            <label for="alertUrl" class="block text-sm font-medium text-gray-700 mb-1">
              URL a monitorear
            </label>
            <input
                id="alertUrl"
                v-model="newAlertUrl"
                type="url"
                required
                placeholder="https://ejemplo.com/pagina"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
            />
          </div>

          <div class="flex gap-3 pt-2">
            <button
                type="button"
                @click="showCreateAlertModal = false"
                class="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-gray-700 font-medium hover:bg-gray-50 transition-colors"
            >
              Cancelar
            </button>
            <button
                type="submit"
                :disabled="creatingAlert"
                class="flex-1 px-4 py-2 bg-indigo-500 text-white rounded-lg font-medium hover:bg-indigo-600 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            >
              <span v-if="creatingAlert" class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin block"></span>
              <span>{{ creatingAlert ? 'Creando...' : 'Crear alerta' }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { useAuthenticatedApiClient } from '@/composables/useApiClient'
import type { GetAlertsByUserIdResponse } from '@/api'

interface Props {
  alerts: GetAlertsByUserIdResponse[]
  loadingAlerts: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  alertDeleted: [alertId: string]
  alertUpdated: []
}>()

const apiClient = useAuthenticatedApiClient()

const openMenuId = ref<string | null>(null)
const showCreateAlertModal = ref(false)
const newAlertName = ref('')
const newAlertUrl = ref('')
const creatingAlert = ref(false)

// Alert counters
const alertCounters = ref<Record<string, number>>({})
const loadingCounters = ref<Record<string, boolean>>({})

// Watch for changes in alerts to load counters
watch(() => props.alerts, async (newAlerts) => {
  if (!props.loadingAlerts && newAlerts.length > 0) {
    // Load counters for each alert
    for (const alert of newAlerts) {
      if (alert.id && alertCounters.value[alert.id] === undefined) {
        await loadAlertCounter(alert.id)
      }
    }
  }
}, { immediate: true })

async function loadAlertCounter(alertId: string) {
  if (loadingCounters.value[alertId]) return // Already loading

  try {
    loadingCounters.value[alertId] = true
    const counter = await apiClient.alert.getAlertCounter(alertId)
    alertCounters.value[alertId] = counter.total || 0
  } catch (error) {
    console.error(`Error al cargar contador para alerta ${alertId}:`, error)
    alertCounters.value[alertId] = 0
  } finally {
    loadingCounters.value[alertId] = false
  }
}

function isAlertActive(alert: GetAlertsByUserIdResponse): boolean {
  return alert.isActive !== false // Por defecto true si no está definido
}


async function handleCreateAlert() {
  if (!newAlertName.value.trim() || !newAlertUrl.value.trim()) {
    return
  }

  try {
    creatingAlert.value = true
    await apiClient.alert.postAlert(newAlertName.value.trim(), newAlertUrl.value.trim())

    // Limpiar formulario y cerrar modal
    newAlertName.value = ''
    newAlertUrl.value = ''
    showCreateAlertModal.value = false

    // Emitir evento para refrescar la lista de alertas
    emit('alertUpdated')

    console.log('Alerta creada correctamente')
  } catch (error) {
    console.error('Error al crear la alerta:', error)
    alert('Error al crear la alerta. Por favor, intenta de nuevo.')
  } finally {
    creatingAlert.value = false
  }
}

function toggleMenu(alertId: string) {
  openMenuId.value = openMenuId.value === alertId ? null : alertId
}

async function handleDeleteAlert(alertId: string) {
  if (!confirm('¿Estás seguro de que quieres eliminar esta alerta?')) {
    return
  }

  try {
    await apiClient.alert.deleteAlert(alertId)
    openMenuId.value = null
    emit('alertDeleted', alertId)
    console.log('Alerta eliminada correctamente')
  } catch (error) {
    console.error('Error al eliminar la alerta:', error)
    alert('Error al eliminar la alerta')
  }
}

async function handleDeactivateAlert(alertId: string) {
  if (!confirm('¿Estás seguro de que quieres desactivar esta alerta?')) {
    return
  }

  try {
    await apiClient.alert.patchAlertDeactivate(alertId)
    openMenuId.value = null
    // Emitir evento para refrescar la lista de alertas
    emit('alertUpdated')
    console.log('Alerta desactivada correctamente')
  } catch (error) {
    console.error('Error al desactivar la alerta:', error)
    alert('Error al desactivar la alerta')
    openMenuId.value = null
  }
}

async function handleActivateAlert(alertId: string) {
  // Verificar si ya hay una alerta activa
  const activeAlerts = props.alerts.filter(a => a.isActive && a.id !== alertId)

  if (activeAlerts.length > 0 && activeAlerts[0]) {
    const activeAlertName = activeAlerts[0]?.name || 'Sin nombre'
    const message = `Solo puedes tener una alerta activa a la vez.\n\nLa alerta "${activeAlertName}" está activa actualmente.\n\nAl activar esta alerta, la otra se desactivará automáticamente.\n\n¿Deseas continuar?`

    if (!confirm(message)) {
      openMenuId.value = null
      return
    }
  }

  try {
    await apiClient.alert.patchAlertActivate(alertId)
    openMenuId.value = null
    // Emitir evento para refrescar la lista de alertas
    emit('alertUpdated')
    console.log('Alerta activada correctamente')
  } catch (error) {
    console.error('Error al activar la alerta:', error)
    alert('Error al activar la alerta')
    openMenuId.value = null
  }
}

// Close menu when clicking outside
function handleClickOutside(event: MouseEvent) {
  const target = event.target as HTMLElement
  if (!target.closest('.relative.group')) {
    openMenuId.value = null
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

