/**
 * 全局错误状态管理
 */
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { ErrorType, ErrorSeverity } from '@/types/error'
import type { AppError, ErrorHandlingOptions } from '@/types/error'

export const useErrorStore = defineStore('error', () => {
  // 状态
  const errors = ref<AppError[]>([])
  const isOnline = ref(navigator.onLine)

  // 计算属性
  const hasErrors = computed(() => errors.value.length > 0)
  const criticalErrors = computed(() =>
    errors.value.filter(error => error.severity === 'CRITICAL')
  )
  const visibleErrors = computed(() =>
    errors.value.filter(error => error.dismissible !== false)
  )

  // 生成错误ID
  function generateErrorId(): string {
    return `error_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`
  }

  // 添加错误
  function addError(
    type: ErrorType,
    title: string,
    message: string,
    options: Partial<AppError & ErrorHandlingOptions> = {}
  ): string {
    const errorId = generateErrorId()

    const error: AppError = {
      id: errorId,
      type,
      severity: options.severity || ErrorSeverity.MEDIUM,
      title,
      message,
      details: options.details,
      timestamp: new Date(),
      context: options.context,
      retryable: options.retryable ?? false,
      dismissible: options.dismissible ?? true
    }

    errors.value.push(error)

    // 自动隐藏错误
    if (options.autoHide !== false) {
      const hideDelay = options.hideDelay || 5000
      setTimeout(() => {
        removeError(errorId)
      }, hideDelay)
    }

    // 记录错误日志
    if (options.logError !== false) {
      console.error(`[${error.type}] ${error.title}:`, error.message, error)
    }

    return errorId
  }

  // 移除错误
  function removeError(errorId: string) {
    const index = errors.value.findIndex(error => error.id === errorId)
    if (index > -1) {
      errors.value.splice(index, 1)
    }
  }

  // 清除所有错误
  function clearErrors() {
    errors.value = []
  }

  // 清除指定类型的错误
  function clearErrorsByType(type: ErrorType) {
    errors.value = errors.value.filter(error => error.type !== type)
  }

  // 网络错误处理
  function handleNetworkError(error: Error & { code?: string; response?: { data?: unknown; status?: number } }, context?: Record<string, unknown>): string {
    let title = '网络错误'
    let message = '请检查网络连接'
    let severity: ErrorSeverity = ErrorSeverity.MEDIUM

    if (error.code === 'NETWORK_ERROR') {
      message = '无法连接到服务器，请检查网络连接'
      severity = ErrorSeverity.HIGH
    } else if (error.code === 'TIMEOUT_ERROR') {
      title = '请求超时'
      message = '服务器响应超时，请稍后重试'
    } else if (!isOnline.value) {
      title = '网络连接断开'
      message = '请检查网络连接后重试'
      severity = ErrorSeverity.HIGH
    }

    return addError(ErrorType.NETWORK, title, message, {
      severity,
      context,
      retryable: true,
      autoHide: false
    })
  }

  // API 错误处理
  function handleApiError(error: Error & { response?: { data?: unknown; status?: number } }, context?: Record<string, unknown>): string {
    const status = error.response?.status
    const data = error.response?.data

    let type: ErrorType = ErrorType.SERVER
    let title = '服务器错误'
    let message = '服务器处理请求时发生错误'
    let severity: ErrorSeverity = ErrorSeverity.MEDIUM

    switch (status) {
      case 400:
        type = ErrorType.VALIDATION
        title = '请求参数错误'
        message = data?.message || '请求参数不正确'
        severity = ErrorSeverity.LOW
        break
      case 401:
        type = ErrorType.AUTHENTICATION
        title = '身份验证失败'
        message = '请重新登录'
        severity = ErrorSeverity.HIGH
        break
      case 403:
        type = ErrorType.AUTHORIZATION
        title = '权限不足'
        message = '您没有权限执行此操作'
        severity = ErrorSeverity.MEDIUM
        break
      case 404:
        type = ErrorType.NOT_FOUND
        title = '资源未找到'
        message = '请求的资源不存在'
        severity = ErrorSeverity.LOW
        break
      case 422:
        type = ErrorType.VALIDATION
        title = '数据验证失败'
        message = data?.message || '提交的数据不符合要求'
        severity = ErrorSeverity.LOW
        break
      case 429:
        title = '请求过于频繁'
        message = '请稍后再试'
        severity = ErrorSeverity.MEDIUM
        break
      case 500:
      case 502:
      case 503:
      case 504:
        title = '服务器错误'
        message = '服务器暂时无法处理请求，请稍后重试'
        severity = ErrorSeverity.HIGH
        break
      default:
        type = ErrorType.UNKNOWN
        title = '未知错误'
        message = data?.message || error.message || '发生了未知错误'
    }

    return addError(type, title, message, {
      severity,
      details: data?.details,
      context: { ...context, status, response: data },
      retryable: status >= 500,
      autoHide: severity === ErrorSeverity.LOW
    })
  }

  // 验证错误处理
  function handleValidationError(
    errors: Record<string, string[]>,
    context?: Record<string, unknown>
  ): string {
    const errorMessages = Object.entries(errors)
      .map(([field, messages]) => `${field}: ${messages.join(', ')}`)
      .join('\n')

    return addError(ErrorType.VALIDATION, '表单验证失败', errorMessages, {
      severity: ErrorSeverity.LOW,
      context,
      dismissible: true,
      autoHide: true,
      hideDelay: 8000
    })
  }

  // 监听网络状态变化
  function initializeNetworkMonitoring() {
    const updateOnlineStatus = () => {
      const wasOffline = !isOnline.value
      isOnline.value = navigator.onLine

      if (wasOffline && isOnline.value) {
        // 网络恢复
        clearErrorsByType(ErrorType.NETWORK)
        addError(ErrorType.NETWORK, '网络已恢复', '网络连接已恢复正常', {
          severity: ErrorSeverity.LOW,
          autoHide: true,
          hideDelay: 3000
        })
      } else if (!isOnline.value) {
        // 网络断开
        addError(ErrorType.NETWORK, '网络连接断开', '请检查网络连接', {
          severity: ErrorSeverity.HIGH,
          autoHide: false,
          retryable: true
        })
      }
    }

    window.addEventListener('online', updateOnlineStatus)
    window.addEventListener('offline', updateOnlineStatus)

    // 返回清理函数
    return () => {
      window.removeEventListener('online', updateOnlineStatus)
      window.removeEventListener('offline', updateOnlineStatus)
    }
  }

  return {
    // 状态
    errors,
    isOnline,
    // 计算属性
    hasErrors,
    criticalErrors,
    visibleErrors,
    // 方法
    addError,
    removeError,
    clearErrors,
    clearErrorsByType,
    handleNetworkError,
    handleApiError,
    handleValidationError,
    initializeNetworkMonitoring
  }
})
