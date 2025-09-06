<template>
  <header class="relative h-14 bg-white/95 dark:bg-gray-900/95 backdrop-blur-lg border-b border-gray-200 dark:border-gray-800 shadow-sm">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-full">
      <div class="flex items-center justify-between h-full">
        <!-- Logo和品牌 -->
        <div class="flex items-center">
          <router-link to="/" class="flex items-center space-x-3 hover:opacity-80 transition-opacity">
            <!-- Logo图标 -->
            <div class="w-8 h-8 bg-gradient-to-br from-blue-500 to-purple-600 rounded-lg flex items-center justify-center">
              <i class="fas fa-code text-white text-lg"></i>
            </div>
            <!-- 品牌名称 -->
            <span class="text-xl font-bold text-gray-900 dark:text-white">CodeSnippet</span>
          </router-link>
        </div>

        <!-- 主导航菜单 (桌面端) -->
        <nav class="hidden lg:flex items-center space-x-8" v-if="isAuthenticated">
          <router-link
            to="/"
            class="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white font-medium transition-colors duration-200 relative group"
            :class="{ 'text-blue-600 dark:text-blue-400': $route.path === '/' }"
          >
            首页
            <span class="absolute -bottom-1 left-0 w-full h-0.5 bg-blue-600 dark:bg-blue-400 transform scale-x-0 group-hover:scale-x-100 transition-transform duration-200" :class="{ 'scale-x-100': $route.path === '/' }"></span>
          </router-link>
          
          <router-link
            to="/snippets"
            class="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white font-medium transition-colors duration-200 relative group"
            :class="{ 'text-blue-600 dark:text-blue-400': $route.path.startsWith('/snippets') }"
          >
            代码片段
            <span class="absolute -bottom-1 left-0 w-full h-0.5 bg-blue-600 dark:bg-blue-400 transform scale-x-0 group-hover:scale-x-100 transition-transform duration-200" :class="{ 'scale-x-100': $route.path.startsWith('/snippets') }"></span>
          </router-link>
          
          <router-link
            to="/snippets/create"
            v-if="canCreateSnippet"
            class="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white font-medium transition-colors duration-200 relative group"
            :class="{ 'text-blue-600 dark:text-blue-400': $route.path === '/snippets/create' }"
          >
            创建片段
            <span class="absolute -bottom-1 left-0 w-full h-0.5 bg-blue-600 dark:bg-blue-400 transform scale-x-0 group-hover:scale-x-100 transition-transform duration-200" :class="{ 'scale-x-100': $route.path === '/snippets/create' }"></span>
          </router-link>
        </nav>

        <!-- 右侧操作区 -->
        <div class="flex items-center space-x-4">
          <!-- 已登录用户 -->
          <div v-if="isAuthenticated" class="flex items-center space-x-4">
            <!-- 用户信息 -->
            <div class="hidden md:flex items-center space-x-3">
              <div class="w-8 h-8 bg-gradient-to-br from-blue-500 to-purple-600 rounded-full flex items-center justify-center text-white text-sm font-semibold">
                {{ userInitials }}
              </div>
              <div class="hidden lg:block">
                <div class="text-sm font-medium text-gray-900 dark:text-white">{{ user?.username }}</div>
                <div class="text-xs text-gray-500 dark:text-gray-400">{{ userRoleText }}</div>
              </div>
            </div>

            <!-- 主题切换 -->
            <button
              @click="toggleTheme"
              class="w-10 h-10 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 rounded-lg flex items-center justify-center transition-colors duration-200"
              title="切换主题"
            >
              <i class="fas fa-moon dark:hidden text-gray-600"></i>
              <i class="fas fa-sun hidden dark:inline text-yellow-400"></i>
            </button>

            <!-- 登出按钮 -->
            <button
              @click="handleLogout"
              class="w-10 h-10 bg-red-100 dark:bg-red-900/20 hover:bg-red-200 dark:hover:bg-red-900/30 rounded-lg flex items-center justify-center transition-colors duration-200"
              title="登出"
            >
              <i class="fas fa-sign-out-alt text-red-600 dark:text-red-400"></i>
            </button>

            <!-- 移动端菜单按钮 -->
            <button
              @click="toggleMobileMenu"
              class="lg:hidden w-10 h-10 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 rounded-lg flex items-center justify-center transition-colors duration-200"
            >
              <i class="fas fa-bars text-gray-600 dark:text-gray-300" v-if="!isMobileMenuOpen"></i>
              <i class="fas fa-times text-gray-600 dark:text-gray-300" v-else></i>
            </button>
          </div>

          <!-- 未登录用户 -->
          <div v-else class="flex items-center space-x-2">
            <!-- 立即登录按钮 -->
            <router-link
              to="/login"
              class="px-4 py-2 text-sm font-semibold text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors duration-200"
            >
              立即登录
            </router-link>

            <!-- 免费注册按钮 -->
            <router-link
              to="/register"
              class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 hover:border-gray-400 rounded-lg transition-colors duration-200"
            >
              免费注册
            </router-link>

            <!-- GitHub徽章 -->
            <a
              href="https://github.com/KhalidDu/CodeShare"
              target="_blank"
              rel="noopener noreferrer"
              class="group relative hidden sm:flex items-center px-3 py-1.5 bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-all duration-200 transform hover:scale-105"
            >
              <svg class="w-4 h-4 text-gray-700 dark:text-gray-300 group-hover:text-orange-500 dark:group-hover:text-orange-400 transition-colors duration-200" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"/>
              </svg>
              <span class="text-sm font-medium text-gray-700 dark:text-gray-300 group-hover:text-orange-600 dark:group-hover:text-orange-400 transition-colors duration-200">
                <span v-if="githubStars > 0">{{ formatStars(githubStars) }}</span>
                <span v-else class="flex items-center">
                  <svg class="w-3 h-3 mr-0.5" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
                  </svg>
                  Star
                </span>
              </span>
              <div class="absolute inset-0 rounded-lg bg-orange-500 opacity-0 group-hover:opacity-5 transition-opacity duration-200"></div>
            </a>
          </div>
        </div>
      </div>
    </div>

    <!-- 移动端导航菜单 -->
    <div v-if="isAuthenticated && isMobileMenuOpen" class="lg:hidden bg-white dark:bg-gray-900 border-t border-gray-200 dark:border-gray-800">
      <div class="px-4 py-2 space-y-1">
        <router-link
          to="/"
          @click="closeMobileMenu"
          class="flex items-center space-x-3 px-3 py-2 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 rounded-lg transition-colors duration-200"
          :class="{ 'bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400': $route.path === '/' }"
        >
          <i class="fas fa-home w-5"></i>
          <span>首页</span>
        </router-link>
        
        <router-link
          to="/snippets"
          @click="closeMobileMenu"
          class="flex items-center space-x-3 px-3 py-2 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 rounded-lg transition-colors duration-200"
          :class="{ 'bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400': $route.path.startsWith('/snippets') }"
        >
          <i class="fas fa-file-code w-5"></i>
          <span>代码片段</span>
        </router-link>
        
        <router-link
          to="/snippets/create"
          v-if="canCreateSnippet"
          @click="closeMobileMenu"
          class="flex items-center space-x-3 px-3 py-2 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 rounded-lg transition-colors duration-200"
          :class="{ 'bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400': $route.path === '/snippets/create' }"
        >
          <i class="fas fa-plus w-5"></i>
          <span>创建片段</span>
        </router-link>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { UserRole } from '@/types'

const router = useRouter()
const authStore = useAuthStore()
const isMobileMenuOpen = ref(false)
const githubStars = ref(0)

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

/**
 * 获取GitHub仓库star数量
 */
async function fetchGithubStars() {
  try {
    const response = await fetch('https://api.github.com/repos/KhalidDu/CodeShare')
    if (response.ok) {
      const data = await response.json()
      githubStars.value = data.stargazers_count || 0
    }
  } catch (error) {
    console.error('获取GitHub star数量失败:', error)
    // 如果API调用失败，设置一个默认值或者保持为0
    githubStars.value = 0
  }
}

/**
 * 格式化star数量（用于小屏幕显示）
 */
function formatStars(stars: number): string {
  if (stars >= 1000) {
    return (stars / 1000).toFixed(1) + 'k'
  }
  return stars.toString()
}

// 组件挂载时获取GitHub star数量
onMounted(() => {
  fetchGithubStars()
})
</script>

<style scoped>
/* 导航链接下划线动画 */
nav a {
  position: relative;
}

nav a::after {
  content: '';
  position: absolute;
  bottom: -2px;
  left: 0;
  width: 100%;
  height: 2px;
  background: linear-gradient(90deg, #3B82F6, #8B5CF6);
  transform: scaleX(0);
  transform-origin: left;
  transition: transform 0.3s ease;
}

nav a:hover::after,
nav a.router-link-active::after {
  transform: scaleX(1);
}

/* Logo渐变效果 */
.bg-gradient-to-br {
  background: linear-gradient(135deg, #3B82F6 0%, #8B5CF6 100%);
}

/* 按钮悬停效果 */
button:hover {
  transform: translateY(-1px);
}

/* 响应式优化 */
@media (max-width: 640px) {
  header {
    height: 3.5rem;
  }
}

@media (max-width: 768px) {
  header {
    height: 4rem;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  * {
    transition: none !important;
    animation: none !important;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  header {
    border-color: #374151 !important;
  }
  
  .dark header {
    border-color: #F3F4F6 !important;
  }
}

/* 深色模式优化 */
.dark .bg-gradient-to-br {
  background: linear-gradient(135deg, #2563EB 0%, #7C3AED 100%);
}

/* 移动端菜单动画 */
.lg-hidden {
  transition: all 0.3s ease;
}

/* 焦点状态优化 */
*:focus {
  outline: 2px solid #3B82F6;
  outline-offset: 2px;
}

*:focus:not(:focus-visible) {
  outline: none;
}

*:focus-visible {
  outline: 2px solid #3B82F6;
  outline-offset: 2px;
}
</style>
