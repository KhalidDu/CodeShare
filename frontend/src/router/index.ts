import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('../views/HomeView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: { requiresGuest: true }
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('../views/RegisterView.vue'),
      meta: { requiresGuest: true }
    },
    {
      path: '/snippets',
      name: 'snippets',
      component: () => import('../views/SnippetsView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/snippets/create',
      name: 'create-snippet',
      component: () => import('../views/CreateSnippetView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/snippets/:id',
      name: 'snippet-detail',
      component: () => import('../views/SnippetDetailView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/snippets/:id/edit',
      name: 'edit-snippet',
      component: () => import('../views/EditSnippetView.vue'),
      meta: { requiresAuth: true }
    }
  ]
})

// 路由守卫
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
  } else if (to.meta.requiresGuest && authStore.isAuthenticated) {
    next('/')
  } else {
    next()
  }
})

export default router
