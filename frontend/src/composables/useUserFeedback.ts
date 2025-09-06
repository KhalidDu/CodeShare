/**
 * 用户反馈组合式函数
 * 提供统一的用户反馈接口
 */
import { useToastStore } from '@/stores/toast'
import { useLoadingStore } from '@/stores/loading'
import type { ToastOptions } from '@/stores/toast'

export function useUserFeedback() {
  const toastStore = useToastStore()
  const loadingStore = useLoadingStore()

  /**
   * 显示成功消息
   */
  function showSuccess(
    message: string,
    options?: Omit<ToastOptions, 'type'>
  ): string {
    return toastStore.success(message, options)
  }

  /**
   * 显示错误消息
   */
  function showError(
    message: string,
    options?: Omit<ToastOptions, 'type'>
  ): string {
    return toastStore.error(message, options)
  }

  /**
   * 显示警告消息
   */
  function showWarning(
    message: string,
    options?: Omit<ToastOptions, 'type'>
  ): string {
    return toastStore.warning(message, options)
  }

  /**
   * 显示信息消息
   */
  function showInfo(
    message: string,
    options?: Omit<ToastOptions, 'type'>
  ): string {
    return toastStore.info(message, options)
  }

  /**
   * 显示操作成功反馈
   */
  function showActionSuccess(
    action: string,
    details?: string,
    options?: Omit<ToastOptions, 'type'>
  ): string {
    const message = details ? `${action}成功：${details}` : `${action}成功`
    return showSuccess(message, {
      duration: 3000,
      showProgress: true,
      ...options
    })
  }

  /**
   * 显示操作失败反馈
   */
  function showActionError(
    action: string,
    error?: string | Error,
    options?: Omit<ToastOptions, 'type'>
  ): string {
    let errorMessage = ''
    if (error) {
      errorMessage = typeof error === 'string' ? error : error.message
    }

    const message = errorMessage ? `${action}失败：${errorMessage}` : `${action}失败`
    return showError(message, {
      duration: 8000,
      closable: true,
      ...options
    })
  }

  /**
   * 显示加载反馈
   */
  function showLoading(
    message: string = '加载中...',
    options?: {
      progress?: boolean
      cancellable?: boolean
      global?: boolean
    }
  ): string {
    if (options?.global) {
      loadingStore.setGlobalLoading(true, message)
      return 'global'
    } else {
      return loadingStore.startLoading(message, {
        progress: options?.progress ? 0 : undefined,
        cancellable: options?.cancellable
      })
    }
  }

  /**
   * 隐藏加载反馈
   */
  function hideLoading(loadingId?: string) {
    if (loadingId === 'global') {
      loadingStore.setGlobalLoading(false)
    } else if (loadingId) {
      loadingStore.stopLoading(loadingId)
    } else {
      loadingStore.clearAllLoading()
    }
  }

  /**
   * 更新加载进度
   */
  function updateLoadingProgress(
    loadingId: string,
    progress: number,
    message?: string
  ) {
    loadingStore.updateLoading(loadingId, { progress, message })
  }

  /**
   * 显示确认对话框
   */
  function showConfirm(
    message: string,
    onConfirm: () => void | Promise<void>,
    onCancel?: () => void,
    options?: {
      title?: string
      confirmText?: string
      cancelText?: string
      type?: 'warning' | 'error' | 'info'
    }
  ): string {
    return toastStore.confirm(
      message,
      async () => {
        try {
          await onConfirm()
        } catch (error) {
          console.error('Confirm action failed:', error)
        }
      },
      onCancel,
      {
        title: options?.title,
        // type: options?.type || 'warning', // Remove this line as type is not in ToastOptions
        position: 'top-center'
      }
    )
  }

  /**
   * 显示网络错误反馈
   */
  function showNetworkError(
    customMessage?: string,
    options?: Omit<ToastOptions, 'type'>
  ): string {
    const message = customMessage || '网络连接失败，请检查网络设置'
    return showError(message, {
      duration: 0, // 不自动关闭
      actions: [
        {
          label: '重试',
          type: 'primary',
          handler: () => {
            window.location.reload()
          }
        }
      ],
      ...options
    })
  }

  /**
   * 显示权限错误反馈
   */
  function showPermissionError(
    customMessage?: string,
    options?: Omit<ToastOptions, 'type'>
  ): string {
    const message = customMessage || '权限不足，无法执行此操作'
    return showError(message, {
      duration: 6000,
      ...options
    })
  }

  /**
   * 显示表单验证错误
   */
  function showValidationErrors(
    _errors: Record<string, string[]>,
    options?: Omit<ToastOptions, 'type'>
  ): string {
    return showError('表单验证失败', {
      title: '请检查以下字段',
      duration: 8000,
      ...options
    })
  }

  /**
   * 批量操作反馈
   */
  function createBatchFeedback(
    totalItems: number,
    operation: string = '处理'
  ) {
    let successCount = 0
    let errorCount = 0
    let loadingId: string | null = null

    const start = () => {
      loadingId = showLoading(`${operation}中...`, {
        progress: true,
        global: true
      })
    }

    const updateProgress = (completed: number, current?: string) => {
      const progress = (completed / totalItems) * 100
      const message = current
        ? `${operation}中... ${current} (${completed}/${totalItems})`
        : `${operation}中... (${completed}/${totalItems})`

      if (loadingId) {
        updateLoadingProgress(loadingId, progress, message)
      }
    }

    const recordSuccess = () => {
      successCount++
    }

    const recordError = (error?: string) => {
      errorCount++
      console.error('Batch operation error:', error)
    }

    const finish = () => {
      if (loadingId) {
        hideLoading(loadingId)
      }

      if (errorCount === 0) {
        showActionSuccess(operation, `成功${operation}${successCount}项`)
      } else if (successCount === 0) {
        showActionError(operation, `${operation}失败，共${errorCount}项失败`)
      } else {
        showWarning(
          `${operation}完成`,
          {
            title: `成功${successCount}项，失败${errorCount}项`,
            duration: 8000
          }
        )
      }
    }

    return {
      start,
      updateProgress,
      recordSuccess,
      recordError,
      finish,
      get stats() {
        return { successCount, errorCount, totalItems }
      }
    }
  }

  /**
   * 包装异步操作，自动显示反馈
   */
  function withFeedback<T extends unknown[], R>(
    fn: (...args: T) => Promise<R>,
    options: {
      loadingMessage?: string
      successMessage?: string | ((result: R) => string)
      errorMessage?: string | ((error: Error) => string)
      showLoading?: boolean
      showSuccess?: boolean
      showError?: boolean
      global?: boolean
    } = {}
  ) {
    return async (...args: T): Promise<R> => {
      let loadingId: string | null = null

      // 显示加载状态
      if (options.showLoading !== false) {
        loadingId = showLoading(
          options.loadingMessage || '处理中...',
          { global: options.global }
        )
      }

      try {
        const result = await fn(...args)

        // 显示成功反馈
        if (options.showSuccess !== false && options.successMessage) {
          const message = typeof options.successMessage === 'function'
            ? options.successMessage(result)
            : options.successMessage
          showSuccess(message)
        }

        return result
      } catch (error: unknown) {
        // 显示错误反馈
        if (options.showError !== false) {
          const errorMessage = error instanceof Error ? error : new Error('未知错误')
          const message = options.errorMessage
            ? typeof options.errorMessage === 'function'
              ? options.errorMessage(errorMessage)
              : options.errorMessage
            : errorMessage.message || '操作失败'
          showError(message)
        }

        throw error
      } finally {
        // 隐藏加载状态
        if (loadingId) {
          hideLoading(loadingId)
        }
      }
    }
  }

  return {
    // 基础反馈
    showSuccess,
    showError,
    showWarning,
    showInfo,

    // 操作反馈
    showActionSuccess,
    showActionError,

    // 加载反馈
    showLoading,
    hideLoading,
    updateLoadingProgress,

    // 特殊反馈
    showConfirm,
    showNetworkError,
    showPermissionError,
    showValidationErrors,

    // 高级功能
    createBatchFeedback,
    withFeedback
  }
}
