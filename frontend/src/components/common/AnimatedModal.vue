<template>
  <Teleport to="body">
    <!-- 背景遮罩 -->
    <Transition
      :name="maskAnimation"
      appear
      @before-enter="onMaskBeforeEnter"
      @after-enter="onMaskAfterEnter"
      @before-leave="onMaskBeforeLeave"
      @after-leave="onMaskAfterLeave"
    >
      <div
        v-if="visible"
        class="modal-mask"
        :class="maskClasses"
        @click="handleMaskClick"
      >
        <!-- 模态框容器 -->
        <Transition
          :name="modalAnimation"
          appear
          @before-enter="onModalBeforeEnter"
          @after-enter="onModalAfterEnter"
          @before-leave="onModalBeforeLeave"
          @after-leave="onModalAfterLeave"
        >
          <div
            v-if="visible"
            class="modal-container"
            :class="containerClasses"
            :style="containerStyles"
            @click.stop
          >
            <!-- 模态框头部 -->
            <div v-if="showHeader" class="modal-header" :class="headerClasses">
              <div class="header-content">
                <!-- 标题 -->
                <div v-if="title || $slots.title" class="modal-title">
                  <slot name="title">
                    <h3 :class="titleClasses">{{ title }}</h3>
                  </slot>
                </div>

                <!-- 副标题 -->
                <div v-if="subtitle || $slots.subtitle" class="modal-subtitle">
                  <slot name="subtitle">
                    <p :class="subtitleClasses">{{ subtitle }}</p>
                  </slot>
                </div>
              </div>

              <!-- 关闭按钮 -->
              <button
                v-if="closable"
                type="button"
                class="modal-close"
                :class="closeButtonClasses"
                @click="handleClose"
              >
                <slot name="close-icon">
                  <svg class="close-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </slot>
              </button>
            </div>

            <!-- 模态框内容 -->
            <div class="modal-body" :class="bodyClasses">
              <slot></slot>
            </div>

            <!-- 模态框底部 -->
            <div v-if="showFooter" class="modal-footer" :class="footerClasses">
              <slot name="footer">
                <!-- 默认按钮组 -->
                <div class="footer-actions">
                  <button
                    v-if="showCancel"
                    type="button"
                    class="footer-btn cancel-btn"
                    :class="cancelButtonClasses"
                    @click="handleCancel"
                  >
                    {{ cancelText }}
                  </button>
                  
                  <button
                    v-if="showConfirm"
                    type="button"
                    class="footer-btn confirm-btn"
                    :class="confirmButtonClasses"
                    @click="handleConfirm"
                  >
                    {{ confirmText }}
                  </button>
                </div>
              </slot>
            </div>

            <!-- 加载状态 -->
            <div v-if="loading" class="modal-loading">
              <div class="loading-content">
                <div class="loading-spinner"></div>
                <span class="loading-text">{{ loadingText }}</span>
              </div>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, ref, watch, nextTick } from 'vue'
import { cn } from '@/lib/utils'

// 定义Props接口
interface Props {
  visible?: boolean
  title?: string
  subtitle?: string
  width?: string | number
  maxWidth?: string | number
  height?: string | number
  maxHeight?: string | number
  variant?: 'default' | 'card' | 'dialog' | 'fullscreen' | 'drawer' | 'panel'
  size?: 'sm' | 'md' | 'lg' | 'xl' | 'full'
  position?: 'center' | 'top' | 'bottom' | 'left' | 'right'
  animation?: 'fade' | 'slide' | 'scale' | 'bounce' | 'elastic' | 'flip' | 'rotate' | 'zoom'
  maskAnimation?: 'fade' | 'blur' | 'scale'
  modalAnimation?: 'fade' | 'slide' | 'scale' | 'bounce' | 'elastic' | 'flip' | 'rotate' | 'zoom'
  closable?: boolean
  maskClosable?: boolean
  showHeader?: boolean
  showFooter?: boolean
  showCancel?: boolean
  showConfirm?: boolean
  cancelText?: string
  confirmText?: string
  loading?: boolean
  loadingText?: string
  scrollable?: boolean
  destroyOnClose?: boolean
  zIndex?: number
  customClass?: string
}

// 定义Emits
interface Emits {
  (e: 'update:visible', visible: boolean): void
  (e: 'open'): void
  (e: 'opened'): void
  (e: 'close'): void
  (e: 'closed'): void
  (e: 'cancel'): void
  (e: 'confirm'): void
  (e: 'mask-click'): void
}

const props = withDefaults(defineProps<Props>(), {
  visible: false,
  width: '500px',
  maxWidth: '90vw',
  height: 'auto',
  maxHeight: '90vh',
  variant: 'default',
  size: 'md',
  position: 'center',
  animation: 'scale',
  maskAnimation: 'fade',
  modalAnimation: 'scale',
  closable: true,
  maskClosable: true,
  showHeader: true,
  showFooter: true,
  showCancel: true,
  showConfirm: true,
  cancelText: '取消',
  confirmText: '确认',
  loading: false,
  loadingText: '加载中...',
  scrollable: true,
  destroyOnClose: false,
  zIndex: 1000,
  customClass: ''
})

const emit = defineEmits<Emits>()

// 响应式状态
const internalVisible = ref(props.visible)

// 监听visible变化
watch(() => props.visible, (newVal) => {
  internalVisible.value = newVal
})

// 监听内部visible变化
watch(internalVisible, (newVal) => {
  emit('update:visible', newVal)
})

// 计算遮罩类名
const maskClasses = computed(() => {
  return [
    'fixed inset-0 bg-black/50 backdrop-blur-sm',
    props.maskAnimation === 'blur' ? 'backdrop-blur-md' : '',
    `z-${props.zIndex}`
  ].join(' ')
})

// 计算容器类名
const containerClasses = computed(() => {
  const baseClasses = 'modal-container relative bg-white dark:bg-gray-800 shadow-2xl'
  
  const variantClasses = {
    default: 'rounded-lg',
    card: 'rounded-xl shadow-lg',
    dialog: 'rounded-2xl shadow-xl',
    fullscreen: 'rounded-none inset-0 m-0',
    drawer: 'rounded-l-lg shadow-xl',
    panel: 'rounded-lg shadow-lg'
  }

  const sizeClasses = {
    sm: 'max-w-sm',
    md: 'max-w-md',
    lg: 'max-w-lg',
    xl: 'max-w-xl',
    full: 'max-w-full'
  }

  const positionClasses = {
    center: 'top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2',
    top: 'top-0 left-1/2 transform -translate-x-1/2',
    bottom: 'bottom-0 left-1/2 transform -translate-x-1/2',
    left: 'left-0 top-1/2 transform -translate-y-1/2',
    right: 'right-0 top-1/2 transform -translate-y-1/2'
  }

  return cn(
    baseClasses,
    variantClasses[props.variant],
    sizeClasses[props.size],
    positionClasses[props.position],
    props.customClass
  )
})

// 计算容器样式
const containerStyles = computed(() => {
  const styles: Record<string, string> = {}
  
  if (typeof props.width === 'number') {
    styles.width = `${props.width}px`
  } else {
    styles.width = props.width
  }
  
  if (typeof props.maxWidth === 'number') {
    styles.maxWidth = `${props.maxWidth}px`
  } else {
    styles.maxWidth = props.maxWidth
  }
  
  if (typeof props.height === 'number') {
    styles.height = `${props.height}px`
  } else {
    styles.height = props.height
  }
  
  if (typeof props.maxHeight === 'number') {
    styles.maxHeight = `${props.maxHeight}px`
  } else {
    styles.maxHeight = props.maxHeight
  }
  
  return styles
})

// 计算头部类名
const headerClasses = computed(() => {
  return [
    'modal-header flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-700',
    props.closable ? 'pr-12' : ''
  ].join(' ')
})

// 计算标题类名
const titleClasses = computed(() => {
  return 'text-lg font-semibold text-gray-900 dark:text-white'
})

// 计算副标题类名
const subtitleClasses = computed(() => {
  return 'text-sm text-gray-500 dark:text-gray-400 mt-1'
})

// 计算关闭按钮类名
const closeButtonClasses = computed(() => {
  return [
    'modal-close absolute top-4 right-4 w-8 h-8 flex items-center justify-center rounded-full',
    'text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-700',
    'transition-colors duration-200'
  ].join(' ')
})

// 计算主体类名
const bodyClasses = computed(() => {
  return [
    'modal-body p-6',
    props.scrollable ? 'overflow-y-auto max-h-[calc(90vh-200px)]' : ''
  ].join(' ')
})

// 计算底部类名
const footerClasses = computed(() => {
  return 'modal-footer flex items-center justify-end p-6 border-t border-gray-200 dark:border-gray-700 space-x-3'
})

// 计算取消按钮类名
const cancelButtonClasses = computed(() => {
  return [
    'footer-btn px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md',
    'hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500',
    'dark:text-gray-300 dark:bg-gray-700 dark:border-gray-600 dark:hover:bg-gray-600'
  ].join(' ')
})

// 计算确认按钮类名
const confirmButtonClasses = computed(() => {
  return [
    'footer-btn px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md',
    'hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500',
    'dark:bg-blue-500 dark:hover:bg-blue-600'
  ].join(' ')
})

// 处理遮罩点击
function handleMaskClick() {
  if (props.maskClosable && !props.loading) {
    emit('mask-click')
    handleClose()
  }
}

// 处理关闭
function handleClose() {
  if (!props.loading) {
    internalVisible.value = false
    emit('close')
  }
}

// 处理取消
function handleCancel() {
  if (!props.loading) {
    internalVisible.value = false
    emit('cancel')
  }
}

// 处理确认
function handleConfirm() {
  if (!props.loading) {
    emit('confirm')
  }
}

// 动画事件处理
function onMaskBeforeEnter() {
  document.body.style.overflow = 'hidden'
}

function onMaskAfterEnter() {
  emit('opened')
}

function onMaskBeforeLeave() {
  document.body.style.overflow = ''
}

function onMaskAfterLeave() {
  emit('closed')
}

function onModalBeforeEnter() {
  emit('open')
}

function onModalAfterEnter() {
  // 模态框完全打开后的处理
}

function onModalBeforeLeave() {
  // 模态框开始关闭前的处理
}

function onModalAfterLeave() {
  // 模态框完全关闭后的处理
}

// 暴露方法
defineExpose({
  open: () => {
    internalVisible.value = true
  },
  close: () => {
    internalVisible.value = false
  },
  toggle: () => {
    internalVisible.value = !internalVisible.value
  }
})
</script>

<style scoped>
/* 遮罩动画 */
.mask-fade-enter-active,
.mask-fade-leave-active {
  transition: opacity 0.3s ease;
}

.mask-fade-enter-from,
.mask-fade-leave-to {
  opacity: 0;
}

.mask-blur-enter-active,
.mask-blur-leave-active {
  transition: all 0.3s ease;
}

.mask-blur-enter-from,
.mask-blur-leave-to {
  opacity: 0;
  backdrop-filter: blur(0);
}

.mask-scale-enter-active,
.mask-scale-leave-active {
  transition: all 0.3s ease;
}

.mask-scale-enter-from,
.mask-scale-leave-to {
  opacity: 0;
  transform: scale(0.9);
}

/* 模态框动画 */
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.3s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

.modal-slide-enter-active,
.modal-slide-leave-active {
  transition: all 0.3s ease;
}

.modal-slide-enter-from {
  opacity: 0;
  transform: translateY(-50px);
}

.modal-slide-leave-to {
  opacity: 0;
  transform: translateY(50px);
}

.modal-scale-enter-active,
.modal-scale-leave-active {
  transition: all 0.3s ease;
}

.modal-scale-enter-from,
.modal-scale-leave-to {
  opacity: 0;
  transform: scale(0.9);
}

.modal-bounce-enter-active {
  animation: bounce-in 0.5s ease;
}

.modal-bounce-leave-active {
  animation: bounce-out 0.3s ease;
}

.modal-elastic-enter-active {
  animation: elastic-in 0.5s ease;
}

.modal-elastic-leave-active {
  animation: elastic-out 0.3s ease;
}

.modal-flip-enter-active {
  animation: flip-in 0.5s ease;
}

.modal-flip-leave-active {
  animation: flip-out 0.3s ease;
}

.modal-rotate-enter-active {
  animation: rotate-in 0.5s ease;
}

.modal-rotate-leave-active {
  animation: rotate-out 0.3s ease;
}

.modal-zoom-enter-active {
  animation: zoom-in 0.5s ease;
}

.modal-zoom-leave-active {
  animation: zoom-out 0.3s ease;
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

/* 关闭按钮样式 */
.close-icon {
  width: 16px;
  height: 16px;
}

/* 加载状态 */
.modal-loading {
  position: absolute;
  inset: 0;
  background-color: rgba(255, 255, 255, 0.9);
  dark:bg-gray-800/90;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10;
  border-radius: inherit;
}

.loading-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
}

.loading-spinner {
  width: 24px;
  height: 24px;
  border: 2px solid #e5e7eb;
  border-top-color: #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.loading-text {
  font-size: 0.875rem;
  color: #6b7280;
  dark:text-gray-400;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 响应式优化 */
@media (max-width: 768px) {
  .modal-container {
    width: 95vw !important;
    max-width: 95vw !important;
    margin: 1rem;
  }
  
  .modal-header,
  .modal-body,
  .modal-footer {
    padding: 1rem;
  }
  
  .modal-title {
    font-size: 1rem;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .modal-mask,
  .modal-container {
    transition: none !important;
    animation: none !important;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .modal-container {
    border: 2px solid currentColor;
  }
  
  .modal-header,
  .modal-footer {
    border-width: 2px;
  }
}

/* 深色模式优化 */
.dark .modal-loading {
  background-color: rgba(31, 41, 55, 0.95);
}

.dark .loading-spinner {
  border-color: #4b5563;
  border-top-color: #60a5fa;
}

.dark .loading-text {
  color: #9ca3af;
}
</style>