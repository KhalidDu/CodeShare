<template>
  <div :class="loaderClasses" :style="loaderStyles">
    <!-- 背景遮罩 -->
    <div v-if="overlay" class="loader-overlay" :class="overlayClasses"></div>

    <!-- 加载内容容器 -->
    <div class="loader-content" :class="contentClasses">
      <!-- 主要加载动画 -->
      <div class="loader-main" :class="mainClasses">
        <!-- Spinner 类型 -->
        <div v-if="type === 'spinner'" class="spinner-container">
          <div class="spinner" :class="spinnerClasses"></div>
          <div v-if="showTrail" class="spinner-trail"></div>
        </div>

        <!-- Dots 类型 -->
        <div v-else-if="type === 'dots'" class="dots-container">
          <div
            v-for="i in 3"
            :key="i"
            class="dot"
            :class="dotClasses"
            :style="{ animationDelay: `${(i - 1) * 0.1}s` }"
          ></div>
        </div>

        <!-- Pulse 类型 -->
        <div v-else-if="type === 'pulse'" class="pulse-container">
          <div class="pulse-ring"></div>
          <div class="pulse-core"></div>
        </div>

        <!-- Bars 类型 -->
        <div v-else-if="type === 'bars'" class="bars-container">
          <div
            v-for="i in 4"
            :key="i"
            class="bar"
            :class="barClasses"
            :style="{ animationDelay: `${(i - 1) * 0.1}s` }"
          ></div>
        </div>

        <!-- Circle 类型 -->
        <div v-else-if="type === 'circle'" class="circle-container">
          <svg class="circle-svg" :class="circleClasses" viewBox="0 0 50 50">
            <circle
              class="circle-path"
              cx="25"
              cy="25"
              r="20"
              fill="none"
              stroke="currentColor"
              stroke-width="3"
              stroke-linecap="round"
            ></circle>
          </svg>
        </div>

        <!-- Skeleton 类型 -->
        <div v-else-if="type === 'skeleton'" class="skeleton-container">
          <div class="skeleton-line" :class="skeletonClasses"></div>
          <div class="skeleton-line short" :class="skeletonClasses"></div>
          <div class="skeleton-line medium" :class="skeletonClasses"></div>
        </div>

        <!-- Custom 类型 -->
        <div v-else-if="type === 'custom'" class="custom-container">
          <slot name="custom"></slot>
        </div>
      </div>

      <!-- 文本标签 -->
      <div v-if="text" class="loader-text" :class="textClasses">
        {{ text }}
      </div>

      <!-- 副标题 -->
      <div v-if="subtext" class="loader-subtext" :class="subtextClasses">
        {{ subtext }}
      </div>

      <!-- 进度信息 -->
      <div v-if="showProgress" class="loader-progress" :class="progressClasses">
        <div class="progress-bar">
          <div
            class="progress-fill"
            :class="progressFillClasses"
            :style="{ width: `${progress}%` }"
          ></div>
        </div>
        <div v-if="showProgressText" class="progress-text">
          {{ progress }}%
        </div>
      </div>

      <!-- 操作按钮 -->
      <div v-if="showCancel" class="loader-actions">
        <button
          @click="$emit('cancel')"
          class="cancel-btn"
          :class="cancelBtnClasses"
        >
          {{ cancelText }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { cn } from '@/lib/utils'

// 定义Props接口
interface Props {
  type?: 'spinner' | 'dots' | 'pulse' | 'bars' | 'circle' | 'skeleton' | 'custom'
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl'
  color?: 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info'
  variant?: 'default' | 'gradient' | 'outline' | 'ghost'
  text?: string
  subtext?: string
  overlay?: boolean
  center?: boolean
  fullscreen?: boolean
  showProgress?: boolean
  progress?: number
  showProgressText?: boolean
  showCancel?: boolean
  cancelText?: string
  animation?: 'none' | 'spin' | 'pulse' | 'bounce' | 'elastic'
  speed?: 'slow' | 'normal' | 'fast'
  direction?: 'horizontal' | 'vertical'
  customClass?: string
}

// 定义Emits
interface Emits {
  (e: 'cancel'): void
  (e: 'complete'): void
  (e: 'update:progress', value: number): void
}

const props = withDefaults(defineProps<Props>(), {
  type: 'spinner',
  size: 'md',
  color: 'primary',
  variant: 'default',
  overlay: false,
  center: true,
  fullscreen: false,
  showProgress: false,
  progress: 0,
  showProgressText: false,
  showCancel: false,
  cancelText: '取消',
  animation: 'none',
  speed: 'normal',
  direction: 'horizontal',
  customClass: ''
})

const emit = defineEmits<Emits>()

// 响应式状态
const internalProgress = ref(props.progress)

// 计算加载器类名
const loaderClasses = computed(() => {
  const baseClasses = 'enhanced-loader'
  
  const layoutClasses = [
    props.fullscreen ? 'fixed inset-0 z-50' : 'relative',
    props.center ? 'flex items-center justify-center' : '',
    props.overlay ? 'bg-black/20 dark:bg-black/40 backdrop-blur-sm' : ''
  ].filter(Boolean).join(' ')

  return cn(baseClasses, layoutClasses, props.customClass)
})

// 计算加载器样式
const loaderStyles = computed(() => {
  return {
    zIndex: props.fullscreen ? 9999 : 'auto'
  }
})

// 计算遮罩类名
const overlayClasses = computed(() => {
  return [
    'absolute inset-0 bg-black/10 dark:bg-black/20 backdrop-blur-sm',
    props.fullscreen ? 'fixed' : ''
  ].join(' ')
})

// 计算内容类名
const contentClasses = computed(() => {
  const baseClasses = 'flex flex-col items-center justify-center space-y-3 p-4'
  
  const bgClasses = {
    default: 'bg-white dark:bg-gray-800 rounded-lg shadow-lg',
    gradient: 'bg-gradient-to-br from-white to-gray-50 dark:from-gray-800 dark:to-gray-900 rounded-lg shadow-lg',
    outline: 'bg-transparent border border-gray-200 dark:border-gray-700 rounded-lg',
    ghost: 'bg-transparent'
  }

  return cn(baseClasses, bgClasses[props.variant])
})

// 计算主要加载类名
const mainClasses = computed(() => {
  const sizeClasses = {
    xs: 'w-4 h-4',
    sm: 'w-6 h-6',
    md: 'w-8 h-8',
    lg: 'w-12 h-12',
    xl: 'w-16 h-16'
  }

  const colorClasses = {
    primary: 'text-blue-600 dark:text-blue-400',
    secondary: 'text-gray-600 dark:text-gray-400',
    success: 'text-green-600 dark:text-green-400',
    warning: 'text-yellow-600 dark:text-yellow-400',
    error: 'text-red-600 dark:text-red-400',
    info: 'text-cyan-600 dark:text-cyan-400'
  }

  const animationClasses = {
    none: '',
    spin: 'animate-spin',
    pulse: 'animate-pulse',
    bounce: 'animate-bounce',
    elastic: 'animate-elastic'
  }

  return cn(sizeClasses[props.size], colorClasses[props.color], animationClasses[props.animation])
})

// 计算Spinner类名
const spinnerClasses = computed(() => {
  const speedClasses = {
    slow: 'duration-2000',
    normal: 'duration-1000',
    fast: 'duration-500'
  }

  return cn('border-2 border-current border-t-transparent', speedClasses[props.speed])
})

// 计算点类名
const dotClasses = computed(() => {
  const speedClasses = {
    slow: 'animate-pulse-slow',
    normal: 'animate-pulse',
    fast: 'animate-pulse-fast'
  }

  return cn('w-2 h-2 bg-current rounded-full', speedClasses[props.speed])
})

// 计算条形类名
const barClasses = computed(() => {
  const speedClasses = {
    slow: 'animate-bars-slow',
    normal: 'animate-bars',
    fast: 'animate-bars-fast'
  }

  return cn('w-1 h-6 bg-current rounded', speedClasses[props.speed])
})

// 计算圆形类名
const circleClasses = computed(() => {
  const speedClasses = {
    slow: 'animate-spin-slow',
    normal: 'animate-spin',
    fast: 'animate-spin-fast'
  }

  return cn(speedClasses[props.speed])
})

// 计算骨架类名
const skeletonClasses = computed(() => {
  return cn('bg-gray-200 dark:bg-gray-700 rounded animate-pulse')
})

// 计算文本类名
const textClasses = computed(() => {
  const sizeClasses = {
    xs: 'text-xs',
    sm: 'text-sm',
    md: 'text-base',
    lg: 'text-lg',
    xl: 'text-xl'
  }

  const colorClasses = {
    primary: 'text-gray-700 dark:text-gray-300',
    secondary: 'text-gray-600 dark:text-gray-400',
    success: 'text-green-700 dark:text-green-300',
    warning: 'text-yellow-700 dark:text-yellow-300',
    error: 'text-red-700 dark:text-red-300',
    info: 'text-cyan-700 dark:text-cyan-300'
  }

  return cn('font-medium text-center', sizeClasses[props.size], colorClasses[props.color])
})

// 计算副文本类名
const subtextClasses = computed(() => {
  return cn('text-sm text-gray-500 dark:text-gray-400 text-center')
})

// 计算进度条类名
const progressClasses = computed(() => {
  return cn('w-full max-w-xs space-y-2')
})

// 计算进度填充类名
const progressFillClasses = computed(() => {
  const colorClasses = {
    primary: 'bg-blue-600 dark:bg-blue-400',
    secondary: 'bg-gray-600 dark:bg-gray-400',
    success: 'bg-green-600 dark:bg-green-400',
    warning: 'bg-yellow-600 dark:bg-yellow-400',
    error: 'bg-red-600 dark:bg-red-400',
    info: 'bg-cyan-600 dark:bg-cyan-400'
  }

  return cn('h-2 rounded-full transition-all duration-300 ease-out', colorClasses[props.color])
})

// 计算取消按钮类名
const cancelBtnClasses = computed(() => {
  return cn(
    'px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 dark:text-gray-300 dark:bg-gray-700 dark:border-gray-600 dark:hover:bg-gray-600'
  )
})

// 监听进度变化
onMounted(() => {
  if (props.showProgress) {
    internalProgress.value = props.progress
  }
})

// 暴露方法
defineExpose({
  updateProgress: (value: number) => {
    internalProgress.value = value
    emit('update:progress', value)
  },
  complete: () => {
    emit('complete')
  }
})
</script>

<style scoped>
/* Spinner 动画 */
.spinner {
  border-style: solid;
  border-radius: 50%;
}

.spinner-trail {
  position: absolute;
  inset: 0;
  border: 2px solid currentColor;
  border-radius: 50%;
  opacity: 0.3;
  animation: spin 1.5s linear infinite;
}

/* Dots 动画 */
.dots-container {
  display: flex;
  gap: 0.25rem;
}

.dot {
  animation: dot-pulse 1.4s ease-in-out infinite both;
}

@keyframes dot-pulse {
  0%, 80%, 100% {
    transform: scale(0);
    opacity: 0.5;
  }
  40% {
    transform: scale(1);
    opacity: 1;
  }
}

/* Pulse 动画 */
.pulse-container {
  position: relative;
}

.pulse-ring {
  position: absolute;
  inset: 0;
  border: 2px solid currentColor;
  border-radius: 50%;
  opacity: 0.3;
  animation: pulse-ring 1.5s ease-out infinite;
}

.pulse-core {
  position: absolute;
  inset: 25%;
  background-color: currentColor;
  border-radius: 50%;
  animation: pulse-core 1.5s ease-in-out infinite;
}

@keyframes pulse-ring {
  0% {
    transform: scale(0.8);
    opacity: 1;
  }
  100% {
    transform: scale(2);
    opacity: 0;
  }
}

@keyframes pulse-core {
  0%, 100% {
    transform: scale(1);
    opacity: 1;
  }
  50% {
    transform: scale(0.8);
    opacity: 0.8;
  }
}

/* Bars 动画 */
.bars-container {
  display: flex;
  gap: 0.25rem;
  align-items: center;
}

.bar {
  animation: bar-stretch 1.2s ease-in-out infinite;
}

@keyframes bar-stretch {
  0%, 40%, 100% {
    transform: scaleY(0.4);
  }
  20% {
    transform: scaleY(1);
  }
}

/* Circle SVG 动画 */
.circle-svg {
  transform: rotate(-90deg);
}

.circle-path {
  stroke-dasharray: 126;
  stroke-dashoffset: 126;
  animation: circle-dash 1.5s ease-in-out infinite;
}

@keyframes circle-dash {
  0% {
    stroke-dashoffset: 126;
  }
  50% {
    stroke-dashoffset: 31.5;
  }
  100% {
    stroke-dashoffset: 126;
  }
}

/* Skeleton 动画 */
.skeleton-container {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  width: 100%;
  max-w-xs;
}

.skeleton-line {
  height: 0.75rem;
}

.skeleton-line.short {
  width: 60%;
}

.skeleton-line.medium {
  width: 80%;
}

/* 进度条 */
.progress-bar {
  width: 100%;
  height: 0.5rem;
  background-color: #e5e7eb;
  border-radius: 9999px;
  overflow: hidden;
  dark:bg-gray-700;
}

.progress-text {
  text-align: center;
  font-size: 0.875rem;
  color: #6b7280;
  dark:text-gray-400;
}

/* 自定义动画速度 */
.animate-pulse-slow {
  animation-duration: 2s;
}

.animate-pulse-fast {
  animation-duration: 0.5s;
}

.animate-spin-slow {
  animation-duration: 2s;
}

.animate-spin-fast {
  animation-duration: 0.5s;
}

.animate-bars-slow {
  animation-duration: 1.8s;
}

.animate-bars-fast {
  animation-duration: 0.8s;
}

/* 弹性动画 */
.animate-elastic {
  animation: elastic-bounce 1s ease-in-out infinite;
}

@keyframes elastic-bounce {
  0%, 100% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.1);
  }
}

/* 响应式优化 */
@media (max-width: 768px) {
  .loader-content {
    padding: 1rem;
  }
  
  .text-lg {
    font-size: 1rem;
  }
  
  .text-xl {
    font-size: 1.125rem;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .spinner,
  .dot,
  .pulse-ring,
  .pulse-core,
  .bar,
  .circle-svg,
  .skeleton-line {
    animation: none !important;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .spinner {
    border-width: 3px;
  }
  
  .progress-bar {
    border: 1px solid currentColor;
  }
}
</style>