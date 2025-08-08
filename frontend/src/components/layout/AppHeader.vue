<template>
  <header class="h-14 md:h-16 bg-white dark:bg-gray-900 backdrop-blur-xl border-b border-slate-300 dark:border-gray-700 flex items-center px-4 md:px-6 gap-4 md:gap-6 shadow-sm dark:shadow-lg">
    <!-- 品牌标识 -->
    <div class="flex-none">
      <router-link to="/" class="text-lg md:text-xl font-bold text-blue-600 dark:text-blue-400 flex items-center gap-2 hover:text-blue-700 dark:hover:text-blue-300 transition-colors duration-200">
        <i class="fas fa-code text-xl md:text-2xl"></i>
        <span class="hidden sm:inline">CodeSnippet</span>
      </router-link>
    </div>

    <!-- 主导航菜单 -->
    <nav class="flex-1 hidden lg:flex justify-center" v-if="isAuthenticated">
      <ul class="flex items-center gap-1">
        <li>
          <router-link
            to="/"
            class="flex items-center gap-2 px-3 py-2 text-sm font-medium text-slate-600 dark:text-slate-300 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900 rounded-lg transition-all duration-200"
            :class="{ 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900': $route.path === '/' }"
          >
            <i class="fas fa-home"></i>
            <span>首页</span>
          </router-link>
        </li>
        <li>
          <router-link
            to="/snippets"
            class="flex items-center gap-2 px-3 py-2 text-sm font-medium text-slate-600 dark:text-slate-300 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900 rounded-lg transition-all duration-200"
            :class="{ 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900': $route.path.startsWith('/snippets') }"
          >
            <i class="fas fa-file-code"></i>
            <span>代码片段</span>
          </router-link>
        </li>
        <li v-if="canCreateSnippet">
          <router-link
            to="/snippets/create"
            class="flex items-center gap-2 px-3 py-2 text-sm font-medium text-slate-600 dark:text-slate-300 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900 rounded-lg transition-all duration-200"
            :class="{ 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900': $route.path === '/snippets/create' }"
          >
            <i class="fas fa-plus"></i>
            <span>创建片段</span>
          </router-link>
        </li>
      </ul>
    </nav>

    <!-- 用户信息和操作 -->
    <div class="flex-none flex items-center gap-3" v-if="isAuthenticated">
      <!-- 用户头像和信息 -->
      <div class="hidden md:flex items-center gap-3">
        <div class="w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center text-sm font-semibold">
          {{ userInitials }}
        </div>
        <div class="hidden lg:block">
          <div class="text-sm font-medium text-slate-700 dark:text-slate-200">{{ user?.username }}</div>
          <div class="text-xs text-slate-500 dark:text-slate-400">{{ userRoleText }}</div>
        </div>
      </div>

      <!-- 操作按钮 -->
      <div class="flex items-center gap-2">
        <!-- 主题切换按钮 -->
        <button
          @click="toggleTheme"
          class="w-9 h-9 bg-gray-100 hover:bg-gray-200 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-600 dark:text-gray-300 rounded-lg transition-all duration-200 flex items-center justify-center"
          title="切换主题"
        >
          <i class="fas fa-moon dark:hidden"></i>
          <i class="fas fa-sun hidden dark:inline"></i>
        </button>

        <!-- 登出按钮 -->
        <button
          @click="handleLogout"
          class="w-9 h-9 bg-red-100 hover:bg-red-200 dark:bg-red-900 dark:hover:bg-red-800 text-red-600 dark:text-red-400 rounded-lg transition-all duration-200 flex items-center justify-center"
          title="登出"
        >
          <i class="fas fa-sign-out-alt"></i>
        </button>
      </div>

      <!-- 移动端菜单按钮 -->
      <button
        @click="toggleMobileMenu"
        class="lg:hidden w-9 h-9 bg-gray-100 hover:bg-gray-200 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-600 dark:text-gray-300 rounded-lg transition-all duration-200 flex items-center justify-center"
        :class="{ 'bg-blue-100 dark:bg-blue-900 text-blue-600 dark:text-blue-400': isMobileMenuOpen }"
      >
        <i class="fas fa-bars" v-if="!isMobileMenuOpen"></i>
        <i class="fas fa-times" v-else></i>
      </button>
    </div>

    <!-- 未登录状态的登录按钮 -->
    <div class="flex-none" v-else>
      <router-link
        to="/login"
        class="btn-primary"
      >
        登录
      </router-link>
    </div>
  </header>

  <!-- 移动端导航菜单 -->
  <nav
    v-if="isAuthenticated && isMobileMenuOpen"
    class="lg:hidden bg-white dark:bg-gray-900 backdrop-blur-xl border-b border-slate-300 dark:border-gray-700 shadow-lg"
  >
    <ul class="px-4 py-2 space-y-1">
      <li>
        <router-link
          to="/"
          @click="closeMobileMenu"
          class="flex items-center gap-3 px-3 py-2 text-sm font-medium text-slate-600 dark:text-slate-300 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900 rounded-lg transition-all duration-200"
          :class="{ 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900': $route.path === '/' }"
        >
          <i class="fas fa-home w-4"></i>
          <span>首页</span>
        </router-link>
      </li>
      <li>
        <router-link
          to="/snippets"
          @click="closeMobileMenu"
          class="flex items-center gap-3 px-3 py-2 text-sm font-medium text-slate-600 dark:text-slate-300 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900 rounded-lg transition-all duration-200"
          :class="{ 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900': $route.path.startsWith('/snippets') }"
        >
          <i class="fas fa-file-code w-4"></i>
          <span>代码片段</span>
        </router-link>
      </li>
      <li v-if="canCreateSnippet">
        <router-link
          to="/snippets/create"
          @click="closeMobileMenu"
          class="flex items-center gap-3 px-3 py-2 text-sm font-medium text-slate-600 dark:text-slate-300 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900 rounded-lg transition-all duration-200"
          :class="{ 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900': $route.path === '/snippets/create' }"
        >
          <i class="fas fa-plus w-4"></i>
          <span>创建片段</span>
        </router-link>
      </li>
    </ul>
  </nav>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { UserRole } from '@/types'

const router = useRouter()
const authStore = useAuthStore()
const isMobileMenuOpen = ref(false)

// 定义事件
const emit = defineEmits<{
  'toggle-mobile-menu': []
}>()

// 计算属性
const isAuthenticated = computed(() => authStore.isAuthenticated)
const user = computed(() => authStore.user)

/**
 * 检查用户是否可以创建代码片段
 */
const canCreateSnippet = computed(() => {
  if (!user.value) return false
  return user.value.role === UserRole.Admin || user.value.role === UserRole.Editor
})

/**
 * 获取用户姓名首字母
 */
const userInitials = computed(() => {
  if (!user.value?.username) return 'U'
  return user.value.username.charAt(0).toUpperCase()
})

/**
 * 获取用户角色文本
 */
const userRoleText = computed(() => {
  if (!user.value) return ''
  switch (user.value.role) {
    case UserRole.Admin:
      return '管理员'
    case UserRole.Editor:
      return '编辑者'
    case UserRole.Viewer:
      return '查看者'
    default:
      return ''
  }
})

/**
 * 处理用户登出
 */
async function handleLogout() {
  try {
    await authStore.logout()
    router.push('/login')
  } catch (error) {
    console.error('登出失败:', error)
  }
}

/**
 * 切换移动端菜单显示状态
 */
function toggleMobileMenu() {
  isMobileMenuOpen.value = !isMobileMenuOpen.value
  emit('toggle-mobile-menu')
}

/**
 * 关闭移动端菜单
 */
function closeMobileMenu() {
  isMobileMenuOpen.value = false
}

/**
 * 切换主题
 */
function toggleTheme() {
  const html = document.documentElement
  if (html.classList.contains('dark')) {
    html.classList.remove('dark')
    localStorage.setItem('theme', 'light')
  } else {
    html.classList.add('dark')
    localStorage.setItem('theme', 'dark')
  }
}
</script>

<style scoped>
/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  * {
    transition: none !important;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .bg-white\/95 {
    background: white !important;
  }

  .dark .bg-gray-900\/95 {
    background: black !important;
  }
}
</style>
