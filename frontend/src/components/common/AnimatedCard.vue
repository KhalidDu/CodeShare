<template>
  <div
    :class="cardClasses"
    :style="cardStyles"
    @mouseenter="handleMouseEnter"
    @mouseleave="handleMouseLeave"
    @click="handleClick"
  >
    <!-- 卡片背景装饰 -->
    <div v-if="showDecoration" class="card-decoration">
      <div class="decoration-bg" :class="decorationClasses"></div>
    </div>

    <!-- 卡片内容 -->
    <div class="card-content" :class="contentClasses">
      <div v-if="$slots.header" class="card-header">
        <slot name="header"></slot>
      </div>

      <div v-if="$slots.image" class="card-image">
        <slot name="image"></slot>
      </div>

      <div class="card-body" :class="bodyClasses">
        <slot></slot>
      </div>

      <div v-if="$slots.footer" class="card-footer">
        <slot name="footer"></slot>
      </div>
    </div>

    <!-- 加载状态 -->
    <div v-if="loading" class="card-loading">
      <div class="loading-spinner">
        <div class="spinner"></div>
      </div>
    </div>

    <!-- 悬浮操作按钮 -->
    <div v-if="hoverActions && isHovered" class="card-actions">
      <slot name="actions"></slot>
    </div>

    <!-- 波纹效果 -->
    <div v-if="ripple" class="ripple-container">
      <div
        v-for="(ripple, index) in ripples"
        :key="index"
        class="ripple"
        :style="ripple.style"
        @animationend="removeRipple(index)"
      ></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, reactive } from 'vue'
import { cn } from '@/lib/utils'

// 定义Props接口
interface Props {
  variant?: 'default' | 'elevated' | 'outlined' | 'filled' | 'glass' | 'neumorphic'
  size?: 'sm' | 'md' | 'lg' | 'xl'
  rounded?: 'none' | 'sm' | 'md' | 'lg' | 'xl' | 'full'
  shadow?: 'none' | 'sm' | 'md' | 'lg' | 'xl' | '2xl'
  animation?: 'none' | 'fade' | 'slide' | 'scale' | 'bounce' | 'elastic' | 'flip' | 'rotate'
  interactive?: boolean
  loading?: boolean
  hoverable?: boolean
  clickable?: boolean
  hoverActions?: boolean
  ripple?: boolean
  showDecoration?: boolean
  gradient?: boolean
  border?: boolean
  disabled?: boolean
  selected?: boolean
  hoverScale?: number
  transitionDuration?: number
  customClass?: string
}

// 定义Emits
interface Emits {
  (e: 'click', event: MouseEvent): void
  (e: 'mouseenter', event: MouseEvent): void
  (e: 'mouseleave', event: MouseEvent): void
  (e: 'animation-end'): void
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'default',
  size: 'md',
  rounded: 'md',
  shadow: 'md',
  animation: 'none',
  interactive: true,
  loading: false,
  hoverable: true,
  clickable: false,
  hoverActions: false,
  ripple: false,
  showDecoration: false,
  gradient: false,
  border: false,
  disabled: false,
  selected: false,
  hoverScale: 1.02,
  transitionDuration: 300,
  customClass: ''
})

const emit = defineEmits<Emits>()

// 响应式状态
const isHovered = ref(false)
const ripples = ref<Array<{ style: Record<string, string> }>>([])

// 计算卡片类名
const cardClasses = computed(() => {
  const baseClasses = 'animated-card relative overflow-hidden transition-all duration-300 ease-in-out'
  
  const variantClasses = {
    default: 'bg-white dark:bg-gray-800',
    elevated: 'bg-white dark:bg-gray-800 shadow-lg',
    outlined: 'bg-transparent border border-gray-200 dark:border-gray-700',
    filled: 'bg-gray-100 dark:bg-gray-900',
    glass: 'bg-white/10 dark:bg-gray-800/10 backdrop-blur-md border border-white/20 dark:border-gray-700/20',
    neumorphic: 'bg-gray-100 dark:bg-gray-800 shadow-[inset_0_2px_4px_rgba(0,0,0,0.06),inset_0_-2px_4px_rgba(255,255,255,0.1),0_4px_6px_-1px_rgba(0,0,0,0.1),0_2px_4px_-1px_rgba(0,0,0,0.06)] dark:shadow-[inset_0_2px_4px_rgba(255,255,255,0.1),inset_0_-2px_4px_rgba(0,0,0,0.06),0_4px_6px_-1px_rgba(0,0,0,0.3),0_2px_4px_-1px_rgba(0,0,0,0.2)]'
  }

  const sizeClasses = {
    sm: 'p-3',
    md: 'p-4',
    lg: 'p-6',
    xl: 'p-8'
  }

  const roundedClasses = {
    none: 'rounded-none',
    sm: 'rounded-sm',
    md: 'rounded-md',
    lg: 'rounded-lg',
    xl: 'rounded-xl',
    full: 'rounded-full'
  }

  const shadowClasses = {
    none: 'shadow-none',
    sm: 'shadow-sm',
    md: 'shadow-md',
    lg: 'shadow-lg',
    xl: 'shadow-xl',
    '2xl': 'shadow-2xl'
  }

  const animationClasses = {
    none: '',
    fade: 'animate-fade-in',
    slide: 'animate-slide-in-up',
    scale: 'animate-scale-in',
    bounce: 'animate-bounce-in',
    elastic: 'animate-elastic-in',
    flip: 'animate-flip-in',
    rotate: 'animate-rotate-in'
  }

  const stateClasses = [
    props.interactive && !props.disabled ? 'cursor-pointer' : 'cursor-default',
    props.disabled ? 'opacity-50 pointer-events-none' : '',
    props.selected ? 'ring-2 ring-blue-500 dark:ring-blue-400' : '',
    props.gradient ? 'bg-gradient-to-br from-blue-50 to-purple-50 dark:from-gray-800 dark:to-gray-900' : '',
    props.border ? 'border border-gray-200 dark:border-gray-700' : '',
    props.hoverable && !props.disabled ? 'hover:shadow-xl hover:-translate-y-1' : '',
    props.clickable && !props.disabled ? 'active:scale-95' : ''
  ].filter(Boolean).join(' ')

  return cn(
    baseClasses,
    variantClasses[props.variant],
    sizeClasses[props.size],
    roundedClasses[props.rounded],
    shadowClasses[props.shadow],
    animationClasses[props.animation],
    stateClasses,
    props.customClass
  )
})

// 计算卡片样式
const cardStyles = computed(() => {
  const styles: Record<string, string> = {}
  
  if (isHovered.value && props.hoverable && !props.disabled) {
    styles.transform = `scale(${props.hoverScale})`
  }
  
  styles.transitionDuration = `${props.transitionDuration}ms`
  
  return styles
})

// 计算装饰类名
const decorationClasses = computed(() => {
  return [
    'absolute inset-0 opacity-10 dark:opacity-20',
    'bg-gradient-to-br from-blue-500 via-purple-500 to-pink-500',
    'animate-pulse-slow'
  ].join(' ')
})

// 计算内容类名
const contentClasses = computed(() => {
  return 'relative z-10'
})

// 计算主体类名
const bodyClasses = computed(() => {
  return [
    'flex-1',
    props.loading ? 'opacity-50' : ''
  ].join(' ')
})

// 处理鼠标进入
function handleMouseEnter(event: MouseEvent) {
  if (!props.disabled) {
    isHovered.value = true
    emit('mouseenter', event)
  }
}

// 处理鼠标离开
function handleMouseLeave(event: MouseEvent) {
  if (!props.disabled) {
    isHovered.value = false
    emit('mouseleave', event)
  }
}

// 处理点击事件
function handleClick(event: MouseEvent) {
  if (!props.disabled && props.clickable) {
    emit('click', event)
    
    // 创建波纹效果
    if (props.ripple) {
      createRipple(event)
    }
  }
}

// 创建波纹效果
function createRipple(event: MouseEvent) {
  const card = event.currentTarget as HTMLElement
  const rect = card.getBoundingClientRect()
  
  const size = Math.max(rect.width, rect.height)
  const x = event.clientX - rect.left - size / 2
  const y = event.clientY - rect.top - size / 2
  
  const ripple = {
    style: {
      width: `${size}px`,
      height: `${size}px`,
      left: `${x}px`,
      top: `${y}px`,
      animation: 'ripple 0.6s ease-out'
    }
  }
  
  ripples.value.push(ripple)
}

// 移除波纹效果
function removeRipple(index: number) {
  ripples.value.splice(index, 1)
}

// 暴露方法给父组件
defineExpose({
  isHovered,
  createRipple
})
</script>

<style scoped>
/* 波纹动画 */
@keyframes ripple {
  0% {
    transform: scale(0);
    opacity: 1;
  }
  100% {
    transform: scale(4);
    opacity: 0;
  }
}

.ripple {
  position: absolute;
  border-radius: 50%;
  background-color: rgba(59, 130, 246, 0.5);
  pointer-events: none;
  animation: ripple 0.6s ease-out;
}

.ripple-container {
  position: absolute;
  inset: 0;
  pointer-events: none;
  overflow: hidden;
  border-radius: inherit;
}

/* 卡片装饰 */
.card-decoration {
  position: absolute;
  inset: 0;
  pointer-events: none;
  border-radius: inherit;
  overflow: hidden;
}

.decoration-bg {
  position: absolute;
  inset: 0;
}

/* 加载状态 */
.card-loading {
  position: absolute;
  inset: 0;
  background-color: rgba(255, 255, 255, 0.8);
  dark:bg-gray-800/80;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 20;
}

.loading-spinner {
  display: flex;
  align-items: center;
  justify-content: center;
}

.spinner {
  width: 24px;
  height: 24px;
  border: 2px solid #e5e7eb;
  border-top-color: #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 悬浮操作按钮 */
.card-actions {
  position: absolute;
  top: 1rem;
  right: 1rem;
  display: flex;
  gap: 0.5rem;
  z-index: 15;
}

/* 卡片内容布局 */
.card-content {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.card-header {
  flex-shrink: 0;
}

.card-image {
  flex-shrink: 0;
  margin: -1rem -1rem 1rem -1rem;
}

.card-body {
  flex: 1;
  min-height: 0;
}

.card-footer {
  flex-shrink: 0;
  margin-top: auto;
}

/* 深色模式优化 */
.dark .card-loading {
  background-color: rgba(31, 41, 55, 0.9);
}

.dark .spinner {
  border-color: #4b5563;
  border-top-color: #60a5fa;
}

/* 响应式优化 */
@media (max-width: 768px) {
  .animated-card {
    transition-duration: 200ms;
  }
  
  .card-actions {
    top: 0.5rem;
    right: 0.5rem;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .animated-card {
    transition: none !important;
    animation: none !important;
  }
  
  .ripple {
    animation: none !important;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .animated-card {
    border: 2px solid currentColor;
  }
  
  .card-decoration {
    display: none;
  }
}
</style>