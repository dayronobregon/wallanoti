import {defineStore} from 'pinia'
import {ref, computed} from 'vue'
import type {UserDetailsResponse} from '@/api'
import {useApiClient, useAuthenticatedApiClient} from "@/composables/useApiClient.ts";

export const useAuthStore = defineStore('auth', () => {
        const bearerToken = ref<string | null>(null)
        const needsSignup = ref(false)
        const user = ref<UserDetailsResponse | null>(null)
        const waitingForVerificationCode = ref(false)
        const userName = ref<string | null>(null)

        const isAuthenticated = computed(() => bearerToken.value !== null)

        function waitForVerificationCode() {
            waitingForVerificationCode.value = true
            bearerToken.value = null
            needsSignup.value = false
            user.value = null
        }

        function verified(token: string) {
            bearerToken.value = token
            needsSignup.value = false
            waitingForVerificationCode.value = false
        }

        async function getUser() {
            try {
                let userResponse = await useAuthenticatedApiClient().user.getUser();
                console.log("userResponse:", userResponse)
                if (userResponse === undefined || userResponse === null) {
                    logout()
                    return
                }
                user.value = userResponse

            } catch (err) {
                console.error(err)
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
