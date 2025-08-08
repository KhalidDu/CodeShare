import { ref, onMounted } from 'vue'

/**
 * 资源预加载 Composable
 * 提供各种资源的预加载功能
 */

interface PreloadOptions {
  /** 预加载优先级 */
  priority?: 'high' | 'low'
  /** 是否跨域 */
  crossorigin?: 'anonymous' | 'use-credentials'
  /** 资源类型 */
  as?: 'script' | 'style' | 'image' | 'font' | 'fetch' | 'document'
}

interface PreloadResult {
  success: boolean
  error?: Error
  duration: number
}

export function useResourcePreloader() {
  const preloadedResources = ref<Set<string>>(new Set())
  const preloadResults = ref<Map<string, PreloadResult>>(new Map())
  const isPreloading = ref(false)

  /**
   * 预加载单个资源
   */
  const preloadResource = async (
    href: string,
    options: PreloadOptions = {}
  ): Promise<PreloadResult> => {
    const startTime = performance.now()

    // 如果已经预加载过，直接返回结果
    if (preloadedResources.value.has(href)) {
      const existingResult = preloadResults.value.get(href)
      if (existingResult) {
        return existingResult
      }
    }

    return new Promise((resolve) => {
      const link = document.createElement('link')
      link.rel = 'preload'
      link.href = href

      if (options.as) {
        link.as = options.as
      }

      if (options.crossorigin) {
        link.crossOrigin = options.crossorigin
      }

      const handleLoad = () => {
        const duration = performance.now() - startTime
        const result: PreloadResult = { success: true, duration }

        preloadedResources.value.add(href)
        preloadResults.value.set(href, result)

        cleanup()
        resolve(result)
      }

      const handleError = (error: Event) => {
        const duration = performance.now() - startTime
        const result: PreloadResult = {
          success: false,
          error: new Error(`预加载失败: ${href}`),
          duration
        }

        preloadResults.value.set(href, result)

        cleanup()
        resolve(result)
      }

      const cleanup = () => {
        link.removeEventListener('load', handleLoad)
        link.removeEventListener('error', handleError)
        // 保留 link 元素以维持预加载效果
      }

      link.addEventListener('load', handleLoad)
      link.addEventListener('error', handleError)

      document.head.appendChild(link)
    })
  }

  /**
   * 批量预加载资源
   */
  const preloadResources = async (
    resources: Array<{ href: string; options?: PreloadOptions }>,
    options: { concurrency?: number; timeout?: number } = {}
  ): Promise<PreloadResult[]> => {
    const { concurrency = 3, timeout = 10000 } = options
    isPreloading.value = true

    try {
      const chunks = []
      for (let i = 0; i < resources.length; i += concurrency) {
        chunks.push(resources.slice(i, i + concurrency))
      }

      const results: PreloadResult[] = []

      for (const chunk of chunks) {
        const promises = chunk.map(({ href, options }) =>
          Promise.race([
            preloadResource(href, options),
            new Promise<PreloadResult>((resolve) =>
              setTimeout(() => resolve({
                success: false,
                error: new Error(`预加载超时: ${href}`),
                duration: timeout
              }), timeout)
            )
          ])
        )

        const chunkResults = await Promise.all(promises)
        results.push(...chunkResults)
      }

      return results
    } finally {
      isPreloading.value = false
    }
  }

  /**
   * 预连接到域名
   */
  const preconnect = (href: string, crossorigin?: boolean): void => {
    // 检查是否已经存在相同的预连接
    const existing = document.querySelector(`link[rel="preconnect"][href="${href}"]`)
    if (existing) return

    const link = document.createElement('link')
    link.rel = 'preconnect'
    link.href = href

    if (crossorigin) {
      link.crossOrigin = 'anonymous'
    }

    document.head.appendChild(link)
  }

  /**
   * DNS 预解析
   */
  const dnsPrefetch = (href: string): void => {
    // 检查是否已经存在相同的 DNS 预解析
    const existing = document.querySelector(`link[rel="dns-prefetch"][href="${href}"]`)
    if (existing) return

    const link = document.createElement('link')
    link.rel = 'dns-prefetch'
    link.href = href
    document.head.appendChild(link)
  }

  /**
   * 预加载关键资源
   */
  const preloadCriticalResources = async (): Promise<void> => {
    const criticalResources = [
      // 字体文件
      { href: '/fonts/main.woff2', options: { as: 'font' as const, crossorigin: 'anonymous' as const } },
      // 关键 CSS
      { href: '/css/critical.css', options: { as: 'style' as const } },
      // 关键图片
      { href: '/images/logo.svg', options: { as: 'image' as const } }
    ]

    await preloadResources(criticalResources)
  }

  /**
   * 预加载路由资源
   */
  const preloadRouteResources = async (routeName: string): Promise<void> => {
    const routeResourceMap: Record<string, Array<{ href: string; options?: PreloadOptions }>> = {
      'snippets': [
        { href: '/api/snippets?page=1&pageSize=20', options: { as: 'fetch' } },
        { href: '/js/monaco-editor.js', options: { as: 'script' } }
      ],
      'editor': [
        { href: '/js/monaco-editor.js', options: { as: 'script' } },
        { href: '/css/monaco-editor.css', options: { as: 'style' } }
      ],
      'settings': [
        { href: '/api/user/profile', options: { as: 'fetch' } }
      ]
    }

    const resources = routeResourceMap[routeName]
    if (resources) {
      await preloadResources(resources)
    }
  }

  /**
   * 智能预加载 - 基于用户行为预测
   */
  const smartPreload = (): void => {
    // 监听鼠标悬停事件，预加载可能访问的页面
    document.addEventListener('mouseover', (event) => {
      const target = event.target as HTMLElement
      const link = target.closest('a[href]') as HTMLAnchorElement

      if (link && link.href && !preloadedResources.value.has(link.href)) {
        // 延迟预加载，避免误触发
        setTimeout(() => {
          if (link.matches(':hover')) {
            preloadResource(link.href, { as: 'document' })
          }
        }, 100)
      }
    })

    // 监听触摸开始事件（移动端）
    document.addEventListener('touchstart', (event) => {
      const target = event.target as HTMLElement
      const link = target.closest('a[href]') as HTMLAnchorElement

      if (link && link.href && !preloadedResources.value.has(link.href)) {
        preloadResource(link.href, { as: 'document' })
      }
    })
  }

  /**
   * 获取预加载统计信息
   */
  const getPreloadStats = () => {
    const total = preloadResults.value.size
    const successful = Array.from(preloadResults.value.values())
      .filter(result => result.success).length
    const failed = total - successful
    const averageDuration = total > 0
      ? Array.from(preloadResults.value.values())
          .reduce((sum, result) => sum + result.duration, 0) / total
      : 0

    return {
      total,
      successful,
      failed,
      successRate: total > 0 ? (successful / total) * 100 : 0,
      averageDuration: Math.round(averageDuration)
    }
  }

  /**
   * 清理预加载资源
   */
  const cleanup = (): void => {
    // 移除预加载的 link 元素
    const preloadLinks = document.querySelectorAll('link[rel="preload"]')
    preloadLinks.forEach(link => {
      const href = link.getAttribute('href')
      if (href && preloadedResources.value.has(href)) {
        link.remove()
      }
    })

    preloadedResources.value.clear()
    preloadResults.value.clear()
  }

  /**
   * 检查资源是否已预加载
   */
  const isResourcePreloaded = (href: string): boolean => {
    return preloadedResources.value.has(href)
  }

  /**
   * 获取预加载结果
   */
  const getPreloadResult = (href: string): PreloadResult | undefined => {
    return preloadResults.value.get(href)
  }

  // 在组件挂载时启用智能预加载
  onMounted(() => {
    smartPreload()
    preloadCriticalResources()
  })

  return {
    preloadedResources,
    preloadResults,
    isPreloading,
    preloadResource,
    preloadResources,
    preconnect,
    dnsPrefetch,
    preloadCriticalResources,
    preloadRouteResources,
    smartPreload,
    getPreloadStats,
    cleanup,
    isResourcePreloaded,
    getPreloadResult
  }
}

/**
 * 创建资源预加载策略
 */
export function createPreloadStrategy() {
  const { preloadResources, preconnect, dnsPrefetch } = useResourcePreloader()

  return {
    /**
     * 首页预加载策略
     */
    async homepage() {
      // 预连接到 API 服务器
      preconnect('/api')

      // DNS 预解析外部资源
      dnsPrefetch('https://fonts.googleapis.com')
      dnsPrefetch('https://cdn.jsdelivr.net')

      // 预加载关键资源
      await preloadResources([
        { href: '/api/snippets?page=1&pageSize=10', options: { as: 'fetch' } },
        { href: '/fonts/inter.woff2', options: { as: 'font', crossorigin: 'anonymous' } }
      ])
    },

    /**
     * 编辑器页面预加载策略
     */
    async editor() {
      await preloadResources([
        { href: '/js/monaco-editor.js', options: { as: 'script' } },
        { href: '/css/monaco-editor.css', options: { as: 'style' } },
        { href: '/api/languages', options: { as: 'fetch' } },
        { href: '/api/tags', options: { as: 'fetch' } }
      ])
    },

    /**
     * 用户设置页面预加载策略
     */
    async settings() {
      await preloadResources([
        { href: '/api/user/profile', options: { as: 'fetch' } },
        { href: '/api/user/preferences', options: { as: 'fetch' } }
      ])
    }
  }
}
