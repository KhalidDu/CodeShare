// 无限滚动工具函数
// 为消息列表提供无限滚动加载功能

import { ref, computed, onMounted, onUnmounted, nextTick, watch } from 'vue'
import type { Ref } from 'vue'

export interface InfiniteScrollOptions {
  threshold?: number
  distance?: number
  direction?: 'vertical' | 'horizontal'
  immediate?: boolean
  debounce?: number
  disabled?: boolean
}

export interface InfiniteScrollResult {
  isLoading: Ref<boolean>
  hasMore: Ref<boolean>
  page: Ref<number>
  error: Ref<Error | null>
  containerRef: Ref<HTMLElement | null>
  loadMore: () => Promise<void>
  reset: () => void
  reload: () => Promise<void>
  setDisabled: (disabled: boolean) => void
  checkScrollPosition: () => void
  scrollToTop: () => void
  scrollToBottom: () => void
}

export function useInfiniteScroll<T = any>(
  loadFunction: (page: number) => Promise<{ data: T[]; hasMore: boolean }>,
  options: InfiniteScrollOptions = {}
): InfiniteScrollResult {
  const {
    threshold = 100,
    distance = 0,
    direction = 'vertical',
    immediate = true,
    debounce = 100,
    disabled = false
  } = options

  // 响应式状态
  const isLoading = ref(false)
  const hasMore = ref(true)
  const page = ref(1)
  const error = ref<Error | null>(null)
  const containerRef = ref<HTMLElement | null>(null)
  const isDisabled = ref(disabled)
  let debounceTimer: number | null = null

  // 检查是否应该加载更多
  const shouldLoadMore = () => {
    if (!containerRef.value || isDisabled.value || isLoading.value || !hasMore.value) {
      return false
    }

    const container = containerRef.value
    const isVertical = direction === 'vertical'

    if (isVertical) {
      const { scrollTop, scrollHeight, clientHeight } = container
      const remainingScroll = scrollHeight - scrollTop - clientHeight
      return remainingScroll <= threshold + distance
    } else {
      const { scrollLeft, scrollWidth, clientWidth } = container
      const remainingScroll = scrollWidth - scrollLeft - clientWidth
      return remainingScroll <= threshold + distance
    }
  }

  // 加载更多数据
  const loadMore = async () => {
    if (isLoading.value || !hasMore.value || isDisabled.value) {
      return
    }

    isLoading.value = true
    error.value = null

    try {
      const result = await loadFunction(page.value)
      
      if (result.hasMore) {
        page.value++
      } else {
        hasMore.value = false
      }
    } catch (err) {
      error.value = err instanceof Error ? err : new Error('加载失败')
      hasMore.value = false
    } finally {
      isLoading.value = false
    }
  }

  // 防抖处理
  const debouncedLoadMore = () => {
    if (debounceTimer) {
      clearTimeout(debounceTimer)
    }
    
    debounceTimer = window.setTimeout(() => {
      if (shouldLoadMore()) {
        loadMore()
      }
    }, debounce)
  }

  // 检查滚动位置
  const checkScrollPosition = () => {
    if (shouldLoadMore()) {
      debouncedLoadMore()
    }
  }

  // 处理滚动事件
  const handleScroll = () => {
    checkScrollPosition()
  }

  // 重置状态
  const reset = () => {
    isLoading.value = false
    hasMore.value = true
    page.value = 1
    error.value = null
  }

  // 重新加载
  const reload = async () => {
    reset()
    if (immediate) {
      await loadMore()
    }
  }

  // 设置禁用状态
  const setDisabled = (disabled: boolean) => {
    isDisabled.value = disabled
  }

  // 滚动到顶部
  const scrollToTop = () => {
    if (!containerRef.value) return
    
    containerRef.value.scrollTo({
      top: 0,
      behavior: 'smooth'
    })
  }

  // 滚动到底部
  const scrollToBottom = () => {
    if (!containerRef.value) return
    
    const container = containerRef.value
    container.scrollTo({
      top: container.scrollHeight,
      behavior: 'smooth'
    })
  }

  // 监听容器引用变化
  watch(containerRef, (newValue) => {
    if (newValue) {
      newValue.addEventListener('scroll', handleScroll, { passive: true })
      
      // 立即检查一次
      nextTick(() => {
        checkScrollPosition()
      })
    }
  })

  // 组件挂载时初始化
  onMounted(() => {
    if (immediate && !isDisabled.value) {
      loadMore()
    }
  })

  // 组件卸载时清理
  onUnmounted(() => {
    if (containerRef.value) {
      containerRef.value.removeEventListener('scroll', handleScroll)
    }
    
    if (debounceTimer) {
      clearTimeout(debounceTimer)
    }
  })

  return {
    isLoading,
    hasMore,
    page,
    error,
    containerRef,
    loadMore,
    reset,
    reload,
    setDisabled,
    checkScrollPosition,
    scrollToTop,
    scrollToBottom
  }
}

// 消息特定的无限滚动
export function useMessageInfiniteScroll(
  loadFunction: (page: number) => Promise<{ messages: any[]; hasMore: boolean; total?: number }>,
  options: InfiniteScrollOptions = {}
) {
  const baseScroll = useInfiniteScroll(loadFunction, options)
  const allMessages = ref<any[]>([])
  const totalMessages = ref(0)

  // 重写loadMore方法
  const loadMore = async () => {
    if (baseScroll.isLoading.value || !baseScroll.hasMore.value || baseScroll.isDisabled.value) {
      return
    }

    baseScroll.isLoading.value = true
    baseScroll.error.value = null

    try {
      const result = await loadFunction(baseScroll.page.value)
      
      // 添加新消息到列表
      allMessages.value = [...allMessages.value, ...result.messages]
      
      if (result.total !== undefined) {
        totalMessages.value = result.total
      }
      
      if (result.hasMore) {
        baseScroll.page.value++
      } else {
        baseScroll.hasMore.value = false
      }
    } catch (err) {
      baseScroll.error.value = err instanceof Error ? err : new Error('加载消息失败')
      baseScroll.hasMore.value = false
    } finally {
      baseScroll.isLoading.value = false
    }
  }

  // 重写reset方法
  const reset = () => {
    baseScroll.reset()
    allMessages.value = []
    totalMessages.value = 0
  }

  // 重写reload方法
  const reload = async () => {
    reset()
    if (options.immediate !== false && !baseScroll.isDisabled.value) {
      await loadMore()
    }
  }

  // 添加新消息到列表（用于实时更新）
  const addMessage = (message: any) => {
    allMessages.value = [message, ...allMessages.value]
    totalMessages.value++
  }

  // 更新消息
  const updateMessage = (messageId: string, updates: Partial<any>) => {
    const index = allMessages.value.findIndex(msg => msg.id === messageId)
    if (index !== -1) {
      allMessages.value[index] = { ...allMessages.value[index], ...updates }
    }
  }

  // 删除消息
  const removeMessage = (messageId: string) => {
    allMessages.value = allMessages.value.filter(msg => msg.id !== messageId)
    totalMessages.value = Math.max(0, totalMessages.value - 1)
  }

  // 标记消息为已读
  const markMessagesAsRead = (messageIds: string[]) => {
    allMessages.value = allMessages.value.map(msg => ({
      ...msg,
      isRead: messageIds.includes(msg.id) ? true : msg.isRead
    }))
  }

  return {
    ...baseScroll,
    allMessages: computed(() => allMessages.value),
    totalMessages: computed(() => totalMessages.value),
    loadMore,
    reset,
    reload,
    addMessage,
    updateMessage,
    removeMessage,
    markMessagesAsRead
  }
}

// 自动无限滚动（用于聊天界面等）
export function useAutoInfiniteScroll(
  containerRef: Ref<HTMLElement | null>,
  options: {
    enabled?: boolean
    behavior?: 'auto' | 'smooth'
    threshold?: number
  } = {}
) {
  const { enabled = true, behavior = 'smooth', threshold = 50 } = options
  const isAutoScrolling = ref(false)
  const userScrolled = ref(false)
  let lastScrollTop = 0

  // 检查是否应该自动滚动
  const shouldAutoScroll = () => {
    if (!containerRef.value || !enabled.value || userScrolled.value) {
      return false
    }

    const container = containerRef.value
    const { scrollTop, scrollHeight, clientHeight } = container
    const distanceFromBottom = scrollHeight - scrollTop - clientHeight
    
    return distanceFromBottom <= threshold
  }

  // 自动滚动到底部
  const autoScrollToBottom = () => {
    if (!shouldAutoScroll()) return

    isAutoScrolling.value = true
    
    if (containerRef.value) {
      containerRef.value.scrollTo({
        top: containerRef.value.scrollHeight,
        behavior
      })
    }

    setTimeout(() => {
      isAutoScrolling.value = false
    }, 100)
  }

  // 处理滚动事件
  const handleScroll = () => {
    if (!containerRef.value) return

    const container = containerRef.value
    const currentScrollTop = container.scrollTop
    
    // 检测用户是否向上滚动
    if (currentScrollTop < lastScrollTop && !isAutoScrolling.value) {
      userScrolled.value = true
    }
    
    // 如果用户滚动到底部，重置用户滚动状态
    if (currentScrollTop + container.clientHeight >= container.scrollHeight - threshold) {
      userScrolled.value = false
    }
    
    lastScrollTop = currentScrollTop
  }

  // 重置用户滚动状态
  const resetUserScroll = () => {
    userScrolled.value = false
  }

  // 监听容器引用变化
  watch(containerRef, (newValue) => {
    if (newValue) {
      newValue.addEventListener('scroll', handleScroll, { passive: true })
    }
  })

  // 组件卸载时清理
  onUnmounted(() => {
    if (containerRef.value) {
      containerRef.value.removeEventListener('scroll', handleScroll)
    }
  })

  return {
    isAutoScrolling: computed(() => isAutoScrolling.value),
    userScrolled: computed(() => userScrolled.value),
    autoScrollToBottom,
    resetUserScroll
  }
}