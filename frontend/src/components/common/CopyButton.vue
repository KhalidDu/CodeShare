<template>
  <button
    @click="handleCopy"
    :disabled="copying || disabled"
    :class="[
      'copy-button',
      {
        'copy-button--copying': copying,
        'copy-button--success': showSuccess,
        'copy-button--error': showError
      },
      buttonClass
    ]"
    :title="tooltip"
  >
    <span class="copy-button__icon">
      <svg v-if="!copying && !showSuccess && !showError" viewBox="0 0 16 16" width="16" height="16">
        <path d="M0 6.75C0 5.784.784 5 1.75 5h1.5a.75.75 0 0 1 0 1.5h-1.5a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-1.5a.75.75 0 0 1 1.5 0v1.5A1.75 1.75 0 0 1 8.25 16h-7.5A1.75 1.75 0 0 1 0 14.25Z"></path>
        <path d="M5 1.75C5 .784 5.784 0 6.75 0h7.5C15.216 0 16 .784 16 1.75v7.5A1.75 1.75 0 0 1 14.25 11h-7.5A1.75 1.75 0 0 1 5 9.25Zm1.75-.25a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-7.5a.25.25 0 0 0-.25-.25Z"></path>
      </svg>
      <svg v-else-if="copying" viewBox="0 0 16 16" width="16" height="16" class="loading-icon">
        <path d="M8 12a4 4 0 1 1 0-8 4 4 0 0 1 0 8zm0-1.5a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5z"></path>
      </svg>
      <svg v-else-if="showSuccess" viewBox="0 0 16 16" width="16" height="16">
        <path d="M13.78 4.22a.75.75 0 0 1 0 1.06l-7.25 7.25a.75.75 0 0 1-1.06 0L2.22 9.28a.75.75 0 0 1 1.06-1.06L6 10.94l6.72-6.72a.75.75 0 0 1 1.06 0Z"></path>
      </svg>
      <svg v-else-if="showError" viewBox="0 0 16 16" width="16" height="16">
        <path d="M3.72 3.72a.75.75 0 0 1 1.06 0L8 6.94l3.22-3.22a.75.75 0 1 1 1.06 1.06L9.06 8l3.22 3.22a.75.75 0 1 1-1.06 1.06L8 9.06l-3.22 3.22a.75.75 0 0 1-1.06-1.06L6.94 8 3.72 4.78a.75.75 0 0 1 0-1.06Z"></path>
      </svg>
    </span>
    <span v-if="showText" class="copy-button__text">
      {{ buttonText }}
    </span>
  </button>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { clipboardService } from '@/services/clipboardService'

interface Props {
  /** 要复制的文本内容 */
  text: string
  /** 代码片段ID，用于记录复制统计 */
  snippetId?: string
  /** 是否显示文本 */
  showText?: boolean
  /** 自定义按钮文本 */
  customText?: string
  /** 是否禁用 */
  disabled?: boolean
  /** 自定义CSS类 */
  buttonClass?: string
  /** 工具提示文本 */
  tooltip?: string
  /** 成功提示持续时间（毫秒） */
  successDuration?: number
  /** 错误提示持续时间（毫秒） */
  errorDuration?: number
}

interface Emits {
  /** 复制成功事件 */
  (e: 'copy-success'): void
  /** 复制失败事件 */
  (e: 'copy-error', error: Error): void
}

const props = withDefaults(defineProps<Props>(), {
  showText: true,
  customText: '',
  disabled: false,
  buttonClass: '',
  tooltip: '复制到剪贴板',
  successDuration: 2000,
  errorDuration: 3000
})

const emit = defineEmits<Emits>()

// 响应式状态
const copying = ref(false)
const showSuccess = ref(false)
const showError = ref(false)

// 计算属性
const buttonText = computed(() => {
  if (copying.value) return '复制中...'
  if (showSuccess.value) return '已复制'
  if (showError.value) return '复制失败'
  return props.customText || '复制'
})

/**
 * 处理复制操作
 */
const handleCopy = async () => {
  if (copying.value || props.disabled || !props.text) return

  copying.value = true
  showSuccess.value = false
  showError.value = false

  try {
    // 尝试使用现代剪贴板 API
    if (navigator.clipboard && window.isSecureContext) {
      await navigator.clipboard.writeText(props.text)
    } else {
      // 降级处理：使用传统方法
      await fallbackCopyToClipboard(props.text)
    }

    // 记录复制统计（如果提供了 snippetId）
    if (props.snippetId) {
      try {
        await clipboardService.recordCopy(props.snippetId)
      } catch (recordError) {
        console.warn('Failed to record copy statistics:', recordError)
        // 不影响复制操作的成功状态
      }
    }

    // 显示成功状态
    showSuccess.value = true
    emit('copy-success')

    // 自动隐藏成功状态
    setTimeout(() => {
      showSuccess.value = false
    }, props.successDuration)

  } catch (error) {
    console.error('Copy failed:', error)
    
    // 显示错误状态
    showError.value = true
    emit('copy-error', error as Error)

    // 自动隐藏错误状态
    setTimeout(() => {
      showError.value = false
    }, props.errorDuration)
  } finally {
    copying.value = false
  }
}

/**
 * 降级复制方法（用于不支持现代剪贴板 API 的环境）
 */
const fallbackCopyToClipboard = (text: string): Promise<void> => {
  return new Promise((resolve, reject) => {
    try {
      const textArea = document.createElement('textarea')
      textArea.value = text
      textArea.style.position = 'fixed'
      textArea.style.left = '-999999px'
      textArea.style.top = '-999999px'
      document.body.appendChild(textArea)
      textArea.focus()
      textArea.select()
      
      const successful = document.execCommand('copy')
      document.body.removeChild(textArea)
      
      if (successful) {
        resolve()
      } else {
        reject(new Error('execCommand copy failed'))
      }
    } catch (error) {
      reject(error)
    }
  })
}

// 监听文本变化，重置状态
watch(() => props.text, () => {
  showSuccess.value = false
  showError.value = false
})
</script>

<style scoped>
.copy-button {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  border: 1px solid #d0d7de;
  border-radius: 6px;
  background: #f6f8fa;
  color: #24292f;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  user-select: none;
}

.copy-button:hover:not(:disabled) {
  background: #f3f4f6;
  border-color: #8c959f;
}

.copy-button:active:not(:disabled) {
  background: #ebecf0;
  border-color: #8c959f;
  transform: translateY(1px);
}

.copy-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.copy-button--copying {
  background: #fff3cd;
  border-color: #ffeaa7;
  color: #856404;
}

.copy-button--success {
  background: #d1e7dd;
  border-color: #badbcc;
  color: #0f5132;
}

.copy-button--error {
  background: #f8d7da;
  border-color: #f5c2c7;
  color: #842029;
}

.copy-button__icon {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.copy-button__icon svg {
  fill: currentColor;
}

.loading-icon {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

.copy-button__text {
  white-space: nowrap;
}

/* 不同尺寸变体 */
.copy-button--small {
  padding: 4px 8px;
  font-size: 11px;
}

.copy-button--large {
  padding: 8px 16px;
  font-size: 14px;
}

/* 不同样式变体 */
.copy-button--primary {
  background: #0969da;
  border-color: #0969da;
  color: #fff;
}

.copy-button--primary:hover:not(:disabled) {
  background: #0860ca;
  border-color: #0860ca;
}

.copy-button--minimal {
  background: transparent;
  border: none;
  padding: 4px;
  color: #656d76;
}

.copy-button--minimal:hover:not(:disabled) {
  background: rgba(0, 0, 0, 0.05);
  color: #24292f;
}
</style>