import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { UserRole } from '@/types'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('../views/HomeView.vue')
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
      component: () => import('../views/SnippetsView.vue')
    },
    {
      path: '/snippets/create',
      name: 'create-snippet',
      component: () => import('../views/CreateSnippetView.vue'),
      meta: {
        requiresAuth: true,
        requiredRoles: [UserRole.Admin, UserRole.Editor]
      }
    },
    {
      path: '/snippets/:id',
      name: 'snippet-detail',
      component: () => import('../views/SnippetDetailView.vue')
    },
    {
      path: '/snippets/:id/edit',
      name: 'edit-snippet',
      component: () => import('../views/EditSnippetView.vue'),
      meta: {
        requiresAuth: true,
        requiredRoles: [UserRole.Admin, UserRole.Editor]
      }
    },
    {
      path: '/snippets/:snippetId/compare',
      name: 'version-compare',
      component: () => import('../views/VersionCompareView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/clipboard/history',
      name: 'clipboard-history',
      component: () => import('../views/ClipboardHistoryView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/editor-test',
      name: 'editor-test',
      component: () => import('../views/EditorTestView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/admin/users',
      name: 'user-management',
      component: () => import('../views/UserManagementView.vue'),
      meta: {
        requiresAuth: true,
        requiredRoles: [UserRole.Admin]
      }
    },
    {
      path: '/settings',
      name: 'user-settings',
      component: () => import('../views/UserSettingsView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/style-test',
      name: 'style-test',
      component: () => import('../views/StyleTestView.vue')
    },
    {
      path: '/share/:token',
      name: 'share-view',
      component: () => import('../views/ShareView.vue'),
      meta: {
        title: '分享链接访问',
        public: true
      }
    },
    {
      path: '/shares',
      name: 'share-management',
      component: () => import('../views/ShareManagementView.vue'),
      meta: {
        requiresAuth: true,
        title: '分享管理'
      }
    }
  ]
})

// 路由守卫 - 增强版权限检查
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  // 检查是否需要认证
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
    return
  }

  // 检查是否为访客页面
  if (to.meta.requiresGuest && authStore.isAuthenticated) {
    next('/')
    return
  }

  // 检查角色权限
  if (to.meta.requiredRoles && authStore.user) {
    const requiredRoles = to.meta.requiredRoles as UserRole[]
    const userRole = authStore.user.role

    if (!requiredRoles.includes(userRole)) {
      // 权限不足，重定向到首页或显示错误页面
      next('/')
      return
    }
  }

  next()
})

export default router
