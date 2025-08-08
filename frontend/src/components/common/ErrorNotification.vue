<!--
  错误通知组件
  显示单个错误信息，支持不同类型和严重程度的样式
-->
<template>
  <div
    :class="[
      'error-notification',
      `error-${error.type.toLowerCase()}`,
      `severity-${error.severity.toLowerCase()}`
    ]"
    role="alert"
    :aria-live="error.severity === 'CRITICAL' ? 'assertive' : 'polite'"
  >
    <!-- 错误图标 -->
    <div class="error-icon">
      <i :class="getErrorIcon(error.type)"></i>
    </div>

    <!-- 错误内容 -->
    <div class="error-content">
      <h4 class="error-title">{{ error.title }}</h4>
      <p class="error-message">{{ error.message }}</p>

      <!-- 错误详情（可展开） -->
      <div v-if="error.details" class="error-details">
        <button
          @click="showDetails = !showDetails"
          class="details-toggle"
          :aria-expanded="showDetails"
        >
          <i :class="showDetails ? 'icon-chevron-up' : 'icon-chevron-down'"></i>
          {{ showDetails ? '隐藏详情' : '查看详情' }}
        </button>

        <div v-show="showDetails" class="details-content">
          <pre>{{ error.details }}</pre>
        </div>
      </div>

      <!-- 时间戳 -->
      <div class="error-timestamp">
        {{ formatTimestamp(error.timestamp) }}
      </div>
    </div>

    <!-- 操作按钮 -->
    <div class="error-actions">
      <!-- 重试按钮 -->
      <button
        v-if="error.retryable"
        @click="$emit('retry', error.id)"
        class="btn btn-sm btn-outline-primary"
        :disabled="retrying"
      >
        <i v-if="retrying" class="icon-loader spinning"></i>
        <i v-else class="icon-refresh"></i>
        重试
      </button>

      <!-- 关闭按钮 -->
      <button
        v-if="error.dismissible"
        @click="$emit('dismiss', error.id)"
        class="btn btn-sm btn-ghost"
        aria-label="关闭错误提示"
      >
        <i class="icon-x"></i>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { AppError, ErrorType } from '@/types/error'

// 组件属性
interface Props {
  error: AppError
  retrying?: boolean
}

// 组件事件
interface Emits {
  retry: [errorId: string]
  dismiss: [errorId: string]
}

defineProps<Props>()
defineEmits<Emits>()

// 组件状态
const showDetails = ref(false)

/**
 * 获取错误类型对应的图标
 */
function getErrorIcon(type: ErrorType): string {
  const iconMap: Record<ErrorType, string> = {
    NETWORK: 'icon-wifi-off',
    VALIDATION: 'icon-alert-triangle',
    AUTHENTICATION: 'icon-lock',
    AUTHORIZATION: 'icon-shield-off',
    NOT_FOUND: 'icon-search',
    SERVER: 'icon-server',
    TIMEOUT: 'icon-clock',
    UNKNOWN: 'icon-help-circle'
  }
  return iconMap[type] || 'icon-alert-circle'
}

/**
 * 格式化时间戳
 */
function formatTimestamp(timestamp: Date): string {
  const now = new Date()
  const diff = now.getTime() - timestamp.getTime()

  if (diff < 60000) { // 1分钟内
    return '刚刚'
  } else if (diff < 3600000) { // 1小时内
    return `${Math.floor(diff / 60000)}分钟前`
  } else if (diff < 86400000) { // 24小时内
    return `${Math.floor(diff / 3600000)}小时前`
  } else {
    return timestamp.toLocaleString('zh-CN', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  }
}
</script>

<style scoped>
.error-notification {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 16px;
  border-radius: 8px;
  border-left: 4px solid;
  background: white;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  margin-bottom: 12px;
  animation: slideIn 0.3s ease-out;
}

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

/* 错误类型样式 */
.error-network {
  border-left-color: #f59e0b;
  background: #fffbeb;
}

.error-validation {
  border-left-color: #3b82f6;
  background: #eff6ff;
}

.error-authentication,
.error-authorization {
  border-left-color: #ef4444;
  background: #fef2f2;
}

.error-not_found {
  border-left-color: #6b7280;
  background: #f9fafb;
}

.error-server,
.error-timeout {
  border-left-color: #dc2626;
  background: #fef2f2;
}

.error-unknown {
  border-left-color: #8b5cf6;
  background: #f5f3ff;
}

/* 严重程度样式 */
.severity-critical {
  border-left-width: 6px;
  box-shadow: 0 4px 12px rgba(220, 38, 38, 0.2);
}

.severity-high {
  border-left-width: 5px;
}

.severity-low {
  opacity: 0.9;
}

.error-icon {
  flex-shrink: 0;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-top: 2px;
}

.error-icon i {
  font-size: 20px;
  color: inherit;
}

.error-content {
  flex: 1;
  min-width: 0;
}

.error-title {
  font-size: 14px;
  font-weight: 600;
  margin: 0 0 4px 0;
  color: #1f2937;
}

.error-message {
  font-size: 13px;
  margin: 0 0 8px 0;
  color: #4b5563;
  line-height: 1.4;
}

.error-details {
  margin-top: 8px;
}

.details-toggle {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
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
  margin-top: 8px;
  padding: 8px;
  background: #f3f4f6;
  border-radius: 4px;
  font-size: 11px;
}

.details-content pre {
  margin: 0;
  white-space: pre-wrap;
  word-break: break-word;
  color: #374151;
}

.error-timestamp {
  font-size: 11px;
  color: #9ca3af;
  margin-top: 4px;
}

.error-actions {
  flex-shrink: 0;
  display: flex;
  align-items: flex-start;
  gap: 8px;
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
  .error-notification {
    padding: 12px;
    gap: 8px;
  }

  .error-actions {
    flex-direction: column;
    gap: 4px;
  }
}
</style>
