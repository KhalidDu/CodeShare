// 虚拟滚动工具函数
// 为消息列表提供高性能的虚拟滚动支持

import { ref, computed, onMounted, onUnmounted, nextTick, watch } from 'vue'
import type { Ref } from 'vue'

export interface VirtualScrollOptions {
  itemHeight: number
  containerHeight: number
  overscan?: number
  buffer?: number
}

export interface VirtualScrollResult {
  visibleItems: Ref<any[]>
  startIndex: Ref<number>
  endIndex: Ref<number>
  scrollTop: Ref<number>
  containerRef: Ref<HTMLElement | null>
  contentRef: Ref<HTMLElement | null>
  handleScroll: () => void
  scrollToIndex: (index: number) => void
  scrollToTop: () => void
  scrollToBottom: () => void
  refresh: () => void
  getTotalHeight: () => number
  getVisibleRange: () => { start: number; end: number }
  calculateVisibleRange: (scrollTop: number, containerHeight: number) => { start: number; end: number }
}

export function useVirtualScroll<T = any>(
  items: Ref<T[]>,
  options: VirtualScrollOptions
): VirtualScrollResult {
  const {
    itemHeight,
    containerHeight,
    overscan = 5,
    buffer = 3
  } = options

  // 响应式状态
  const containerRef = ref<HTMLElement | null>(null)
  const contentRef = ref<HTMLElement | null>(null)
  const scrollTop = ref(0)
  const startIndex = ref(0)
  const endIndex = ref(0)
  const isScrolling = ref(false)
  let scrollTimeout: number | null = null

  // 计算可见项目
  const visibleItems = computed(() => {
    const start = Math.max(0, startIndex.value - buffer)
    const end = Math.min(items.value.length - 1, endIndex.value + buffer)
    return items.value.slice(start, end + 1)
  })

  // 计算总高度
  const getTotalHeight = () => {
    return items.value.length * itemHeight
  }

  // 获取可见范围
  const getVisibleRange = () => {
    return {
      start: startIndex.value,
      end: endIndex.value
    }
  }

  // 计算可见范围（用于性能监控和调试）
  const calculateVisibleRange = (scrollTopValue: number, containerHeightValue: number) => {
    const start = Math.floor(scrollTopValue / itemHeight)
    const visibleCount = Math.ceil(containerHeightValue / itemHeight)
    const end = Math.min(items.value.length - 1, start + visibleCount - 1)
    
    return {
      start: Math.max(0, start - overscan),
      end: Math.min(items.value.length - 1, end + overscan)
    }
  }

  // 更新可见项目范围
  const updateVisibleRange = () => {
    if (!containerRef.value) return

    const container = containerRef.value
    const scrollTopValue = container.scrollTop
    
    // 计算可见项目的起始和结束索引
    const start = Math.floor(scrollTopValue / itemHeight)
    const visibleCount = Math.ceil(containerHeight / itemHeight)
    const end = Math.min(items.value.length - 1, start + visibleCount - 1)

    startIndex.value = Math.max(0, start - overscan)
    endIndex.value = Math.min(items.value.length - 1, end + overscan)
    scrollTop.value = scrollTopValue

    // 更新内容的transform样式
    if (contentRef.value) {
      const offsetY = startIndex.value * itemHeight
      contentRef.value.style.transform = `translateY(${offsetY}px)`
    }
  }

  // 处理滚动事件
  const handleScroll = () => {
    if (!containerRef.value) return

    isScrolling.value = true
    
    // 清除之前的超时
    if (scrollTimeout) {
      clearTimeout(scrollTimeout)
    }

    // 设置新的超时来标记滚动结束
    scrollTimeout = window.setTimeout(() => {
      isScrolling.value = false
    }, 150)

    updateVisibleRange()
  }

  // 滚动到指定索引
  const scrollToIndex = (index: number) => {
    if (!containerRef.value || index < 0 || index >= items.value.length) return

    const container = containerRef.value
    const targetScrollTop = index * itemHeight
    
    container.scrollTo({
      top: targetScrollTop,
      behavior: 'smooth'
    })
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
    const totalHeight = getTotalHeight()
    
    container.scrollTo({
      top: totalHeight,
      behavior: 'smooth'
    })
  }

  // 刷新虚拟滚动
  const refresh = () => {
    nextTick(() => {
      updateVisibleRange()
    })
  }

  // 监听项目变化
  watch(items, () => {
    refresh()
  }, { deep: true })

  // 监听容器引用变化
  watch(containerRef, (newValue) => {
    if (newValue) {
      updateVisibleRange()
    }
  })

  // 组件挂载时初始化
  onMounted(() => {
    nextTick(() => {
      updateVisibleRange()
    })
  })

  // 组件卸载时清理
  onUnmounted(() => {
    if (scrollTimeout) {
      clearTimeout(scrollTimeout)
    }
  })

  return {
    visibleItems,
    startIndex,
    endIndex,
    scrollTop,
    containerRef,
    contentRef,
    handleScroll,
    scrollToIndex,
    scrollToTop,
    scrollToBottom,
    refresh,
    getTotalHeight,
    getVisibleRange,
    calculateVisibleRange
  }
}

// 动态高度虚拟滚动
export function useDynamicVirtualScroll<T = any>(
  items: Ref<T[]>,
  getHeight: (item: T, index: number) => number,
  options: {
    containerHeight: number
    overscan?: number
    buffer?: number
    defaultItemHeight?: number
  }
) {
  const {
    containerHeight,
    overscan = 5,
    buffer = 3,
    defaultItemHeight = 50
  } = options

  const containerRef = ref<HTMLElement | null>(null)
  const contentRef = ref<HTMLElement | null>(null)
  const scrollTop = ref(0)
  const startIndex = ref(0)
  const endIndex = ref(0)
  const itemPositions = ref<number[]>([])
  const totalHeight = ref(0)

  // 计算项目位置
  const calculateItemPositions = () => {
    const positions: number[] = []
    let currentTop = 0

    for (let i = 0; i < items.value.length; i++) {
      positions[i] = currentTop
      const height = getHeight(items.value[i], i)
      currentTop += height
    }

    itemPositions.value = positions
    totalHeight.value = currentTop
  }

  // 更新可见项目范围
  const updateVisibleRange = () => {
    if (!containerRef.value) return

    const container = containerRef.value
    const scrollTopValue = container.scrollTop
    scrollTop.value = scrollTopValue

    // 二分查找找到起始项目
    let start = 0
    let end = items.value.length - 1
    
    while (start <= end) {
      const mid = Math.floor((start + end) / 2)
      if (itemPositions.value[mid] <= scrollTopValue) {
        start = mid + 1
      } else {
        end = mid - 1
      }
    }

    startIndex.value = Math.max(0, start - 1 - overscan)
    
    // 找到结束项目
    const viewportBottom = scrollTopValue + containerHeight
    let visibleEnd = start
    
    while (visibleEnd < items.value.length && itemPositions.value[visibleEnd] < viewportBottom) {
      visibleEnd++
    }
    
    endIndex.value = Math.min(items.value.length - 1, visibleEnd - 1 + overscan)

    // 更新内容的transform样式
    if (contentRef.value) {
      const offsetY = itemPositions.value[startIndex.value] || 0
      contentRef.value.style.transform = `translateY(${offsetY}px)`
    }
  }

  // 处理滚动事件
  const handleScroll = () => {
    if (!containerRef.value) return
    updateVisibleRange()
  }

  // 其他方法与静态高度版本类似
  const scrollToIndex = (index: number) => {
    if (!containerRef.value || index < 0 || index >= items.value.length) return

    const container = containerRef.value
    const targetScrollTop = itemPositions.value[index] || 0
    
    container.scrollTo({
      top: targetScrollTop,
      behavior: 'smooth'
    })
  }

  // 监听项目变化
  watch(items, () => {
    calculateItemPositions()
    updateVisibleRange()
  }, { deep: true })

  return {
    visibleItems: computed(() => {
      const start = Math.max(0, startIndex.value - buffer)
      const end = Math.min(items.value.length - 1, endIndex.value + buffer)
      return items.value.slice(start, end + 1).map((item, index) => ({
        item,
        index: start + index,
        position: itemPositions.value[start + index] || 0,
        height: getHeight(item, start + index)
      }))
    }),
    startIndex,
    endIndex,
    scrollTop,
    containerRef,
    contentRef,
    handleScroll,
    scrollToIndex,
    totalHeight: computed(() => totalHeight.value),
    refresh: () => {
      calculateItemPositions()
      updateVisibleRange()
    }
  }
}