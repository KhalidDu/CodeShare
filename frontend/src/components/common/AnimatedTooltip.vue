<template>
  <div class="tooltip-wrapper" ref="wrapperRef">
    <!-- 触发器 -->
    <div
      ref="triggerRef"
      class="tooltip-trigger"
      @mouseenter="handleMouseEnter"
      @mouseleave="handleMouseLeave"
      @click="handleClick"
      @focus="handleFocus"
      @blur="handleBlur"
    >
      <slot name="trigger"></slot>
    </div>

    <!-- 提示框内容 -->
    <Teleport to="body">
      <Transition
        :name="animation"
        appear
        @before-enter="onBeforeEnter"
        @after-enter="onAfterEnter"
        @before-leave="onBeforeLeave"
        @after-leave="onAfterLeave"
      >
        <div
          v-if="visible"
          ref="tooltipRef"
          class="tooltip-content"
          :class="tooltipClasses"
          :style="tooltipStyles"
        >
          <!-- 箭头 -->
          <div class="tooltip-arrow" :class="arrowClasses" :style="arrowStyles"></div>

          <!-- 提示框主体 -->
          <div class="tooltip-body" :class="bodyClasses">
            <!-- 标题 -->
            <div v-if="title || $slots.title" class="tooltip-title">
              <slot name="title">
                <span :class="titleClasses">{{ title }}</span>
              </slot>
            </div>

            <!-- 主要内容 -->
            <div class="tooltip-main">
              <slot>
                <span v-if="text" :class="textClasses">{{ text }}</span>
              </slot>
            </div>

            <!-- 副标题 -->
            <div v-if="subtitle || $slots.subtitle" class="tooltip-subtitle">
              <slot name="subtitle">
                <span :class="subtitleClasses">{{ subtitle }}</span>
              </slot>
            </div>

            <!-- 操作按钮 -->
            <div v-if="showActions" class="tooltip-actions">
              <slot name="actions">
                <button
                  v-if="showClose"
                  type="button"
                  class="tooltip-close"
                  :class="closeButtonClasses"
                  @click="handleClose"
                >
                  {{ closeText }}
                </button>
              </slot>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, reactive, nextTick, onMounted, onUnmounted } from 'vue'
import { cn } from '@/lib/utils'

// 定义Props接口
interface Props {
  text?: string
  title?: string
  subtitle?: string
  placement?: 'top' | 'bottom' | 'left' | 'right' | 'top-start' | 'top-end' | 'bottom-start' | 'bottom-end' | 'left-start' | 'left-end' | 'right-start' | 'right-end'
  trigger?: 'hover' | 'click' | 'focus' | 'manual'
  variant?: 'default' | 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info' | 'dark' | 'light'
  size?: 'sm' | 'md' | 'lg'
  animation?: 'fade' | 'slide' | 'scale' | 'bounce' | 'elastic' | 'flip'
  delay?: number
  duration?: number
  showArrow?: boolean
  showClose?: boolean
  closeText?: string
  showActions?: boolean
  disabled?: boolean
  interactive?: boolean
  maxWidth?: string | number
  zIndex?: number
  offset?: number
  customClass?: string
}

// 定义Emits
interface Emits {
  (e: 'show'): void
  (e: 'shown'): void
  (e: 'hide'): void
  (e: 'hidden'): void
  (e: 'click', event: MouseEvent): void
  (e: 'close'): void
}

const props = withDefaults(defineProps<Props>(), {
  placement: 'top',
  trigger: 'hover',
  variant: 'default',
  size: 'md',
  animation: 'fade',
  delay: 200,
  duration: 200,
  showArrow: true,
  showClose: false,
  closeText: '关闭',
  showActions: false,
  disabled: false,
  interactive: true,
  maxWidth: 300,
  zIndex: 1000,
  offset: 8,
  customClass: ''
})

const emit = defineEmits<Emits>()

// 响应式状态
const visible = ref(false)
const triggerRef = ref<HTMLElement>()
const tooltipRef = ref<HTMLElement>()
const wrapperRef = ref<HTMLElement>()
let showTimer: number | null = null
let hideTimer: number | null = null

// 计算提示框类名
const tooltipClasses = computed(() => {
  const baseClasses = 'tooltip-content absolute z-50 pointer-events-none'
  
  const variantClasses = {
    default: 'bg-gray-900 text-white dark:bg-gray-700',
    primary: 'bg-blue-600 text-white dark:bg-blue-500',
    secondary: 'bg-gray-600 text-white dark:bg-gray-500',
    success: 'bg-green-600 text-white dark:bg-green-500',
    warning: 'bg-yellow-500 text-white dark:bg-yellow-600',
    error: 'bg-red-600 text-white dark:bg-red-500',
    info: 'bg-cyan-600 text-white dark:bg-cyan-500',
    dark: 'bg-gray-900 text-white',
    light: 'bg-white text-gray-900 border border-gray-200 dark:bg-gray-800 dark:border-gray-700 dark:text-white'
  }

  const sizeClasses = {
    sm: 'text-xs px-2 py-1 min-w-[80px]',
    md: 'text-sm px-3 py-2 min-w-[120px]',
    lg: 'text-base px-4 py-3 min-w-[160px]'
  }

  const interactiveClasses = props.interactive ? 'pointer-events-auto' : ''

  return cn(
    baseClasses,
    variantClasses[props.variant],
    sizeClasses[props.size],
    interactiveClasses,
    props.customClass
  )
})

// 计算提示框样式
const tooltipStyles = computed(() => {
  const styles: Record<string, string> = {
    zIndex: props.zIndex.toString()
  }
  
  if (typeof props.maxWidth === 'number') {
    styles.maxWidth = `${props.maxWidth}px`
  } else {
    styles.maxWidth = props.maxWidth
  }
  
  return styles
})

// 计算箭头类名
const arrowClasses = computed(() => {
  const baseClasses = 'tooltip-arrow absolute w-3 h-3 transform rotate-45'
  
  const variantClasses = {
    default: 'bg-gray-900 dark:bg-gray-700',
    primary: 'bg-blue-600 dark:bg-blue-500',
    secondary: 'bg-gray-600 dark:bg-gray-500',
    success: 'bg-green-600 dark:bg-green-500',
    warning: 'bg-yellow-500 dark:bg-yellow-600',
    error: 'bg-red-600 dark:bg-red-500',
    info: 'bg-cyan-600 dark:bg-cyan-500',
    dark: 'bg-gray-900',
    light: 'bg-white border border-gray-200 dark:bg-gray-800 dark:border-gray-700'
  }

  return cn(baseClasses, variantClasses[props.variant])
})

// 计算箭头样式
const arrowStyles = computed(() => {
  const styles: Record<string, string> = {}
  
  // 根据placement设置箭头位置
  switch (props.placement) {
    case 'top':
    case 'top-start':
    case 'top-end':
      styles.bottom = '-6px'
      styles.left = '50%'
      styles.transform = 'translateX(-50%) rotate(45deg)'
      break
    case 'bottom':
    case 'bottom-start':
    case 'bottom-end':
      styles.top = '-6px'
      styles.left = '50%'
      styles.transform = 'translateX(-50%) rotate(45deg)'
      break
    case 'left':
    case 'left-start':
    case 'left-end':
      styles.right = '-6px'
      styles.top = '50%'
      styles.transform = 'translateY(-50%) rotate(45deg)'
      break
    case 'right':
    case 'right-start':
    case 'right-end':
      styles.left = '-6px'
      styles.top = '50%'
      styles.transform = 'translateY(-50%) rotate(45deg)'
      break
  }
  
  return styles
})

// 计算主体类名
const bodyClasses = computed(() => {
  return 'tooltip-body relative z-10'
})

// 计算标题类名
const titleClasses = computed(() => {
  return 'font-semibold block mb-1'
})

// 计算文本类名
const textClasses = computed(() => {
  return 'block'
})

// 计算副标题类名
const subtitleClasses = computed(() => {
  return 'text-xs opacity-75 block mt-1'
})

// 计算关闭按钮类名
const closeButtonClasses = computed(() => {
  return [
    'tooltip-close mt-2 px-2 py-1 text-xs rounded',
    'bg-white/20 hover:bg-white/30 text-white',
    'transition-colors duration-200'
  ].join(' ')
})

// 计算操作区域类名
const actionsClasses = computed(() => {
  return 'tooltip-actions mt-2 flex justify-end'
})

// 显示提示框
async function show() {
  if (props.disabled || visible.value) return
  
  if (hideTimer) {
    clearTimeout(hideTimer)
    hideTimer = null
  }
  
  showTimer = window.setTimeout(() => {
    visible.value = true
    emit('show')
    
    nextTick(() => {
      updatePosition()
      emit('shown')
    })
  }, props.delay)
}

// 隐藏提示框
function hide() {
  if (showTimer) {
    clearTimeout(showTimer)
    showTimer = null
  }
  
  hideTimer = window.setTimeout(() => {
    visible.value = false
    emit('hide')
    
    setTimeout(() => {
      emit('hidden')
    }, props.duration)
  }, props.delay)
}

// 更新位置
function updatePosition() {
  if (!triggerRef.value || !tooltipRef.value) return
  
  const triggerRect = triggerRef.value.getBoundingClientRect()
  const tooltipRect = tooltipRef.value.getBoundingClientRect()
  const scrollLeft = window.pageXOffset || document.documentElement.scrollLeft
  const scrollTop = window.pageYOffset || document.documentElement.scrollTop
  
  let left = 0
  let top = 0
  
  switch (props.placement) {
    case 'top':
      left = triggerRect.left + triggerRect.width / 2 - tooltipRect.width / 2
      top = triggerRect.top - tooltipRect.height - props.offset
      break
    case 'top-start':
      left = triggerRect.left
      top = triggerRect.top - tooltipRect.height - props.offset
      break
    case 'top-end':
      left = triggerRect.right - tooltipRect.width
      top = triggerRect.top - tooltipRect.height - props.offset
      break
    case 'bottom':
      left = triggerRect.left + triggerRect.width / 2 - tooltipRect.width / 2
      top = triggerRect.bottom + props.offset
      break
    case 'bottom-start':
      left = triggerRect.left
      top = triggerRect.bottom + props.offset
      break
    case 'bottom-end':
      left = triggerRect.right - tooltipRect.width
      top = triggerRect.bottom + props.offset
      break
    case 'left':
      left = triggerRect.left - tooltipRect.width - props.offset
      top = triggerRect.top + triggerRect.height / 2 - tooltipRect.height / 2
      break
    case 'left-start':
      left = triggerRect.left - tooltipRect.width - props.offset
      top = triggerRect.top
      break
    case 'left-end':
      left = triggerRect.left - tooltipRect.width - props.offset
      top = triggerRect.bottom - tooltipRect.height
      break
    case 'right':
      left = triggerRect.right + props.offset
      top = triggerRect.top + triggerRect.height / 2 - tooltipRect.height / 2
      break
    case 'right-start':
      left = triggerRect.right + props.offset
      top = triggerRect.top
      break
    case 'right-end':
      left = triggerRect.right + props.offset
      top = triggerRect.bottom - tooltipRect.height
      break
  }
  
  // 边界检查
  const viewportWidth = window.innerWidth
  const viewportHeight = window.innerHeight
  
  if (left < 0) left = 0
  if (left + tooltipRect.width > viewportWidth) left = viewportWidth - tooltipRect.width
  if (top < 0) top = 0
  if (top + tooltipRect.height > viewportHeight) top = viewportHeight - tooltipRect.height
  
  tooltipRef.value.style.left = `${left + scrollLeft}px`
  tooltipRef.value.style.top = `${top + scrollTop}px`
}

// 事件处理
function handleMouseEnter() {
  if (props.trigger === 'hover') {
    show()
  }
}

function handleMouseLeave() {
  if (props.trigger === 'hover') {
    hide()
  }
}

function handleClick(event: MouseEvent) {
  if (props.trigger === 'click') {
    event.preventDefault()
    if (visible.value) {
      hide()
    } else {
      show()
    }
  }
  emit('click', event)
}

function handleFocus() {
  if (props.trigger === 'focus') {
    show()
  }
}

function handleBlur() {
  if (props.trigger === 'focus') {
    hide()
  }
}

function handleClose() {
  hide()
  emit('close')
}

// 动画事件处理
function onBeforeEnter() {
  // 动画开始前的处理
}

function onAfterEnter() {
  // 动画完成后的处理
}

function onBeforeLeave() {
  // 动画开始前的处理
}

function onAfterLeave() {
  // 动画完成后的处理
}

// 生命周期钩子
onMounted(() => {
  window.addEventListener('scroll', updatePosition)
  window.addEventListener('resize', updatePosition)
})

onUnmounted(() => {
  window.removeEventListener('scroll', updatePosition)
  window.removeEventListener('resize', updatePosition)
  
  if (showTimer) {
    clearTimeout(showTimer)
  }
  if (hideTimer) {
    clearTimeout(hideTimer)
  }
})

// 暴露方法
defineExpose({
  show,
  hide,
  toggle: () => {
    if (visible.value) {
      hide()
    } else {
      show()
    }
  },
  updatePosition,
  visible
})
</script>

<style scoped>
/* 提示框动画 */
.tooltip-fade-enter-active,
.tooltip-fade-leave-active {
  transition: opacity 0.2s ease;
}

.tooltip-fade-enter-from,
.tooltip-fade-leave-to {
  opacity: 0;
}

.tooltip-slide-enter-active,
.tooltip-slide-leave-active {
  transition: all 0.2s ease;
}

.tooltip-slide-enter-from {
  opacity: 0;
  transform: translateY(-10px);
}

.tooltip-slide-leave-to {
  opacity: 0;
  transform: translateY(10px);
}

.tooltip-scale-enter-active,
.tooltip-scale-leave-active {
  transition: all 0.2s ease;
}

.tooltip-scale-enter-from,
.tooltip-scale-leave-to {
  opacity: 0;
  transform: scale(0.8);
}

.tooltip-bounce-enter-active {
  animation: bounce-in 0.4s ease;
}

.tooltip-bounce-leave-active {
  animation: bounce-out 0.2s ease;
}

.tooltip-elastic-enter-active {
  animation: elastic-in 0.4s ease;
}

.tooltip-elastic-leave-active {
  animation: elastic-out 0.2s ease;
}

.tooltip-flip-enter-active {
  animation: flip-in 0.4s ease;
}

.tooltip-flip-leave-active {
  animation: flip-out 0.2s ease;
}

/* 动画关键帧 */
@keyframes bounce-in {
  0% {
    opacity: 0;
    transform: scale(0.3);
  }
  50% {
    transform: scale(1.05);
  }
  70% {
    transform: scale(0.9);
  }
  100% {
    opacity: 1;
    transform: scale(1);
  }
}

@keyframes bounce-out {
  0% {
    opacity: 1;
    transform: scale(1);
  }
  100% {
    opacity: 0;
    transform: scale(0.3);
  }
}

@keyframes elastic-in {
  0% {
    opacity: 0;
    transform: scale(0.3);
  }
  50% {
    opacity: 1;
    transform: scale(1.1);
  }
  100% {
    opacity: 1;
    transform: scale(1);
  }
}

@keyframes elastic-out {
  0% {
    opacity: 1;
    transform: scale(1);
  }
  100% {
    opacity: 0;
    transform: scale(0.3);
  }
}

@keyframes flip-in {
  0% {
    opacity: 0;
    transform: perspective(400px) rotateY(90deg);
  }
  100% {
    opacity: 1;
    transform: perspective(400px) rotateY(0deg);
  }
}

@keyframes flip-out {
  0% {
    opacity: 1;
    transform: perspective(400px) rotateY(0deg);
  }
  100% {
    opacity: 0;
    transform: perspective(400px) rotateY(-90deg);
  }
}

/* 提示框主体布局 */
.tooltip-body {
  border-radius: 6px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
}

.tooltip-title,
.tooltip-main,
.tooltip-subtitle {
  position: relative;
  z-index: 1;
}

/* 箭头样式 */
.tooltip-arrow {
  z-index: 0;
}

/* 响应式优化 */
@media (max-width: 768px) {
  .tooltip-content {
    max-width: 250px !important;
  }
  
  .text-base {
    font-size: 0.875rem;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .tooltip-content {
    transition: none !important;
    animation: none !important;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .tooltip-body {
    border: 2px solid currentColor;
  }
  
  .tooltip-arrow {
    border: 2px solid currentColor;
  }
}

/* 深色模式优化 */
.dark .tooltip-body {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.3), 0 2px 4px -1px rgba(0, 0, 0, 0.2);
}

.dark .tooltip-close {
  background-color: rgba(255, 255, 255, 0.1);
}

.dark .tooltip-close:hover {
  background-color: rgba(255, 255, 255, 0.2);
}
</style>