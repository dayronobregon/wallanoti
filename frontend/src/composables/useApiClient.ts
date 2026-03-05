import {ApiClient} from '@/api'
import {useAuthStore} from '@/stores/auth'

let apiClientInstance: ApiClient | null = null
let authenticatedApiClientInstance: ApiClient | null = null

export function useApiClient() {
    if (!apiClientInstance) {

        const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:3000'

        apiClientInstance = new ApiClient({
            BASE: baseUrl
        })
    }

    return apiClientInstance
}

export function useAuthenticatedApiClient() {

    if (authenticatedApiClientInstance === null) {
        const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:3000'

        const authStore = useAuthStore()
        const token = authStore.bearerToken

        authenticatedApiClientInstance = new ApiClient({
            BASE: baseUrl,
            WITH_CREDENTIALS: token != null,
            TOKEN: token ?? ''
        })
    }

    return authenticatedApiClientInstance;
}
