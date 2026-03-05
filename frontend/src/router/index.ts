import {createRouter, createWebHistory} from 'vue-router'
import {useAuthStore} from '@/stores/auth'

const LoginView = () => import('@/views/LoginView.vue')
const HomeView = () => import('@/views/HomeView.vue')

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: '/login',
            name: 'login',
            component: LoginView,
            meta: {guestOnly: true},
        },
        {
            path: '/',
            name: 'home',
            component: HomeView,
            meta: {requiresAuth: true},
        },
        {path: '/:pathMatch(.*)*', redirect: '/'},
    ],
})

router.beforeEach((to) => {
    const authStore = useAuthStore()
    const isAuthenticated = authStore.isAuthenticated

    if (to.meta?.requiresAuth && !isAuthenticated) return {name: 'login'}
    if (to.meta?.guestOnly && isAuthenticated) return {name: 'home'}
    return true
})

export default router

