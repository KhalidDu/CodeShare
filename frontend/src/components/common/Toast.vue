<template>
  <Teleport to="body">
    <Transition
      name="toast"
      enter-active-class="toast-enter-active"
      leave-active-class="toast-leave-active"
      enter-from-class="toast-enter-from"
      leave-to-class="toast-leave-to"
    >
      <div
        v-if="visible"
        :class="[
          'toast',
          `toast--${type}`,
          {
            'toast--closable': closable
          }
        ]"
        :style="{ zIndex }"
      >
        <div class="toast__icon">
          <svg v-if="type === 'success'" viewBox="0 0 16 16" width="16" height="16">
            <path d="M13.78 4.22a.75.75 0 0 1 0 1.06l-7.25 7.25a.75.75 0 0 1-1.06 0L2.22 9.28a.75.75 0 0 1 1.06-1.06L6 10.94l6.72-6.72a.75.75 0 0 1 1.06 0Z"></path>
          </svg>
          <svg v-else-if="type === 'error'" viewBox="0 0 16 16" width="16" height="16">
            <path d="M3.72 3.72a.75.75 0 0 1 1.06 0L8 6.94l3.22-3.22a.75.75 0 1 1 1.06 1.06L9.06 8l3.22 3.22a.75.75 0 1 1-1.06 1.06L8 9.06l-3.22 3.22a.75.75 0 0 1-1.06-1.06L6.94 8 3.72 4.78a.75.75 0 0 1 0-1.06Z"></path>
          </svg>
          <svg v-else-if="type === 'warning'" viewBox="0 0 16 16" width="16" height="16">
            <path d="M6.457 1.047c.659-1.234 2.427-1.234 3.086 0l6.082 11.378A1.75 1.75 0 0 1 14.082 15H1.918a1.75 1.75 0 0 1-1.543-2.575Zm1.763.707a.25.25 0 0 0-.44 0L1.698 13.132a.25.25 0 0 0 .22.368h12.164a.25.25 0 0 0 .22-.368Zm.53 3.996v2.5a.75.75 0 0 1-1.5 0v-2.5a.75.75 0 0 1 1.5 0ZM9 11a1 1 0 1 1-2 0 1 1 0 0 1 2 0Z"></path>
          </svg>
          <svg v-else viewBox="0 0 16 16" width="16" height="16">
            <path d="M0 8a8 8 0 1 1 16 0A8 8 0 0 1 0 8Zm8-6.5a6.5 6.5 0 1 0 0 13 6.5 6.5 0 0 0 0-13ZM6.5 7.75A.75.75 0 0 1 7.25 7h1a.75.75 0 0 1 .75.75v2.75h.25a.75.75 0 0 1 0 1.5h-2a.75.75 0 0 1 0-1.5h.25v-2h-.25a.75.75 0 0 1-.75-.75ZM8 6a1 1 0 1 1 0-2 1 1 0 0 1 0 2Z"></path>
          </svg>
        </div>
        
        <div class="toast__content">
          <div v-if="title" class="toast__title">{{ title }}</div>
          <div class="toast__message">{{ message }}</div>
        </div>

        <button
          v-if="closable"
          @click="close"
          class="toast__close"
          aria-label="关闭"
        >
          <svg viewBox="0 0 16 16" width="12" height="12">
            <path d="M3.72 3.72a.75.75 0 0 1 1.06 0L8 6.94l3.22-3.22a.75.75 0 1 1 1.06 1.06L9.06 8l3.22 3.22a.75.75 0 1 1-1.06 1.06L8 9.06l-3.22 3.22a.75.75 0 0 1-1.06-1.06L6.94 8 3.72 4.78a.75.75 0 0 1 0-1.06Z"></path>
          </svg>
        </button>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'

interface Props {
  /** 消息内容 */
  message: string
  /** 消息标题 */
  title?: string
  /** 消息类型 */
  type?: 'success' | 'error' | 'warning' | 'info'
  /** 自动关闭时间（毫秒），0 表示不自动关闭 */
  duration?: number
  /** 是否显示关闭按钮 */
  closable?: boolean
  /** z-index 层级 */
  zIndex?: number
}

interface Emits {
  /** 关闭事件 */
  (e: 'close'): void
}

const props = withDefaults(defineProps<Props>(), {
  type: 'info',
  duration: 3000,
  closable: true,
  zIndex: 9999
})

const emit = defineEmits<Emits>()

// 响应式状态
const visible = ref(false)
let autoCloseTimer: number | null = null

/**
 * 显示 Toast
 */
const show = () => {
  visible.value = true
  
  // 设置自动关闭
  if (props.duration > 0) {
    autoCloseTimer = window.setTimeout(() => {
      close()
    }, props.duration)
  }
}

/**
 * 关闭 Toast
 */
const close = () => {
  visible.value = false
  
  // 清除自动关闭定时器
  if (autoCloseTimer) {
    clearTimeout(autoCloseTimer)
    autoCloseTimer = null
  }
  
  emit('close')
}

// 生命周期
onMounted(() => {
  show()
})

onUnmounted(() => {
  if (autoCloseTimer) {
    clearTimeout(autoCloseTimer)
  }
})

// 暴露方法给父组件
defineExpose({
  show,
  close
})
</script>

<style scoped>
.toast {
  position: fixed;
  top: 20px;
  right: 20px;
  min-width: 300px;
  max-width: 500px;
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 16px;
  background: #fff;
  border: 1px solid #e1e5e9;
  border-radius: 8px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  font-size: 14px;
  line-height: 1.5;
}

.toast--success {
  border-left: 4px solid #28a745;
}

.toast--success .toast__icon {
  color: #28a745;
}

.toast--error {
  border-left: 4px solid #dc3545;
}

.toast--error .toast__icon {
  color: #dc3545;
}

.toast--warning {
  border-left: 4px solid #ffc107;
}

.toast--warning .toast__icon {
  color: #856404;
}

.toast--info {
  border-left: 4px solid #17a2b8;
}

.toast--info .toast__icon {
  color: #17a2b8;
}

.toast__icon {
  flex-shrink: 0;
  margin-top: 2px;
}

.toast__icon svg {
  fill: currentColor;
}

.toast__content {
  flex: 1;
  min-width: 0;
}

.toast__title {
  font-weight: 600;
  color: #24292f;
  margin-bottom: 4px;
}

.toast__message {
  color: #656d76;
  word-wrap: break-word;
}

.toast__close {
  flex-shrink: 0;
  padding: 4px;
  border: none;
  background: transparent;
  color: #656d76;
  cursor: pointer;
  border-radius: 4px;
  transition: all 0.2s ease;
}

.toast__close:hover {
  background: rgba(0, 0, 0, 0.05);
  color: #24292f;
}

.toast__close svg {
  fill: currentColor;
}

/* 动画效果 */
.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(100%);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .toast {
    left: 20px;
    right: 20px;
    min-width: auto;
    max-width: none;
  }
  
  .toast-enter-from,
  .toast-leave-to {
    transform: translateY(-100%);
  }
}
</style>