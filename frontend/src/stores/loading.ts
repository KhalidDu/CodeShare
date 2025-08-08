/**
 * 全局加载状态管理
 */
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

// 加载状态接口
export interface LoadingState {
  id: string
  message?: string
  progress?: number
  cancellable?: boolean
  timestamp: Date
}

export const useLoadingStore = defineStore('loading', () => {
  // 状态
  const loadingStates = ref<Map<string, LoadingState>>(new Map())
  const globalLoading = ref(false)

  // 计算属性
  const isLoading = computed(() => loadingStates.value.size > 0 || globalLoading.value)
  const loadingCount = computed(() => loadingStates.value.size)
  const loadingList = computed(() => Array.from(loadingStates.value.values()))
  const primaryLoading = computed(() => {
    const states = loadingList.value
    return states.length > 0 ? states[states.length - 1] : null
  })

  // 生成加载ID
  function generateLoadingId(): string {
    return `loading_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`
  }

  /**
   * 开始加载
   */
  function startLoading(
    message?: string,
    options: {
      id?: string
      progress?: number
      cancellable?: boolean
    } = {}
  ): string {
    const id = options.id || generateLoadingId()

    const loadingState: LoadingState = {
      id,
      message,
      progress: options.progress,
      cancellable: options.cancellable ?? false,
      timestamp: new Date()
    }

    loadingStates.value.set(id, loadingState)
    return id
  }

  /**
   * 更新加载状态
   */
  function updateLoading(
    id: string,
    updates: Partial<Omit<LoadingState, 'id' | 'timestamp'>>
  ) {
    const state = loadingStates.value.get(id)
    if (state) {
      Object.assign(state, updates)
    }
  }

  /**
   * 停止加载
   */
  function stopLoading(id: string) {
    loadingStates.value.delete(id)
  }

  /**
   * 清除所有加载状态
   */
  function clearAllLoading() {
    loadingStates.value.clear()
    globalLoading.value = false
  }

  /**
   * 设置全局加载状态
   */
  function setGlobalLoading(loading: boolean, message?: string) {
    globalLoading.value = loading

    if (loading && message) {
      startLoading(message, { id: 'global' })
    } else if (!loading) {
      stopLoading('global')
    }
  }

  /**
   * 包装异步函数，自动管理加载状态
   */
  function withLoading<T extends any[], R>(
    fn: (...args: T) => Promise<R>,
    message?: string,
    options: {
      id?: string
      showProgress?: boolean
      cancellable?: boolean
    } = {}
  ) {
    return async (...args: T): Promise<R> => {
      const loadingId = startLoading(message, {
        id: options.id,
        cancellable: options.cancellable,
        progress: options.showProgress ? 0 : undefined
      })

      try {
        const result = await fn(...args)

        if (options.showProgress) {
          updateLoading(loadingId, { progress: 100 })
          // 短暂显示完成状态
          setTimeout(() => stopLoading(loadingId), 300)
        } else {
          stopLoading(loadingId)
        }

        return result
      } catch (error) {
        stopLoading(loadingId)
        throw error
      }
    }
  }

  /**
   * 创建进度更新函数
   */
  function createProgressUpdater(loadingId: string) {
    return (progress: number, message?: string) => {
      updateLoading(loadingId, { progress, message })
    }
  }

  /**
   * 批量操作加载管理
   */
  function withBatchLoading<T>(
    items: T[],
    processor: (item: T, index: number, updateProgress: (progress: number) => void) => Promise<void>,
    message: string = '处理中...'
  ) {
    return new Promise<void>((resolve, reject) => {
      const loadingId = startLoading(message, {
        progress: 0,
        cancellable: true
      })

      let completed = 0
      const total = items.length

      const processNext = async () => {
        if (completed >= total) {
          updateLoading(loadingId, { progress: 100, message: '完成' })
          setTimeout(() => {
            stopLoading(loadingId)
            resolve()
          }, 500)
          return
        }

        const item = items[completed]
        const updateProgress = (itemProgress: number) => {
          const totalProgress = ((completed + itemProgress / 100) / total) * 100
          updateLoading(loadingId, {
            progress: totalProgress,
            message: `${message} (${completed + 1}/${total})`
          })
        }

        try {
          await processor(item, completed, updateProgress)
          completed++
          updateProgress(100)

          // 短暂延迟，让用户看到进度更新
          setTimeout(processNext, 50)
        } catch (error) {
          stopLoading(loadingId)
          reject(error)
        }
      }

      processNext()
    })
  }

  return {
    // 状态
    loadingStates,
    globalLoading,

    // 计算属性
    isLoading,
    loadingCount,
    loadingList,
    primaryLoading,

    // 方法
    startLoading,
    updateLoading,
    stopLoading,
    clearAllLoading,
    setGlobalLoading,
    withLoading,
    createProgressUpdater,
    withBatchLoading
  }
})
