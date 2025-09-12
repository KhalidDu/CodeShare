<template>
  <Transition
    :name="name"
    :mode="mode"
    :appear="appear"
    :duration="duration"
    :css="css"
    @before-enter="onBeforeEnter"
    @enter="onEnter"
    @after-enter="onAfterEnter"
    @enter-cancelled="onEnterCancelled"
    @before-leave="onBeforeLeave"
    @leave="onLeave"
    @after-leave="onAfterLeave"
    @leave-cancelled="onLeaveCancelled"
  >
    <slot></slot>
  </Transition>
</template>

<script setup lang="ts">
import { computed } from 'vue'

// 定义Props接口
interface Props {
  name?: string
  type?: 'fade' | 'slide' | 'scale' | 'bounce' | 'elastic' | 'flip' | 'rotate' | 'zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'collapse' | 'expand'
  mode?: 'in-out' | 'out-in' | 'default'
  appear?: boolean
  duration?: number | { enter: number; leave: number }
  css?: boolean
  delay?: number
  easing?: string
  direction?: 'up' | 'down' | 'left' | 'right'
  distance?: number
  scale?: number
  opacity?: boolean
  stagger?: boolean
  staggerDelay?: number
  customClass?: string
}

// 定义Emits
interface Emits {
  (e: 'before-enter', el: Element): void
  (e: 'enter', el: Element, done: () => void): void
  (e: 'after-enter', el: Element): void
  (e: 'enter-cancelled', el: Element): void
  (e: 'before-leave', el: Element): void
  (e: 'leave', el: Element, done: () => void): void
  (e: 'after-leave', el: Element): void
  (e: 'leave-cancelled', el: Element): void
}

const props = withDefaults(defineProps<Props>(), {
  type: 'fade',
  mode: 'default',
  appear: false,
  css: true,
  delay: 0,
  easing: 'ease',
  direction: 'up',
  distance: 30,
  scale: 0.8,
  opacity: true,
  stagger: false,
  staggerDelay: 50,
  customClass: ''
})

const emit = defineEmits<Emits>()

// 计算过渡名称
const name = computed(() => {
  if (props.name) return props.name
  
  const typeMap: Record<string, string> = {
    fade: 'animated-fade',
    slide: `animated-slide-${props.direction}`,
    'slide-up': 'animated-slide-up',
    'slide-down': 'animated-slide-down',
    'slide-left': 'animated-slide-left',
    'slide-right': 'animated-slide-right',
    scale: 'animated-scale',
    bounce: 'animated-bounce',
    elastic: 'animated-elastic',
    flip: 'animated-flip',
    rotate: 'animated-rotate',
    zoom: 'animated-zoom',
    collapse: 'animated-collapse',
    expand: 'animated-expand'
  }
  
  return typeMap[props.type] || 'animated-fade'
})

// 计算持续时间
const duration = computed(() => {
  if (typeof props.duration === 'number') {
    return {
      enter: props.duration,
      leave: props.duration
    }
  }
  
  return props.duration || {
    enter: 300,
    leave: 300
  }
})

// 事件处理
function onBeforeEnter(el: Element) {
  emit('before-enter', el)
  
  if (props.stagger) {
    const element = el as HTMLElement
    const children = element.children
    Array.from(children).forEach((child, index) => {
      const childElement = child as HTMLElement
      childElement.style.transitionDelay = `${index * props.staggerDelay}ms`
    })
  }
}

function onEnter(el: Element, done: () => void) {
  emit('enter', el, done)
}

function onAfterEnter(el: Element) {
  emit('after-enter', el)
}

function onEnterCancelled(el: Element) {
  emit('enter-cancelled', el)
}

function onBeforeLeave(el: Element) {
  emit('before-leave', el)
}

function onLeave(el: Element, done: () => void) {
  emit('leave', el, done)
}

function onAfterLeave(el: Element) {
  emit('after-leave', el)
}

function onLeaveCancelled(el: Element) {
  emit('leave-cancelled', el)
}

// 暴露方法
defineExpose({
  name,
  duration
})
</script>

<style scoped>
/* 淡入淡出 */
.animated-fade-enter-active,
.animated-fade-leave-active {
  transition: opacity 0.3s ease;
}

.animated-fade-enter-from,
.animated-fade-leave-to {
  opacity: 0;
}

/* 滑动动画 */
.animated-slide-up-enter-active,
.animated-slide-up-leave-active {
  transition: all 0.3s ease;
}

.animated-slide-up-enter-from {
  opacity: 0;
  transform: translateY(30px);
}

.animated-slide-up-leave-to {
  opacity: 0;
  transform: translateY(-30px);
}

.animated-slide-down-enter-active,
.animated-slide-down-leave-active {
  transition: all 0.3s ease;
}

.animated-slide-down-enter-from {
  opacity: 0;
  transform: translateY(-30px);
}

.animated-slide-down-leave-to {
  opacity: 0;
  transform: translateY(30px);
}

.animated-slide-left-enter-active,
.animated-slide-left-leave-active {
  transition: all 0.3s ease;
}

.animated-slide-left-enter-from {
  opacity: 0;
  transform: translateX(30px);
}

.animated-slide-left-leave-to {
  opacity: 0;
  transform: translateX(-30px);
}

.animated-slide-right-enter-active,
.animated-slide-right-leave-active {
  transition: all 0.3s ease;
}

.animated-slide-right-enter-from {
  opacity: 0;
  transform: translateX(-30px);
}

.animated-slide-right-leave-to {
  opacity: 0;
  transform: translateX(30px);
}

/* 缩放动画 */
.animated-scale-enter-active,
.animated-scale-leave-active {
  transition: all 0.3s ease;
}

.animated-scale-enter-from,
.animated-scale-leave-to {
  opacity: 0;
  transform: scale(0.8);
}

/* 弹跳动画 */
.animated-bounce-enter-active {
  animation: bounce-in 0.5s ease;
}

.animated-bounce-leave-active {
  animation: bounce-out 0.3s ease;
}

/* 弹性动画 */
.animated-elastic-enter-active {
  animation: elastic-in 0.5s ease;
}

.animated-elastic-leave-active {
  animation: elastic-out 0.3s ease;
}

/* 翻转动画 */
.animated-flip-enter-active {
  animation: flip-in 0.5s ease;
}

.animated-flip-leave-active {
  animation: flip-out 0.3s ease;
}

/* 旋转动画 */
.animated-rotate-enter-active {
  animation: rotate-in 0.5s ease;
}

.animated-rotate-leave-active {
  animation: rotate-out 0.3s ease;
}

/* 缩放动画 */
.animated-zoom-enter-active {
  animation: zoom-in 0.5s ease;
}

.animated-zoom-leave-active {
  animation: zoom-out 0.3s ease;
}

/* 折叠动画 */
.animated-collapse-enter-active,
.animated-collapse-leave-active {
  transition: all 0.3s ease;
  overflow: hidden;
}

.animated-collapse-enter-from {
  opacity: 0;
  max-height: 0;
  transform: scaleY(0);
}

.animated-collapse-leave-to {
  opacity: 0;
  max-height: 0;
  transform: scaleY(0);
}

/* 展开动画 */
.animated-expand-enter-active,
.animated-expand-leave-active {
  transition: all 0.3s ease;
  overflow: hidden;
}

.animated-expand-enter-from {
  opacity: 0;
  max-height: 0;
  transform: scaleY(0);
}

.animated-expand-leave-to {
  opacity: 0;
  max-height: 0;
  transform: scaleY(0);
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

@keyframes rotate-in {
  0% {
    opacity: 0;
    transform: rotate(-180deg) scale(0.3);
  }
  100% {
    opacity: 1;
    transform: rotate(0deg) scale(1);
  }
}

@keyframes rotate-out {
  0% {
    opacity: 1;
    transform: rotate(0deg) scale(1);
  }
  100% {
    opacity: 0;
    transform: rotate(180deg) scale(0.3);
  }
}

@keyframes zoom-in {
  0% {
    opacity: 0;
    transform: scale(0.3);
  }
  100% {
    opacity: 1;
    transform: scale(1);
  }
}

@keyframes zoom-out {
  0% {
    opacity: 1;
    transform: scale(1);
  }
  100% {
    opacity: 0;
    transform: scale(0.3);
  }
}

/* 响应式优化 */
@media (max-width: 768px) {
  .animated-fade-enter-active,
  .animated-fade-leave-active,
  .animated-slide-up-enter-active,
  .animated-slide-up-leave-active,
  .animated-slide-down-enter-active,
  .animated-slide-down-leave-active,
  .animated-slide-left-enter-active,
  .animated-slide-left-leave-active,
  .animated-slide-right-enter-active,
  .animated-slide-right-leave-active,
  .animated-scale-enter-active,
  .animated-scale-leave-active {
    transition-duration: 0.2s;
  }
  
  .animated-bounce-enter-active,
  .animated-elastic-enter-active,
  .animated-flip-enter-active,
  .animated-rotate-enter-active,
  .animated-zoom-enter-active {
    animation-duration: 0.3s;
  }
  
  .animated-bounce-leave-active,
  .animated-elastic-leave-active,
  .animated-flip-leave-active,
  .animated-rotate-leave-active,
  .animated-zoom-leave-active {
    animation-duration: 0.2s;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .animated-fade-enter-active,
  .animated-fade-leave-active,
  .animated-slide-up-enter-active,
  .animated-slide-up-leave-active,
  .animated-slide-down-enter-active,
  .animated-slide-down-leave-active,
  .animated-slide-left-enter-active,
  .animated-slide-left-leave-active,
  .animated-slide-right-enter-active,
  .animated-slide-right-leave-active,
  .animated-scale-enter-active,
  .animated-scale-leave-active,
  .animated-collapse-enter-active,
  .animated-collapse-leave-active,
  .animated-expand-enter-active,
  .animated-expand-leave-active {
    transition: none !important;
  }
  
  .animated-bounce-enter-active,
  .animated-bounce-leave-active,
  .animated-elastic-enter-active,
  .animated-elastic-leave-active,
  .animated-flip-enter-active,
  .animated-flip-leave-active,
  .animated-rotate-enter-active,
  .animated-rotate-leave-active,
  .animated-zoom-enter-active,
  .animated-zoom-leave-active {
    animation: none !important;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .animated-fade-enter-from,
  .animated-fade-leave-to,
  .animated-slide-up-enter-from,
  .animated-slide-up-leave-to,
  .animated-slide-down-enter-from,
  .animated-slide-down-leave-to,
  .animated-slide-left-enter-from,
  .animated-slide-left-leave-to,
  .animated-slide-right-enter-from,
  .animated-slide-right-leave-to,
  .animated-scale-enter-from,
  .animated-scale-leave-to {
    opacity: 0 !important;
  }
}

/* 深色模式优化 */
.dark .animated-fade-enter-from,
.dark .animated-fade-leave-to,
.dark .animated-slide-up-enter-from,
.dark .animated-slide-up-leave-to,
.dark .animated-slide-down-enter-from,
.dark .animated-slide-down-leave-to,
.dark .animated-slide-left-enter-from,
.dark .animated-slide-left-leave-to,
.dark .animated-slide-right-enter-from,
.dark .animated-slide-right-leave-to,
.dark .animated-scale-enter-from,
.dark .animated-scale-leave-to {
  filter: brightness(0.8);
}
</style>