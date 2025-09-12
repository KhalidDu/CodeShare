<template>
  <div
    ref="containerRef"
    class="responsive-container"
    :class="containerClasses"
    :style="containerStyles"
    :data-container-id="containerId"
    v-bind="$attrs"
  >
    <!-- 默认插槽 -->
    <slot />
    
    <!-- 断点指示器（仅在开发模式下显示） -->
    <div v-if="showBreakpointIndicator" class="breakpoint-indicator">
      <span class="breakpoint-label">{{ currentBreakpoint }}</span>
      <span class="device-label">{{ deviceType }}</span>
      <span class="size-label">{{ windowWidth }}x{{ windowHeight }}</span>
    </div>
    
    <!-- 布局切换按钮（仅在开发模式下显示） -->
    <div v-if="showLayoutControls" class="layout-controls">
      <button
        v-for="layout in layoutTypes"
        :key="layout"
        class="layout-btn"
        :class="{ active: currentLayout === layout }"
        @click="switchLayout(layout)"
      >
        {{ getLayoutLabel(layout) }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useEnhancedBreakpoints, type LayoutType } from '@/composables/useEnhancedBreakpoints'
import { cn } from '@/lib/utils'

// Props 接口
interface Props {
  // 基础配置
  containerId?: string
  fluid?: boolean
  maxWidth?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | '4xl' | '5xl' | '6xl' | '7xl' | 'full' | string
  minWidth?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | string
  height?: 'auto' | 'screen' | 'full' | 'min-screen' | string
  
  // 布局配置
  layout?: LayoutType
  padding?: 'none' | 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | string
  margin?: 'none' | 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | string
  gap?: 'none' | 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | string
  
  // 网格配置
  grid?: boolean
  gridCols?: 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 'none'
  gridGap?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | string
  
  // 响应式配置
  responsive?: boolean
  responsivePadding?: Partial<Record<string, string>>
  responsiveMargin?: Partial<Record<string, string>>
  responsiveGap?: Partial<Record<string, string>>
  responsiveGridCols?: Partial<Record<string, number>>
  
  // 断点配置
  breakpoints?: Record<string, number>
  
  // 可视化配置
  showBreakpointIndicator?: boolean
  showLayoutControls?: boolean
  debugMode?: boolean
  
  // 主题配置
  variant?: 'default' | 'card' | 'glass' | 'neumorphic' | 'minimal'
  background?: 'none' | 'solid' | 'gradient' | 'transparent'
  
  // 动画配置
  animation?: 'none' | 'fade' | 'slide' | 'scale' | 'bounce'
  transition?: boolean
  
  // 状态配置
  loading?: boolean
  disabled?: boolean
  readonly?: boolean
  
  // 无障碍配置
  role?: string
  ariaLabel?: string
  ariaDescribedBy?: string
}

const props = withDefaults(defineProps<Props>(), {
  containerId: () => `container-${Date.now()}`,
  fluid: false,
  maxWidth: '6xl',
  height: 'auto',
  layout: 'auto',
  padding: 'md',
  margin: 'none',
  gap: 'none',
  grid: false,
  gridCols: 1,
  gridGap: 'md',
  responsive: true,
  showBreakpointIndicator: false,
  showLayoutControls: false,
  debugMode: false,
  variant: 'default',
  background: 'none',
  animation: 'none',
  transition: true,
  loading: false,
  disabled: false,
  readonly: false,
  role: 'region'
})

// 事件
interface Emits {
  'breakpoint-change': [breakpoint: string, width: number]
  'layout-change': [layout: LayoutType]
  'resize': [width: number, height: number]
  'click': [event: Event]
}

const emit = defineEmits<Emits>()

// 响应式系统
const breakpoints = useEnhancedBreakpoints({
  autoLayoutSwitching: true,
  adaptiveSpacing: true,
  responsiveTypography: true
})

// 容器引用
const containerRef = ref<HTMLElement>()

// 容器状态
const containerSize = ref({ width: 0, height: 0 })
const containerBreakpoint = ref<string>('default')
const isIntersecting = ref(false)

// 计算属性
const currentLayout = computed<LayoutType>(() => {
  if (props.layout !== 'auto') return props.layout
  return breakpoints.layoutState.value.currentLayout
})

const currentBreakpoint = computed(() => breakpoints.currentBreakpoint.value)

const deviceType = computed(() => breakpoints.deviceInfo.value.type)

const windowWidth = computed(() => breakpoints.windowWidth.value)

const windowHeight = computed(() => breakpoints.windowHeight.value)

// 布局类型
const layoutTypes: LayoutType[] = ['mobile', 'tablet', 'desktop-sidebar', 'desktop-full']

// 容器类名
const containerClasses = computed(() => {
  const classes = ['responsive-container']
  
  // 基础样式
  if (props.fluid) {
    classes.push('container-fluid')
  } else {
    classes.push('container')
  }
  
  // 最大宽度
  if (!props.fluid && props.maxWidth) {
    classes.push(`max-w-${props.maxWidth}`)
  }
  
  // 最小宽度
  if (props.minWidth) {
    classes.push(`min-w-${props.minWidth}`)
  }
  
  // 高度
  if (props.height) {
    classes.push(`h-${props.height}`)
  }
  
  // 内边距
  if (props.responsive && props.responsivePadding) {
    const padding = breakpoints.enhancedResponsiveValue(
      props.responsivePadding,
      props.padding || 'md',
      'spacing'
    )
    classes.push(`p-${padding}`)
  } else if (props.padding !== 'none') {
    classes.push(`p-${props.padding}`)
  }
  
  // 外边距
  if (props.responsive && props.responsiveMargin) {
    const margin = breakpoints.enhancedResponsiveValue(
      props.responsiveMargin,
      props.margin || 'none',
      'spacing'
    )
    if (margin !== 'none') {
      classes.push(`m-${margin}`)
    }
  } else if (props.margin !== 'none') {
    classes.push(`m-${props.margin}`)
  }
  
  // 间距
  if (props.responsive && props.responsiveGap) {
    const gap = breakpoints.enhancedResponsiveValue(
      props.responsiveGap,
      props.gap || 'none',
      'spacing'
    )
    if (gap !== 'none') {
      classes.push(`gap-${gap}`)
    }
  } else if (props.gap !== 'none') {
    classes.push(`gap-${props.gap}`)
  }
  
  // 网格布局
  if (props.grid) {
    classes.push('grid')
    
    if (props.responsive && props.responsiveGridCols) {
      const cols = breakpoints.enhancedResponsiveValue(
        props.responsiveGridCols,
        props.gridCols,
        'grid'
      )
      classes.push(`grid-cols-${cols}`)
    } else {
      classes.push(`grid-cols-${props.gridCols}`)
    }
    
    if (props.gridGap !== 'none') {
      classes.push(`gap-${props.gridGap}`)
    }
  }
  
  // 变体样式
  classes.push(`variant-${props.variant}`)
  
  // 背景样式
  classes.push(`bg-${props.background}`)
  
  // 动画样式
  if (props.animation !== 'none') {
    classes.push(`animate-${props.animation}`)
  }
  
  // 过渡效果
  if (props.transition) {
    classes.push('transition-all')
  }
  
  // 状态样式
  if (props.loading) classes.push('loading')
  if (props.disabled) classes.push('disabled')
  if (props.readonly) classes.push('readonly')
  
  // 调试模式
  if (props.debugMode) {
    classes.push('debug-mode')
  }
  
  // 断点类名
  classes.push(`breakpoint-${currentBreakpoint.value}`)
  classes.push(`device-${deviceType.value}`)
  classes.push(`layout-${currentLayout.value}`)
  
  return cn(classes)
})

// 容器样式
const containerStyles = computed(() => {
  const styles: Record<string, string> = {}
  
  // 自适应间距
  if (props.responsive) {
    const spacingScale = breakpoints.layoutState.value.spacingScale
    
    if (props.padding !== 'none' && !props.responsivePadding) {
      const basePadding = getSpacingValue(props.padding)
      styles.padding = `${basePadding * spacingScale}px`
    }
    
    if (props.gap !== 'none' && !props.responsiveGap) {
      const baseGap = getSpacingValue(props.gap)
      styles.gap = `${baseGap * spacingScale}px`
    }
  }
  
  // 自适应字体大小
  if (props.responsive) {
    const typographyScale = breakpoints.layoutState.value.typographyScale
    styles.fontSize = `${16 * typographyScale}px`
  }
  
  // 容器查询样式
  if (containerSize.value.width > 0) {
    styles.setProperty('--container-width', `${containerSize.value.width}px`)
    styles.setProperty('--container-height', `${containerSize.value.height}px`)
  }
  
  return styles
})

// 获取间距值
function getSpacingValue(spacing: string): number {
  const spacingMap: Record<string, number> = {
    'none': 0,
    'xs': 4,
    'sm': 8,
    'md': 16,
    'lg': 24,
    'xl': 32,
    '2xl': 48,
    '3xl': 64
  }
  
  return spacingMap[spacing] || 16
}

// 获取布局标签
function getLayoutLabel(layout: LayoutType): string {
  const labels: Record<LayoutType, string> = {
    'mobile': '移动',
    'tablet': '平板',
    'desktop-sidebar': '桌面',
    'desktop-full': '全宽'
  }
  return labels[layout] || layout
}

// 切换布局
function switchLayout(layout: LayoutType) {
  breakpoints.layoutState.value.currentLayout = layout
  emit('layout-change', layout)
}

// 更新容器尺寸
function updateContainerSize() {
  if (containerRef.value) {
    const rect = containerRef.value.getBoundingClientRect()
    containerSize.value = {
      width: rect.width,
      height: rect.height
    }
    
    // 确定容器断点
    const width = rect.width
    let breakpoint = 'default'
    
    if (width < 640) breakpoint = 'xs'
    else if (width < 768) breakpoint = 'sm'
    else if (width < 1024) breakpoint = 'md'
    else if (width < 1280) breakpoint = 'lg'
    else if (width < 1536) breakpoint = 'xl'
    else if (width < 1920) breakpoint = '2xl'
    else breakpoint = '3xl'
    
    if (breakpoint !== containerBreakpoint.value) {
      containerBreakpoint.value = breakpoint
      emit('breakpoint-change', breakpoint, width)
    }
  }
}

// 处理点击事件
function handleClick(event: Event) {
  if (!props.disabled) {
    emit('click', event)
  }
}

// 监听窗口大小变化
const handleResize = () => {
  updateContainerSize()
  emit('resize', windowWidth.value, windowHeight.value)
}

// 设置Intersection Observer
const setupIntersectionObserver = () => {
  if (typeof IntersectionObserver === 'undefined') return
  
  const observer = new IntersectionObserver(
    ([entry]) => {
      isIntersecting.value = entry.isIntersecting
    },
    {
      threshold: [0, 0.1, 0.5, 1],
      rootMargin: '50px'
    }
  )
  
  if (containerRef.value) {
    observer.observe(containerRef.value)
  }
  
  return observer
}

// 组件挂载时初始化
onMounted(() => {
  updateContainerSize()
  
  // 监听窗口大小变化
  window.addEventListener('resize', handleResize)
  
  // 设置Intersection Observer
  const observer = setupIntersectionObserver()
  
  // 清理函数
  onUnmounted(() => {
    window.removeEventListener('resize', handleResize)
    if (observer) observer.disconnect()
  })
})

// 监听响应式变化
watch(() => currentBreakpoint.value, (newBreakpoint) => {
  updateContainerSize()
})

watch(() => currentLayout.value, (newLayout) => {
  updateContainerSize()
})

// 暴露方法给父组件
defineExpose({
  containerRef,
  containerSize,
  containerBreakpoint,
  currentLayout,
  currentBreakpoint,
  isIntersecting,
  updateContainerSize,
  switchLayout
})
</script>

<style scoped>
.responsive-container {
  position: relative;
  box-sizing: border-box;
  transition: all 0.3s ease;
}

/* 容器基础样式 */
.container {
  width: 100%;
  margin-left: auto;
  margin-right: auto;
}

.container-fluid {
  width: 100%;
  max-width: none;
}

/* 变体样式 */
.variant-default {
  background-color: transparent;
}

.variant-card {
  background: var(--gradient-surface);
  border: 1px solid rgba(0, 0, 0, 0.06);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-md);
}

.variant-glass {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: var(--radius-lg);
}

.variant-neumorphic {
  background: var(--gray-100);
  border-radius: var(--radius-xl);
  box-shadow: 
    inset 2px 2px 5px rgba(255, 255, 255, 0.7),
    inset -2px -2px 5px rgba(0, 0, 0, 0.1);
}

.variant-minimal {
  background: transparent;
  border: none;
}

/* 背景样式 */
.bg-none {
  background: transparent;
}

.bg-solid {
  background: var(--gray-50);
}

.bg-gradient {
  background: var(--gradient-primary);
}

.bg-transparent {
  background: transparent;
}

/* 状态样式 */
.loading {
  opacity: 0.7;
  pointer-events: none;
}

.loading::after {
  content: '';
  position: absolute;
  top: 50%;
  left: 50%;
  width: 20px;
  height: 20px;
  margin: -10px 0 0 -10px;
  border: 2px solid var(--primary-500);
  border-top-color: transparent;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.disabled {
  opacity: 0.5;
  pointer-events: none;
  cursor: not-allowed;
}

.readonly {
  pointer-events: none;
}

/* 调试模式样式 */
.debug-mode {
  border: 2px dashed var(--primary-500);
  position: relative;
}

.debug-mode::before {
  content: attr(data-container-id);
  position: absolute;
  top: -20px;
  left: 0;
  font-size: 10px;
  color: var(--primary-600);
  background: var(--primary-50);
  padding: 2px 4px;
  border-radius: 2px;
}

/* 断点指示器 */
.breakpoint-indicator {
  position: fixed;
  bottom: 10px;
  right: 10px;
  background: rgba(0, 0, 0, 0.8);
  color: white;
  padding: 8px 12px;
  border-radius: 6px;
  font-size: 12px;
  font-family: monospace;
  z-index: 9999;
  display: flex;
  gap: 8px;
  align-items: center;
}

.breakpoint-label {
  color: #60a5fa;
  font-weight: bold;
}

.device-label {
  color: #34d399;
}

.size-label {
  color: #fbbf24;
}

/* 布局控制按钮 */
.layout-controls {
  position: fixed;
  top: 10px;
  right: 10px;
  background: rgba(0, 0, 0, 0.8);
  padding: 8px;
  border-radius: 6px;
  display: flex;
  gap: 4px;
  z-index: 9999;
}

.layout-btn {
  padding: 4px 8px;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  color: white;
  border-radius: 4px;
  font-size: 11px;
  cursor: pointer;
  transition: all 0.2s;
}

.layout-btn:hover {
  background: rgba(255, 255, 255, 0.2);
}

.layout-btn.active {
  background: var(--primary-500);
  border-color: var(--primary-500);
}

/* 动画 */
@keyframes spin {
  to { transform: rotate(360deg); }
}

/* 响应式断点类 */
.breakpoint-xs { --breakpoint-name: 'xs'; }
.breakpoint-sm { --breakpoint-name: 'sm'; }
.breakpoint-md { --breakpoint-name: 'md'; }
.breakpoint-lg { --breakpoint-name: 'lg'; }
.breakpoint-xl { --breakpoint-name: 'xl'; }
.breakpoint-2xl { --breakpoint-name: '2xl'; }
.breakpoint-3xl { --breakpoint-name: '3xl'; }

/* 设备类型类 */
.device-mobile { --device-type: 'mobile'; }
.device-tablet { --device-type: 'tablet'; }
.device-desktop { --device-type: 'desktop'; }
.device-large-desktop { --device-type: 'large-desktop'; }

/* 布局类型类 */
.layout-mobile { --layout-type: 'mobile'; }
.layout-tablet { --layout-type: 'tablet'; }
.layout-desktop-sidebar { --layout-type: 'desktop-sidebar'; }
.layout-desktop-full { --layout-type: 'desktop-full'; }

/* 响应式隐藏/显示 */
@media (max-width: 639px) {
  .hidden-mobile { display: none !important; }
  .mobile-only { display: block !important; }
}

@media (min-width: 640px) and (max-width: 767px) {
  .hidden-sm { display: none !important; }
  .sm-only { display: block !important; }
}

@media (min-width: 768px) and (max-width: 1023px) {
  .hidden-md { display: none !important; }
  .md-only { display: block !important; }
}

@media (min-width: 1024px) and (max-width: 1279px) {
  .hidden-lg { display: none !important; }
  .lg-only { display: block !important; }
}

@media (min-width: 1280px) and (max-width: 1535px) {
  .hidden-xl { display: none !important; }
  .xl-only { display: block !important; }
}

@media (min-width: 1536px) {
  .hidden-2xl { display: none !important; }
  .desktop-only { display: block !important; }
}

/* 深色模式 */
.dark .variant-card {
  background: rgba(30, 41, 59, 0.8);
  border-color: rgba(71, 85, 105, 0.5);
}

.dark .variant-neumorphic {
  background: var(--gray-800);
  box-shadow: 
    inset 2px 2px 5px rgba(255, 255, 255, 0.1),
    inset -2px -2px 5px rgba(0, 0, 0, 0.3);
}

.dark .bg-solid {
  background: var(--gray-900);
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .variant-card {
    border-width: 2px;
  }
  
  .layout-btn {
    border-width: 2px;
  }
}

/* 减少动画模式 */
@media (prefers-reduced-motion: reduce) {
  .responsive-container {
    transition: none;
  }
  
  .loading::after {
    animation: none;
    border-top-color: var(--primary-500);
  }
}
</style>