<!--
  错误容器组件
  管理和显示所有错误通知
-->
<template>
  <Teleport to="body">
    <div
      v-if="visibleErrors.length > 0"
      class="error-container"
      role="region"
      aria-label="错误通知"
    >
      <TransitionGroup name="error-list" tag="div">
        <ErrorNotification
          v-for="error in visibleErrors"
          :key="error.id"
          :error="error"
          :retrying="retryingErrors.has(error.id)"
          @retry="handleRetry"
          @dismiss="handleDismiss"
        />
      </TransitionGroup>

      <!-- 批量操作 -->
      <div v-if="visibleErrors.length > 1" class="error-batch-actions">
        <button
          @click="handleDismissAll"
          class="btn btn-sm btn-ghost"
        >
          <i class="icon-x"></i>
          清除所有
        </button>

        <button
          v-if="retryableErrors.length > 0"
          @click="handleRetryAll"
          class="btn btn-sm btn-outline-primary"
          :disabled="retryingErrors.size > 0"
        >
          <i v-if="retryingErrors.size > 0" class="icon-loader spinning"></i>
          <i v-else class="icon-refresh"></i>
          重试所有
        </button>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useErrorStore } from '@/stores/error'
import ErrorNotification from './ErrorNotification.vue'

// 使用错误状态管理
const errorStore = useErrorStore()

// 组件状态
const retryingErrors = ref(new Set<string>())

// 计算属性
const visibleErrors = computed(() => errorStore.visibleErrors)
const retryableErrors = computed(() =>
  visibleErrors.value.filter(error => error.retryable)
)

// 组件事件
interface Emits {
  retry: [errorId: string, error: any]
}

const emit = defineEmits<Emits>()

/**
 * 处理重试操作
 */
async function handleRetry(errorId: string) {
  const error = visibleErrors.value.find(e => e.id === errorId)
  if (!error) return

  retryingErrors.value.add(errorId)

  try {
    // 发出重试事件，让父组件处理具体的重试逻辑
    emit('retry', errorId, error)

    // 重试成功后移除错误
    setTimeout(() => {
      errorStore.removeError(errorId)
      retryingErrors.value.delete(errorId)
    }, 1000)
  } catch (err) {
    // 重试失败，保留错误
    retryingErrors.value.delete(errorId)
    console.error('Retry failed:', err)
  }
}

/**
 * 处理关闭操作
 */
function handleDismiss(errorId: string) {
  errorStore.removeError(errorId)
}

/**
 * 处理关闭所有错误
 */
function handleDismissAll() {
  errorStore.clearErrors()
}

/**
 * 处理重试所有错误
 */
async function handleRetryAll() {
  const errors = retryableErrors.value

  for (const error of errors) {
    if (!retryingErrors.value.has(error.id)) {
      await handleRetry(error.id)
      // 添加小延迟避免同时发送太多请求
      await new Promise(resolve => setTimeout(resolve, 100))
    }
  }
}
</script>

<style scoped>
.error-container {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 9999;
  max-width: 400px;
  width: 100%;
  max-height: calc(100vh - 40px);
  overflow-y: auto;
  pointer-events: none;
}

.error-container > * {
  pointer-events: auto;
}

.error-batch-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  padding: 8px 0;
  border-top: 1px solid #e5e7eb;
  margin-top: 8px;
  background: white;
  border-radius: 0 0 8px 8px;
}

/* 过渡动画 */
.error-list-enter-active,
.error-list-leave-active {
  transition: all 0.3s ease;
}

.error-list-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.error-list-leave-to {
  opacity: 0;
  transform: translateX(100%);
}

.error-list-move {
  transition: transform 0.3s ease;
}

.btn {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
  text-decoration: none;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.btn-sm {
  padding: 2px 6px;
  font-size: 11px;
}

.btn-outline-primary {
  color: #3b82f6;
  border-color: #3b82f6;
  background: white;
}

.btn-outline-primary:hover:not(:disabled) {
  background: #3b82f6;
  color: white;
}

.btn-outline-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-ghost {
  color: #6b7280;
  background: transparent;
}

.btn-ghost:hover {
  color: #374151;
  background: #f3f4f6;
}

.spinning {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* 响应式设计 */
@media (max-width: 640px) {
  .error-container {
    top: 10px;
    right: 10px;
    left: 10px;
    max-width: none;
  }
}

/* 滚动条样式 */
.error-container::-webkit-scrollbar {
  width: 4px;
}

.error-container::-webkit-scrollbar-track {
  background: transparent;
}

.error-container::-webkit-scrollbar-thumb {
  background: #d1d5db;
  border-radius: 2px;
}

.error-container::-webkit-scrollbar-thumb:hover {
  background: #9ca3af;
}
</style>
