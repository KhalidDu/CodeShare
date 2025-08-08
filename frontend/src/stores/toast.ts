/**
 * Toast 通知状态管理
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { ToastType, ToastPosition, ToastAction } from '@/components/common/ToastNotification.vue'

// Toast 接口
export interface Toast {
  id: string
  type: ToastType
  title?: string
  message: string
  duration: number
  closable: boolean
  showProgress: boolean
  position: ToastPosition
  actions: ToastAction[]
  timestamp: Date
}

// Toast 选项
export interface ToastOptions {
  type?: ToastType
  title?: string
  duration?: number
  closable?: boolean
  showProgress?: boolean
  position?: ToastPosition
  actions?: ToastAction[]
}

export const useToastStore = defineStore('toast', () => {
  // 状态
  const toasts = ref<Toast[]>([])
  const maxToasts = ref(5) // 最大同时显示的 Toast 数量

  // 生成 Toast ID
  function generateToastId(): string {
    return `toast_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`
  }

  /**
   * 添加 Toast
   */
  function addToast(message: string, options: ToastOptions = {}): string {
    const id = generateToastId()

    const toast: Toast = {
      id,
      type: options.type || 'info',
      title: options.title,
      message,
      duration: options.duration ?? 5000,
      closable: options.closable ?? true,
      showProgress: options.showProgress ?? true,
      position: options.position || 'top-right',
      actions: options.actions || [],
      timestamp: new Date()
    }

    // 如果超过最大数量，移除最旧的 Toast
    if (toasts.value.length >= maxToasts.value) {
      const oldestToast = toasts.value[0]
      removeToast(oldestToast.id)
    }

    toasts.value.push(toast)

    // 自动移除 Toast
    if (toast.duration > 0) {
      setTimeout(() => {
        removeToast(id)
      }, toast.duration)
    }

    return id
  }

  /**
   * 移除 Toast
   */
  function removeToast(id: string) {
    const index = toasts.value.findIndex(toast => toast.id === id)
    if (index > -1) {
      toasts.value.splice(index, 1)
    }
  }

  /**
   * 清除所有 Toast
   */
  function clearAllToasts() {
    toasts.value = []
  }

  /**
   * 清除指定位置的 Toast
   */
  function clearToastsByPosition(position: ToastPosition) {
    toasts.value = toasts.value.filter(toast => toast.position !== position)
  }

  /**
   * 显示成功消息
   */
  function success(message: string, options: Omit<ToastOptions, 'type'> = {}): string {
    return addToast(message, { ...options, type: 'success' })
  }

  /**
   * 显示错误消息
   */
  function error(message: string, options: Omit<ToastOptions, 'type'> = {}): string {
    return addToast(message, {
      ...options,
      type: 'error',
      duration: options.duration ?? 8000 // 错误消息显示更长时间
    })
  }

  /**
   * 显示警告消息
   */
  function warning(message: string, options: Omit<ToastOptions, 'type'> = {}): string {
    return addToast(message, {
      ...options,
      type: 'warning',
      duration: options.duration ?? 6000
    })
  }

  /**
   * 显示信息消息
   */
  function info(message: string, options: Omit<ToastOptions, 'type'> = {}): string {
    return addToast(message, { ...options, type: 'info' })
  }

  /**
   * 显示操作成功消息
   */
  function actionSuccess(action: string, options: Omit<ToastOptions, 'type'> = {}): string {
    return success(`${action}成功`, {
      duration: 3000,
      showProgress: true,
      ...options
    })
  }

  /**
   * 显示操作失败消息
   */
  function actionError(action: string, errorMessage?: string, options: Omit<ToastOptions, 'type'> = {}): string {
    const message = errorMessage ? `${action}失败：${errorMessage}` : `${action}失败`
    return error(message, {
      duration: 8000,
      closable: true,
      ...options
    })
  }

  /**
   * 显示加载完成消息
   */
  function loadingComplete(message: string = '加载完成', options: Omit<ToastOptions, 'type'> = {}): string {
    return success(message, {
      duration: 2000,
      showProgress: false,
      ...options
    })
  }

  /**
   * 显示网络错误消息
   */
  function networkError(options: Omit<ToastOptions, 'type'> = {}): string {
    return error('网络连接失败，请检查网络设置', {
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
   * 显示权限错误消息
   */
  function permissionError(options: Omit<ToastOptions, 'type'> = {}): string {
    return error('权限不足，无法执行此操作', {
      duration: 6000,
      ...options
    })
  }

  /**
   * 显示确认操作 Toast
   */
  function confirm(
    message: string,
    onConfirm: () => void,
    onCancel?: () => void,
    options: Omit<ToastOptions, 'type' | 'actions'> = {}
  ): string {
    return addToast(message, {
      ...options,
      type: 'warning',
      duration: 0, // 不自动关闭
      actions: [
        {
          label: '确认',
          type: 'primary',
          handler: onConfirm
        },
        {
          label: '取消',
          type: 'secondary',
          handler: onCancel || (() => {})
        }
      ]
    })
  }

  /**
   * 更新 Toast
   */
  function updateToast(id: string, updates: Partial<Omit<Toast, 'id' | 'timestamp'>>) {
    const toast = toasts.value.find(t => t.id === id)
    if (toast) {
      Object.assign(toast, updates)
    }
  }

  return {
    // 状态
    toasts,
    maxToasts,

    // 基础方法
    addToast,
    removeToast,
    clearAllToasts,
    clearToastsByPosition,
    updateToast,

    // 便捷方法
    success,
    error,
    warning,
    info,
    actionSuccess,
    actionError,
    loadingComplete,
    networkError,
    permissionError,
    confirm
  }
})
