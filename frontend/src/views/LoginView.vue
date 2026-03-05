<script setup lang="ts">
import {ref} from 'vue'
import {useRouter} from 'vue-router'
import {useAuthStore} from '@/stores/auth'
import {TelegramService} from '@/services/telegram'

// Dentro de setup:

const router = useRouter()
const authStore = useAuthStore()

const verificationCode = ref<'' | null>();
const verificationCodeError = ref(false)
const loading = ref(false)

async function onSubmitUsername() {
  loading.value = true;

  if (authStore.userName == null || authStore.userName.length == 0) {
    loading.value = false;
    return;
  }

  await authStore.login();
  loading.value = false;
}

async function onSubmitVerificationCode() {
  loading.value = true;

  if (verificationCode.value == null || verificationCode.value.length == 0 || authStore.userName == null) {
    loading.value = false;
    return;
  }
  let result = await authStore.verify(authStore.userName, verificationCode.value);

  if (!result) {
    loading.value = false;
    verificationCodeError.value = true;
    return;
  }


  if (authStore.isAuthenticated) {
    await router.push({name: 'home'});
  }

  loading.value = false;

}
</script>

<template>
  <div class="form-container">
    <h1 class="text-2xl font-bold text-center text-gray-900 mb-8">Iniciar sesión</h1>

    <form @submit.prevent="authStore.userName == null ? onSubmitUsername() : onSubmitUsername()" class="space-y-6">
      <div class="form-field">
        <label for="username" class="form-label">Usuario de telegram</label>
        <input
            id="username"
            type="text"
            v-model.trim="authStore.userName"
            :disabled="loading || authStore.waitingForVerificationCode"
            placeholder="tu_usuario"
            autocomplete="username"
            class="form-input"
            @keyup.enter.prevent="onSubmitUsername"
        />
        <button
            v-if="!authStore.waitingForVerificationCode"
            type="button"
            :disabled="authStore.userName == null || authStore.userName?.length === 0 || loading"
            @click="onSubmitUsername"
            class="btn-primary w-full">
          {{ loading ? 'Enviando…' : 'Siguiente' }}
        </button>
      </div>

      <p v-if="!authStore.isAuthenticated && authStore.needsSignup" class="text-hint text-center">
        ¿No tienes usuario? Crea tu usuario tocando iniciar en el bot de Telegram
        <a :href="TelegramService().getBotUrl()" target="_blank" rel="noopener" class="link-primary">{{
            TelegramService().getBotHandle()
          }}</a>.
      </p>

      <div class="form-field" v-if="authStore.waitingForVerificationCode">
        <label for="code" class="form-label">Código (Telegram)</label>
        <input
            id="code"
            type="text"
            ref="codeEl"
            v-model.trim="verificationCode"
            :disabled="loading"
            inputmode="numeric"
            autocomplete="one-time-code"
            placeholder="Introduce el código"
            class="form-input"
            @keyup.enter.prevent="onSubmitVerificationCode"
        />
        <button
            type="button"
            :disabled="loading || verificationCode == null || verificationCode.length === 0"
            @click="onSubmitVerificationCode"
            class="btn-primary w-full"
        >
          {{ loading ? 'Verificando…' : 'Verificar' }}
        </button>
      </div>

      <p v-if="verificationCodeError" class="text-error text-center">Error verificando el código. Asegúrate de que es correcto.</p>
    </form>
  </div>
</template>

<style scoped>
/* All styles are now handled by Tailwind CSS classes */
</style>
