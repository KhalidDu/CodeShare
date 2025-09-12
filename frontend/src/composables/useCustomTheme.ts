import { ref, computed, watch, onMounted } from 'vue'
import { useLocalStorage } from '@vueuse/core'

// 主题颜色接口
export interface ThemeColors {
  primary: {
    50: string
    100: string
    200: string
    300: string
    400: string
    500: string
    600: string
    700: string
    800: string
    900: string
    950: string
  }
  secondary: {
    50: string
    100: string
    200: string
    300: string
    400: string
    500: string
    600: string
    700: string
    800: string
    900: string
    950: string
  }
  success: {
    50: string
    100: string
    200: string
    300: string
    400: string
    500: string
    600: string
    700: string
    800: string
    900: string
    950: string
  }
  warning: {
    50: string
    100: string
    200: string
    300: string
    400: string
    500: string
    600: string
    700: string
    800: string
    900: string
    950: string
  }
  error: {
    50: string
    100: string
    200: string
    300: string
    400: string
    500: string
    600: string
    700: string
    800: string
    900: string
    950: string
  }
  info: {
    50: string
    100: string
    200: string
    300: string
    400: string
    500: string
    600: string
    700: string
    800: string
    900: string
    950: string
  }
  gray: {
    50: string
    100: string
    200: string
    300: string
    400: string
    500: string
    600: string
    700: string
    800: string
    900: string
    950: string
  }
}

// 主题配置接口
export interface ThemeConfig {
  id: string
  name: string
  description: string
  colors: ThemeColors
  typography: {
    fontFamily: string
    fontSize: {
      xs: string
      sm: string
      base: string
      lg: string
      xl: string
      '2xl': string
      '3xl': string
      '4xl': string
    }
    lineHeight: {
      tight: string
      normal: string
      relaxed: string
      loose: string
    }
    letterSpacing: {
      tight: string
      normal: string
      wide: string
    }
  }
  spacing: {
    xs: string
    sm: string
    md: string
    lg: string
    xl: string
    '2xl': string
    '3xl': string
    '4xl': string
  }
  borderRadius: {
    none: string
    sm: string
    md: string
    lg: string
    xl: string
    '2xl': string
    '3xl': string
    full: string
  }
  shadows: {
    sm: string
    md: string
    lg: string
    xl: string
    '2xl': string
  }
  animations: {
    duration: {
      fast: string
      normal: string
      slow: string
    }
    easing: {
      ease: string
      easeIn: string
      easeOut: string
      easeInOut: string
    }
  }
}

// 默认主题配置
const defaultThemes: ThemeConfig[] = [
  {
    id: 'light',
    name: '浅色主题',
    description: '明亮清新的默认主题',
    colors: {
      primary: {
        50: '#eff6ff',
        100: '#dbeafe',
        200: '#bfdbfe',
        300: '#93c5fd',
        400: '#60a5fa',
        500: '#3b82f6',
        600: '#2563eb',
        700: '#1d4ed8',
        800: '#1e40af',
        900: '#1e3a8a',
        950: '#172554'
      },
      secondary: {
        50: '#f8fafc',
        100: '#f1f5f9',
        200: '#e2e8f0',
        300: '#cbd5e1',
        400: '#94a3b8',
        500: '#64748b',
        600: '#475569',
        700: '#334155',
        800: '#1e293b',
        900: '#0f172a',
        950: '#020617'
      },
      success: {
        50: '#f0fdf4',
        100: '#dcfce7',
        200: '#bbf7d0',
        300: '#86efac',
        400: '#4ade80',
        500: '#22c55e',
        600: '#16a34a',
        700: '#15803d',
        800: '#166534',
        900: '#14532d',
        950: '#052e16'
      },
      warning: {
        50: '#fffbeb',
        100: '#fef3c7',
        200: '#fde68a',
        300: '#fcd34d',
        400: '#fbbf24',
        500: '#f59e0b',
        600: '#d97706',
        700: '#b45309',
        800: '#92400e',
        900: '#78350f',
        950: '#451a03'
      },
      error: {
        50: '#fef2f2',
        100: '#fee2e2',
        200: '#fecaca',
        300: '#fca5a5',
        400: '#f87171',
        500: '#ef4444',
        600: '#dc2626',
        700: '#b91c1c',
        800: '#991b1b',
        900: '#7f1d1d',
        950: '#450a0a'
      },
      info: {
        50: '#f0f9ff',
        100: '#e0f2fe',
        200: '#bae6fd',
        300: '#7dd3fc',
        400: '#38bdf8',
        500: '#0ea5e9',
        600: '#0284c7',
        700: '#0369a1',
        800: '#075985',
        900: '#0c4a6e',
        950: '#082f49'
      },
      gray: {
        50: '#f9fafb',
        100: '#f3f4f6',
        200: '#e5e7eb',
        300: '#d1d5db',
        400: '#9ca3af',
        500: '#6b7280',
        600: '#4b5563',
        700: '#374151',
        800: '#1f2937',
        900: '#111827',
        950: '#030712'
      }
    },
    typography: {
      fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
      fontSize: {
        xs: '0.75rem',
        sm: '0.875rem',
        base: '1rem',
        lg: '1.125rem',
        xl: '1.25rem',
        '2xl': '1.5rem',
        '3xl': '1.875rem',
        '4xl': '2.25rem'
      },
      lineHeight: {
        tight: '1.25',
        normal: '1.5',
        relaxed: '1.625',
        loose: '2'
      },
      letterSpacing: {
        tight: '-0.025em',
        normal: '0',
        wide: '0.025em'
      }
    },
    spacing: {
      xs: '0.5rem',
      sm: '0.75rem',
      md: '1rem',
      lg: '1.5rem',
      xl: '2rem',
      '2xl': '2.5rem',
      '3xl': '3rem',
      '4xl': '4rem'
    },
    borderRadius: {
      none: '0',
      sm: '0.125rem',
      md: '0.375rem',
      lg: '0.5rem',
      xl: '0.75rem',
      '2xl': '1rem',
      '3xl': '1.5rem',
      full: '9999px'
    },
    shadows: {
      sm: '0 1px 2px 0 rgb(0 0 0 / 0.05)',
      md: '0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)',
      lg: '0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)',
      xl: '0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1)',
      '2xl': '0 25px 50px -12px rgb(0 0 0 / 0.25)'
    },
    animations: {
      duration: {
        fast: '150ms',
        normal: '300ms',
        slow: '500ms'
      },
      easing: {
        ease: 'ease',
        easeIn: 'ease-in',
        easeOut: 'ease-out',
        easeInOut: 'ease-in-out'
      }
    }
  },
  {
    id: 'dark',
    name: '深色主题',
    description: '护眼的深色模式',
    colors: {
      primary: {
        50: '#eff6ff',
        100: '#dbeafe',
        200: '#bfdbfe',
        300: '#93c5fd',
        400: '#60a5fa',
        500: '#3b82f6',
        600: '#2563eb',
        700: '#1d4ed8',
        800: '#1e40af',
        900: '#1e3a8a',
        950: '#172554'
      },
      secondary: {
        50: '#f8fafc',
        100: '#f1f5f9',
        200: '#e2e8f0',
        300: '#cbd5e1',
        400: '#94a3b8',
        500: '#64748b',
        600: '#475569',
        700: '#334155',
        800: '#1e293b',
        900: '#0f172a',
        950: '#020617'
      },
      success: {
        50: '#f0fdf4',
        100: '#dcfce7',
        200: '#bbf7d0',
        300: '#86efac',
        400: '#4ade80',
        500: '#22c55e',
        600: '#16a34a',
        700: '#15803d',
        800: '#166534',
        900: '#14532d',
        950: '#052e16'
      },
      warning: {
        50: '#fffbeb',
        100: '#fef3c7',
        200: '#fde68a',
        300: '#fcd34d',
        400: '#fbbf24',
        500: '#f59e0b',
        600: '#d97706',
        700: '#b45309',
        800: '#92400e',
        900: '#78350f',
        950: '#451a03'
      },
      error: {
        50: '#fef2f2',
        100: '#fee2e2',
        200: '#fecaca',
        300: '#fca5a5',
        400: '#f87171',
        500: '#ef4444',
        600: '#dc2626',
        700: '#b91c1c',
        800: '#991b1b',
        900: '#7f1d1d',
        950: '#450a0a'
      },
      info: {
        50: '#f0f9ff',
        100: '#e0f2fe',
        200: '#bae6fd',
        300: '#7dd3fc',
        400: '#38bdf8',
        500: '#0ea5e9',
        600: '#0284c7',
        700: '#0369a1',
        800: '#075985',
        900: '#0c4a6e',
        950: '#082f49'
      },
      gray: {
        50: '#f9fafb',
        100: '#f3f4f6',
        200: '#e5e7eb',
        300: '#d1d5db',
        400: '#9ca3af',
        500: '#6b7280',
        600: '#4b5563',
        700: '#374151',
        800: '#1f2937',
        900: '#111827',
        950: '#030712'
      }
    },
    typography: {
      fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
      fontSize: {
        xs: '0.75rem',
        sm: '0.875rem',
        base: '1rem',
        lg: '1.125rem',
        xl: '1.25rem',
        '2xl': '1.5rem',
        '3xl': '1.875rem',
        '4xl': '2.25rem'
      },
      lineHeight: {
        tight: '1.25',
        normal: '1.5',
        relaxed: '1.625',
        loose: '2'
      },
      letterSpacing: {
        tight: '-0.025em',
        normal: '0',
        wide: '0.025em'
      }
    },
    spacing: {
      xs: '0.5rem',
      sm: '0.75rem',
      md: '1rem',
      lg: '1.5rem',
      xl: '2rem',
      '2xl': '2.5rem',
      '3xl': '3rem',
      '4xl': '4rem'
    },
    borderRadius: {
      none: '0',
      sm: '0.125rem',
      md: '0.375rem',
      lg: '0.5rem',
      xl: '0.75rem',
      '2xl': '1rem',
      '3xl': '1.5rem',
      full: '9999px'
    },
    shadows: {
      sm: '0 1px 2px 0 rgb(0 0 0 / 0.3)',
      md: '0 4px 6px -1px rgb(0 0 0 / 0.4), 0 2px 4px -2px rgb(0 0 0 / 0.4)',
      lg: '0 10px 15px -3px rgb(0 0 0 / 0.4), 0 4px 6px -4px rgb(0 0 0 / 0.4)',
      xl: '0 20px 25px -5px rgb(0 0 0 / 0.4), 0 8px 10px -6px rgb(0 0 0 / 0.4)',
      '2xl': '0 25px 50px -12px rgb(0 0 0 / 0.6)'
    },
    animations: {
      duration: {
        fast: '150ms',
        normal: '300ms',
        slow: '500ms'
      },
      easing: {
        ease: 'ease',
        easeIn: 'ease-in',
        easeOut: 'ease-out',
        easeInOut: 'ease-in-out'
      }
    }
  }
]

// 自定义主题系统
export function useCustomTheme() {
  // 当前主题ID
  const currentThemeId = useLocalStorage<string>('current-theme-id', 'light')
  
  // 自定义主题列表
  const customThemes = useLocalStorage<ThemeConfig[]>('custom-themes', [])
  
  // 所有可用主题
  const allThemes = computed(() => [...defaultThemes, ...customThemes.value])
  
  // 当前主题配置
  const currentTheme = computed<ThemeConfig>(() => {
    return allThemes.value.find(theme => theme.id === currentThemeId.value) || defaultThemes[0]
  })
  
  // 系统主题偏好
  const prefersDark = ref(false)
  
  // 检测系统主题偏好
  const detectSystemTheme = () => {
    if (typeof window !== 'undefined') {
      prefersDark.value = window.matchMedia('(prefers-color-scheme: dark)').matches
    }
  }
  
  // 应用主题到DOM
  const applyTheme = (theme: ThemeConfig) => {
    if (typeof document === 'undefined') return
    
    const root = document.documentElement
    
    // 移除现有主题类
    root.classList.remove('theme-light', 'theme-dark')
    
    // 应用颜色变量
    Object.entries(theme.colors).forEach(([colorName, colorValues]) => {
      Object.entries(colorValues).forEach(([shade, value]) => {
        root.style.setProperty(`--color-${colorName}-${shade}`, value)
      })
    })
    
    // 应用排版变量
    root.style.setProperty('--font-family', theme.typography.fontFamily)
    Object.entries(theme.typography.fontSize).forEach(([size, value]) => {
      root.style.setProperty(`--font-size-${size}`, value)
    })
    Object.entries(theme.typography.lineHeight).forEach(([type, value]) => {
      root.style.setProperty(`--line-height-${type}`, value)
    })
    Object.entries(theme.typography.letterSpacing).forEach(([type, value]) => {
      root.style.setProperty(`--letter-spacing-${type}`, value)
    })
    
    // 应用间距变量
    Object.entries(theme.spacing).forEach(([size, value]) => {
      root.style.setProperty(`--spacing-${size}`, value)
    })
    
    // 应用圆角变量
    Object.entries(theme.borderRadius).forEach(([size, value]) => {
      root.style.setProperty(`--radius-${size}`, value)
    })
    
    // 应用阴影变量
    Object.entries(theme.shadows).forEach(([size, value]) => {
      root.style.setProperty(`--shadow-${size}`, value)
    })
    
    // 应用动画变量
    Object.entries(theme.animations.duration).forEach(([speed, value]) => {
      root.style.setProperty(`--duration-${speed}`, value)
    })
    Object.entries(theme.animations.easing).forEach(([type, value]) => {
      root.style.setProperty(`--easing-${type}`, value)
    })
    
    // 添加主题类
    root.classList.add(`theme-${theme.id}`)
  }
  
  // 切换主题
  const switchTheme = (themeId: string) => {
    const theme = allThemes.value.find(t => t.id === themeId)
    if (theme) {
      currentThemeId.value = themeId
      applyTheme(theme)
    }
  }
  
  // 创建自定义主题
  const createCustomTheme = (baseThemeId: string, customConfig: Partial<ThemeConfig>): ThemeConfig => {
    const baseTheme = allThemes.value.find(t => t.id === baseThemeId)
    if (!baseTheme) {
      throw new Error(`Base theme ${baseThemeId} not found`)
    }
    
    const newTheme: ThemeConfig = {
      id: `custom-${Date.now()}`,
      name: customConfig.name || `自定义主题 ${customThemes.value.length + 1}`,
      description: customConfig.description || '基于默认主题的自定义主题',
      colors: {
        ...baseTheme.colors,
        ...customConfig.colors
      },
      typography: {
        ...baseTheme.typography,
        ...customConfig.typography
      },
      spacing: {
        ...baseTheme.spacing,
        ...customConfig.spacing
      },
      borderRadius: {
        ...baseTheme.borderRadius,
        ...customConfig.borderRadius
      },
      shadows: {
        ...baseTheme.shadows,
        ...customConfig.shadows
      },
      animations: {
        ...baseTheme.animations,
        ...customConfig.animations
      }
    }
    
    return newTheme
  }
  
  // 保存自定义主题
  const saveCustomTheme = (theme: ThemeConfig) => {
    const existingIndex = customThemes.value.findIndex(t => t.id === theme.id)
    if (existingIndex >= 0) {
      customThemes.value[existingIndex] = theme
    } else {
      customThemes.value.push(theme)
    }
  }
  
  // 删除自定义主题
  const deleteCustomTheme = (themeId: string) => {
    customThemes.value = customThemes.value.filter(t => t.id !== themeId)
    
    // 如果删除的是当前主题，切换到默认主题
    if (currentThemeId.value === themeId) {
      switchTheme('light')
    }
  }
  
  // 更新自定义主题
  const updateCustomTheme = (themeId: string, updates: Partial<ThemeConfig>) => {
    const themeIndex = customThemes.value.findIndex(t => t.id === themeId)
    if (themeIndex >= 0) {
      customThemes.value[themeIndex] = {
        ...customThemes.value[themeIndex],
        ...updates
      }
      
      // 如果更新的是当前主题，重新应用
      if (currentThemeId.value === themeId) {
        applyTheme(customThemes.value[themeIndex])
      }
    }
  }
  
  // 导出主题配置
  const exportTheme = (themeId: string): string => {
    const theme = allThemes.value.find(t => t.id === themeId)
    if (!theme) {
      throw new Error(`Theme ${themeId} not found`)
    }
    
    return JSON.stringify(theme, null, 2)
  }
  
  // 导入主题配置
  const importTheme = (themeJson: string): ThemeConfig => {
    try {
      const theme = JSON.parse(themeJson) as ThemeConfig
      
      // 验证主题配置
      if (!theme.id || !theme.name || !theme.colors) {
        throw new Error('Invalid theme configuration')
      }
      
      // 确保ID唯一
      if (allThemes.value.some(t => t.id === theme.id)) {
        theme.id = `custom-${Date.now()}`
      }
      
      return theme
    } catch (error) {
      throw new Error('Invalid theme JSON format')
    }
  }
  
  // 重置到默认主题
  const resetToDefault = () => {
    switchTheme('light')
  }
  
  // 获取主题预览样式
  const getThemePreview = (theme: ThemeConfig): string => {
    return `
      background: linear-gradient(135deg, ${theme.colors.primary[500]} 0%, ${theme.colors.primary[600]} 100%);
      color: ${theme.colors.gray[50]};
      font-family: ${theme.typography.fontFamily};
    `
  }
  
  // 验证主题配置
  const validateTheme = (theme: Partial<ThemeConfig>): string[] => {
    const errors: string[] = []
    
    if (!theme.name || theme.name.trim() === '') {
      errors.push('主题名称不能为空')
    }
    
    if (!theme.colors) {
      errors.push('主题颜色配置不能为空')
    } else {
      // 验证必需的颜色
      const requiredColors = ['primary', 'gray']
      requiredColors.forEach(colorName => {
        if (!theme.colors![colorName as keyof ThemeColors]) {
          errors.push(`缺少必需的颜色: ${colorName}`)
        }
      })
    }
    
    return errors
  }
  
  // 监听系统主题变化
  const setupSystemThemeListener = () => {
    if (typeof window !== 'undefined') {
      const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
      const handleChange = (e: MediaQueryListEvent) => {
        prefersDark.value = e.matches
      }
      
      mediaQuery.addEventListener('change', handleChange)
      
      // 清理函数
      return () => {
        mediaQuery.removeEventListener('change', handleChange)
      }
    }
  }
  
  // 初始化
  onMounted(() => {
    detectSystemTheme()
    setupSystemThemeListener()
    applyTheme(currentTheme.value)
  })
  
  // 监听当前主题变化
  watch(currentTheme, (newTheme) => {
    applyTheme(newTheme)
  }, { deep: true })
  
  return {
    // 状态
    currentThemeId,
    currentTheme,
    allThemes,
    customThemes,
    prefersDark,
    
    // 方法
    switchTheme,
    createCustomTheme,
    saveCustomTheme,
    deleteCustomTheme,
    updateCustomTheme,
    exportTheme,
    importTheme,
    resetToDefault,
    getThemePreview,
    validateTheme,
    detectSystemTheme
  }
}