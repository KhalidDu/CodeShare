/**
 * 加载状态组合式函数
 * 提供便捷的加载状态管理
 */
import { ref, computed } from 'vue'
import { useLoadingStore } from '@/stores/loading'

export function useLoading(initialMessage?: string) {
  const loadingStore = useLoadingStore()

  // 局部加载状态
  const localLoading = ref(false)
  const localMessage = ref(initialMessage || '加载中...')
  const localProgress = ref<number | undefined>(undefined)

  // 全局加载 ID
  let globalLoadingId: string | null = null

  // 计算属性
  const isLoading = computed(() => localLoading.value || loadingStore.isLoading)
  const message = computed(() => localMessage.value)
  const progress = computed(() => localProgress.value)

  /**
   * 开始局部加载
   */
  function startLoading(message?: string, showProgress?: boolean) {
    localLoading.value = true
    if (message) {
      localMessage.value = message
    }
    if (showProgress) {
      localProgress.value = 0
    }
  }

  /**
   * 停止局部加载
   */
  function stopLoading() {
    localLoading.value = false
    localProgress.value = undefined
  }

  /**
   * 更新加载消息
   */
  function updateMessage(message: string) {
    localMessage.value = message
  }

  /**
   * 更新加载进度
   */
  function updateProgress(progress: number) {
    localProgress.value = Math.max(0, Math.min(100, progress))
  }

  /**
   * 开始全局加载
   */
  function startGlobalLoading(message?: string, options?: { progress?: boolean; cancellable?: boolean }) {
    globalLoadingId = loadingStore.startLoading(message || localMessage.value, {
      progress: options?.progress ? 0 : undefined,
      cancellable: options?.cancellable
    })
    return globalLoadingId
  }

  /**
   * 停止全局加载
   */
  function stopGlobalLoading() {
    if (globalLoadingId) {
      loadingStore.stopLoading(globalLoadingId)
      globalLoadingId = null
    }
  }

  /**
   * 更新全局加载状态
   */
  function updateGlobalLoading(updates: { message?: string; progress?: number }) {
    if (globalLoadingId) {
      loadingStore.updateLoading(globalLoadingId, updates)
    }
  }

  /**
   * 包装异步函数，自动管理加载状态
   */
  function withLoading<T extends any[], R>(
    fn: (...args: T) => Promise<R>,
    options?: {
      message?: string
      global?: boolean
      showProgress?: boolean
      onProgress?: (progress: number) => void
    }
  ) {
    return async (...args: T): Promise<R> => {
      const message = options?.message || localMessage.value

      if (options?.global) {
        startGlobalLoading(message, {
          progress: options.showProgress,
          cancellable: false
        })
      } else {
        startLoading(message, options?.showProgress)
      }

      try {
        // 如果提供了进度回调，创建进度更新函数
        if (options?.onProgress) {
          const updateProgressFn = (progress: number) => {
            options.onProgress!(progress)
            if (options.global) {
              updateGlobalLoading({ progress })
            } else {
              updateProgress(progress)
            }
          }

          // 将进度更新函数传递给异步函数（如果支持）
          if (fn.length > args.length) {
            return await (fn as any)(...args, updateProgressFn)
          }
        }

        const result = await fn(...args)

        // 显示完成进度
        if (options?.showProgress) {
          if (options.global) {
            updateGlobalLoading({ progress: 100 })
          } else {
            updateProgress(100)
          }

          // 短暂显示完成状态
          await new Promise(resolve => setTimeout(resolve, 300))
        }

        return result
      } finally {
        if (options?.global) {
          stopGlobalLoading()
        } else {
          stopLoading()
        }
      }
    }
  }

  /**
   * 创建步骤式加载
   */
  function createStepLoading(steps: string[]) {
    let currentStep = 0

    const nextStep = (customMessage?: string) => {
      if (currentStep < steps.length) {
        const message = customMessage || steps[currentStep]
        const progress = ((currentStep + 1) / steps.length) * 100

        updateMessage(message)
        updateProgress(progress)

        if (globalLoadingId) {
          updateGlobalLoading({ message, progress })
        }

        currentStep++
      }
    }

    const reset = () => {
      currentStep = 0
      updateProgress(0)
    }

    return { nextStep, reset, currentStep: () => currentStep }
  }

  /**
   * 批量处理加载
   */
  async function withBatchLoading<T>(
    items: T[],
    processor: (item: T, index: number) => Promise<void>,
    options?: {
      message?: string
      global?: boolean
      batchSize?: number
    }
  ) {
    const message = options?.message || '批量处理中...'
    const batchSize = options?.batchSize || 1
    const total = items.length

    if (options?.global) {
      startGlobalLoading(message, { progress: true })
    } else {
      startLoading(message, true)
    }

    try {
      for (let i = 0; i < total; i += batchSize) {
        const batch = items.slice(i, i + batchSize)

        // 并行处理批次内的项目
        await Promise.all(
          batch.map((item, batchIndex) =>
            processor(item, i + batchIndex)
          )
        )

        // 更新进度
        const progress = ((i + batch.length) / total) * 100
        const progressMessage = `${message} (${Math.min(i + batch.length, total)}/${total})`

        if (options?.global) {
          updateGlobalLoading({ progress, message: progressMessage })
        } else {
          updateProgress(progress)
          updateMessage(progressMessage)
        }

        // 短暂延迟，让用户看到进度更新
        if (i + batchSize < total) {
          await new Promise(resolve => setTimeout(resolve, 50))
        }
      }
    } finally {
      if (options?.global) {
        stopGlobalLoading()
      } else {
        stopLoading()
      }
    }
  }

  return {
    // 状态
    isLoading,
    message,
    progress,

    // 局部加载控制
    startLoading,
    stopLoading,
    updateMessage,
    updateProgress,

    // 全局加载控制
    startGlobalLoading,
    stopGlobalLoading,
    updateGlobalLoading,

    // 高级功能
    withLoading,
    createStepLoading,
    withBatchLoading
  }
}

/**
 * 简单的加载状态 Hook
 */
export function useSimpleLoading(initialLoading = false) {
  const loading = ref(initialLoading)

  const startLoading = () => {
    loading.value = true
  }

  const stopLoading = () => {
    loading.value = false
  }

  const withLoading = async <T>(fn: () => Promise<T>): Promise<T> => {
    startLoading()
    try {
      return await fn()
    } finally {
      stopLoading()
    }
  }

  return {
    loading,
    startLoading,
    stopLoading,
    withLoading
  }
}
