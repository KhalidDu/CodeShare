<!--
  错误边界组件
  捕获子组件中的错误并显示友好的错误页面
-->
<template>
  <div v-if="hasError" class="error-boundary">
    <div class="error-boundary-content">
      <!-- 错误图标 -->
      <div class="error-icon">
        <i class="icon-alert-triangle"></i>
      </div>

      <!-- 错误信息 -->
      <div class="error-info">
        <h2 class="error-title">{{ errorTitle }}</h2>
        <p class="error-message">{{ errorMessage }}</p>

        <!-- 错误详情（开发环境） -->
        <div v-if="showDetails && errorDetails" class="error-details">
          <button
            @click="showErrorDetails = !showErrorDetails"
            class="details-toggle"
          >
            <i :class="showErrorDetails ? 'icon-chevron-up' : 'icon-chevron-down'"></i>
            {{ showErrorDetails ? '隐藏详情' : '查看详情' }}
          </button>

          <div v-show="showErrorDetails" class="details-content">
            <pre>{{ errorDetails }}</pre>
          </div>
        </div>
      </div>

      <!-- 操作按钮 -->
      <div class="error-actions">
        <button @click="handleReload" class="btn btn-primary">
          <i class="icon-refresh"></i>
          重新加载
        </button>

        <button @click="handleGoHome" class="btn btn-secondary">
          <i class="icon-home"></i>
          返回首页
        </button>

        <button
          v-if="showReportButton"
          @click="handleReportError"
          class="btn btn-outline"
        >
          <i class="icon-bug"></i>
          报告问题
        </button>
      </div>
    </div>
  </div>

  <!-- 正常渲染子组件 -->
  <slot v-else />
</template>

<script setup lang="ts">
import { ref, computed, onErrorCaptured, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useErrorStore } from '@/stores/error'
import { ErrorType, ErrorSeverity } from '@/types/error'

// 组件属性
interface Props {
  fallbackTitle?: string
  fallbackMessage?: string
  showDetails?: boolean
  showReportButton?: boolean
  onError?: (error: Error, instance: any, info: string) => void
}

const props = withDefaults(defineProps<Props>(), {
  fallbackTitle: '页面出现错误',
  fallbackMessage: '抱歉，页面加载时出现了错误。请尝试刷新页面或返回首页。',
  showDetails: import.meta.env.DEV,
  showReportButton: true
})

// 路由和状态管理
const router = useRouter()
const errorStore = useErrorStore()

// 组件状态
const hasError = ref(false)
const errorInfo = ref<{
  error: Error
  instance: any
  info: string
} | null>(null)
const showErrorDetails = ref(false)

// 计算属性
const errorTitle = computed(() => {
  if (errorInfo.value?.error.name === 'ChunkLoadError') {
    return '资源加载失败'
  }
  return props.fallbackTitle
})

const errorMessage = computed(() => {
  if (errorInfo.value?.error.name === 'ChunkLoadError') {
    return '页面资源加载失败，可能是网络问题或应用已更新。请刷新页面重试。'
  }
  return props.fallbackMessage
})

const errorDetails = computed(() => {
  if (!errorInfo.value) return ''

  const { error, info } = errorInfo.value
  return `错误类型: ${error.name}
错误信息: ${error.message}
错误堆栈: ${error.stack}
组件信息: ${info}`
})

// 错误捕获
onErrorCaptured((error: Error, instance: any, info: string) => {
  console.error('ErrorBoundary caught an error:', error, info)

  hasError.value = true
  errorInfo.value = { error, instance, info }

  // 调用自定义错误处理函数
  props.onError?.(error, instance, info)

  // 添加到全局错误状态
  errorStore.addError(
    ErrorType.UNKNOWN,
    '组件错误',
    error.message,
    {
      severity: ErrorSeverity.HIGH,
      details: errorDetails.value,
      context: { componentInfo: info },
      dismissible: false,
      autoHide: false
    }
  )

  // 阻止错误继续向上传播
  return false
})

// 全局错误监听
onMounted(() => {
  // 监听未捕获的 Promise 错误
  const handleUnhandledRejection = (event: PromiseRejectionEvent) => {
    console.error('Unhandled promise rejection:', event.reason)

    hasError.value = true
    errorInfo.value = {
      error: new Error(event.reason?.message || 'Unhandled Promise Rejection'),
      instance: null,
      info: 'Promise rejection'
    }

    errorStore.addError(
      ErrorType.UNKNOWN,
      'Promise 错误',
      event.reason?.message || '未处理的 Promise 错误',
      {
        severity: ErrorSeverity.HIGH,
        details: String(event.reason),
        dismissible: false,
        autoHide: false
      }
    )
  }

  // 监听全局 JavaScript 错误
  const handleGlobalError = (event: ErrorEvent) => {
    console.error('Global error:', event.error)

    hasError.value = true
    errorInfo.value = {
      error: event.error || new Error(event.message),
      instance: null,
      info: `${event.filename}:${event.lineno}:${event.colno}`
    }

    errorStore.addError(
      ErrorType.UNKNOWN,
      'JavaScript 错误',
      event.message,
      {
        severity: ErrorSeverity.HIGH,
        details: event.error?.stack,
        context: {
          filename: event.filename,
          lineno: event.lineno,
          colno: event.colno
        },
        dismissible: false,
        autoHide: false
      }
    )
  }

  window.addEventListener('unhandledrejection', handleUnhandledRejection)
  window.addEventListener('error', handleGlobalError)

  // 清理函数
  return () => {
    window.removeEventListener('unhandledrejection', handleUnhandledRejection)
    window.removeEventListener('error', handleGlobalError)
  }
})

/**
 * 重新加载页面
 */
function handleReload() {
  window.location.reload()
}

/**
 * 返回首页
 */
function handleGoHome() {
  hasError.value = false
  errorInfo.value = null
  router.push('/')
}

/**
 * 报告错误
 */
function handleReportError() {
  if (!errorInfo.value) return

  const { error, info } = errorInfo.value
  const reportData = {
    error: {
      name: error.name,
      message: error.message,
      stack: error.stack
    },
    componentInfo: info,
    userAgent: navigator.userAgent,
    url: window.location.href,
    timestamp: new Date().toISOString()
  }

  // 这里可以发送错误报告到服务器
  console.log('Error report:', reportData)

  // 显示成功提示
  errorStore.addError(
    ErrorType.UNKNOWN,
    '错误已报告',
    '感谢您的反馈，我们会尽快处理这个问题。',
    {
      severity: ErrorSeverity.LOW,
      autoHide: true,
      hideDelay: 3000
    }
  )
}

/**
 * 重置错误状态（供外部调用）
 */
function resetError() {
  hasError.value = false
  errorInfo.value = null
  showErrorDetails.value = false
}

// 暴露方法给父组件
defineExpose({
  resetError
})
</script>

<style scoped>
.error-boundary {
  min-height: 400px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  background: #fafafa;
}

.error-boundary-content {
  max-width: 600px;
  text-align: center;
  background: white;
  padding: 40px;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.error-icon {
  margin-bottom: 24px;
}

.error-icon i {
  font-size: 64px;
  color: #ef4444;
}

.error-info {
  margin-bottom: 32px;
}

.error-title {
  font-size: 24px;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 12px 0;
}

.error-message {
  font-size: 16px;
  color: #6b7280;
  line-height: 1.6;
  margin: 0 0 20px 0;
}

.error-details {
  text-align: left;
  margin-top: 20px;
}

.details-toggle {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 14px;
  color: #6b7280;
  background: none;
  border: none;
  cursor: pointer;
  padding: 0;
  text-decoration: underline;
}

.details-toggle:hover {
  color: #374151;
}

.details-content {
  margin-top: 12px;
  padding: 16px;
  background: #f3f4f6;
  border-radius: 6px;
  text-align: left;
}

.details-content pre {
  margin: 0;
  font-size: 12px;
  line-height: 1.4;
  white-space: pre-wrap;
  word-break: break-word;
  color: #374151;
}

.error-actions {
  display: flex;
  justify-content: center;
  gap: 12px;
  flex-wrap: wrap;
}

.btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  text-decoration: none;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.btn-primary {
  background: #3b82f6;
  color: white;
}

.btn-primary:hover {
  background: #2563eb;
}

.btn-secondary {
  background: #6b7280;
  color: white;
}

.btn-secondary:hover {
  background: #4b5563;
}

.btn-outline {
  color: #6b7280;
  border-color: #d1d5db;
  background: white;
}

.btn-outline:hover {
  background: #f9fafb;
  border-color: #9ca3af;
}

/* 响应式设计 */
@media (max-width: 640px) {
  .error-boundary-content {
    padding: 24px;
    margin: 0 10px;
  }

  .error-title {
    font-size: 20px;
  }

  .error-message {
    font-size: 14px;
  }

  .error-actions {
    flex-direction: column;
  }

  .btn {
    width: 100%;
    justify-content: center;
  }
}
</style>
