import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useBreakpoints, type Breakpoint, type DeviceType, type LayoutType } from './useBreakpoints'

// 增强功能类型定义
export interface ContainerQuery {
  minWidth?: number
  maxWidth?: number
  minHeight?: number
  maxHeight?: number
  orientation?: 'portrait' | 'landscape'
  aspectRatio?: string
}

export interface ResponsiveConfig {
  // 基础响应式
  breakpoints?: Record<string, number>
  containerQueries?: Record<string, ContainerQuery>
  
  // 设备检测
  detectDeviceType?: boolean
  detectTouch?: boolean
  detectNetwork?: boolean
  
  // 性能优化
  enablePerformanceMonitoring?: boolean
  lazyLoadBreakpoints?: boolean
  
  // 布局优化
  autoLayoutSwitching?: boolean
  adaptiveSpacing?: boolean
  responsiveTypography?: boolean
  
  // 用户体验
  reduceMotion?: boolean
  highContrast?: boolean
  preferReducedData?: boolean
}

export interface DeviceInfo {
  type: DeviceType
  model?: string
  os?: string
  browser?: string
  isTouch: boolean
  isMobile: boolean
  isTablet: boolean
  isDesktop: boolean
  screenSize: {
    width: number
    height: number
    availWidth: number
    availHeight: number
  }
  pixelRatio: number
  orientation: 'portrait' | 'landscape'
  connection?: {
    effectiveType: string
    downlink: number
    rtt: number
    saveData: boolean
  }
}

export interface PerformanceMetrics {
  fps: number
  memoryUsage?: number
  networkSpeed?: number
  renderTime: number
  interactionTime: number
  isLowEndDevice: boolean
  recommendations: string[]
}

// 增强响应式系统
export function useEnhancedBreakpoints(config: ResponsiveConfig = {}) {
  const baseBreakpoints = useBreakpoints()
  
  // 配置合并
  const finalConfig = computed(() => ({
    detectDeviceType: true,
    detectTouch: true,
    detectNetwork: true,
    enablePerformanceMonitoring: true,
    lazyLoadBreakpoints: true,
    autoLayoutSwitching: true,
    adaptiveSpacing: true,
    responsiveTypography: true,
    ...config
  }))
  
  // 设备信息
  const deviceInfo = ref<DeviceInfo>({
    type: 'desktop',
    isTouch: false,
    isMobile: false,
    isTablet: false,
    isDesktop: true,
    screenSize: {
      width: 1024,
      height: 768,
      availWidth: 1024,
      availHeight: 768
    },
    pixelRatio: 1,
    orientation: 'landscape'
  })
  
  // 容器查询状态
  const containerStates = ref<Record<string, boolean>>({})
  
  // 性能指标
  const performanceMetrics = ref<PerformanceMetrics>({
    fps: 60,
    renderTime: 0,
    interactionTime: 0,
    isLowEndDevice: false,
    recommendations: []
  })
  
  // 用户偏好
  const userPreferences = ref({
    reduceMotion: false,
    highContrast: false,
    preferReducedData: false,
    colorScheme: 'light' as 'light' | 'dark'
  })
  
  // 响应式布局状态
  const layoutState = ref({
    currentLayout: 'desktop-sidebar' as LayoutType,
    sidebarCollapsed: false,
    mobileMenuOpen: false,
    contentMaxWidth: '1200px',
    spacingScale: 1,
    typographyScale: 1
  })
  
  // 检测设备信息
  const detectDeviceInfo = () => {
    if (typeof window === 'undefined') return
    
    const ua = navigator.userAgent
    const width = window.innerWidth
    const height = window.innerHeight
    
    // 设备类型检测
    let type: DeviceType = 'desktop'
    let model: string | undefined
    let os: string | undefined
    
    // 移动设备检测
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(ua)) {
      type = 'mobile'
      
      if (/iPad/i.test(ua)) {
        type = 'tablet'
        model = 'iPad'
      } else if (/iPhone/i.test(ua)) {
        model = 'iPhone'
      } else if (/Android/i.test(ua)) {
        model = 'Android Device'
        os = 'Android'
      }
    }
    
    // 操作系统检测
    if (!os) {
      if (/Windows/i.test(ua)) os = 'Windows'
      else if (/Mac/i.test(ua)) os = 'macOS'
      else if (/Linux/i.test(ua)) os = 'Linux'
      else if (/iOS/i.test(ua)) os = 'iOS'
    }
    
    // 浏览器检测
    let browser: string
    if (/Chrome/i.test(ua)) browser = 'Chrome'
    else if (/Firefox/i.test(ua)) browser = 'Firefox'
    else if (/Safari/i.test(ua)) browser = 'Safari'
    else if (/Edge/i.test(ua)) browser = 'Edge'
    else browser = 'Unknown'
    
    // 触摸检测
    const isTouch = 'ontouchstart' in window || navigator.maxTouchPoints > 0
    
    deviceInfo.value = {
      type,
      model,
      os,
      browser,
      isTouch,
      isMobile: type === 'mobile',
      isTablet: type === 'tablet',
      isDesktop: type === 'desktop' || type === 'large-desktop',
      screenSize: {
        width,
        height,
        availWidth: screen.width,
        availHeight: screen.height
      },
      pixelRatio: window.devicePixelRatio || 1,
      orientation: height > width ? 'portrait' : 'landscape'
    }
  }
  
  // 检测网络连接
  const detectNetworkConnection = () => {
    if (typeof window === 'undefined' || !('connection' in navigator)) return
    
    const connection = (navigator as any).connection
    
    deviceInfo.value.connection = {
      effectiveType: connection.effectiveType || '4g',
      downlink: connection.downlink || 10,
      rtt: connection.rtt || 100,
      saveData: connection.saveData || false
    }
  }
  
  // 检测用户偏好
  const detectUserPreferences = () => {
    if (typeof window === 'undefined') return
    
    // 减少动画偏好
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches
    
    // 高对比度偏好
    const prefersHighContrast = window.matchMedia('(prefers-contrast: high)').matches
    
    // 减少数据使用偏好
    const prefersReducedData = window.matchMedia('(prefers-reduced-data: reduce)').matches
    
    // 颜色主题偏好
    const prefersDarkColorScheme = window.matchMedia('(prefers-color-scheme: dark)').matches
    
    userPreferences.value = {
      reduceMotion: prefersReducedMotion,
      highContrast: prefersHighContrast,
      preferReducedData: prefersReducedData,
      colorScheme: prefersDarkColorScheme ? 'dark' : 'light'
    }
  }
  
  // 监控性能
  const monitorPerformance = () => {
    if (typeof window === 'undefined') return
    
    // FPS 监控
    let lastTime = performance.now()
    let frames = 0
    
    const measureFPS = () => {
      frames++
      const currentTime = performance.now()
      
      if (currentTime >= lastTime + 1000) {
        const fps = Math.round((frames * 1000) / (currentTime - lastTime))
        performanceMetrics.value.fps = fps
        frames = 0
        lastTime = currentTime
        
        // 性能建议
        const recommendations: string[] = []
        
        if (fps < 30) {
          recommendations.push('FPS较低，建议简化动画或减少复杂效果')
          performanceMetrics.value.isLowEndDevice = true
        }
        
        if (deviceInfo.value.connection?.effectiveType === '2g') {
          recommendations.push('网络较慢，建议启用数据节省模式')
        }
        
        if (deviceInfo.value.pixelRatio > 2) {
          recommendations.push('高分辨率屏幕，注意图片资源优化')
        }
        
        performanceMetrics.value.recommendations = recommendations
      }
      
      requestAnimationFrame(measureFPS)
    }
    
    measureFPS()
    
    // 内存使用监控（如果支持）
    if ('memory' in performance) {
      const memory = (performance as any).memory
      performanceMetrics.value.memoryUsage = memory.usedJSHeapSize / memory.totalJSHeapSize
    }
  }
  
  // 更新布局状态
  const updateLayoutState = () => {
    const { deviceType, windowWidth, currentBreakpoint } = baseBreakpoints
    
    // 自动布局切换
    if (finalConfig.value.autoLayoutSwitching) {
      let newLayout: LayoutType = 'desktop-sidebar'
      
      if (deviceType.value === 'mobile') {
        newLayout = 'mobile'
      } else if (deviceType.value === 'tablet') {
        newLayout = 'tablet'
      } else if (windowWidth.value >= 1400) {
        newLayout = 'desktop-full'
      }
      
      layoutState.value.currentLayout = newLayout
    }
    
    // 自适应间距
    if (finalConfig.value.adaptiveSpacing) {
      const spacingScale = windowWidth.value < 640 ? 0.75 : 
                          windowWidth.value < 1024 ? 0.875 : 1
      layoutState.value.spacingScale = spacingScale
    }
    
    // 响应式排版
    if (finalConfig.value.responsiveTypography) {
      const typographyScale = windowWidth.value < 640 ? 0.875 :
                             windowWidth.value < 768 ? 0.9375 : 1
      layoutState.value.typographyScale = typographyScale
    }
    
    // 内容最大宽度
    const maxWidth = baseBreakpoints.responsiveValue(
      {
        xs: '100%',
        sm: '640px',
        md: '768px',
        lg: '1024px',
        xl: '1280px',
        '2xl': '1536px',
        '3xl': '1920px'
      },
      '1200px'
    )
    layoutState.value.contentMaxWidth = maxWidth
  }
  
  // 容器查询
  const setupContainerQueries = () => {
    if (typeof window === 'undefined' || !('ResizeObserver' in window)) return
    
    const observer = new ResizeObserver((entries) => {
      entries.forEach(entry => {
        const target = entry.target as HTMLElement
        const id = target.dataset.containerId || 'default'
        
        if (finalConfig.value.containerQueries?.[id]) {
          const query = finalConfig.value.containerQueries[id]
          const { width, height } = entry.contentRect
          
          let matches = true
          
          if (query.minWidth && width < query.minWidth) matches = false
          if (query.maxWidth && width > query.maxWidth) matches = false
          if (query.minHeight && height < query.minHeight) matches = false
          if (query.maxHeight && height > query.maxHeight) matches = false
          if (query.orientation) {
            const orientation = height > width ? 'portrait' : 'landscape'
            if (orientation !== query.orientation) matches = false
          }
          if (query.aspectRatio) {
            const aspectRatio = (width / height).toFixed(2)
            if (aspectRatio !== query.aspectRatio) matches = false
          }
          
          containerStates.value[id] = matches
        }
      })
    })
    
    // 观察所有带有容器查询的元素
    document.querySelectorAll('[data-container-id]').forEach(el => {
      observer.observe(el)
    })
    
    return observer
  }
  
  // 响应式值计算（增强版）
  const enhancedResponsiveValue = <T>(
    values: Partial<Record<Breakpoint | DeviceType | LayoutType | string, T>>,
    defaultValue: T,
    context?: string
  ): T => {
    // 根据上下文确定查找顺序
    let keys: string[] = []
    
    if (context === 'layout') {
      keys = [
        layoutState.value.currentLayout,
        deviceInfo.value.type,
        baseBreakpoints.currentBreakpoint.value
      ]
    } else if (context === 'device') {
      keys = [
        deviceInfo.value.type,
        baseBreakpoints.currentBreakpoint.value
      ]
    } else {
      keys = [
        baseBreakpoints.currentBreakpoint.value,
        deviceInfo.value.type
      ]
    }
    
    // 按优先级查找值
    for (const key of keys) {
      if (values[key] !== undefined) {
        return values[key]!
      }
    }
    
    return defaultValue
  }
  
  // 自适应间距
  const adaptiveSpacing = (baseSpacing: number): number => {
    return baseSpacing * layoutState.value.spacingScale
  }
  
  // 自适应字体大小
  const adaptiveFontSize = (baseSize: number): number => {
    return baseSize * layoutState.value.typographyScale
  }
  
  // 获取优化建议
  const getOptimizationSuggestions = (): string[] => {
    const suggestions: string[] = []
    
    // 性能建议
    if (performanceMetrics.value.fps < 30) {
      suggestions.push('考虑简化动画效果以提升性能')
    }
    
    // 网络建议
    if (deviceInfo.value.connection?.effectiveType === '2g') {
      suggestions.push('网络较慢，建议压缩图片资源')
    }
    
    // 设备建议
    if (deviceInfo.value.isMobile) {
      suggestions.push('移动设备访问，确保触控友好')
    }
    
    // 用户偏好建议
    if (userPreferences.value.reduceMotion) {
      suggestions.push('用户偏好减少动画，建议提供静态替代方案')
    }
    
    if (userPreferences.value.preferReducedData) {
      suggestions.push('用户偏好节省数据，建议减少资源加载')
    }
    
    return suggestions
  }
  
  // 监听变化
  watch([
    () => baseBreakpoints.windowWidth.value,
    () => baseBreakpoints.windowHeight.value,
    () => deviceInfo.value.type
  ], () => {
    updateLayoutState()
  }, { immediate: true })
  
  // 组件挂载时初始化
  onMounted(() => {
    if (typeof window === 'undefined') return
    
    // 检测设备信息
    if (finalConfig.value.detectDeviceType) {
      detectDeviceInfo()
    }
    
    // 检测网络连接
    if (finalConfig.value.detectNetwork) {
      detectNetworkConnection()
    }
    
    // 检测用户偏好
    detectUserPreferences()
    
    // 监控性能
    if (finalConfig.value.enablePerformanceMonitoring) {
      monitorPerformance()
    }
    
    // 设置容器查询
    const observer = setupContainerQueries()
    
    // 监听网络变化
    if (finalConfig.value.detectNetwork && 'connection' in navigator) {
      (navigator as any).connection.addEventListener('change', detectNetworkConnection)
    }
    
    // 监听用户偏好变化
    const mediaQueries = [
      window.matchMedia('(prefers-reduced-motion: reduce)'),
      window.matchMedia('(prefers-contrast: high)'),
      window.matchMedia('(prefers-reduced-data: reduce)'),
      window.matchMedia('(prefers-color-scheme: dark)')
    ]
    
    mediaQueries.forEach(mq => {
      mq.addEventListener('change', detectUserPreferences)
    })
    
    // 清理函数
    onUnmounted(() => {
      if (observer) observer.disconnect()
      
      if (finalConfig.value.detectNetwork && 'connection' in navigator) {
        (navigator as any).connection.removeEventListener('change', detectNetworkConnection)
      }
      
      mediaQueries.forEach(mq => {
        mq.removeEventListener('change', detectUserPreferences)
      })
    })
  })
  
  return {
    // 继承基础功能
    ...baseBreakpoints,
    
    // 设备信息
    deviceInfo,
    
    // 用户偏好
    userPreferences,
    
    // 布局状态
    layoutState,
    
    // 性能指标
    performanceMetrics,
    
    // 容器查询状态
    containerStates,
    
    // 增强方法
    enhancedResponsiveValue,
    adaptiveSpacing,
    adaptiveFontSize,
    getOptimizationSuggestions,
    
    // 配置
    config: finalConfig
  }
}