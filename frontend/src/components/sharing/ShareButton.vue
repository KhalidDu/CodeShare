<template>
  <button
    @click="handleShare"
    :disabled="sharing || disabled"
    :class="[
      'share-button',
      {
        'share-button--sharing': sharing,
        'share-button--success': showSuccess,
        'share-button--error': showError
      },
      buttonClass
    ]"
    :title="tooltip"
  >
    <span class="share-button__icon">
      <svg v-if="!sharing && !showSuccess && !showError" viewBox="0 0 16 16" width="16" height="16">
        <path d="M8 2a2 2 0 1 0 0 4 2 2 0 0 0 0-4ZM6 5a2 2 0 1 1-4 0 2 2 0 0 1 4 0Zm6 0a2 2 0 1 1-4 0 2 2 0 0 1 4 0Zm-3 4a1 1 0 0 0-1 1v1H4a1 1 0 0 0 0 2h4v1a1 1 0 0 0 2 0v-1h4a1 1 0 0 0 0-2h-4v-1a1 1 0 0 0-1-1Z"/>
      </svg>
      <svg v-else-if="sharing" viewBox="0 0 16 16" width="16" height="16" class="loading-icon">
        <path d="M8 12a4 4 0 1 1 0-8 4 4 0 0 1 0 8zm0-1.5a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5z"></path>
      </svg>
      <svg v-else-if="showSuccess" viewBox="0 0 16 16" width="16" height="16">
        <path d="M13.78 4.22a.75.75 0 0 1 0 1.06l-7.25 7.25a.75.75 0 0 1-1.06 0L2.22 9.28a.75.75 0 0 1 1.06-1.06L6 10.94l6.72-6.72a.75.75 0 0 1 1.06 0Z"></path>
      </svg>
      <svg v-else-if="showError" viewBox="0 0 16 16" width="16" height="16">
        <path d="M3.72 3.72a.75.75 0 0 1 1.06 0L8 6.94l3.22-3.22a.75.75 0 1 1 1.06 1.06L9.06 8l3.22 3.22a.75.75 0 1 1-1.06 1.06L8 9.06l-3.22 3.22a.75.75 0 0 1-1.06-1.06L6.94 8 3.72 4.78a.75.75 0 0 1 0-1.06Z"></path>
      </svg>
    </span>
    <span v-if="showText" class="share-button__text">
      {{ buttonText }}
    </span>
  </button>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { shareService } from '@/services/shareService'
import type { CreateShareTokenRequest, ShareToken, SharePermission } from '@/types'

interface Props {
  /** 代码片段ID */
  snippetId: string
  /** 分享权限 */
  permission?: SharePermission
  /** 过期时间（ISO字符串） */
  expiresAt?: string
  /** 密码保护 */
  password?: string
  /** 最大访问次数 */
  maxAccessCount?: number
  /** 允许下载 */
  allowDownload?: boolean
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
  /** 是否自动复制分享链接到剪贴板 */
  autoCopyLink?: boolean
}

interface Emits {
  /** 分享成功事件 */
  (e: 'share-success', shareToken: ShareToken): void
  /** 分享失败事件 */
  (e: 'share-error', error: Error): void
  /** 分享链接已复制事件 */
  (e: 'link-copied', shareToken: ShareToken): void
}

const props = withDefaults(defineProps<Props>(), {
  permission: 'VIEW' as SharePermission,
  allowDownload: true,
  showText: true,
  customText: '',
  disabled: false,
  buttonClass: '',
  tooltip: '分享代码片段',
  successDuration: 2000,
  errorDuration: 3000,
  autoCopyLink: true
})

const emit = defineEmits<Emits>()

// 响应式状态
const sharing = ref(false)
const showSuccess = ref(false)
const showError = ref(false)

// 计算属性
const buttonText = computed(() => {
  if (sharing.value) return '分享中...'
  if (showSuccess.value) return '已分享'
  if (showError.value) return '分享失败'
  return props.customText || '分享'
})

/**
 * 处理分享操作
 */
const handleShare = async () => {
  if (sharing.value || props.disabled || !props.snippetId) return

  sharing.value = true
  showSuccess.value = false
  showError.value = false

  try {
    // 构建分享请求
    const shareRequest: CreateShareTokenRequest = {
      snippetId: props.snippetId,
      permission: props.permission,
      expiresAt: props.expiresAt,
      password: props.password,
      maxAccessCount: props.maxAccessCount,
      allowDownload: props.allowDownload
    }

    // 验证请求参数
    const validation = shareService.validateShareRequest(shareRequest)
    if (!validation.isValid) {
      throw new Error(`分享参数无效: ${validation.errors.join(', ')}`)
    }

    // 检查用户是否可以创建分享令牌
    const canCreate = await shareService.canCreateShareToken(props.snippetId)
    if (!canCreate.canCreate) {
      throw new Error(canCreate.reason || '无法创建分享链接')
    }

    // 创建分享令牌
    const shareToken = await shareService.createShareToken(shareRequest)

    // 显示成功状态
    showSuccess.value = true
    emit('share-success', shareToken)

    // 自动复制分享链接
    if (props.autoCopyLink) {
      await copyShareLink(shareToken)
    }

    // 自动隐藏成功状态
    setTimeout(() => {
      showSuccess.value = false
    }, props.successDuration)

  } catch (error) {
    console.error('Share failed:', error)
    
    // 显示错误状态
    showError.value = true
    emit('share-error', error as Error)

    // 自动隐藏错误状态
    setTimeout(() => {
      showError.value = false
    }, props.errorDuration)
  } finally {
    sharing.value = false
  }
}

/**
 * 复制分享链接到剪贴板
 */
const copyShareLink = async (shareToken: ShareToken): Promise<void> => {
  try {
    const shareLink = shareService.getShareLink(shareToken.token)
    
    // 尝试使用现代剪贴板 API
    if (navigator.clipboard && window.isSecureContext) {
      await navigator.clipboard.writeText(shareLink)
    } else {
      // 降级处理：使用传统方法
      await fallbackCopyToClipboard(shareLink)
    }

    // 触发链接复制事件
    emit('link-copied', shareToken)
  } catch (error) {
    console.warn('Failed to copy share link:', error)
    // 不影响分享操作的成功状态
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

// 监听代码片段ID变化，重置状态
watch(() => props.snippetId, () => {
  showSuccess.value = false
  showError.value = false
})
</script>

<style scoped>
.share-button {
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

.share-button:hover:not(:disabled) {
  background: #f3f4f6;
  border-color: #8c959f;
}

.share-button:active:not(:disabled) {
  background: #ebecf0;
  border-color: #8c959f;
  transform: translateY(1px);
}

.share-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.share-button--sharing {
  background: #fff3cd;
  border-color: #ffeaa7;
  color: #856404;
}

.share-button--success {
  background: #d1e7dd;
  border-color: #badbcc;
  color: #0f5132;
}

.share-button--error {
  background: #f8d7da;
  border-color: #f5c2c7;
  color: #842029;
}

.share-button__icon {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.share-button__icon svg {
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

.share-button__text {
  white-space: nowrap;
}

/* 不同尺寸变体 */
.share-button--small {
  padding: 4px 8px;
  font-size: 11px;
}

.share-button--large {
  padding: 8px 16px;
  font-size: 14px;
}

/* 不同样式变体 */
.share-button--primary {
  background: #0969da;
  border-color: #0969da;
  color: #fff;
}

.share-button--primary:hover:not(:disabled) {
  background: #0860ca;
  border-color: #0860ca;
}

.share-button--minimal {
  background: transparent;
  border: none;
  padding: 4px;
  color: #656d76;
}

.share-button--minimal:hover:not(:disabled) {
  background: rgba(0, 0, 0, 0.05);
  color: #24292f;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .share-button {
    padding: 5px 10px;
    font-size: 11px;
  }
  
  .share-button__text {
    display: none;
  }
  
  .share-button--large .share-button__text {
    display: inline;
  }
}

/* 暗色主题支持 */
@media (prefers-color-scheme: dark) {
  .share-button {
    background: #30363d;
    border-color: #8b949e;
    color: #f0f6fc;
  }
  
  .share-button:hover:not(:disabled) {
    background: #484f58;
    border-color: #8b949e;
  }
  
  .share-button:active:not(:disabled) {
    background: #636e7b;
    border-color: #8b949e;
  }
  
  .share-button--minimal {
    color: #8b949e;
  }
  
  .share-button--minimal:hover:not(:disabled) {
    background: rgba(240, 246, 252, 0.1);
    color: #f0f6fc;
  }
}
</style>