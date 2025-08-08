<!--
  Toast 通知组件
  用于显示操作成功/失败的反馈信息
-->
<template>
  <div
    :class="[
      'toast-notification',
      `toast-${type}`,
      `position-${position}`
    ]"
    role="alert"
    :aria-live="type === 'error' ? 'assertive' : 'polite'"
  >
    <!-- 图标 -->
    <div class="toast-icon">
      <i :class="getIcon(type)"></i>
    </div>

    <!-- 内容 -->
    <div class="toast-content">
      <div v-if="title" class="toast-title">{{ title }}</div>
      <div class="toast-message">{{ message }}</div>
    </div>

    <!-- 操作按钮 -->
    <div v-if="actions.length > 0" class="toast-actions">
      <button
        v-for="action in actions"
        :key="action.label"
        @click="handleAction(action)"
        :class="['toast-action', `action-${action.type || 'default'}`]"
      >
        {{ action.label }}
      </button>
    </div>

    <!-- 关闭按钮 -->
    <button
      v-if="closable"
      @click="$emit('close')"
      class="toast-close"
      aria-label="关闭通知"
    >
      <i class="icon-x"></i>
    </button>

    <!-- 进度条 -->
    <div
      v-if="showProgress && duration > 0"
      class="toast-progress"
      :style="{ animationDuration: `${duration}ms` }"
    ></div>
  </div>
</template>

<script setup lang="ts">
// 通知类型
export type ToastType = 'success' | 'error' | 'warning' | 'info'

// 通知位置
export type ToastPosition = 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left' | 'top-center' | 'bottom-center'

// 操作按钮
export interface ToastAction {
  label: string
  type?: 'primary' | 'secondary' | 'danger'
  handler: () => void
}

// 组件属性
interface Props {
  type?: ToastType
  title?: string
  message: string
  duration?: number
  closable?: boolean
  showProgress?: boolean
  position?: ToastPosition
  actions?: ToastAction[]
}

// 组件事件
interface Emits {
  close: []
  action: [action: ToastAction]
}

const props = withDefaults(defineProps<Props>(), {
  type: 'info',
  duration: 5000,
  closable: true,
  showProgress: true,
  position: 'top-right',
  actions: () => []
})

const emit = defineEmits<Emits>()

/**
 * 获取通知类型对应的图标
 */
function getIcon(type: ToastType): string {
  const iconMap: Record<ToastType, string> = {
    success: 'icon-check-circle',
    error: 'icon-x-circle',
    warning: 'icon-alert-triangle',
    info: 'icon-info'
  }
  return iconMap[type]
}

/**
 * 处理操作按钮点击
 */
function handleAction(action: ToastAction) {
  action.handler()
  emit('action', action)
}
</script>

<style scoped>
.toast-notification {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  min-width: 300px;
  max-width: 500px;
  padding: 16px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  border-left: 4px solid;
  position: relative;
  overflow: hidden;
  animation: slideIn 0.3s ease-out;
}

/* 通知类型样式 */
.toast-success {
  border-left-color: #10b981;
  background: #f0fdf4;
}

.toast-success .toast-icon {
  color: #10b981;
}

.toast-error {
  border-left-color: #ef4444;
  background: #fef2f2;
}

.toast-error .toast-icon {
  color: #ef4444;
}

.toast-warning {
  border-left-color: #f59e0b;
  background: #fffbeb;
}

.toast-warning .toast-icon {
  color: #f59e0b;
}

.toast-info {
  border-left-color: #3b82f6;
  background: #eff6ff;
}

.toast-info .toast-icon {
  color: #3b82f6;
}

/* 图标 */
.toast-icon {
  flex-shrink: 0;
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-top: 2px;
}

.toast-icon i {
  font-size: 18px;
}

/* 内容 */
.toast-content {
  flex: 1;
  min-width: 0;
}

.toast-title {
  font-size: 14px;
  font-weight: 600;
  color: #1f2937;
  margin-bottom: 4px;
}

.toast-message {
  font-size: 13px;
  color: #4b5563;
  line-height: 1.4;
}

/* 操作按钮 */
.toast-actions {
  display: flex;
  gap: 8px;
  margin-top: 8px;
}

.toast-action {
  padding: 4px 12px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.action-primary {
  background: #3b82f6;
  color: white;
}

.action-primary:hover {
  background: #2563eb;
}

.action-secondary {
  background: #f3f4f6;
  color: #374151;
  border-color: #d1d5db;
}

.action-secondary:hover {
  background: #e5e7eb;
}

.action-danger {
  background: #ef4444;
  color: white;
}

.action-danger:hover {
  background: #dc2626;
}

.action-default {
  background: transparent;
  color: #6b7280;
  text-decoration: underline;
}

.action-default:hover {
  color: #374151;
}

/* 关闭按钮 */
.toast-close {
  flex-shrink: 0;
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: none;
  border: none;
  color: #9ca3af;
  cursor: pointer;
  border-radius: 4px;
  transition: all 0.2s;
}

.toast-close:hover {
  color: #6b7280;
  background: rgba(0, 0, 0, 0.05);
}

.toast-close i {
  font-size: 14px;
}

/* 进度条 */
.toast-progress {
  position: absolute;
  bottom: 0;
  left: 0;
  height: 3px;
  background: currentColor;
  opacity: 0.3;
  animation: progress linear;
  transform-origin: left;
}

@keyframes progress {
  from {
    transform: scaleX(1);
  }
  to {
    transform: scaleX(0);
  }
}

/* 入场动画 */
@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateX(100%);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

/* 位置样式（由容器组件处理） */
.position-top-right,
.position-top-left,
.position-top-center {
  animation-name: slideInFromTop;
}

.position-bottom-right,
.position-bottom-left,
.position-bottom-center {
  animation-name: slideInFromBottom;
}

@keyframes slideInFromTop {
  from {
    opacity: 0;
    transform: translateY(-100%);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes slideInFromBottom {
  from {
    opacity: 0;
    transform: translateY(100%);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* 响应式设计 */
@media (max-width: 640px) {
  .toast-notification {
    min-width: 280px;
    max-width: calc(100vw - 32px);
    padding: 12px;
  }

  .toast-actions {
    flex-direction: column;
    gap: 4px;
  }

  .toast-action {
    width: 100%;
    text-align: center;
  }
}

/* 减少动画效果（无障碍性） */
@media (prefers-reduced-motion: reduce) {
  .toast-notification {
    animation: none;
  }

  .toast-progress {
    animation: none;
    display: none;
  }
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .toast-notification {
    border: 2px solid;
    border-left-width: 6px;
  }

  .toast-success {
    border-color: #10b981;
    background: white;
  }

  .toast-error {
    border-color: #ef4444;
    background: white;
  }

  .toast-warning {
    border-color: #f59e0b;
    background: white;
  }

  .toast-info {
    border-color: #3b82f6;
    background: white;
  }
}
</style>
