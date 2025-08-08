<!--
  加载遮罩组件
  全屏或局部区域的加载遮罩
-->
<template>
  <Teleport :to="teleportTo" :disabled="!teleport">
    <Transition name="loading-overlay" appear>
      <div
        v-if="visible"
        :class="[
          'loading-overlay',
          {
            'overlay-fullscreen': fullscreen,
            'overlay-absolute': !fullscreen,
            'overlay-blur': blur,
            'overlay-dark': dark
          }
        ]"
        @click="handleOverlayClick"
      >
        <div class="loading-content" @click.stop>
          <!-- 加载指示器 -->
          <LoadingSpinner
            :size="spinnerSize"
            :variant="spinnerVariant"
            :text="message"
            :show-text="showMessage"
          />

          <!-- 进度条 -->
          <div v-if="showProgress && progress !== undefined" class="progress-container">
            <div class="progress-bar">
              <div
                class="progress-fill"
                :style="{ width: `${Math.max(0, Math.min(100, progress))}%` }"
              ></div>
            </div>
            <div class="progress-text">
              {{ Math.round(progress) }}%
            </div>
          </div>

          <!-- 取消按钮 -->
          <button
            v-if="cancellable"
            @click="handleCancel"
            class="cancel-button"
            type="button"
          >
            <i class="icon-x"></i>
            取消
          </button>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import LoadingSpinner from './LoadingSpinner.vue'

// 组件属性
interface Props {
  visible?: boolean
  message?: string
  showMessage?: boolean
  progress?: number
  showProgress?: boolean
  cancellable?: boolean
  fullscreen?: boolean
  blur?: boolean
  dark?: boolean
  teleport?: boolean
  teleportTo?: string
  spinnerSize?: 'xs' | 'sm' | 'md' | 'lg' | 'xl'
  spinnerVariant?: 'primary' | 'secondary' | 'light' | 'dark'
  clickToClose?: boolean
}

// 组件事件
interface Emits {
  cancel: []
  close: []
}

const props = withDefaults(defineProps<Props>(), {
  visible: true,
  message: '加载中...',
  showMessage: true,
  showProgress: false,
  cancellable: false,
  fullscreen: true,
  blur: true,
  dark: false,
  teleport: true,
  teleportTo: 'body',
  spinnerSize: 'lg',
  spinnerVariant: 'primary',
  clickToClose: false
})

const emit = defineEmits<Emits>()

/**
 * 处理遮罩点击
 */
function handleOverlayClick() {
  if (props.clickToClose) {
    emit('close')
  }
}

/**
 * 处理取消操作
 */
function handleCancel() {
  emit('cancel')
}
</script>

<style scoped>
.loading-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9998;
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(2px);
}

.overlay-absolute {
  position: absolute;
}

.overlay-fullscreen {
  position: fixed;
}

.overlay-blur {
  backdrop-filter: blur(4px);
}

.overlay-dark {
  background: rgba(0, 0, 0, 0.6);
}

.overlay-dark .loading-content {
  color: white;
}

.loading-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  padding: 32px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  max-width: 90vw;
  max-height: 90vh;
  text-align: center;
}

.overlay-dark .loading-content {
  background: rgba(31, 41, 55, 0.95);
  color: white;
}

.progress-container {
  width: 100%;
  min-width: 200px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.progress-bar {
  width: 100%;
  height: 6px;
  background: #e5e7eb;
  border-radius: 3px;
  overflow: hidden;
}

.overlay-dark .progress-bar {
  background: rgba(255, 255, 255, 0.2);
}

.progress-fill {
  height: 100%;
  background: #3b82f6;
  border-radius: 3px;
  transition: width 0.3s ease;
}

.overlay-dark .progress-fill {
  background: #60a5fa;
}

.progress-text {
  font-size: 12px;
  color: #6b7280;
  font-weight: 500;
}

.overlay-dark .progress-text {
  color: #d1d5db;
}

.cancel-button {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  background: transparent;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  color: #6b7280;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
}

.cancel-button:hover {
  background: #f3f4f6;
  border-color: #9ca3af;
  color: #374151;
}

.overlay-dark .cancel-button {
  border-color: rgba(255, 255, 255, 0.3);
  color: #d1d5db;
}

.overlay-dark .cancel-button:hover {
  background: rgba(255, 255, 255, 0.1);
  border-color: rgba(255, 255, 255, 0.5);
  color: white;
}

/* 过渡动画 */
.loading-overlay-enter-active,
.loading-overlay-leave-active {
  transition: all 0.3s ease;
}

.loading-overlay-enter-from,
.loading-overlay-leave-to {
  opacity: 0;
  backdrop-filter: blur(0px);
}

.loading-overlay-enter-from .loading-content,
.loading-overlay-leave-to .loading-content {
  transform: scale(0.9);
  opacity: 0;
}

/* 响应式设计 */
@media (max-width: 640px) {
  .loading-content {
    padding: 24px;
    margin: 20px;
  }

  .progress-container {
    min-width: 150px;
  }
}

/* 减少动画效果（无障碍性） */
@media (prefers-reduced-motion: reduce) {
  .loading-overlay-enter-active,
  .loading-overlay-leave-active {
    transition: opacity 0.1s ease;
  }

  .loading-overlay-enter-from .loading-content,
  .loading-overlay-leave-to .loading-content {
    transform: none;
  }

  .progress-fill {
    transition: none;
  }
}
</style>
