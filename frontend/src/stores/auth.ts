import {ref, computed} from 'vue'
import {defineStore} from 'pinia'
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
                const userResponse = await useAuthenticatedApiClient().user.getUser();
                if (userResponse === undefined || userResponse === null) {
                    logout()
                    return
                }
                user.value = userResponse
                authError.value = null

            } catch (err) {
                console.error(err)
                if (err instanceof ApiError && [401, 403, 404].includes(err.status)) {
                    logout()
                    authError.value = 'Tu sesion expiro. Vuelve a iniciar sesion.'
                    return
                }

                authError.value = 'No pudimos cargar tu sesion. Intenta de nuevo.'
            }
        }

        async function login() {
            try {
                if (userName.value == null || userName.value?.length === 0) return

                const result = await useApiClient().auth.postAuthLogin({
                    userName: userName.value,
                }) as string | undefined;

                if (result === undefined) {
                    logout()
                    needsSignup.value = true
                    authError.value = null
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

                const result = await useApiClient().auth.postAuthVerify({
                    userName,
                    verificationCode,
                }) as string | undefined;

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
            authError.value = null
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
        persist: {
            omit: ['authError'],
        },
    },
)
