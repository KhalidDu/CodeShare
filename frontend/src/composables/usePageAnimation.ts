import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

// 动画类型定义
export type PageAnimationType = 
  | 'fade' 
  | 'slide-up' 
  | 'slide-down' 
  | 'slide-left' 
  | 'slide-right'
  | 'scale' 
  | 'bounce' 
  | 'elastic' 
  | 'flip' 
  | 'rotate' 
  | 'zoom'
  | 'none'

// 动画配置接口
export interface PageAnimationConfig {
  type: PageAnimationType
  duration: number
  delay: number
  easing: string
  stagger: number
  mode: 'in-out' | 'out-in' | 'default'
  disabled: boolean
}

// 默认动画配置
const defaultAnimationConfig: PageAnimationConfig = {
  type: 'fade',
  duration: 300,
  delay: 0,
  easing: 'cubic-bezier(0.4, 0, 0.2, 1)',
  stagger: 50,
  mode: 'out-in',
  disabled: false
}

// 页面动画系统
export function usePageAnimation(config: Partial<PageAnimationConfig> = {}) {
  const route = useRoute()
  const router = useRouter()
  
  // 动画配置
  const animationConfig = ref<PageAnimationConfig>({
    ...defaultAnimationConfig,
    ...config
  })
  
  // 动画状态
  const isAnimating = ref(false)
  const animationDirection = ref<'forward' | 'backward'>('forward')
  const previousRoute = ref<string>('')
  const currentAnimation = ref<PageAnimationType>('fade')
  
  // 性能监控
  const animationPerformance = ref<{
    lastAnimationTime: number
    averageAnimationTime: number
    animationCount: number
    droppedFrames: number
  }>({
    lastAnimationTime: 0,
    averageAnimationTime: 0,
    animationCount: 0,
    droppedFrames: 0
  })
  
  // 检测是否支持动画
  const supportsAnimation = computed(() => {
    if (typeof window === 'undefined') return false
    
    // 检测用户是否偏好减少动画
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches
    if (prefersReducedMotion) return false
    
    // 检测硬件支持
    const isLowEndDevice = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)
    if (isLowEndDevice) return false
    
    return true
  })
  
  // 获取动画类名
  const getAnimationClass = (type: PageAnimationType, direction: 'enter' | 'leave'): string => {
    if (animationConfig.value.disabled || !supportsAnimation.value) return ''
    
    const classes: Record<PageAnimationType, { enter: string; leave: string }> = {
      'fade': {
        enter: 'animate-fade-in',
        leave: 'animate-fade-out'
      },
      'slide-up': {
        enter: 'animate-slide-up-in',
        leave: 'animate-slide-up-out'
      },
      'slide-down': {
        enter: 'animate-slide-down-in',
        leave: 'animate-slide-down-out'
      },
      'slide-left': {
        enter: 'animate-slide-left-in',
        leave: 'animate-slide-left-out'
      },
      'slide-right': {
        enter: 'animate-slide-right-in',
        leave: 'animate-slide-right-out'
      },
      'scale': {
        enter: 'animate-scale-in',
        leave: 'animate-scale-out'
      },
      'bounce': {
        enter: 'animate-bounce-in',
        leave: 'animate-bounce-out'
      },
      'elastic': {
        enter: 'animate-elastic-in',
        leave: 'animate-elastic-out'
      },
      'flip': {
        enter: 'animate-flip-in',
        leave: 'animate-flip-out'
      },
      'rotate': {
        enter: 'animate-rotate-in',
        leave: 'animate-rotate-out'
      },
      'zoom': {
        enter: 'animate-zoom-in',
        leave: 'animate-zoom-out'
      },
      'none': {
        enter: '',
        leave: ''
      }
    }
    
    return classes[type]?.[direction] || ''
  }
  
  // 获取动画样式
  const getAnimationStyle = (type: PageAnimationType, direction: 'enter' | 'leave'): Record<string, string> => {
    if (animationConfig.value.disabled || !supportsAnimation.value) return {}
    
    const duration = animationConfig.value.duration
    const delay = animationConfig.value.delay
    const easing = animationConfig.value.easing
    
    return {
      animationDuration: `${duration}ms`,
      animationDelay: `${delay}ms`,
      animationTimingFunction: easing,
      animationFillMode: 'both'
    }
  }
  
  // 获取页面转换动画
  const getPageTransitionAnimation = (fromRoute: string, toRoute: string): PageAnimationType => {
    // 如果配置了特定动画，优先使用配置
    if (animationConfig.value.type !== 'fade') {
      return animationConfig.value.type
    }
    
    // 根据路由层级关系自动选择动画
    const fromDepth = fromRoute.split('/').length
    const toDepth = toRoute.split('/').length
    
    if (toDepth > fromDepth) {
      return 'slide-left' // 进入更深页面
    } else if (toDepth < fromDepth) {
      return 'slide-right' // 返回更浅页面
    } else {
      return 'fade' // 同级页面
    }
  }
  
  // 开始动画
  const startAnimation = (type: PageAnimationType) => {
    if (!supportsAnimation.value || animationConfig.value.disabled) return
    
    isAnimating.value = true
    currentAnimation.value = type
    
    // 性能监控开始
    const startTime = performance.now()
    
    // 动画完成后重置状态
    setTimeout(() => {
      isAnimating.value = false
      
      // 更新性能统计
      const endTime = performance.now()
      const animationTime = endTime - startTime
      
      animationPerformance.value.lastAnimationTime = animationTime
      animationPerformance.value.animationCount++
      
      // 计算平均动画时间
      const totalTime = animationPerformance.value.averageAnimationTime * (animationPerformance.value.animationCount - 1)
      animationPerformance.value.averageAnimationTime = (totalTime + animationTime) / animationPerformance.value.animationCount
      
      // 检测丢帧
      if (animationTime > animationConfig.value.duration * 1.5) {
        animationPerformance.value.droppedFrames++
      }
    }, animationConfig.value.duration + animationConfig.value.delay)
  }
  
  // 更新动画配置
  const updateAnimationConfig = (newConfig: Partial<PageAnimationConfig>) => {
    animationConfig.value = {
      ...animationConfig.value,
      ...newConfig
    }
    
    // 保存到本地存储
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('page-animation-config', JSON.stringify(animationConfig.value))
    }
  }
  
  // 重置动画配置
  const resetAnimationConfig = () => {
    animationConfig.value = { ...defaultAnimationConfig }
    
    // 从本地存储删除
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem('page-animation-config')
    }
  }
  
  // 禁用动画
  const disableAnimations = () => {
    updateAnimationConfig({ disabled: true })
  }
  
  // 启用动画
  const enableAnimations = () => {
    updateAnimationConfig({ disabled: false })
  }
  
  // 预设动画配置
  const presetConfigs = {
    fast: {
      type: 'fade' as PageAnimationType,
      duration: 150,
      delay: 0,
      easing: 'cubic-bezier(0.4, 0, 0.2, 1)',
      stagger: 30
    },
    smooth: {
      type: 'slide-up' as PageAnimationType,
      duration: 400,
      delay: 50,
      easing: 'cubic-bezier(0.4, 0, 0.2, 1)',
      stagger: 100
    },
    playful: {
      type: 'bounce' as PageAnimationType,
      duration: 600,
      delay: 100,
      easing: 'cubic-bezier(0.68, -0.55, 0.265, 1.55)',
      stagger: 150
    },
    minimal: {
      type: 'none' as PageAnimationType,
      duration: 0,
      delay: 0,
      easing: 'linear',
      stagger: 0
    }
  }
  
  // 应用预设配置
  const applyPreset = (preset: keyof typeof presetConfigs) => {
    updateAnimationConfig(presetConfigs[preset])
  }
  
  // 获取动画性能报告
  const getPerformanceReport = () => {
    const { averageAnimationTime, animationCount, droppedFrames } = animationPerformance.value
    
    return {
      averageAnimationTime: Math.round(averageAnimationTime),
      animationCount,
      droppedFrames,
      performanceScore: animationCount > 0 ? Math.max(0, 100 - (droppedFrames / animationCount) * 100) : 100,
      recommendation: getPerformanceRecommendation()
    }
  }
  
  // 获取性能建议
  const getPerformanceRecommendation = (): string => {
    const { averageAnimationTime, droppedFrames, animationCount } = animationPerformance.value
    
    if (animationCount === 0) return '暂无数据'
    
    if (droppedFrames / animationCount > 0.3) {
      return '检测到较多丢帧，建议简化动画或降低动画复杂度'
    }
    
    if (averageAnimationTime > 500) {
      return '动画时间较长，建议减少动画持续时间'
    }
    
    if (averageAnimationTime < 200) {
      return '动画性能良好，可以适当增加动画复杂度'
    }
    
    return '动画性能正常'
  }
  
  // 监听路由变化
  watch(() => route.path, (newPath, oldPath) => {
    if (oldPath && newPath !== oldPath) {
      previousRoute.value = oldPath
      
      // 确定动画方向
      const routeHistory = router.options.history
      if (routeHistory && 'state' in routeHistory && routeHistory.state) {
        animationDirection.value = routeHistory.state.back ? 'backward' : 'forward'
      } else {
        animationDirection.value = 'forward'
      }
      
      // 获取适当的动画类型
      const animationType = getPageTransitionAnimation(oldPath, newPath)
      startAnimation(animationType)
    }
  })
  
  // 监听系统动画偏好变化
  const handleReducedMotionChange = (event: MediaQueryListEvent) => {
    if (event.matches) {
      disableAnimations()
    }
  }
  
  // 组件挂载时初始化
  onMounted(() => {
    // 从本地存储加载配置
    if (typeof localStorage !== 'undefined') {
      try {
        const savedConfig = localStorage.getItem('page-animation-config')
        if (savedConfig) {
          animationConfig.value = {
            ...defaultAnimationConfig,
            ...JSON.parse(savedConfig)
          }
        }
      } catch (error) {
        console.warn('Failed to load animation config:', error)
      }
    }
    
    // 监听系统动画偏好
    if (typeof window !== 'undefined') {
      const mediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)')
      mediaQuery.addEventListener('change', handleReducedMotionChange)
      
      // 如果用户偏好减少动画，自动禁用
      if (mediaQuery.matches) {
        disableAnimations()
      }
    }
  })
  
  // 组件卸载时清理
  onUnmounted(() => {
    if (typeof window !== 'undefined') {
      const mediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)')
      mediaQuery.removeEventListener('change', handleReducedMotionChange)
    }
  })
  
  return {
    // 状态
    animationConfig,
    isAnimating,
    animationDirection,
    currentAnimation,
    animationPerformance,
    supportsAnimation,
    
    // 方法
    getAnimationClass,
    getAnimationStyle,
    getPageTransitionAnimation,
    startAnimation,
    updateAnimationConfig,
    resetAnimationConfig,
    disableAnimations,
    enableAnimations,
    applyPreset,
    getPerformanceReport,
    getPerformanceRecommendation,
    
    // 预设
    presetConfigs
  }
}