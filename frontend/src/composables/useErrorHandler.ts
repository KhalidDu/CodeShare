/**
 * 错误处理组合式函数
 * 提供统一的错误处理逻辑
 */
import { useErrorStore } from '@/stores/error'
import { ErrorType, ErrorSeverity } from '@/types/error'
import type { ErrorHandlingOptions } from '@/types/error'

export function useErrorHandler() {
  const errorStore = useErrorStore()

  /**
   * 处理 API 错误
   */
  function handleApiError(
    error: any,
    context?: Record<string, any>,
    options: ErrorHandlingOptions = {}
  ): string {
    console.error('API Error:', error)

    // 如果错误已经被全局拦截器处理过，直接使用处理后的信息
    if (error.apiError) {
      return errorStore.handleApiError(error, context)
    }

    // 处理网络错误
    if (!error.response) {
      return errorStore.handleNetworkError(error, context)
    }

    // 处理其他错误
    return errorStore.handleApiError(error, context)
  }

  /**
   * 处理异步操作错误
   */
  async function handleAsyncError<T>(
    asyncFn: () => Promise<T>,
    errorContext?: string,
    options: ErrorHandlingOptions = {}
  ): Promise<T | null> {
    try {
      return await asyncFn()
    } catch (error: any) {
      const context = errorContext ? { operation: errorContext } : undefined
      handleApiError(error, context, options)
      return null
    }
  }

  /**
   * 处理表单验证错误
   */
  function handleValidationError(
    errors: Record<string, string[]>,
    context?: Record<string, any>
  ): string {
    return errorStore.handleValidationError(errors, context)
  }

  /**
   * 显示成功消息
   */
  function showSuccess(
    title: string,
    message: string,
    options: ErrorHandlingOptions = {}
  ): string {
    return errorStore.addError(ErrorType.UNKNOWN, title, message, {
      severity: ErrorSeverity.LOW,
      autoHide: true,
      hideDelay: 3000,
      ...options
    })
  }

  /**
   * 显示警告消息
   */
  function showWarning(
    title: string,
    message: string,
    options: ErrorHandlingOptions = {}
  ): string {
    return errorStore.addError(ErrorType.VALIDATION, title, message, {
      severity: ErrorSeverity.MEDIUM,
      autoHide: true,
      hideDelay: 5000,
      ...options
    })
  }

  /**
   * 显示信息消息
   */
  function showInfo(
    title: string,
    message: string,
    options: ErrorHandlingOptions = {}
  ): string {
    return errorStore.addError(ErrorType.UNKNOWN, title, message, {
      severity: ErrorSeverity.LOW,
      autoHide: true,
      hideDelay: 4000,
      ...options
    })
  }

  /**
   * 清除错误
   */
  function clearError(errorId: string) {
    errorStore.removeError(errorId)
  }

  /**
   * 清除所有错误
   */
  function clearAllErrors() {
    errorStore.clearErrors()
  }

  /**
   * 包装异步函数，自动处理加载状态和错误
   */
  function withErrorHandling<T extends any[], R>(
    fn: (...args: T) => Promise<R>,
    errorContext?: string
  ) {
    return async (...args: T): Promise<R | null> => {
      try {
        return await fn(...args)
      } catch (error: any) {
        const context = errorContext ? { operation: errorContext } : undefined
        handleApiError(error, context)
        throw error // 重新抛出错误，让调用者决定如何处理
      }
    }
  }

  /**
   * 创建重试函数
   */
  function createRetryHandler<T extends any[], R>(
    fn: (...args: T) => Promise<R>,
    maxRetries: number = 3,
    delay: number = 1000
  ) {
    return async (...args: T): Promise<R> => {
      let lastError: any

      for (let attempt = 1; attempt <= maxRetries; attempt++) {
        try {
          return await fn(...args)
        } catch (error: any) {
          lastError = error

          // 如果是最后一次尝试，或者错误不可重试，直接抛出
          if (attempt === maxRetries || !isRetryableError(error)) {
            throw error
          }

          // 等待后重试
          await new Promise(resolve => setTimeout(resolve, delay * attempt))
        }
      }

      throw lastError
    }
  }

  /**
   * 判断错误是否可重试
   */
  function isRetryableError(error: any): boolean {
    // 网络错误可重试
    if (!error.response) {
      return true
    }

    const status = error.response.status

    // 5xx 服务器错误可重试
    if (status >= 500) {
      return true
    }

    // 429 请求过于频繁可重试
    if (status === 429) {
      return true
    }

    // 408 请求超时可重试
    if (status === 408) {
      return true
    }

    return false
  }

  return {
    // 错误处理方法
    handleApiError,
    handleAsyncError,
    handleValidationError,

    // 消息显示方法
    showSuccess,
    showWarning,
    showInfo,

    // 错误清除方法
    clearError,
    clearAllErrors,

    // 高级功能
    withErrorHandling,
    createRetryHandler,
    isRetryableError,

    // 错误状态
    errors: errorStore.errors,
    hasErrors: errorStore.hasErrors,
    isOnline: errorStore.isOnline
  }
}
