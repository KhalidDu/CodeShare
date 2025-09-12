// 动画预设库
// 提供常用的CSS动画预设和动画组合

// 动画类型定义
export type AnimationDuration = 'fast' | 'normal' | 'slow' | number
export type AnimationEasing = 
  | 'linear'
  | 'ease'
  | 'ease-in'
  | 'ease-out'
  | 'ease-in-out'
  | 'bounce'
  | 'elastic'
  | 'back'
  | 'custom'

// 动画预设接口
export interface AnimationPreset {
  name: string
  keyframes: string
  duration: AnimationDuration
  easing: AnimationEasing
  delay?: number
  fillMode?: 'none' | 'forwards' | 'backwards' | 'both'
  iterationCount?: 'infinite' | number
  direction?: 'normal' | 'reverse' | 'alternate' | 'alternate-reverse'
}

// 动画组合接口
export interface AnimationCombo {
  name: string
  animations: Array<{
    preset: string
    delay?: number
    target?: string
  }>
  stagger?: number
}

// 基础动画预设
export const basicAnimations: Record<string, AnimationPreset> = {
  // 淡入淡出
  'fade-in': {
    name: '淡入',
    keyframes: `
      from { opacity: 0; }
      to { opacity: 1; }
    `,
    duration: 'normal',
    easing: 'ease-in-out',
    fillMode: 'both'
  },
  
  'fade-out': {
    name: '淡出',
    keyframes: `
      from { opacity: 1; }
      to { opacity: 0; }
    `,
    duration: 'normal',
    easing: 'ease-in-out',
    fillMode: 'both'
  },
  
  // 滑动动画
  'slide-up-in': {
    name: '向上滑入',
    keyframes: `
      from { 
        opacity: 0;
        transform: translateY(20px);
      }
      to { 
        opacity: 1;
        transform: translateY(0);
      }
    `,
    duration: 'normal',
    easing: 'ease-out',
    fillMode: 'both'
  },
  
  'slide-up-out': {
    name: '向上滑出',
    keyframes: `
      from { 
        opacity: 1;
        transform: translateY(0);
      }
      to { 
        opacity: 0;
        transform: translateY(-20px);
      }
    `,
    duration: 'normal',
    easing: 'ease-in',
    fillMode: 'both'
  },
  
  'slide-down-in': {
    name: '向下滑入',
    keyframes: `
      from { 
        opacity: 0;
        transform: translateY(-20px);
      }
      to { 
        opacity: 1;
        transform: translateY(0);
      }
    `,
    duration: 'normal',
    easing: 'ease-out',
    fillMode: 'both'
  },
  
  'slide-down-out': {
    name: '向下滑出',
    keyframes: `
      from { 
        opacity: 1;
        transform: translateY(0);
      }
      to { 
        opacity: 0;
        transform: translateY(20px);
      }
    `,
    duration: 'normal',
    easing: 'ease-in',
    fillMode: 'both'
  },
  
  'slide-left-in': {
    name: '向左滑入',
    keyframes: `
      from { 
        opacity: 0;
        transform: translateX(20px);
      }
      to { 
        opacity: 1;
        transform: translateX(0);
      }
    `,
    duration: 'normal',
    easing: 'ease-out',
    fillMode: 'both'
  },
  
  'slide-left-out': {
    name: '向左滑出',
    keyframes: `
      from { 
        opacity: 1;
        transform: translateX(0);
      }
      to { 
        opacity: 0;
        transform: translateX(-20px);
      }
    `,
    duration: 'normal',
    easing: 'ease-in',
    fillMode: 'both'
  },
  
  'slide-right-in': {
    name: '向右滑入',
    keyframes: `
      from { 
        opacity: 0;
        transform: translateX(-20px);
      }
      to { 
        opacity: 1;
        transform: translateX(0);
      }
    `,
    duration: 'normal',
    easing: 'ease-out',
    fillMode: 'both'
  },
  
  'slide-right-out': {
    name: '向右滑出',
    keyframes: `
      from { 
        opacity: 1;
        transform: translateX(0);
      }
      to { 
        opacity: 0;
        transform: translateX(20px);
      }
    `,
    duration: 'normal',
    easing: 'ease-in',
    fillMode: 'both'
  },
  
  // 缩放动画
  'scale-in': {
    name: '缩放进入',
    keyframes: `
      from { 
        opacity: 0;
        transform: scale(0.8);
      }
      to { 
        opacity: 1;
        transform: scale(1);
      }
    `,
    duration: 'normal',
    easing: 'ease-out',
    fillMode: 'both'
  },
  
  'scale-out': {
    name: '缩放退出',
    keyframes: `
      from { 
        opacity: 1;
        transform: scale(1);
      }
      to { 
        opacity: 0;
        transform: scale(0.8);
      }
    `,
    duration: 'normal',
    easing: 'ease-in',
    fillMode: 'both'
  },
  
  'zoom-in': {
    name: '放大进入',
    keyframes: `
      from { 
        opacity: 0;
        transform: scale(0.3);
      }
      to { 
        opacity: 1;
        transform: scale(1);
      }
    `,
    duration: 'normal',
    easing: 'bounce',
    fillMode: 'both'
  },
  
  'zoom-out': {
    name: '缩小退出',
    keyframes: `
      from { 
        opacity: 1;
        transform: scale(1);
      }
      to { 
        opacity: 0;
        transform: scale(0.3);
      }
    `,
    duration: 'normal',
    easing: 'ease-in',
    fillMode: 'both'
  },
  
  // 旋转动画
  'rotate-in': {
    name: '旋转进入',
    keyframes: `
      from { 
        opacity: 0;
        transform: rotate(-180deg) scale(0.8);
      }
      to { 
        opacity: 1;
        transform: rotate(0deg) scale(1);
      }
    `,
    duration: 'normal',
    easing: 'ease-out',
    fillMode: 'both'
  },
  
  'rotate-out': {
    name: '旋转退出',
    keyframes: `
      from { 
        opacity: 1;
        transform: rotate(0deg) scale(1);
      }
      to { 
        opacity: 0;
        transform: rotate(180deg) scale(0.8);
      }
    `,
    duration: 'normal',
    easing: 'ease-in',
    fillMode: 'both'
  },
  
  'flip-in': {
    name: '翻转进入',
    keyframes: `
      from { 
        opacity: 0;
        transform: perspective(400px) rotateY(90deg);
      }
      to { 
        opacity: 1;
        transform: perspective(400px) rotateY(0deg);
      }
    `,
    duration: 'normal',
    easing: 'ease-out',
    fillMode: 'both'
  },
  
  'flip-out': {
    name: '翻转退出',
    keyframes: `
      from { 
        opacity: 1;
        transform: perspective(400px) rotateY(0deg);
      }
      to { 
        opacity: 0;
        transform: perspective(400px) rotateY(90deg);
      }
    `,
    duration: 'normal',
    easing: 'ease-in',
    fillMode: 'both'
  }
}

// 高级动画预设
export const advancedAnimations: Record<string, AnimationPreset> = {
  // 弹性动画
  'bounce-in': {
    name: '弹跳进入',
    keyframes: `
      0% { 
        opacity: 0;
        transform: scale(0.3);
      }
      50% { 
        opacity: 1;
        transform: scale(1.05);
      }
      70% { 
        transform: scale(0.9);
      }
      100% { 
        transform: scale(1);
      }
    `,
    duration: 'normal',
    easing: 'bounce',
    fillMode: 'both'
  },
  
  'bounce-out': {
    name: '弹跳退出',
    keyframes: `
      0% { 
        transform: scale(1);
      }
      25% { 
        transform: scale(0.95);
      }
      50% { 
        opacity: 1;
        transform: scale(1.1);
      }
      100% { 
        opacity: 0;
        transform: scale(0.3);
      }
    `,
    duration: 'normal',
    easing: 'bounce',
    fillMode: 'both'
  },
  
  // 弹性动画
  'elastic-in': {
    name: '弹性进入',
    keyframes: `
      from { 
        opacity: 0;
        transform: scale(0.3) translateX(-100%);
      }
      50% { 
        opacity: 1;
        transform: scale(1.1) translateX(10%);
      }
      100% { 
        transform: scale(1) translateX(0);
      }
    `,
    duration: 'slow',
    easing: 'elastic',
    fillMode: 'both'
  },
  
  'elastic-out': {
    name: '弹性退出',
    keyframes: `
      from { 
        opacity: 1;
        transform: scale(1) translateX(0);
      }
      30% { 
        transform: scale(0.8) translateX(-10%);
      }
      50% { 
        opacity: 1;
        transform: scale(1.1) translateX(10%);
      }
      100% { 
        opacity: 0;
        transform: scale(0.3) translateX(100%);
      }
    `,
    duration: 'slow',
    easing: 'elastic',
    fillMode: 'both'
  },
  
  // 反弹动画
  'back-in': {
    name: '反弹进入',
    keyframes: `
      from { 
        opacity: 0;
        transform: translateX(-100%) skewX(-30deg);
      }
      60% { 
        opacity: 1;
        transform: translateX(20%) skewX(30deg);
      }
      80% { 
        transform: translateX(-10%) skewX(-15deg);
      }
      100% { 
        transform: translateX(0) skewX(0deg);
      }
    `,
    duration: 'normal',
    easing: 'back',
    fillMode: 'both'
  },
  
  'back-out': {
    name: '反弹退出',
    keyframes: `
      from { 
        opacity: 1;
        transform: translateX(0) skewX(0deg);
      }
      20% { 
        transform: translateX(20%) skewX(30deg);
      }
      40% { 
        opacity: 1;
        transform: translateX(-100%) skewX(-30deg);
      }
      100% { 
        opacity: 0;
        transform: translateX(-100%) skewX(-30deg);
      }
    `,
    duration: 'normal',
    easing: 'back',
    fillMode: 'both'
  },
  
  // 脉冲动画
  'pulse': {
    name: '脉冲',
    keyframes: `
      0% { 
        transform: scale(1);
      }
      50% { 
        transform: scale(1.05);
      }
      100% { 
        transform: scale(1);
      }
    `,
    duration: 'slow',
    easing: 'ease-in-out',
    iterationCount: 'infinite'
  },
  
  'pulse-scale': {
    name: '脉冲缩放',
    keyframes: `
      0% { 
        transform: scale(1);
        opacity: 1;
      }
      50% { 
        transform: scale(1.1);
        opacity: 0.7;
      }
      100% { 
        transform: scale(1);
        opacity: 1;
      }
    `,
    duration: 'slow',
    easing: 'ease-in-out',
    iterationCount: 'infinite'
  },
  
  // 摇摆动画
  'shake': {
    name: '摇摆',
    keyframes: `
      0%, 100% { 
        transform: translateX(0);
      }
      10%, 30%, 50%, 70%, 90% { 
        transform: translateX(-5px);
      }
      20%, 40%, 60%, 80% { 
        transform: translateX(5px);
      }
    `,
    duration: 'normal',
    easing: 'ease-in-out'
  },
  
  'swing': {
    name: '摆动',
    keyframes: `
      0%, 100% { 
        transform: rotate(0deg);
      }
      15% { 
        transform: rotate(5deg);
      }
      30% { 
        transform: rotate(-5deg);
      }
      45% { 
        transform: rotate(4deg);
      }
      60% { 
        transform: rotate(-3deg);
      }
      75% { 
        transform: rotate(2deg);
      }
      90% { 
        transform: rotate(-1deg);
      }
    `,
    duration: 'normal',
    easing: 'ease-in-out'
  },
  
  // 闪烁动画
  'flash': {
    name: '闪烁',
    keyframes: `
      0%, 50%, 100% { 
        opacity: 1;
      }
      25%, 75% { 
        opacity: 0;
      }
    `,
    duration: 'normal',
    easing: 'ease-in-out'
  },
  
  'glow': {
    name: '发光',
    keyframes: `
      0%, 100% { 
        box-shadow: 0 0 5px rgba(59, 130, 246, 0.5);
      }
      50% { 
        box-shadow: 0 0 20px rgba(59, 130, 246, 0.8), 0 0 30px rgba(59, 130, 246, 0.6);
      }
    `,
    duration: 'slow',
    easing: 'ease-in-out',
    iterationCount: 'infinite'
  },
  
  // 浮动动画
  'float': {
    name: '浮动',
    keyframes: `
      0%, 100% { 
        transform: translateY(0px);
      }
      50% { 
        transform: translateY(-10px);
      }
    `,
    duration: 'slow',
    easing: 'ease-in-out',
    iterationCount: 'infinite'
  },
  
  'float-rotate': {
    name: '浮动旋转',
    keyframes: `
      0%, 100% { 
        transform: translateY(0px) rotate(0deg);
      }
      25% { 
        transform: translateY(-5px) rotate(90deg);
      }
      50% { 
        transform: translateY(-10px) rotate(180deg);
      }
      75% { 
        transform: translateY(-5px) rotate(270deg);
      }
    `,
    duration: 'slow',
    easing: 'ease-in-out',
    iterationCount: 'infinite'
  }
}

// 动画组合
export const animationCombos: Record<string, AnimationCombo> = {
  // 页面加载动画
  'page-load': {
    name: '页面加载',
    animations: [
      { preset: 'fade-in', delay: 0 },
      { preset: 'slide-up-in', delay: 100 }
    ],
    stagger: 100
  },
  
  // 列表显示动画
  'list-appear': {
    name: '列表出现',
    animations: [
      { preset: 'slide-left-in', delay: 0 }
    ],
    stagger: 50
  },
  
  // 卡片悬停动画
  'card-hover': {
    name: '卡片悬停',
    animations: [
      { preset: 'scale-in', delay: 0 },
      { preset: 'glow', delay: 0 }
    ]
  },
  
  // 按钮点击动画
  'button-click': {
    name: '按钮点击',
    animations: [
      { preset: 'scale-in', delay: 0 },
      { preset: 'pulse', delay: 0 }
    ]
  },
  
  // 模态框显示动画
  'modal-appear': {
    name: '模态框出现',
    animations: [
      { preset: 'fade-in', delay: 0, target: '.modal-backdrop' },
      { preset: 'scale-in', delay: 100, target: '.modal-content' }
    ]
  },
  
  // 错误提示动画
  'error-shake': {
    name: '错误摇摆',
    animations: [
      { preset: 'shake', delay: 0 },
      { preset: 'flash', delay: 200 }
    ]
  },
  
  // 成功提示动画
  'success-bounce': {
    name: '成功弹跳',
    animations: [
      { preset: 'bounce-in', delay: 0 },
      { preset: 'pulse', delay: 300 }
    ]
  },
  
  // 加载动画
  'loading-spin': {
    name: '加载旋转',
    animations: [
      { preset: 'rotate-in', delay: 0, target: '.loading-spinner' }
    ]
  },
  
  // 导航切换动画
  'nav-switch': {
    name: '导航切换',
    animations: [
      { preset: 'slide-right-out', delay: 0, target: '.nav-item-old' },
      { preset: 'slide-left-in', delay: 150, target: '.nav-item-new' }
    ]
  }
}

// 缓动函数预设
export const easingPresets: Record<string, string> = {
  'linear': 'linear',
  'ease': 'ease',
  'ease-in': 'ease-in',
  'ease-out': 'ease-out',
  'ease-in-out': 'ease-in-out',
  'bounce': 'cubic-bezier(0.68, -0.55, 0.265, 1.55)',
  'elastic': 'cubic-bezier(0.175, 0.885, 0.32, 1.275)',
  'back': 'cubic-bezier(0.175, 0.885, 0.32, 1.275)',
  'smooth': 'cubic-bezier(0.4, 0, 0.2, 1)',
  'fast': 'cubic-bezier(0.25, 0.46, 0.45, 0.94)',
  'slow': 'cubic-bezier(0.55, 0.055, 0.675, 0.19)'
}

// 动画时长预设
export const durationPresets: Record<string, number> = {
  'fast': 150,
  'normal': 300,
  'slow': 500,
  'extra-slow': 800,
  'instant': 0
}

// 工具函数
export const animationUtils = {
  // 获取动画时长
  getDuration: (duration: AnimationDuration): number => {
    if (typeof duration === 'number') return duration
    return durationPresets[duration] || durationPresets.normal
  },
  
  // 获取缓动函数
  getEasing: (easing: AnimationEasing): string => {
    return easingPresets[easing] || easingPresets.ease
  },
  
  // 生成CSS动画
  generateCSSAnimation: (preset: AnimationPreset, customName?: string): string => {
    const name = customName || `animation-${Date.now()}`
    const duration = animationUtils.getDuration(preset.duration)
    const easing = animationUtils.getEasing(preset.easing)
    const delay = preset.delay || 0
    const fillMode = preset.fillMode || 'both'
    const iterationCount = preset.iterationCount || 1
    const direction = preset.direction || 'normal'
    
    return `
      @keyframes ${name} {
        ${preset.keyframes}
      }
      
      .animate-${name} {
        animation-name: ${name};
        animation-duration: ${duration}ms;
        animation-timing-function: ${easing};
        animation-delay: ${delay}ms;
        animation-fill-mode: ${fillMode};
        animation-iteration-count: ${iterationCount};
        animation-direction: ${direction};
      }
    `
  },
  
  // 生成动画组合CSS
  generateComboCSS: (combo: AnimationCombo): string => {
    let css = ''
    
    combo.animations.forEach((anim, index) => {
      const preset = { ...basicAnimations[anim.preset], ...advancedAnimations[anim.preset] }
      if (preset) {
        const animName = `${combo.name}-${index}`
        css += animationUtils.generateCSSAnimation(preset, animName)
        
        if (anim.target) {
          css += `
            ${anim.target} {
              animation-name: ${animName};
            }
          `
        }
      }
    })
    
    return css
  },
  
  // 获取所有动画预设
  getAllAnimations: (): Record<string, AnimationPreset> => {
    return { ...basicAnimations, ...advancedAnimations }
  },
  
  // 获取动画类别
  getAnimationCategories: (): Record<string, string[]> => {
    return {
      '基础动画': Object.keys(basicAnimations),
      '高级动画': Object.keys(advancedAnimations),
      '动画组合': Object.keys(animationCombos)
    }
  },
  
  // 获取推荐动画
  getRecommendedAnimations: (context: string): string[] => {
    const recommendations: Record<string, string[]> = {
      '页面加载': ['fade-in', 'slide-up-in', 'scale-in'],
      '列表显示': ['slide-left-in', 'fade-in', 'scale-in'],
      '卡片悬停': ['scale-in', 'glow', 'float'],
      '按钮点击': ['bounce-in', 'pulse', 'scale-in'],
      '模态框': ['fade-in', 'scale-in', 'slide-up-in'],
      '错误提示': ['shake', 'flash', 'bounce-out'],
      '成功提示': ['bounce-in', 'pulse', 'glow'],
      '加载动画': ['rotate-in', 'pulse', 'float'],
      '导航切换': ['slide-left-in', 'slide-right-out', 'fade-in']
    }
    
    return recommendations[context] || recommendations['页面加载']
  }
}

// 导出所有动画
export const allAnimations = {
  ...basicAnimations,
  ...advancedAnimations
}

// 导出所有组合
export const allCombos = animationCombos

// 默认导出
export default {
  basicAnimations,
  advancedAnimations,
  animationCombos,
  easingPresets,
  durationPresets,
  animationUtils,
  allAnimations,
  allCombos
}