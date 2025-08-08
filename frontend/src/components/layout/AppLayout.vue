<template>
  <div
    class="h-screen flex flex-col bg-gradient-to-br from-slate-100 via-slate-50 to-slate-200 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900 text-slate-800 dark:text-slate-200 relative"
    :class="{ 'sidebar-collapsed': isSidebarCollapsed }"
  >
    <!-- 应用头部 -->
    <AppHeader @toggle-mobile-menu="isMobileMenuOpen = !isMobileMenuOpen" />

    <!-- 主要内容区域 -->
    <div class="flex-1 flex overflow-hidden">
      <!-- 侧边栏 -->
      <AppSidebar
        v-if="showSidebar"
        :class="{
          'show': isMobileMenuOpen,
          'collapsed': isSidebarCollapsed
        }"
        @toggle-collapse="handleSidebarToggle"
      />

      <!-- 内容区域 -->
      <div
        class="flex-1 flex flex-col overflow-hidden transition-all duration-300 ease-in-out"
        :class="{
          'ml-0': !showSidebar || isMobileMenuOpen,
          'ml-72': showSidebar && !isSidebarCollapsed && !isMobileMenuOpen,
          'ml-16': showSidebar && isSidebarCollapsed && !isMobileMenuOpen
        }"
      >
        <!-- 面包屑导航 -->
        <div
          v-if="showBreadcrumb"
          class="bg-white dark:bg-gray-900 backdrop-blur-xl border-b border-slate-300 dark:border-gray-700 px-6 py-3 shadow-sm"
        >
          <Breadcrumb :items="breadcrumbItems" :show-home="showHomeBreadcrumb" />
        </div>

        <!-- 页面内容 -->
        <main class="flex-1 overflow-y-auto scrollbar-thin p-6">
          <!-- 页面标题 -->
          <PageTitle
            v-if="pageTitle"
            :title="pageTitle"
            :subtitle="pageSubtitle"
            :icon="pageIcon"
            class="mb-6"
          >
            <template #actions v-if="$slots.pageActions">
              <slot name="pageActions"></slot>
            </template>
          </PageTitle>

          <!-- 主要内容插槽 -->
          <div class="space-y-6">
            <slot></slot>
          </div>
        </main>
      </div>
    </div>

    <!-- 应用底部 -->
    <AppFooter v-if="showFooter" />

    <!-- 移动端遮罩层 -->
    <div
      v-if="isMobileMenuOpen"
      class="fixed inset-0 bg-black/50 backdrop-blur-sm z-40 lg:hidden"
      @click="closeMobileMenu"
    ></div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import AppHeader from './AppHeader.vue'
import AppSidebar from './AppSidebar.vue'
import AppFooter from './AppFooter.vue'
import Breadcrumb from '../common/Breadcrumb.vue'
import PageTitle from '../common/PageTitle.vue'

interface BreadcrumbItem {
  title: string
  path?: string
  icon?: string
}

interface Props {
  showSidebar?: boolean
  showFooter?: boolean
  showBreadcrumb?: boolean
  showHomeBreadcrumb?: boolean
  pageTitle?: string
  pageSubtitle?: string
  pageIcon?: string
  breadcrumbItems?: BreadcrumbItem[]
  containerFluid?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  showSidebar: true,
  showFooter: true,
  showBreadcrumb: true,
  showHomeBreadcrumb: true,
  containerFluid: false
})

defineSlots<{
  default(): any
  pageActions(): any
}>()

const route = useRoute()
const authStore = useAuthStore()

// 响应式状态
const isSidebarCollapsed = ref(false)
const isMobileMenuOpen = ref(false)

// 计算属性
const isAuthenticated = computed(() => authStore.isAuthenticated)

/**
 * 根据认证状态决定是否显示侧边栏
 */
const showSidebar = computed(() => {
  return props.showSidebar && isAuthenticated.value
})

/**
 * 处理侧边栏折叠切换
 */
function handleSidebarToggle(collapsed: boolean) {
  isSidebarCollapsed.value = collapsed
}

/**
 * 关闭移动端菜单
 */
function closeMobileMenu() {
  isMobileMenuOpen.value = false
}

// 监听路由变化，自动关闭移动端菜单
watch(route, () => {
  closeMobileMenu()
})

// 监听窗口大小变化
function handleResize() {
  if (window.innerWidth > 1024) {
    isMobileMenuOpen.value = false
  }
}

// 添加窗口大小变化监听器
if (typeof window !== 'undefined') {
  window.addEventListener('resize', handleResize)
}
</script>

<style scoped>
/* 响应式设计 */
@media (max-width: 1024px) {
  .flex-1 {
    margin-left: 0 !important;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .transition-all {
    transition: none !important;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .bg-gradient-to-br {
    background: white !important;
  }

  .dark .bg-gradient-to-br {
    background: black !important;
  }
}
</style>
