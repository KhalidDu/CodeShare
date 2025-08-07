import { createApp, type App } from 'vue'
import Toast from '@/components/common/Toast.vue'

interface ToastOptions {
  message: string
  title?: string
  type?: 'success' | 'error' | 'warning' | 'info'
  duration?: number
  closable?: boolean
}

class ToastManager {
  private toasts: App[] = []

  /**
   * 显示 Toast 消息
   */
  show(options: ToastOptions): void {
    const container = document.createElement('div')
    document.body.appendChild(container)

    const app = createApp(Toast, {
      ...options,
      onClose: () => {
        this.remove(app, container)
      }
    })

    app.mount(container)
    this.toasts.push(app)
  }

  /**
   * 显示成功消息
   */
  success(message: string, title?: string): void {
    this.show({
      message,
      title,
      type: 'success',
      duration: 3000
    })
  }

  /**
   * 显示错误消息
   */
  error(message: string, title?: string): void {
    this.show({
      message,
      title,
      type: 'error',
      duration: 5000
    })
  }

  /**
   * 显示警告消息
   */
  warning(message: string, title?: string): void {
    this.show({
      message,
      title,
      type: 'warning',
      duration: 4000
    })
  }

  /**
   * 显示信息消息
   */
  info(message: string, title?: string): void {
    this.show({
      message,
      title,
      type: 'info',
      duration: 3000
    })
  }

  /**
   * 移除 Toast
   */
  private remove(app: App, container: HTMLElement): void {
    app.unmount()
    document.body.removeChild(container)
    
    const index = this.toasts.indexOf(app)
    if (index > -1) {
      this.toasts.splice(index, 1)
    }
  }

  /**
   * 清除所有 Toast
   */
  clear(): void {
    this.toasts.forEach(app => {
      app.unmount()
    })
    this.toasts = []
    
    // 清除所有 Toast 容器
    const containers = document.querySelectorAll('[data-toast-container]')
    containers.forEach(container => {
      document.body.removeChild(container)
    })
  }
}

const toastManager = new ToastManager()

/**
 * Toast 消息 composable
 * 提供全局的消息提示功能
 */
export function useToast() {
  return {
    /**
     * 显示自定义 Toast
     */
    show: (options: ToastOptions) => toastManager.show(options),
    
    /**
     * 显示成功消息
     */
    success: (message: string, title?: string) => toastManager.success(message, title),
    
    /**
     * 显示错误消息
     */
    error: (message: string, title?: string) => toastManager.error(message, title),
    
    /**
     * 显示警告消息
     */
    warning: (message: string, title?: string) => toastManager.warning(message, title),
    
    /**
     * 显示信息消息
     */
    info: (message: string, title?: string) => toastManager.info(message, title),
    
    /**
     * 清除所有 Toast
     */
    clear: () => toastManager.clear()
  }
}