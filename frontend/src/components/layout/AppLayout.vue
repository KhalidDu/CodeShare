<template>
  <div class="app-layout" :class="{ 'sidebar-collapsed': isSidebarCollapsed }">
    <!-- 应用头部 -->
    <AppHeader />

    <!-- 主要内容区域 -->
    <div class="layout-main">
      <!-- 侧边栏 -->
      <AppSidebar
        v-if="showSidebar"
        :class="{ show: isMobileMenuOpen }"
        @toggle-collapse="handleSidebarToggle"
      />

      <!-- 内容区域 -->
      <div class="layout-content" :class="{ 'full-width': !showSidebar }">
        <!-- 面包屑导航 -->
        <div class="content-header" v-if="showBreadcrumb">
          <div class="container">
            <Breadcrumb :items="breadcrumbItems" :show-home="showHomeBreadcrumb" />
          </div>
        </div>

        <!-- 页面内容 -->
        <main class="main-content">
          <div class="container">
            <!-- 页面标题 -->
            <PageTitle
              v-if="pageTitle"
              :title="pageTitle"
              :subtitle="pageSubtitle"
              :icon="pageIcon"
            >
              <template #actions v-if="$slots.pageActions">
                <slot name="pageActions"></slot>
              </template>
            </PageTitle>

            <!-- 主要内容插槽 -->
            <div class="page-content">
              <slot></slot>
            </div>
          </div>
        </main>
      </div>
    </div>

    <!-- 应用底部 -->
    <AppFooter v-if="showFooter" />

    <!-- 移动端遮罩层 -->
    <div
      class="mobile-overlay"
      v-if="isMobileMenuOpen"
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
.app-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  position: relative;
}

/* 主要内容区域 */
.layout-main {
  display: flex;
  flex: 1;
  position: relative;
}

/* 内容区域 */
.layout-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  margin-left: 280px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  min-height: calc(100vh - 64px);
}

.layout-content.full-width {
  margin-left: 0;
}

.app-layout.sidebar-collapsed .layout-content {
  margin-left: 64px;
}

/* 内容头部 */
.content-header {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
  position: sticky;
  top: 64px;
  z-index: 998;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

/* 主要内容 */
.main-content {
  flex: 1;
  padding: 1.5rem 0;
  position: relative;
}

.container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 1.5rem;
}

.page-content {
  padding-bottom: 2rem;
  position: relative;
}

/* 移动端遮罩层 */
.mobile-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  z-index: 998;
  display: none;
}

/* 响应式设计 */
@media (max-width: 1024px) {
  .layout-content {
    margin-left: 0;
  }

  .app-layout.sidebar-collapsed .layout-content {
    margin-left: 0;
  }

  .mobile-overlay {
    display: block;
  }
}

@media (max-width: 768px) {
  .container {
    padding: 0 0.75rem;
  }

  .content-header {
    top: 56px;
  }

  .page-content {
    padding-bottom: 1.5rem;
  }
}

@media (max-width: 480px) {
  .container {
    padding: 0 0.5rem;
  }

  .page-content {
    padding-bottom: 1rem;
  }
}

/* 打印样式 */
@media print {
  .app-layout {
    background-color: white;
  }

  .layout-content {
    margin-left: 0;
  }

  .content-header {
    position: static;
    border-bottom: 1px solid #000;
  }

  .main-content {
    padding: 0;
  }

  .container {
    max-width: none;
    padding: 0;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .layout-content {
    transition: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .app-layout {
    background-color: white;
  }

  .content-header {
    border-bottom-width: 2px;
  }
}
</style>
