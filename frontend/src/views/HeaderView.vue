<template>
  <!-- Simple transparent navbar with user info and logout -->
  <nav class="fixed top-0 left-0 right-0 z-50">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex items-center justify-between h-16">
        <!-- Logo or Brand -->
        <div class="flex items-center space-x-2">
          <span class="text-lg font-semibold text-gray-800">WallaNoti</span>
        </div>

        <!-- User info and logout -->
        <div class="flex items-center space-x-4">
          <span class="text-sm text-gray-700 font-medium">
            {{ authStore.user?.userName }}
          </span>
          <button
              @click="handleLogout"
              class="flex items-center space-x-2 px-4 py-2 text-sm text-gray-700 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
              title="Cerrar sesión"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                    d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
            </svg>
            <span>Cerrar sesión</span>
          </button>
        </div>
      </div>
    </div>
  </nav>

</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>

