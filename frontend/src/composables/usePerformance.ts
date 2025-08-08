import { ref, onMounted, onUnmounted } from 'vue'

/**
 * 性能监控 Composable
 * 提供页面性能监控和优化建议
 */

interface PerformanceMetrics {
  /** 首次内容绘制时间 */
  fcp: number
  /** 最大内容绘制时间 */
  lcp: number
  /** 首次输入延迟 */
  fid: number
  /** 累积布局偏移 */
  cls: number
  /** 首次字节时间 */
  ttfb: number
}

interface ResourceTiming {
  name: string
  duration: number
  size: number
  type: string
}

export function usePerformance() {
  const metrics = ref<Partial<PerformanceMetrics>>({})
  const resources = ref<ResourceTiming[]>([])
  const isSupported = ref(false)

  /**
   * 检查浏览器支持
   */
  const checkSupport = () => {
    isSupported.value = 'performance' in window && 'PerformanceObserver' in window
  }

  /**
   * 获取导航时间指标
   */
  const getNavigationMetrics = () => {
    if (!isSupported.value) return

    const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming
    if (navigation) {
      metrics.value.ttfb = navigation.responseStart - navigation.requestStart
    }
  }

  /**
   * 观察 Web Vitals 指标
   */
  const observeWebVitals = () => {
    if (!isSupported.value) return

    // 观察 LCP (Largest Contentful Paint)
    const lcpObserver = new PerformanceObserver((list) => {
      const entries = list.getEntries()
      const lastEntry = entries[entries.length - 1] as any
      metrics.value.lcp = lastEntry.startTime
    })
    lcpObserver.observe({ entryTypes: ['largest-contentful-paint'] })

    // 观察 FCP (First Contentful Paint)
    const fcpObserver = new PerformanceObserver((list) => {
      const entries = list.getEntries()
      entries.forEach((entry) => {
        if (entry.name === 'first-contentful-paint') {
          metrics.value.fcp = entry.startTime
        }
      })
    })
    fcpObserver.observe({ entryTypes: ['paint'] })

    // 观察 CLS (Cumulative Layout Shift)
    let clsValue = 0
    const clsObserver = new PerformanceObserver((list) => {
      for (const entry of list.getEntries() as any[]) {
        if (!entry.hadRecentInput) {
          clsValue += entry.value
          metrics.value.cls = clsValue
        }
      }
    })
    clsObserver.observe({ entryTypes: ['layout-shift'] })

    // 观察 FID (First Input Delay)
    const fidObserver = new PerformanceObserver((list) => {
      for (const entry of list.getEntries() as any[]) {
        metrics.value.fid = entry.processingStart - entry.startTime
      }
    })
    fidObserver.observe({ entryTypes: ['first-input'] })
  }

  /**
   * 获取资源加载时间
   */
  const getResourceMetrics = () => {
    if (!isSupported.value) return

    const resourceEntries = performance.getEntriesByType('resource') as PerformanceResourceTiming[]
    resources.value = resourceEntries.map((entry) => ({
      name: entry.name,
      duration: entry.duration,
      size: entry.transferSize || 0,
      type: getResourceType(entry.name)
    }))
  }

  /**
   * 获取资源类型
   */
  const getResourceType = (url: string): string => {
    if (url.includes('.js')) return 'script'
    if (url.includes('.css')) return 'stylesheet'
    if (url.match(/\.(png|jpg|jpeg|gif|svg|webp)$/)) return 'image'
    if (url.includes('.woff') || url.includes('.ttf')) return 'font'
    return 'other'
  }

  /**
   * 计算性能分数
   */
  const getPerformanceScore = (): number => {
    let score = 100

    // LCP 评分 (理想 < 2.5s)
    if (metrics.value.lcp) {
      if (metrics.value.lcp > 4000) score -= 30
      else if (metrics.value.lcp > 2500) score -= 15
    }

    // FCP 评分 (理想 < 1.8s)
    if (metrics.value.fcp) {
      if (metrics.value.fcp > 3000) score -= 20
      else if (metrics.value.fcp > 1800) score -= 10
    }

    // CLS 评分 (理想 < 0.1)
    if (metrics.value.cls) {
      if (metrics.value.cls > 0.25) score -= 25
      else if (metrics.value.cls > 0.1) score -= 10
    }

    // FID 评分 (理想 < 100ms)
    if (metrics.value.fid) {
      if (metrics.value.fid > 300) score -= 20
      else if (metrics.value.fid > 100) score -= 10
    }

    return Math.max(0, score)
  }

  /**
   * 获取性能建议
   */
  const getPerformanceRecommendations = (): string[] => {
    const recommendations: string[] = []

    if (metrics.value.lcp && metrics.value.lcp > 2500) {
      recommendations.push('优化最大内容绘制时间：考虑压缩图片、使用 CDN 或优化服务器响应时间')
    }

    if (metrics.value.fcp && metrics.value.fcp > 1800) {
      recommendations.push('优化首次内容绘制时间：减少阻塞渲染的资源、内联关键 CSS')
    }

    if (metrics.value.cls && metrics.value.cls > 0.1) {
      recommendations.push('减少累积布局偏移：为图片和广告预留空间、避免在现有内容上方插入内容')
    }

    if (metrics.value.fid && metrics.value.fid > 100) {
      recommendations.push('优化首次输入延迟：减少 JavaScript 执行时间、使用 Web Workers')
    }

    // 检查大资源
    const largeResources = resources.value.filter(r => r.size > 1024 * 1024) // > 1MB
    if (largeResources.length > 0) {
      recommendations.push(`发现 ${largeResources.length} 个大文件，考虑压缩或分割`)
    }

    return recommendations
  }

  /**
   * 记录自定义性能标记
   */
  const mark = (name: string) => {
    if (isSupported.value) {
      performance.mark(name)
    }
  }

  /**
   * 测量两个标记之间的时间
   */
  const measure = (name: string, startMark: string, endMark?: string) => {
    if (isSupported.value) {
      performance.measure(name, startMark, endMark)
      const measure = performance.getEntriesByName(name, 'measure')[0]
      return measure?.duration || 0
    }
    return 0
  }

  /**
   * 清除性能数据
   */
  const clearMetrics = () => {
    if (isSupported.value) {
      performance.clearMarks()
      performance.clearMeasures()
    }
    metrics.value = {}
    resources.value = []
  }

  /**
   * 导出性能报告
   */
  const exportReport = () => {
    const report = {
      timestamp: new Date().toISOString(),
      url: window.location.href,
      userAgent: navigator.userAgent,
      metrics: metrics.value,
      resources: resources.value,
      score: getPerformanceScore(),
      recommendations: getPerformanceRecommendations()
    }

    const blob = new Blob([JSON.stringify(report, null, 2)], { type: 'application/json' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `performance-report-${Date.now()}.json`
    a.click()
    URL.revokeObjectURL(url)
  }

  onMounted(() => {
    checkSupport()
    if (isSupported.value) {
      // 延迟执行以确保页面完全加载
      setTimeout(() => {
        getNavigationMetrics()
        observeWebVitals()
        getResourceMetrics()
      }, 1000)
    }
  })

  return {
    metrics,
    resources,
    isSupported,
    getPerformanceScore,
    getPerformanceRecommendations,
    mark,
    measure,
    clearMetrics,
    exportReport
  }
}

/**
 * 防抖函数 - 性能优化工具
 */
export function debounce<T extends (...args: any[]) => any>(
  func: T,
  wait: number
): (...args: Parameters<T>) => void {
  let timeout: number | null = null

  return (...args: Parameters<T>) => {
    if (timeout) {
      clearTimeout(timeout)
    }
    timeout = window.setTimeout(() => func(...args), wait)
  }
}

/**
 * 节流函数 - 性能优化工具
 */
export function throttle<T extends (...args: any[]) => any>(
  func: T,
  limit: number
): (...args: Parameters<T>) => void {
  let inThrottle = false

  return (...args: Parameters<T>) => {
    if (!inThrottle) {
      func(...args)
      inThrottle = true
      setTimeout(() => inThrottle = false, limit)
    }
  }
}

/**
 * 预加载资源
 */
export function preloadResource(href: string, as: string = 'fetch'): Promise<void> {
  return new Promise((resolve, reject) => {
    const link = document.createElement('link')
    link.rel = 'preload'
    link.href = href
    link.as = as
    link.onload = () => resolve()
    link.onerror = reject
    document.head.appendChild(link)
  })
}

/**
 * 预连接到域名
 */
export function preconnect(href: string): void {
  const link = document.createElement('link')
  link.rel = 'preconnect'
  link.href = href
  document.head.appendChild(link)
}
