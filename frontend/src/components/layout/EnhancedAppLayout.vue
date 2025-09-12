<template>
  <div
    class="enhanced-app-layout"
    :class="[
      layoutClasses,
      {
        'sidebar-collapsed': isSidebarCollapsed,
        'mobile-menu-open': isMobileMenuOpen,
        'loading': isLoading,
        'offline': isOffline
      }
    ]"
    :style="layoutStyles"
  >
    <!-- 应用头部 -->
    <EnhancedAppHeader
      v-if="showHeader"
      :variant="headerVariant"
      :sticky="headerSticky"
      :transparent="headerTransparent"
      @toggle-mobile-menu="toggleMobileMenu"
      @toggle-sidebar="toggleSidebar"
    >
      <template #header-start v-if="$slots.headerStart">
        <slot name="headerStart"></slot>
      </template>
      
      <template #header-center v-if="$slots.headerCenter">
        <slot name="headerCenter"></slot>
      </template>
      
      <template #header-end v-if="$slots.headerEnd">
        <slot name="headerEnd"></slot>
      </template>
    </EnhancedAppHeader>

    <!-- 主要内容区域 -->
    <div class="main-content-area" :class="contentAreaClasses">
      <!-- 侧边栏 -->
      <EnhancedAppSidebar
        v-if="showSidebar"
        :variant="sidebarVariant"
        :collapsible="sidebarCollapsible"
        :collapsed="isSidebarCollapsed"
        :animation="sidebarAnimation"
        :responsive="sidebarResponsive"
        :class="{
          'show': isMobileMenuOpen,
          'collapsed': isSidebarCollapsed
        }"
        @toggle-collapse="handleSidebarToggle"
        @close-mobile="closeMobileMenu"
      >
        <template #sidebar-header v-if="$slots.sidebarHeader">
          <slot name="sidebarHeader"></slot>
        </template>
        
        <template #sidebar-content v-if="$slots.sidebarContent">
          <slot name="sidebarContent"></slot>
        </template>
        
        <template #sidebar-footer v-if="$slots.sidebarFooter">
          <slot name="sidebarFooter"></slot>
        </template>
      </EnhancedAppSidebar>

      <!-- 内容区域 -->
      <div
        class="content-wrapper"
        :class="contentWrapperClasses"
        :style="contentWrapperStyles"
      >
        <!-- 面包屑导航 -->
        <transition :name="breadcrumbAnimation">
          <div
            v-if="showBreadcrumb"
            class="breadcrumb-container"
            :class="breadcrumbClasses"
          >
            <Breadcrumb 
              :items="breadcrumbItems" 
              :show-home="showHomeBreadcrumb"
              :variant="breadcrumbVariant"
              :separator="breadcrumbSeparator"
            />
          </div>
        </transition>

        <!-- 页面内容 -->
        <main 
          class="main-content"
          :class="mainContentClasses"
          :style="mainContentStyles"
        >
          <!-- 页面标题 -->
          <transition :name="pageTitleAnimation">
            <PageTitle
              v-if="pageTitle"
              :key="pageTitle"
              :title="pageTitle"
              :subtitle="pageSubtitle"
              :icon="pageIcon"
              :variant="pageTitleVariant"
              :size="pageTitleSize"
              class="page-title-section"
            >
              <template #actions v-if="$slots.pageActions">
                <slot name="pageActions"></slot>
              </template>
            </PageTitle>
          </transition>

          <!-- 主要内容插槽 -->
          <transition :name="contentAnimation">
            <div 
              :key="route.path"
              class="content-slot"
              :class="contentSlotClasses"
            >
              <!-- 加载状态 -->
              <div v-if="isLoading" class="loading-overlay">
                <EnhancedLoading
                  :variant="loadingVariant"
                  :size="loadingSize"
                  :text="loadingText"
                />
              </div>

              <!-- 离线状态 -->
              <div v-else-if="isOffline" class="offline-overlay">
                <OfflineIndicator
                  :message="offlineMessage"
                  :action="offlineAction"
                />
              </div>

              <!-- 实际内容 -->
              <slot v-else></slot>
            </div>
          </transition>
        </main>
      </div>
    </div>

    <!-- 应用底部 -->
    <EnhancedAppFooter
      v-if="showFooter"
      :variant="footerVariant"
      :sticky="footerSticky"
      :transparent="footerTransparent"
    >
      <template #footer-start v-if="$slots.footerStart">
        <slot name="footerStart"></slot>
      </template>
      
      <template #footer-center v-if="$slots.footerCenter">
        <slot name="footerCenter"></slot>
      </template>
      
      <template #footer-end v-if="$slots.footerEnd">
        <slot name="footerEnd"></slot>
      </template>
    </EnhancedAppFooter>

    <!-- 移动端遮罩层 -->
    <transition :name="overlayAnimation">
      <div
        v-if="isMobileMenuOpen"
        class="mobile-overlay"
        :class="overlayClasses"
        @click="closeMobileMenu"
      ></div>
    </transition>

    <!-- 全局通知容器 -->
    <div class="notification-container" :class="notificationClasses">
      <slot name="notifications"></slot>
    </div>

    <!-- 浮动操作按钮 -->
    <div v-if="showFab" class="fab-container" :class="fabClasses">
      <slot name="fab"></slot>
    </div>

    <!-- 返回顶部按钮 -->
    <transition :name="backToTopAnimation">
      <button
        v-if="showBackToTop"
        class="back-to-top"
        :class="backToTopClasses"
        @click="scrollToTop"
        :aria-label="backToTopLabel"
      >
        <slot name="backToTopIcon">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 10l7-7m0 0l7 7m-7-7v18"/>
          </svg>
        </slot>
      </button>
    </transition>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useBreakpoints } from '@/composables/useBreakpoints'
import EnhancedAppHeader from './EnhancedAppHeader.vue'
import EnhancedAppSidebar from './EnhancedAppSidebar.vue'
import EnhancedAppFooter from './EnhancedAppFooter.vue'
import EnhancedLoading from '@/components/ui/EnhancedLoading.vue'
import OfflineIndicator from '@/components/common/OfflineIndicator.vue'
import Breadcrumb from '@/components/common/Breadcrumb.vue'
import PageTitle from '@/components/common/PageTitle.vue'

// 面包屑项目接口
interface BreadcrumbItem {
  title: string
  path?: string
  icon?: string
  disabled?: boolean
}

// Props 接口
interface Props {
  // 基础配置
  showHeader?: boolean
  showSidebar?: boolean
  showFooter?: boolean
  showBreadcrumb?: boolean
  showHomeBreadcrumb?: boolean
  showFab?: boolean
  showBackToTop?: boolean
  
  // 头部配置
  headerVariant?: 'default' | 'minimal' | 'compact' | 'expanded'
  headerSticky?: boolean
  headerTransparent?: boolean
  
  // 侧边栏配置
  sidebarVariant?: 'default' | 'minimal' | 'compact' | 'overlay'
  sidebarCollapsible?: boolean
  sidebarAnimation?: 'slide' | 'fade' | 'scale' | 'bounce'
  sidebarResponsive?: boolean
  
  // 页面配置
  pageTitle?: string
  pageSubtitle?: string
  pageIcon?: string
  pageTitleVariant?: 'default' | 'minimal' | 'compact'
  pageTitleSize?: 'sm' | 'md' | 'lg' | 'xl'
  
  // 面包屑配置
  breadcrumbVariant?: 'default' | 'minimal' | 'compact'
  breadcrumbSeparator?: 'arrow' | 'slash' | 'dot'
  breadcrumbItems?: BreadcrumbItem[]
  
  // 底部配置
  footerVariant?: 'default' | 'minimal' | 'compact'
  footerSticky?: boolean
  footerTransparent?: boolean
  
  // 布局配置
  layoutVariant?: 'default' | 'minimal' | 'compact' | 'expanded'
  contentMaxWidth?: 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | '4xl' | 'full'
  contentPadding?: 'none' | 'sm' | 'md' | 'lg' | 'xl'
  contentSpacing?: 'sm' | 'md' | 'lg' | 'xl'
  
  // 动画配置
  animation?: 'none' | 'fade' | 'slide' | 'scale' | 'bounce'
  breadcrumbAnimation?: 'fade' | 'slide' | 'scale'
  pageTitleAnimation?: 'fade' | 'slide' | 'scale' | 'bounce'
  contentAnimation?: 'fade' | 'slide' | 'scale' | 'bounce'
  overlayAnimation?: 'fade' | 'slide' | 'scale'
  backToTopAnimation?: 'fade' | 'slide' | 'scale' | 'bounce'
  
  // 状态配置
  isLoading?: boolean
  loadingVariant?: 'spinner' | 'dots' | 'pulse' | 'skeleton'
  loadingSize?: 'sm' | 'md' | 'lg' | 'xl'
  loadingText?: string
  
  isOffline?: boolean
  offlineMessage?: string
  offlineAction?: {
    text: string
    onClick: () => void
  }
  
  // 返回顶部配置
  backToTopThreshold?: number
  backToTopLabel?: string
  
  // 响应式配置
  responsive?: boolean
  mobileBreakpoint?: 'xs' | 'sm' | 'md' | 'lg'
  
  // 主题配置
  theme?: 'light' | 'dark' | 'auto'
  
  // 无障碍配置
  ariaLabel?: string
  reducedMotion?: boolean
}

// Emits 接口
interface Emits {
  (e: 'sidebar-toggle', collapsed: boolean): void
  (e: 'mobile-menu-toggle', open: boolean): void
  (e: 'scroll', scrollTop: number): void
  (e: 'resize', width: number, height: number): void
  (e: 'back-to-top'): void
}

const props = withDefaults(defineProps<Props>(), {
  // 基础配置
  showHeader: true,
  showSidebar: true,
  showFooter: true,
  showBreadcrumb: true,
  showHomeBreadcrumb: true,
  showFab: false,
  showBackToTop: true,
  
  // 头部配置
  headerVariant: 'default',
  headerSticky: true,
  headerTransparent: false,
  
  // 侧边栏配置
  sidebarVariant: 'default',
  sidebarCollapsible: true,
  sidebarAnimation: 'slide',
  sidebarResponsive: true,
  
  // 页面配置
  pageTitleVariant: 'default',
  pageTitleSize: 'md',
  
  // 面包屑配置
  breadcrumbVariant: 'default',
  breadcrumbSeparator: 'arrow',
  
  // 底部配置
  footerVariant: 'default',
  footerSticky: false,
  footerTransparent: false,
  
  // 布局配置
  layoutVariant: 'default',
  contentMaxWidth: '4xl',
  contentPadding: 'lg',
  contentSpacing: 'md',
  
  // 动画配置
  animation: 'fade',
  breadcrumbAnimation: 'fade',
  pageTitleAnimation: 'fade',
  contentAnimation: 'fade',
  overlayAnimation: 'fade',
  backToTopAnimation: 'fade',
  
  // 状态配置
  loadingVariant: 'spinner',
  loadingSize: 'md',
  loadingText: '加载中...',
  
  offlineMessage: '网络连接已断开',
  
  // 返回顶部配置
  backToTopThreshold: 300,
  backToTopLabel: '返回顶部',
  
  // 响应式配置
  responsive: true,
  mobileBreakpoint: 'md',
  
  // 主题配置
  theme: 'auto',
  
  // 无障碍配置
  reducedMotion: false
})

const emit = defineEmits<Emits>()

defineSlots<{
  default(): any
  headerStart(): any
  headerCenter(): any
  headerEnd(): any
  sidebarHeader(): any
  sidebarContent(): any
  sidebarFooter(): any
  pageActions(): any
  footerStart(): any
  footerCenter(): any
  footerEnd(): any
  notifications(): any
  fab(): any
  backToTopIcon(): any
}>()

// 组合式 API
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const breakpoints = useBreakpoints()

// 响应式状态
const isSidebarCollapsed = ref(false)
const isMobileMenuOpen = ref(false)
const scrollTop = ref(0)
const windowWidth = ref(0)
const windowHeight = ref(0)

// 计算属性
const isAuthenticated = computed(() => authStore.isAuthenticated)
const isMobile = computed(() => {
  if (!props.responsive) return false
  return breakpoints.isLessThan(props.mobileBreakpoint)
})

// 布局样式类
const layoutClasses = computed(() => [
  `layout-${props.layoutVariant}`,
  `theme-${props.theme}`,
  `animation-${props.animation}`,
  {
    'reduced-motion': props.reducedMotion,
    'mobile': isMobile.value,
    'desktop': !isMobile.value
  }
])

// 布局样式
const layoutStyles = computed(() => ({
  '--header-height': props.headerVariant === 'compact' ? '56px' : '64px',
  '--footer-height': props.footerVariant === 'compact' ? '40px' : '48px',
  '--sidebar-width': props.sidebarVariant === 'compact' ? '200px' : '256px',
  '--sidebar-collapsed-width': '64px',
  '--content-max-width': props.contentMaxWidth === 'full' ? '100%' : `var(--max-w-${props.contentMaxWidth})`,
  '--content-padding': `var(--space-${props.contentPadding === 'none' ? '0' : props.contentPadding})`,
  '--content-spacing': `var(--space-${props.contentSpacing})`
}))

// 内容区域样式类
const contentAreaClasses = computed(() => [
  'content-area',
  `content-area-${props.layoutVariant}`,
  {
    'with-header': props.showHeader,
    'with-footer': props.showFooter,
    'with-sidebar': props.showSidebar && isAuthenticated.value,
    'sidebar-collapsed': isSidebarCollapsed.value,
    'mobile-menu-open': isMobileMenuOpen.value
  }
])

// 内容包装器样式类
const contentWrapperClasses = computed(() => [
  'content-wrapper',
  `content-wrapper-${props.layoutVariant}`,
  `max-w-${props.contentMaxWidth}`,
  `p-${props.contentPadding}`,
  {
    'transition-all': props.animation !== 'none',
    'ml-0': !props.showSidebar || !isAuthenticated.value || isMobile.value || isMobileMenuOpen.value,
    'ml-[var(--sidebar-width)]': props.showSidebar && isAuthenticated.value && !isSidebarCollapsed.value && !isMobile.value && !isMobileMenuOpen.value,
    'ml-[var(--sidebar-collapsed-width)]': props.showSidebar && isAuthenticated.value && isSidebarCollapsed.value && !isMobile.value && !isMobileMenuOpen.value
  }
])

// 内容包装器样式
const contentWrapperStyles = computed(() => ({
  marginLeft: isMobile.value || isMobileMenuOpen.value ? '0' : 
           !props.showSidebar || !isAuthenticated.value ? '0' :
           isSidebarCollapsed.value ? 'var(--sidebar-collapsed-width)' : 'var(--sidebar-width)'
}))

// 主内容样式类
const mainContentClasses = computed(() => [
  'main-content',
  `main-content-${props.layoutVariant}`,
  {
    'with-breadcrumb': props.showBreadcrumb,
    'with-page-title': props.pageTitle
  }
])

// 主内容样式
const mainContentStyles = computed(() => ({
  minHeight: `calc(100vh - ${props.showHeader ? 'var(--header-height)' : '0'} - ${props.showFooter ? 'var(--footer-height)' : '0'})`
}))

// 内容插槽样式类
const contentSlotClasses = computed(() => [
  'content-slot',
  `content-slot-${props.layoutVariant}`,
  `space-${props.contentSpacing}`
])

// 面包屑样式类
const breadcrumbClasses = computed(() => [
  'breadcrumb-section',
  `breadcrumb-${props.breadcrumbVariant}`,
  {
    'with-page-title': props.pageTitle
  }
])

// 遮罩层样式类
const overlayClasses = computed(() => [
  'mobile-overlay',
  `overlay-${props.overlayAnimation}`,
  {
    'transparent': props.headerTransparent
  }
])

// 通知容器样式类
const notificationClasses = computed(() => [
  'notification-container',
  `notification-${props.layoutVariant}`
])

// FAB 容器样式类
const fabClasses = computed(() => [
  'fab-container',
  `fab-${props.layoutVariant}`
])

// 返回顶部按钮样式类
const backToTopClasses = computed(() => [
  'back-to-top',
  `back-to-top-${props.layoutVariant}`,
  {
    'visible': scrollTop.value > props.backToTopThreshold
  }
])

// 显示侧边栏
const showSidebar = computed(() => {
  return props.showSidebar && isAuthenticated.value
})

// 显示返回顶部按钮
const showBackToTop = computed(() => {
  return props.showBackToTop && scrollTop.value > props.backToTopThreshold
})

// 事件处理函数
function handleSidebarToggle(collapsed: boolean) {
  isSidebarCollapsed.value = collapsed
  emit('sidebar-toggle', collapsed)
  localStorage.setItem('sidebar-collapsed', collapsed.toString())
}

function toggleMobileMenu() {
  isMobileMenuOpen.value = !isMobileMenuOpen.value
  emit('mobile-menu-toggle', isMobileMenuOpen.value)
}

function closeMobileMenu() {
  isMobileMenuOpen.value = false
  emit('mobile-menu-toggle', false)
}

function toggleSidebar() {
  if (props.sidebarCollapsible) {
    isSidebarCollapsed.value = !isSidebarCollapsed.value
    emit('sidebar-toggle', isSidebarCollapsed.value)
    localStorage.setItem('sidebar-collapsed', isSidebarCollapsed.value.toString())
  }
}

function scrollToTop() {
  window.scrollTo({
    top: 0,
    behavior: props.reducedMotion ? 'auto' : 'smooth'
  })
  emit('back-to-top')
}

// 监听器
watch(route, () => {
  closeMobileMenu()
  scrollTop.value = 0
})

watch(isMobile, (newValue) => {
  if (newValue) {
    closeMobileMenu()
  }
})

// 生命周期钩子
onMounted(() => {
  // 恢复侧边栏状态
  const savedCollapsed = localStorage.getItem('sidebar-collapsed')
  if (savedCollapsed) {
    isSidebarCollapsed.value = savedCollapsed === 'true'
  }

  // 监听滚动事件
  const handleScroll = () => {
    scrollTop.value = window.pageYOffset || document.documentElement.scrollTop
    emit('scroll', scrollTop.value)
  }

  // 监听窗口大小变化
  const handleResize = () => {
    windowWidth.value = window.innerWidth
    windowHeight.value = window.innerHeight
    emit('resize', windowWidth.value, windowHeight.value)
    
    // 自动关闭移动端菜单
    if (window.innerWidth > 768) {
      closeMobileMenu()
    }
  }

  window.addEventListener('scroll', handleScroll, { passive: true })
  window.addEventListener('resize', handleResize, { passive: true })

  // 初始化
  handleScroll()
  handleResize()
})

onUnmounted(() => {
  window.removeEventListener('scroll', () => {})
  window.removeEventListener('resize', () => {})
})
</script>

<style scoped>
/* 基础布局样式 */
.enhanced-app-layout {
  @apply min-h-screen flex flex-col;
  background: var(--gradient-background);
  color: var(--gray-800);
  position: relative;
  overflow: hidden;
}

/* 布局变体 */
.layout-default {
  /* 默认布局样式 */
}

.layout-minimal {
  --header-height: 48px;
  --footer-height: 32px;
  --sidebar-width: 200px;
  --sidebar-collapsed-width: 48px;
}

.layout-compact {
  --header-height: 56px;
  --footer-height: 40px;
  --sidebar-width: 180px;
  --sidebar-collapsed-width: 56px;
}

.layout-expanded {
  --header-height: 72px;
  --footer-height: 56px;
  --sidebar-width: 280px;
  --sidebar-collapsed-width: 80px;
}

/* 主题变体 */
.theme-light {
  /* 浅色主题 */
}

.theme-dark {
  /* 深色主题 */
  background: linear-gradient(135deg, #1a1a1a 0%, #2d2d2d 100%);
  color: var(--gray-200);
}

.theme-auto {
  /* 自动主题 */
}

/* 动画变体 */
.animation-none {
  /* 无动画 */
}

.animation-fade {
  /* 淡入淡出动画 */
}

.animation-slide {
  /* 滑动动画 */
}

.animation-scale {
  /* 缩放动画 */
}

.animation-bounce {
  /* 弹跳动画 */
}

/* 主要内容区域 */
.main-content-area {
  @apply flex-1 flex overflow-hidden relative;
  margin-top: var(--header-height);
  margin-bottom: var(--footer-height);
}

.main-content-area.with-header {
  margin-top: var(--header-height);
}

.main-content-area.with-footer {
  margin-bottom: var(--footer-height);
}

/* 内容包装器 */
.content-wrapper {
  @apply flex-1 flex flex-col overflow-hidden transition-all duration-300 ease-in-out;
  margin-left: 0;
}

.content-wrapper.ml-\[var\(--sidebar-width\)\] {
  margin-left: var(--sidebar-width);
}

.content-wrapper.ml-\[var\(--sidebar-collapsed-width\)\] {
  margin-left: var(--sidebar-collapsed-width);
}

/* 主内容 */
.main-content {
  @apply flex-1 overflow-y-auto;
  scrollbar-width: thin;
  scrollbar-color: var(--gray-300) var(--gray-100);
}

.main-content::-webkit-scrollbar {
  width: 6px;
}

.main-content::-webkit-scrollbar-track {
  background: var(--gray-100);
}

.main-content::-webkit-scrollbar-thumb {
  background: var(--gray-300);
  border-radius: var(--radius-full);
}

.main-content::-webkit-scrollbar-thumb:hover {
  background: var(--gray-400);
}

/* 面包屑容器 */
.breadcrumb-container {
  @apply bg-white dark:bg-gray-900 backdrop-blur-xl border-b border-slate-300 dark:border-gray-700 px-6 py-3 shadow-sm;
}

.breadcrumb-minimal {
  @apply bg-transparent border-0 shadow-none px-4 py-2;
}

.breadcrumb-compact {
  @apply px-4 py-2;
}

/* 页面标题部分 */
.page-title-section {
  @apply mb-6;
}

/* 内容插槽 */
.content-slot {
  @apply relative;
}

/* 加载覆盖层 */
.loading-overlay {
  @apply absolute inset-0 bg-white dark:bg-gray-900 bg-opacity-80 dark:bg-opacity-80 flex items-center justify-center z-10;
}

/* 离线覆盖层 */
.offline-overlay {
  @apply absolute inset-0 bg-gray-50 dark:bg-gray-800 flex items-center justify-center z-10;
}

/* 移动端遮罩层 */
.mobile-overlay {
  @apply fixed inset-0 bg-black bg-opacity-50 backdrop-blur-sm z-40 lg:hidden;
}

.mobile-overlay.transparent {
  @apply bg-opacity-30;
}

/* 通知容器 */
.notification-container {
  @apply fixed top-4 right-4 z-50 space-y-2 pointer-events-none;
}

.notification-container > * {
  @apply pointer-events-auto;
}

.notification-minimal {
  @apply top-2 right-2;
}

.notification-compact {
  @apply top-3 right-3;
}

/* FAB 容器 */
.fab-container {
  @apply fixed bottom-6 right-6 z-40;
}

.fab-minimal {
  @apply bottom-4 right-4;
}

.fab-compact {
  @apply bottom-5 right-5;
}

/* 返回顶部按钮 */
.back-to-top {
  @apply fixed bottom-6 left-6 z-40 w-12 h-12 bg-primary-500 hover:bg-primary-600 text-white rounded-full shadow-lg hover:shadow-xl transition-all duration-300 ease-in-out flex items-center justify-center focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2;
  opacity: 0;
  transform: scale(0.8);
  pointer-events: none;
}

.back-to-top.visible {
  opacity: 1;
  transform: scale(1);
  pointer-events: auto;
}

.back-to-top-minimal {
  @apply bottom-4 left-4 w-10 h-10;
}

.back-to-top-compact {
  @apply bottom-5 left-5 w-11 h-11;
}

/* 动画类 */
.fade-enter-active,
.fade-leave-active {
  transition: opacity var(--transition-normal);
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.slide-enter-active,
.slide-leave-active {
  transition: all var(--transition-normal);
}

.slide-enter-from {
  opacity: 0;
  transform: translateX(-20px);
}

.slide-leave-to {
  opacity: 0;
  transform: translateX(20px);
}

.scale-enter-active,
.scale-leave-active {
  transition: all var(--transition-normal);
}

.scale-enter-from,
.scale-leave-to {
  opacity: 0;
  transform: scale(0.9);
}

.bounce-enter-active,
.bounce-leave-active {
  transition: all var(--transition-normal);
}

.bounce-enter-from {
  opacity: 0;
  transform: scale(0.8);
}

.bounce-leave-to {
  opacity: 0;
  transform: scale(1.1);
}

/* 响应式设计 */
@media (max-width: 640px) {
  .enhanced-app-layout {
    --header-height: 56px;
    --footer-height: 40px;
    --content-padding: var(--space-4);
    --content-spacing: var(--space-4);
  }

  .content-wrapper {
    margin-left: 0 !important;
  }

  .breadcrumb-container {
    @apply px-4 py-2;
  }

  .main-content {
    @apply p-4;
  }

  .notification-container {
    @apply top-2 right-2 left-2;
  }

  .fab-container {
    @apply bottom-4 right-4;
  }

  .back-to-top {
    @apply bottom-4 left-4 w-10 h-10;
  }
}

@media (max-width: 480px) {
  .enhanced-app-layout {
    --header-height: 48px;
    --footer-height: 32px;
    --content-padding: var(--space-3);
    --content-spacing: var(--space-3);
  }

  .breadcrumb-container {
    @apply px-3 py-1;
  }

  .main-content {
    @apply p-3;
  }

  .page-title-section {
    @apply mb-4;
  }
}

@media (min-width: 1024px) {
  .content-area.mobile-menu-open .content-wrapper {
    margin-left: 0 !important;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .enhanced-app-layout,
  .content-wrapper,
  .main-content,
  .fade-enter-active,
  .fade-leave-active,
  .slide-enter-active,
  .slide-leave-active,
  .scale-enter-active,
  .scale-leave-active,
  .bounce-enter-active,
  .bounce-leave-active,
  .back-to-top {
    transition: none !important;
    animation: none !important;
  }

  .back-to-top:hover {
    transform: none !important;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .enhanced-app-layout {
    border: 2px solid var(--gray-800);
  }

  .breadcrumb-container {
    border-width: 2px;
  }

  .back-to-top {
    border: 2px solid var(--primary-600);
  }
}

/* 焦点样式 */
.enhanced-app-layout:focus-within {
  outline: none;
}

.back-to-top:focus-visible {
  @apply ring-2 ring-primary-500 ring-offset-2;
}

/* 打印样式 */
@media print {
  .enhanced-app-layout {
    @apply bg-white text-black;
  }

  .main-content-area,
  .content-wrapper,
  .main-content {
    margin: 0 !important;
    padding: 0 !important;
  }

  .breadcrumb-container,
  .mobile-overlay,
  .notification-container,
  .fab-container,
  .back-to-top {
    @apply hidden;
  }
}

/* 深色模式适配 */
@media (prefers-color-scheme: dark) {
  .theme-auto {
    background: linear-gradient(135deg, #1a1a1a 0%, #2d2d2d 100%);
    color: var(--gray-200);
  }

  .main-content::-webkit-scrollbar-track {
    background: var(--gray-800);
  }

  .main-content::-webkit-scrollbar-thumb {
    background: var(--gray-600);
  }

  .main-content::-webkit-scrollbar-thumb:hover {
    background: var(--gray-500);
  }
}

/* 加载状态 */
.enhanced-app-layout.loading {
  pointer-events: none;
}

.enhanced-app-layout.loading .content-wrapper {
  @apply opacity-50;
}

/* 离线状态 */
.enhanced-app-layout.offline {
  filter: grayscale(0.5);
}

/* 移动端特定样式 */
.mobile .content-wrapper {
  margin-left: 0 !important;
}

.mobile .sidebar-collapsed .content-wrapper {
  margin-left: 0 !important;
}

/* 桌面端特定样式 */
.desktop .mobile-overlay {
  @apply hidden;
}

/* 侧边栏折叠状态 */
.sidebar-collapsed .content-wrapper {
  margin-left: var(--sidebar-collapsed-width);
}

.sidebar-collapsed.mobile .content-wrapper {
  margin-left: 0;
}

/* 移动端菜单打开状态 */
.mobile-menu-open .mobile-overlay {
  @apply block;
}

.mobile-menu-open .content-wrapper {
  margin-left: 0 !important;
}
</style>