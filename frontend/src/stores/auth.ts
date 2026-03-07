import {defineStore} from 'pinia'
import {ref, computed} from 'vue'
import type {UserDetailsResponse} from '@/api'
import {useApiClient, useAuthenticatedApiClient} from "@/composables/useApiClient.ts";
import {ApiError} from '@/api/core/ApiError'

export const useAuthStore = defineStore('auth', () => {
        const bearerToken = ref<string | null>(null)
        const needsSignup = ref(false)
        const user = ref<UserDetailsResponse | null>(null)
        const waitingForVerificationCode = ref(false)
        const userName = ref<string | null>(null)
        const authError = ref<string | null>(null)

        const isAuthenticated = computed(() => bearerToken.value !== null)

        function waitForVerificationCode() {
            waitingForVerificationCode.value = true
            bearerToken.value = null
            needsSignup.value = false
            user.value = null
            authError.value = null
        }

        function verified(token: string) {
            bearerToken.value = token
            needsSignup.value = false
            waitingForVerificationCode.value = false
            authError.value = null
        }

        function resolveUserFriendlyAuthError(err: unknown, fallback: string): string {
            if (err instanceof ApiError && err.status === 429) {
                return 'Demasiados intentos. Espera un momento e intentalo de nuevo.'
            }

            if (err instanceof ApiError) {
                return `${fallback} (HTTP ${err.status})`
            }

            if (err instanceof Error) {
                return `${fallback} (${err.message})`
            }

            return fallback
        }

        async function getUser() {
            try {
                let userResponse = await useAuthenticatedApiClient().user.getUser();
                if (userResponse === undefined || userResponse === null) {
                    logout()
                    return
                }
                user.value = userResponse
                authError.value = null

            } catch (err) {
                console.error(err)
                authError.value = 'Tu sesion expiro. Vuelve a iniciar sesion.'
                logout()
            }
        }

        async function login() {
            try {
                if (userName.value == null || userName.value?.length === 0) return

                let result = await useApiClient().auth.postAuthLogin({
                    userName: userName.value,
                });

                if (result === undefined) {
                    needsSignup.value = true
                    return
                }

                waitForVerificationCode()
            } catch (err) {
                console.error(err)
                authError.value = resolveUserFriendlyAuthError(err, 'No pudimos enviar el codigo de verificacion')
                alert(authError.value)
            }
        }

        async function verify(userName: string, verificationCode: string): Promise<boolean> {
            try {
                if (verificationCode.length === 0) return false

                let result = await useApiClient().auth.postAuthVerify({
                    userName,
                    verificationCode,
                });

                if (result === undefined) {
                    return false;
                }

                verified(result)
                await getUser()
            } catch (err) {
                console.error(err)
                authError.value = resolveUserFriendlyAuthError(err, 'No pudimos verificar el codigo')
                alert(authError.value)
            }

            return bearerToken.value != null
        }


        function logout() {
            bearerToken.value = null
            needsSignup.value = false
            user.value = null
            waitingForVerificationCode.value = false
        }

        return {
            needsSignup,
            waitingForVerificationCode,
            user,
            userName,
            authError,
            bearerToken,
            isAuthenticated,
            getUser,
            logout,
            login,
            verify
        }
    },
    {
        persist: true,
    },
)
