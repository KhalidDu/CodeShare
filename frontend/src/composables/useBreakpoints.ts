import { ref, computed, onMounted, onUnmounted } from 'vue'

// 断点定义
export type Breakpoint = 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl'
export type DeviceType = 'mobile' | 'tablet' | 'desktop' | 'large-desktop'
export type Orientation = 'portrait' | 'landscape'
export type LayoutType = 'mobile' | 'tablet' | 'desktop-sidebar' | 'desktop-full'

// 断点配置
export const breakpoints: Record<Breakpoint, number> = {
  xs: 0,
  sm: 640,
  md: 768,
  lg: 1024,
  xl: 1280,
  '2xl': 1536,
  '3xl': 1920
}

// 设备类型映射
export const deviceTypeMap: Record<DeviceType, Breakpoint[]> = {
  mobile: ['xs', 'sm'],
  tablet: ['md'],
  desktop: ['lg', 'xl'],
  'large-desktop': ['2xl', '3xl']
}

// 基础断点系统
export function useBreakpoints() {
  const windowWidth = ref(typeof window !== 'undefined' ? window.innerWidth : 1024)
  const windowHeight = ref(typeof window !== 'undefined' ? window.innerHeight : 768)
  
  // 计算当前断点
  const currentBreakpoint = computed<Breakpoint>(() => {
    const width = windowWidth.value
    const breakpointKeys = Object.keys(breakpoints) as Breakpoint[]
    
    for (let i = breakpointKeys.length - 1; i >= 0; i--) {
      const key = breakpointKeys[i]
      if (width >= breakpoints[key]) {
        return key
      }
    }
    
    return 'xs'
  })
  
  // 计算设备类型
  const deviceType = computed<DeviceType>(() => {
    const breakpoint = currentBreakpoint.value
    
    for (const [type, breakpointList] of Object.entries(deviceTypeMap)) {
      if (breakpointList.includes(breakpoint)) {
        return type as DeviceType
      }
    }
    
    return 'mobile'
  })
  
  // 计算屏幕方向
  const orientation = computed<Orientation>(() => {
    return windowHeight.value > windowWidth.value ? 'portrait' : 'landscape'
  })
  
  // 计算布局类型
  const layoutType = computed<LayoutType>(() => {
    if (deviceType.value === 'mobile') return 'mobile'
    if (deviceType.value === 'tablet') return 'tablet'
    if (windowWidth.value >= 1200) return 'desktop-full'
    return 'desktop-sidebar'
  })
  
  // 断点判断函数
  const isXs = computed(() => currentBreakpoint.value === 'xs')
  const isSm = computed(() => currentBreakpoint.value === 'sm')
  const isMd = computed(() => currentBreakpoint.value === 'md')
  const isLg = computed(() => currentBreakpoint.value === 'lg')
  const isXl = computed(() => currentBreakpoint.value === 'xl')
  const is2xl = computed(() => currentBreakpoint.value === '2xl')
  const is3xl = computed(() => currentBreakpoint.value === '3xl')
  
  // 设备类型判断函数
  const isMobile = computed(() => deviceType.value === 'mobile')
  const isTablet = computed(() => deviceType.value === 'tablet')
  const isDesktop = computed(() => deviceType.value === 'desktop' || deviceType.value === 'large-desktop')
  const isLargeDesktop = computed(() => deviceType.value === 'large-desktop')
  
  // 范围判断函数
  const greaterOrEqual = (breakpoint: Breakpoint) => {
    return windowWidth.value >= breakpoints[breakpoint]
  }
  
  const greaterThan = (breakpoint: Breakpoint) => {
    return windowWidth.value > breakpoints[breakpoint]
  }
  
  const lessOrEqual = (breakpoint: Breakpoint) => {
    return windowWidth.value <= breakpoints[breakpoint]
  }
  
  const lessThan = (breakpoint: Breakpoint) => {
    return windowWidth.value < breakpoints[breakpoint]
  }
  
  const between = (min: Breakpoint, max: Breakpoint) => {
    return windowWidth.value >= breakpoints[min] && windowWidth.value <= breakpoints[max]
  }
  
  // 响应式值计算
  const responsiveValue = <T>(values: Partial<Record<Breakpoint, T>>, defaultValue: T): T => {
    const breakpoint = currentBreakpoint.value
    const breakpointKeys = Object.keys(breakpoints) as Breakpoint[]
    
    // 从当前断点开始向上查找
    for (let i = breakpointKeys.indexOf(breakpoint); i >= 0; i--) {
      const key = breakpointKeys[i]
      if (values[key] !== undefined) {
        return values[key]!
      }
    }
    
    return defaultValue
  }
  
  // 监听窗口大小变化
  const updateWindowSize = () => {
    if (typeof window !== 'undefined') {
      windowWidth.value = window.innerWidth
      windowHeight.value = window.innerHeight
    }
  }
  
  // 组件挂载时添加监听器
  onMounted(() => {
    if (typeof window !== 'undefined') {
      window.addEventListener('resize', updateWindowSize)
      // 初始化尺寸
      updateWindowSize()
    }
  })
  
  // 组件卸载时移除监听器
  onUnmounted(() => {
    if (typeof window !== 'undefined') {
      window.removeEventListener('resize', updateWindowSize)
    }
  })
  
  return {
    // 状态
    windowWidth,
    windowHeight,
    currentBreakpoint,
    deviceType,
    orientation,
    layoutType,
    
    // 断点判断
    isXs,
    isSm,
    isMd,
    isLg,
    isXl,
    is2xl,
    is3xl,
    
    // 设备类型判断
    isMobile,
    isTablet,
    isDesktop,
    isLargeDesktop,
    
    // 范围判断
    greaterOrEqual,
    greaterThan,
    lessOrEqual,
    lessThan,
    between,
    
    // 工具函数
    responsiveValue,
    
    // 配置
    breakpoints,
    deviceTypeMap
  }
}