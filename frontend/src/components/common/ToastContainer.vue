<!--
  Toast 容器组件
  管理和显示所有 Toast 通知
-->
<template>
  <Teleport to="body">
    <div
      v-for="position in positions"
      :key="position"
      :class="[
        'toast-container',
        `container-${position}`
      ]"
    >
      <TransitionGroup name="toast-list" tag="div">
        <ToastNotification
          v-for="toast in getToastsByPosition(position)"
          :key="toast.id"
          :type="toast.type"
          :title="toast.title"
          :message="toast.message"
          :duration="toast.duration"
          :closable="toast.closable"
          :show-progress="toast.showProgress"
          :position="position"
          :actions="toast.actions"
          @close="removeToast(toast.id)"
          @action="handleToastAction(toast.id, $event)"
        />
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useToastStore } from '@/stores/toast'
import ToastNotification from './ToastNotification.vue'
import type { ToastPosition } from './ToastNotification.vue'

// 使用 Toast 状态管理
const toastStore = useToastStore()

// 所有可能的位置
const positions: ToastPosition[] = [
  'top-right',
  'top-left',
  'top-center',
  'bottom-right',
  'bottom-left',
  'bottom-center'
]

/**
 * 根据位置获取 Toast 列表
 */
function getToastsByPosition(position: ToastPosition) {
  return toastStore.toasts.filter(toast => toast.position === position)
}

/**
 * 移除 Toast
 */
function removeToast(id: string) {
  toastStore.removeToast(id)
}

/**
 * 处理 Toast 操作
 */
function handleToastAction(toastId: string, action: any) {
  // 执行操作后可以选择关闭 Toast
  // removeToast(toastId)
}
</script>

<style scoped>
.toast-container {
  position: fixed;
  z-index: 9999;
  pointer-events: none;
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-height: calc(100vh - 40px);
  overflow-y: auto;
}

.toast-container > * {
  pointer-events: auto;
}

/* 位置样式 */
.container-top-right {
  top: 20px;
  right: 20px;
}

.container-top-left {
  top: 20px;
  left: 20px;
}

.container-top-center {
  top: 20px;
  left: 50%;
  transform: translateX(-50%);
}

.container-bottom-right {
  bottom: 20px;
  right: 20px;
  flex-direction: column-reverse;
}

.container-bottom-left {
  bottom: 20px;
  left: 20px;
  flex-direction: column-reverse;
}

.container-bottom-center {
  bottom: 20px;
  left: 50%;
  transform: translateX(-50%);
  flex-direction: column-reverse;
}

/* 过渡动画 */
.toast-list-enter-active,
.toast-list-leave-active {
  transition: all 0.3s ease;
}

.toast-list-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.toast-list-leave-to {
  opacity: 0;
  transform: translateX(100%);
}

/* 左侧位置的动画 */
.container-top-left .toast-list-enter-from,
.container-bottom-left .toast-list-enter-from,
.container-top-left .toast-list-leave-to,
.container-bottom-left .toast-list-leave-to {
  transform: translateX(-100%);
}

/* 中心位置的动画 */
.container-top-center .toast-list-enter-from,
.container-bottom-center .toast-list-enter-from {
  transform: translateY(-100%);
}

.container-top-center .toast-list-leave-to,
.container-bottom-center .toast-list-leave-to {
  transform: translateY(-100%);
}

.container-bottom-center .toast-list-enter-from {
  transform: translateY(100%);
}

.container-bottom-center .toast-list-leave-to {
  transform: translateY(100%);
}

.toast-list-move {
  transition: transform 0.3s ease;
}

/* 响应式设计 */
@media (max-width: 640px) {
  .toast-container {
    left: 16px !important;
    right: 16px !important;
    top: 16px !important;
    bottom: 16px !important;
    transform: none !important;
  }

  .container-top-center,
  .container-bottom-center {
    left: 16px;
    right: 16px;
    transform: none;
  }
}

/* 滚动条样式 */
.toast-container::-webkit-scrollbar {
  width: 4px;
}

.toast-container::-webkit-scrollbar-track {
  background: transparent;
}

.toast-container::-webkit-scrollbar-thumb {
  background: rgba(0, 0, 0, 0.2);
  border-radius: 2px;
}

.toast-container::-webkit-scrollbar-thumb:hover {
  background: rgba(0, 0, 0, 0.3);
}

/* 减少动画效果（无障碍性） */
@media (prefers-reduced-motion: reduce) {
  .toast-list-enter-active,
  .toast-list-leave-active {
    transition: opacity 0.1s ease;
  }

  .toast-list-enter-from,
  .toast-list-leave-to {
    transform: none;
  }

  .toast-list-move {
    transition: none;
  }
}
</style>
