<template>
  <component
    :is="tag"
    :class="buttonClasses"
    :style="buttonStyles"
    :disabled="disabled || loading"
    :type="nativeType"
    @click="handleClick"
    @mouseenter="handleMouseEnter"
    @mouseleave="handleMouseLeave"
    @focus="handleFocus"
    @blur="handleBlur"
  >
    <!-- 按钮背景装饰 -->
    <div v-if="showDecoration" class="button-decoration">
      <div class="decoration-bg"></div>
    </div>

    <!-- 按钮内容 -->
    <div class="button-content" :class="contentClasses">
      <!-- 左侧图标 -->
      <div v-if="$slots.prefix || prefixIcon" class="button-prefix">
        <slot name="prefix">
          <i v-if="prefixIcon" :class="prefixIcon"></i>
        </slot>
      </div>

      <!-- 主要内容 -->
      <div class="button-main">
        <!-- 加载状态 -->
        <div v-if="loading" class="button-loading">
          <div class="loading-spinner" :class="loadingSpinnerClasses"></div>
        </div>

        <!-- 按钮文本 -->
        <span v-if="text" class="button-text" :class="textClasses">
          {{ text }}
        </span>

        <!-- 默认插槽 -->
        <slot v-else></slot>
      </div>

      <!-- 右侧图标 -->
      <div v-if="$slots.suffix || suffixIcon" class="button-suffix">
        <slot name="suffix">
          <i v-if="suffixIcon" :class="suffixIcon"></i>
        </slot>
      </div>
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

    <!-- 发光效果 -->
    <div v-if="glow" class="button-glow"></div>
  </component>
</template>

<script setup lang="ts">
import { computed, ref, reactive } from 'vue'
import { cn } from '@/lib/utils'

// 定义Props接口
interface Props {
  tag?: 'button' | 'a' | 'router-link'
  variant?: 'default' | 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'ghost' | 'outline' | 'gradient' | 'glass' | 'neumorphic'
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl'
  shape?: 'rounded' | 'pill' | 'square'
  animation?: 'none' | 'bounce' | 'elastic' | 'flip' | 'rotate' | 'pulse' | 'shake' | 'glow'
  text?: string
  prefixIcon?: string
  suffixIcon?: string
  nativeType?: 'button' | 'submit' | 'reset'
  disabled?: boolean
  loading?: boolean
  block?: boolean
  rounded?: boolean
  outlined?: boolean
  flat?: boolean
  raised?: boolean
  ripple?: boolean
  glow?: boolean
  showDecoration?: boolean
  hoverScale?: number
  activeScale?: number
  transitionDuration?: number
  customClass?: string
}

// 定义Emits
interface Emits {
  (e: 'click', event: MouseEvent): void
  (e: 'mouseenter', event: MouseEvent): void
  (e: 'mouseleave', event: MouseEvent): void
  (e: 'focus', event: FocusEvent): void
  (e: 'blur', event: FocusEvent): void
  (e: 'animation-start'): void
  (e: 'animation-end'): void
}

const props = withDefaults(defineProps<Props>(), {
  tag: 'button',
  variant: 'default',
  size: 'md',
  shape: 'rounded',
  animation: 'none',
  nativeType: 'button',
  disabled: false,
  loading: false,
  block: false,
  rounded: false,
  outlined: false,
  flat: false,
  raised: false,
  ripple: false,
  glow: false,
  showDecoration: false,
  hoverScale: 1.05,
  activeScale: 0.95,
  transitionDuration: 200,
  customClass: ''
})

const emit = defineEmits<Emits>()

// 响应式状态
const isHovered = ref(false)
const isFocused = ref(false)
const isActive = ref(false)
const ripples = ref<Array<{ style: Record<string, string> }>>([])

// 计算按钮类名
const buttonClasses = computed(() => {
  const baseClasses = 'animated-button relative inline-flex items-center justify-center font-medium transition-all duration-200 ease-in-out outline-none focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed disabled:pointer-events-none select-none'

  const variantClasses = {
    default: 'bg-gray-100 text-gray-700 hover:bg-gray-200 dark:bg-gray-700 dark:text-gray-300 dark:hover:bg-gray-600',
    primary: 'bg-blue-600 text-white hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600',
    secondary: 'bg-gray-600 text-white hover:bg-gray-700 dark:bg-gray-500',
    success: 'bg-green-600 text-white hover:bg-green-700 dark:bg-green-500',
    warning: 'bg-yellow-500 text-white hover:bg-yellow-600 dark:bg-yellow-600',
    error: 'bg-red-600 text-white hover:bg-red-700 dark:bg-red-500',
    ghost: 'bg-transparent text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800',
    outline: 'bg-transparent border border-current text-current hover:bg-current hover:text-white',
    gradient: 'bg-gradient-to-r from-blue-500 to-purple-600 text-white hover:from-blue-600 hover:to-purple-700',
    glass: 'bg-white/10 backdrop-blur-md border border-white/20 text-white hover:bg-white/20',
    neumorphic: 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 shadow-[inset_0_2px_4px_rgba(0,0,0,0.06),inset_0_-2px_4px_rgba(255,255,255,0.1),0_4px_6px_-1px_rgba(0,0,0,0.1),0_2px_4px_-1px_rgba(0,0,0,0.06)] dark:shadow-[inset_0_2px_4px_rgba(255,255,255,0.1),inset_0_-2px_4px_rgba(0,0,0,0.06),0_4px_6px_-1px_rgba(0,0,0,0.3),0_2px_4px_-1px_rgba(0,0,0,0.2)] hover:shadow-[inset_0_2px_4px_rgba(0,0,0,0.06),inset_0_-2px_4px_rgba(255,255,255,0.1),0_8px_12px_-1px_rgba(0,0,0,0.15),0_4px_8px_-1px_rgba(0,0,0,0.1)] dark:hover:shadow-[inset_0_2px_4px_rgba(255,255,255,0.1),inset_0_-2px_4px_rgba(0,0,0,0.06),0_8px_12px_-1px_rgba(0,0,0,0.4),0_4px_8px_-1px_rgba(0,0,0,0.2)]'
  }

  const sizeClasses = {
    xs: 'text-xs px-2 py-1 min-h-[24px]',
    sm: 'text-sm px-3 py-1.5 min-h-[32px]',
    md: 'text-base px-4 py-2 min-h-[40px]',
    lg: 'text-lg px-6 py-3 min-h-[48px]',
    xl: 'text-xl px-8 py-4 min-h-[56px]'
  }

  const shapeClasses = {
    rounded: 'rounded-md',
    pill: 'rounded-full',
    square: 'rounded-none'
  }

  const animationClasses = {
    none: '',
    bounce: 'hover:animate-bounce',
    elastic: 'hover:animate-elastic',
    flip: 'hover:animate-flip',
    rotate: 'hover:animate-rotate',
    pulse: 'hover:animate-pulse',
    shake: 'hover:animate-shake',
    glow: 'hover:animate-glow'
  }

  const stateClasses = [
    props.block ? 'w-full' : '',
    props.rounded ? 'rounded-full' : '',
    props.outlined ? 'border border-current bg-transparent' : '',
    props.flat ? 'shadow-none bg-transparent' : '',
    props.raised ? 'shadow-md hover:shadow-lg' : '',
    props.glow ? 'shadow-lg hover:shadow-xl' : '',
    !props.disabled && !props.loading ? 'cursor-pointer' : '',
    !props.disabled && !props.loading ? 'hover:scale-105 active:scale-95' : '',
    props.glow ? 'hover:shadow-lg hover:shadow-blue-500/25' : ''
  ].filter(Boolean).join(' ')

  return cn(
    baseClasses,
    variantClasses[props.variant],
    sizeClasses[props.size],
    shapeClasses[props.shape],
    animationClasses[props.animation],
    stateClasses,
    props.customClass
  )
})

// 计算按钮样式
const buttonStyles = computed(() => {
  const styles: Record<string, string> = {}
  
  if (isHovered.value && !props.disabled && !props.loading) {
    styles.transform = `scale(${props.hoverScale})`
  }
  
  if (isActive.value && !props.disabled && !props.loading) {
    styles.transform = `scale(${props.activeScale})`
  }
  
  styles.transitionDuration = `${props.transitionDuration}ms`
  
  return styles
})

// 计算内容类名
const contentClasses = computed(() => {
  return 'relative z-10 flex items-center justify-center space-x-2 w-full h-full'
})

// 计算文本类名
const textClasses = computed(() => {
  return [
    'font-medium',
    props.loading ? 'invisible' : ''
  ].join(' ')
})

// 计算加载动画类名
const loadingSpinnerClasses = computed(() => {
  const sizeClasses = {
    xs: 'w-3 h-3',
    sm: 'w-4 h-4',
    md: 'w-5 h-5',
    lg: 'w-6 h-6',
    xl: 'w-7 h-7'
  }

  return cn('border-2 border-white border-t-transparent rounded-full animate-spin', sizeClasses[props.size])
})

// 处理点击事件
function handleClick(event: MouseEvent) {
  if (!props.disabled && !props.loading) {
    emit('click', event)
    
    // 创建波纹效果
    if (props.ripple) {
      createRipple(event)
    }
    
    // 触发动画
    if (props.animation !== 'none') {
      emit('animation-start')
      setTimeout(() => {
        emit('animation-end')
      }, props.transitionDuration)
    }
  }
}

// 处理鼠标进入
function handleMouseEnter(event: MouseEvent) {
  if (!props.disabled && !props.loading) {
    isHovered.value = true
    emit('mouseenter', event)
  }
}

// 处理鼠标离开
function handleMouseLeave(event: MouseEvent) {
  if (!props.disabled && !props.loading) {
    isHovered.value = false
    isActive.value = false
    emit('mouseleave', event)
  }
}

// 处理焦点事件
function handleFocus(event: FocusEvent) {
  if (!props.disabled && !props.loading) {
    isFocused.value = true
    emit('focus', event)
  }
}

// 处理失焦事件
function handleBlur(event: FocusEvent) {
  if (!props.disabled && !props.loading) {
    isFocused.value = false
    emit('blur', event)
  }
}

// 创建波纹效果
function createRipple(event: MouseEvent) {
  const button = event.currentTarget as HTMLElement
  const rect = button.getBoundingClientRect()
  
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
  isFocused,
  isActive,
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
  background-color: rgba(255, 255, 255, 0.5);
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

/* 按钮装饰 */
.button-decoration {
  position: absolute;
  inset: 0;
  pointer-events: none;
  border-radius: inherit;
  overflow: hidden;
}

.decoration-bg {
  position: absolute;
  inset: 0;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.2) 0%, rgba(255, 255, 255, 0.05) 100%);
  opacity: 0;
  transition: opacity 0.3s ease;
}

.animated-button:hover .decoration-bg {
  opacity: 1;
}

/* 发光效果 */
.button-glow {
  position: absolute;
  inset: -4px;
  background: radial-gradient(circle, rgba(59, 130, 246, 0.3) 0%, transparent 70%);
  border-radius: inherit;
  opacity: 0;
  transition: opacity 0.3s ease;
  pointer-events: none;
}

.animated-button:hover .button-glow {
  opacity: 1;
}

/* 按钮内容布局 */
.button-content {
  position: relative;
  z-index: 10;
}

.button-loading {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.button-main {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 0;
}

.button-text {
  white-space: nowrap;
}

.button-prefix,
.button-suffix {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

/* 自定义动画 */
@keyframes elastic {
  0%, 100% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.05);
  }
}

.animate-elastic {
  animation: elastic 0.5s ease-in-out;
}

@keyframes flip {
  0% {
    transform: rotateY(0);
  }
  50% {
    transform: rotateY(180deg);
  }
  100% {
    transform: rotateY(360deg);
  }
}

.animate-flip {
  animation: flip 0.6s ease-in-out;
}

@keyframes rotate {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}

.animate-rotate {
  animation: rotate 0.5s ease-in-out;
}

@keyframes shake {
  0%, 100% {
    transform: translateX(0);
  }
  25% {
    transform: translateX(-5px);
  }
  75% {
    transform: translateX(5px);
  }
}

.animate-shake {
  animation: shake 0.5s ease-in-out;
}

@keyframes glow {
  0%, 100% {
    box-shadow: 0 0 5px rgba(59, 130, 246, 0.5);
  }
  50% {
    box-shadow: 0 0 20px rgba(59, 130, 246, 0.8), 0 0 30px rgba(59, 130, 246, 0.6);
  }
}

.animate-glow {
  animation: glow 1s ease-in-out infinite;
}

/* 响应式优化 */
@media (max-width: 768px) {
  .animated-button {
    transition-duration: 150ms;
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
  .animated-button {
    transition: none !important;
    animation: none !important;
  }
  
  .ripple {
    animation: none !important;
  }
  
  .loading-spinner {
    animation: none !important;
    border-top-color: transparent !important;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .animated-button {
    border: 2px solid currentColor;
  }
  
  .button-decoration {
    display: none;
  }
  
  .button-glow {
    display: none;
  }
}

/* 深色模式优化 */
.dark .ripple {
  background-color: rgba(255, 255, 255, 0.3);
}

.dark .button-glow {
  background: radial-gradient(circle, rgba(96, 165, 250, 0.3) 0%, transparent 70%);
}
</style>