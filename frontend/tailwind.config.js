/** @type {import('tailwindcss').Config} */
import forms from '@tailwindcss/forms'

export default {
  darkMode: "class",
  content: ["./index.html", "./src/**/*.{vue,js,ts,jsx,tsx}"],
  plugins: [
    forms,
  ],
  
  // 扩展设计令牌
  theme: {
    extend: {
      // 颜色扩展
      colors: {
        // 主色调扩展
        primary: {
          50: 'var(--primary-50)',
          100: 'var(--primary-100)',
          200: 'var(--primary-200)',
          300: 'var(--primary-300)',
          400: 'var(--primary-400)',
          500: 'var(--primary-500)',
          600: 'var(--primary-600)',
          700: 'var(--primary-700)',
          800: 'var(--primary-800)',
          900: 'var(--primary-900)',
        },
        // 中性色扩展
        gray: {
          50: 'var(--gray-50)',
          100: 'var(--gray-100)',
          200: 'var(--gray-200)',
          300: 'var(--gray-300)',
          400: 'var(--gray-400)',
          500: 'var(--gray-500)',
          600: 'var(--gray-600)',
          700: 'var(--gray-700)',
          800: 'var(--gray-800)',
          900: 'var(--gray-900)',
        },
        // 语义色彩扩展
        success: {
          50: '#d4edda',
          100: '#c3e6cb',
          200: '#a3d9a5',
          300: '#82cc7f',
          400: '#62bf5a',
          500: '#28a745',
          600: '#218838',
          700: '#1e7e34',
          800: '#1a6828',
          900: '#155724',
        },
        warning: {
          50: '#fff3cd',
          100: '#ffeaa7',
          200: '#ffd93d',
          300: '#ffc107',
          400: '#ffb300',
          500: '#ffc107',
          600: '#e0a800',
          700: '#d39e00',
          800: '#c69500',
          900: '#b17e00',
        },
        error: {
          50: '#f8d7da',
          100: '#f5c6cb',
          200: '#f1aeb5',
          300: '#ea868f',
          400: '#e35d6a',
          500: '#dc3545',
          600: '#c82333',
          700: '#bd2130',
          800: '#b21f2d',
          900: '#a71e2a',
        },
        info: {
          50: '#d1ecf1',
          100: '#bee5eb',
          200: '#85d6e3',
          300: '#5dc7db',
          400: '#34b8d3',
          500: '#17a2b8',
          600: '#138496',
          700: '#117a8b',
          800: '#0c6d72',
          900: '#0a5c5e',
        },
      },
      
      // 边框半径扩展
      borderRadius: {
        'xs': 'var(--radius-xs)',
        'sm': 'var(--radius-sm)',
        'md': 'var(--radius-md)',
        'lg': 'var(--radius-lg)',
        'xl': 'var(--radius-xl)',
        '2xl': 'var(--radius-2xl)',
        '3xl': 'var(--radius-3xl)',
        '4xl': 'var(--radius-4xl)',
        'full': 'var(--radius-full)',
      },
      
      // 间距扩展
      spacing: {
        'px': 'var(--space-px)',
        '0': 'var(--space-0)',
        '1': 'var(--space-1)',
        '2': 'var(--space-2)',
        '3': 'var(--space-3)',
        '4': 'var(--space-4)',
        '5': 'var(--space-5)',
        '6': 'var(--space-6)',
        '8': 'var(--space-8)',
        '10': 'var(--space-10)',
        '12': 'var(--space-12)',
        '14': 'var(--space-14)',
        '16': 'var(--space-16)',
        '20': 'var(--space-20)',
        '24': 'var(--space-24)',
        '32': 'var(--space-32)',
        '40': 'var(--space-40)',
        '48': 'var(--space-48)',
        '56': 'var(--space-56)',
        '64': 'var(--space-64)',
      },
      
      // 字体大小扩展
      fontSize: {
        'xs': 'var(--text-xs)',
        'sm': 'var(--text-sm)',
        'base': 'var(--text-base)',
        'lg': 'var(--text-lg)',
        'xl': 'var(--text-xl)',
        '2xl': 'var(--text-2xl)',
        '3xl': 'var(--text-3xl)',
        '4xl': 'var(--text-4xl)',
        '5xl': 'var(--text-5xl)',
        '6xl': 'var(--text-6xl)',
        '7xl': 'var(--text-7xl)',
        '8xl': 'var(--text-8xl)',
        '9xl': 'var(--text-9xl)',
      },
      
      // 字体权重扩展
      fontWeight: {
        'extralight': 'var(--font-extralight)',
        'light': 'var(--font-light)',
        'normal': 'var(--font-normal)',
        'medium': 'var(--font-medium)',
        'semibold': 'var(--font-semibold)',
        'bold': 'var(--font-bold)',
        'extrabold': 'var(--font-extrabold)',
        'black': 'var(--font-black)',
      },
      
      // 行高扩展
      lineHeight: {
        '3': 'var(--leading-3)',
        '4': 'var(--leading-4)',
        '5': 'var(--leading-5)',
        '6': 'var(--leading-6)',
        '7': 'var(--leading-7)',
        '8': 'var(--leading-8)',
        '9': 'var(--leading-9)',
        '10': 'var(--leading-10)',
        'tight': 'var(--leading-tight)',
        'normal': 'var(--leading-normal)',
        'relaxed': 'var(--leading-relaxed)',
      },
      
      // 字母间距扩展
      letterSpacing: {
        'tighter': 'var(--tracking-tighter)',
        'tight': 'var(--tracking-tight)',
        'normal': 'var(--tracking-normal)',
        'wide': 'var(--tracking-wide)',
        'wider': 'var(--tracking-wider)',
        'widest': 'var(--tracking-widest)',
      },
      
      // 阴影扩展
      boxShadow: {
        'sm': 'var(--shadow-sm)',
        'md': 'var(--shadow-md)',
        'lg': 'var(--shadow-lg)',
        'xl': 'var(--shadow-xl)',
        'inner': 'var(--shadow-inner)',
        'primary': 'var(--shadow-primary)',
        'primary-lg': 'var(--shadow-primary-lg)',
        'glow-sm': 'var(--shadow-glow-sm)',
        'glow-md': 'var(--shadow-glow-md)',
        'glow-lg': 'var(--shadow-glow-lg)',
        'success': 'var(--shadow-colored-success)',
        'warning': 'var(--shadow-colored-warning)',
        'error': 'var(--shadow-colored-error)',
        'info': 'var(--shadow-colored-info)',
        'none': 'none',
      },
      
      // Z-index扩展
      zIndex: {
        'negative': 'var(--z-negative)',
        'content': 'var(--z-content)',
        'fixed-header': 'var(--z-fixed-header)',
        'dropdown': 'var(--z-dropdown)',
        'sticky': 'var(--z-sticky)',
        'fixed': 'var(--z-fixed)',
        'modal-backdrop': 'var(--z-modal-backdrop)',
        'modal': 'var(--z-modal)',
        'popover': 'var(--z-popover)',
        'tooltip': 'var(--z-tooltip)',
        'notification': 'var(--z-notification)',
        'critical': 'var(--z-critical)',
      },
      
      // 最大宽度扩展
      maxWidth: {
        'xs': 'var(--max-w-xs)',
        'sm': 'var(--max-w-sm)',
        'md': 'var(--max-w-md)',
        'lg': 'var(--max-w-lg)',
        'xl': 'var(--max-w-xl)',
        '2xl': 'var(--max-w-2xl)',
        '3xl': 'var(--max-w-3xl)',
        '4xl': 'var(--max-w-4xl)',
        '5xl': 'var(--max-w-5xl)',
        '6xl': 'var(--max-w-6xl)',
        '7xl': 'var(--max-w-7xl)',
        'full': 'var(--max-w-full)',
        'screen-sm': 'var(--max-w-screen-sm)',
        'screen-md': 'var(--max-w-screen-md)',
        'screen-lg': 'var(--max-w-screen-lg)',
        'screen-xl': 'var(--max-w-screen-xl)',
        'screen-2xl': 'var(--max-w-screen-2xl)',
      },
      
      // 最小高度扩展
      minHeight: {
        'screen': 'var(--min-h-screen)',
        'full': 'var(--min-h-full)',
        '0': 'var(--min-h-0)',
      },
      
      // 最大高度扩展
      maxHeight: {
        'screen': 'var(--max-h-screen)',
        'full': 'var(--max-h-full)',
        '0': 'var(--max-h-0)',
      },
      
      // 透明度扩展
      opacity: {
        '0': 'var(--opacity-0)',
        '10': 'var(--opacity-10)',
        '20': 'var(--opacity-20)',
        '30': 'var(--opacity-30)',
        '40': 'var(--opacity-40)',
        '50': 'var(--opacity-50)',
        '60': 'var(--opacity-60)',
        '70': 'var(--opacity-70)',
        '80': 'var(--opacity-80)',
        '90': 'var(--opacity-90)',
        '100': 'var(--opacity-100)',
      },
      
      // 动画扩展
      animation: {
        'bounce': 'bounce 1s infinite',
        'elastic': 'elastic 0.8s ease-out',
        'wiggle': 'wiggle 1s ease-in-out',
        'heartbeat': 'heartbeat 1.3s ease-in-out infinite',
        'light-bounce': 'lightBounce 2s ease-in-out infinite',
        'flash': 'flash 1.5s ease-in-out infinite',
        'zoom-in': 'zoomIn 0.3s ease-out',
        'zoom-out': 'zoomOut 0.3s ease-out',
        'flip-in': 'flipIn 0.6s ease-out',
        'flip-out': 'flipOut 0.6s ease-out',
        'slide-in-left': 'slideInLeft 0.3s ease-out',
        'slide-in-right': 'slideInRight 0.3s ease-out',
        'slide-in-up': 'slideInUp 0.3s ease-out',
        'slide-in-down': 'slideInDown 0.3s ease-out',
        'rotate-in': 'rotateIn 0.3s ease-out',
        'rotate-out': 'rotateOut 0.3s ease-out',
        'fade-in': 'fadeIn 0.3s ease-out',
        'slide-in': 'slideIn 0.3s ease-out',
        'pulse': 'pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'spin': 'spin 1s linear infinite',
        'none': 'none',
      },
      
      // 关键帧定义
      keyframes: {
        bounce: {
          '0%, 100%': {
            transform: 'translateY(-25%)',
            animationTimingFunction: 'cubic-bezier(0.8, 0, 1, 1)',
          },
          '50%': {
            transform: 'translateY(0)',
            animationTimingFunction: 'cubic-bezier(0, 0, 0.2, 1)',
          },
        },
        elastic: {
          '0%': {
            transform: 'scale(0)',
          },
          '50%': {
            transform: 'scale(1.1)',
          },
          '100%': {
            transform: 'scale(1)',
          },
        },
        wiggle: {
          '0%, 7%': {
            transform: 'rotateZ(0)',
          },
          '15%': {
            transform: 'rotateZ(-15deg)',
          },
          '20%': {
            transform: 'rotateZ(10deg)',
          },
          '25%': {
            transform: 'rotateZ(-10deg)',
          },
          '30%': {
            transform: 'rotateZ(6deg)',
          },
          '35%': {
            transform: 'rotateZ(-4deg)',
          },
          '40%, 100%': {
            transform: 'rotateZ(0)',
          },
        },
        heartbeat: {
          '0%': {
            transform: 'scale(1)',
          },
          '14%': {
            transform: 'scale(1.3)',
          },
          '28%': {
            transform: 'scale(1)',
          },
          '42%': {
            transform: 'scale(1.3)',
          },
          '70%': {
            transform: 'scale(1)',
          },
        },
        lightBounce: {
          '0%, 100%': {
            transform: 'translateY(0)',
          },
          '50%': {
            transform: 'translateY(-5px)',
          },
        },
        flash: {
          '0%, 50%, 100%': {
            opacity: '1',
          },
          '25%, 75%': {
            opacity: '0',
          },
        },
        zoomIn: {
          'from': {
            opacity: '0',
            transform: 'scale(0.5)',
          },
          'to': {
            opacity: '1',
            transform: 'scale(1)',
          },
        },
        zoomOut: {
          'from': {
            opacity: '1',
            transform: 'scale(1)',
          },
          'to': {
            opacity: '0',
            transform: 'scale(0.5)',
          },
        },
        flipIn: {
          'from': {
            transform: 'perspective(400px) rotateY(90deg)',
            opacity: '0',
          },
          'to': {
            transform: 'perspective(400px) rotateY(0deg)',
            opacity: '1',
          },
        },
        flipOut: {
          'from': {
            transform: 'perspective(400px) rotateY(0deg)',
            opacity: '1',
          },
          'to': {
            transform: 'perspective(400px) rotateY(90deg)',
            opacity: '0',
          },
        },
        slideInLeft: {
          'from': {
            opacity: '0',
            transform: 'translateX(-100px)',
          },
          'to': {
            opacity: '1',
            transform: 'translateX(0)',
          },
        },
        slideInRight: {
          'from': {
            opacity: '0',
            transform: 'translateX(100px)',
          },
          'to': {
            opacity: '1',
            transform: 'translateX(0)',
          },
        },
        slideInUp: {
          'from': {
            opacity: '0',
            transform: 'translateY(100px)',
          },
          'to': {
            opacity: '1',
            transform: 'translateY(0)',
          },
        },
        slideInDown: {
          'from': {
            opacity: '0',
            transform: 'translateY(-100px)',
          },
          'to': {
            opacity: '1',
            transform: 'translateY(0)',
          },
        },
        rotateIn: {
          'from': {
            opacity: '0',
            transform: 'rotate(-200deg)',
          },
          'to': {
            opacity: '1',
            transform: 'rotate(0deg)',
          },
        },
        rotateOut: {
          'from': {
            opacity: '1',
            transform: 'rotate(0deg)',
          },
          'to': {
            opacity: '0',
            transform: 'rotate(200deg)',
          },
        },
        fadeIn: {
          'from': {
            opacity: '0',
            transform: 'translateY(10px)',
          },
          'to': {
            opacity: '1',
            transform: 'translateY(0)',
          },
        },
        slideIn: {
          'from': {
            opacity: '0',
            transform: 'translateX(-20px)',
          },
          'to': {
            opacity: '1',
            transform: 'translateX(0)',
          },
        },
        pulse: {
          '0%, 100%': {
            opacity: '1',
          },
          '50%': {
            opacity: '0.5',
          },
        },
        spin: {
          'from': {
            transform: 'rotate(0deg)',
          },
          'to': {
            transform: 'rotate(360deg)',
          },
        },
      },
      
      // 过渡扩展
      transitionProperty: {
        'width': 'width',
        'height': 'height',
        'padding': 'padding',
        'margin': 'margin',
        'font-size': 'font-size',
        'background-color': 'background-color',
        'border-color': 'border-color',
        'box-shadow': 'box-shadow',
        'transform': 'transform',
        'opacity': 'opacity',
        'filter': 'filter',
        'backdrop-filter': 'backdrop-filter',
      },
      
      // 过渡持续时间扩展
      transitionDuration: {
        '75': '75ms',
        '100': '100ms',
        '150': '150ms',
        '200': '200ms',
        '300': '300ms',
        '500': '500ms',
        '700': '700ms',
        '1000': '1000ms',
      },
      
      // 过渡时间函数扩展
      transitionTimingFunction: {
        'linear': 'linear',
        'in': 'cubic-bezier(0.4, 0, 1, 1)',
        'out': 'cubic-bezier(0, 0, 0.2, 1)',
        'in-out': 'cubic-bezier(0.4, 0, 0.2, 1)',
        'bounce': 'var(--animation-bounce)',
        'elastic': 'var(--animation-elastic)',
        'smooth': 'var(--animation-smooth)',
        'enter': 'var(--animation-enter)',
        'exit': 'var(--animation-exit)',
      },
      
      // 背景图片扩展
      backgroundImage: {
        'gradient-primary': 'var(--gradient-primary)',
        'gradient-primary-light': 'var(--gradient-primary-light)',
        'gradient-primary-dark': 'var(--gradient-primary-dark)',
        'gradient-success': 'var(--gradient-success)',
        'gradient-warning': 'var(--gradient-warning)',
        'gradient-error': 'var(--gradient-error)',
        'gradient-info': 'var(--gradient-info)',
        'gradient-surface': 'var(--gradient-surface)',
        'gradient-background': 'var(--gradient-background)',
        'gradient-glass': 'var(--gradient-glass)',
        'gradient-metallic': 'var(--gradient-metallic)',
        'gradient-neon': 'var(--gradient-neon)',
        'gradient-sunset': 'var(--gradient-sunset)',
        'gradient-ocean': 'var(--gradient-ocean)',
        'gradient-forest': 'var(--gradient-forest)',
      },
      
      // 背景大小扩展
      backgroundSize: {
        'auto': 'auto',
        'cover': 'cover',
        'contain': 'contain',
        'full': '100% 100%',
      },
      
      // 背景位置扩展
      backgroundPosition: {
        'center': 'center',
        'top': 'top',
        'bottom': 'bottom',
        'left': 'left',
        'right': 'right',
        'top-left': 'top left',
        'top-right': 'top right',
        'bottom-left': 'bottom left',
        'bottom-right': 'bottom right',
      },
      
      // 混合模式扩展
      mixBlendMode: {
        'normal': 'var(--mix-blend-normal)',
        'multiply': 'var(--mix-blend-multiply)',
        'screen': 'var(--mix-blend-screen)',
        'overlay': 'var(--mix-blend-overlay)',
        'darken': 'var(--mix-blend-darken)',
        'lighten': 'var(--mix-blend-lighten)',
        'color-dodge': 'var(--mix-blend-color-dodge)',
        'color-burn': 'var(--mix-blend-color-burn)',
        'hard-light': 'var(--mix-blend-hard-light)',
        'soft-light': 'var(--mix-blend-soft-light)',
        'difference': 'var(--mix-blend-difference)',
        'exclusion': 'var(--mix-blend-exclusion)',
        'hue': 'var(--mix-blend-hue)',
        'saturation': 'var(--mix-blend-saturation)',
        'color': 'var(--mix-blend-color)',
        'luminosity': 'var(--mix-blend-luminosity)',
      },
      
      // 滤镜扩展
      filter: {
        'none': 'none',
        'blur-sm': 'var(--blur-sm)',
        'blur': 'var(--blur-md)',
        'blur-md': 'var(--blur-md)',
        'blur-lg': 'var(--blur-lg)',
        'blur-xl': 'var(--blur-xl)',
      },
      
      // 背景滤镜扩展
      backdropFilter: {
        'none': 'none',
        'blur-sm': 'var(--blur-sm)',
        'blur': 'var(--blur-md)',
        'blur-md': 'var(--blur-md)',
        'blur-lg': 'var(--blur-lg)',
        'blur-xl': 'var(--blur-xl)',
      },
      
      // 网格系统扩展
      gridTemplateColumns: {
        '1': 'var(--grid-cols-1)',
        '2': 'var(--grid-cols-2)',
        '3': 'var(--grid-cols-3)',
        '4': 'var(--grid-cols-4)',
        '5': 'var(--grid-cols-5)',
        '6': 'var(--grid-cols-6)',
        '12': 'var(--grid-cols-12)',
      },
      
      // 位置扩展
      inset: {
        '0': 'var(--inset-0)',
        'x-0': 'var(--inset-x-0)',
        'y-0': 'var(--inset-y-0)',
        'top-0': 'var(--inset-top-0)',
        'right-0': 'var(--inset-right-0)',
        'bottom-0': 'var(--inset-bottom-0)',
        'left-0': 'var(--inset-left-0)',
      },
    },
  },
  
  // 变体配置
  variants: {
    extend: {
      // 响应式断点扩展
      screens: {
        'xs': '320px',
        'sm': '640px',
        'md': '768px',
        'lg': '1024px',
        'xl': '1280px',
        '2xl': '1536px',
        '3xl': '1920px',
      },
      
      // 悬停状态扩展
      hover: {
        '@media (hover: hover)': {
          '&:hover': {
            '&:not(:disabled)': {},
          },
        },
      },
      
      // 焦点状态扩展
      focus: {
        '&:focus': {
          outline: '2px solid var(--primary-500)',
          outlineOffset: '2px',
        },
      },
      
      // 禁用状态扩展
      disabled: {
        '&:disabled': {
          opacity: '0.5',
          cursor: 'not-allowed',
          pointerEvents: 'none',
        },
      },
      
      // 减少动画偏好
      'motion-reduce': {
        '@media (prefers-reduced-motion: reduce)': {
          animationDuration: '0.01ms !important',
          animationIterationCount: '1 !important',
          transitionDuration: '0.01ms !important',
        },
      },
      
      // 高对比度模式
      'contrast-high': {
        '@media (prefers-contrast: high)': {
          borderWidth: '2px',
        },
      },
    },
  },
  
  // 核心插件配置
  corePlugins: {
    // 确保所有核心插件都启用
    preflight: true,
    accessibility: true,
    animation: true,
    backgroundSize: true,
    backgroundImage: true,
    backgroundPosition: true,
    backdropFilter: true,
    borderCollapse: true,
    borderColor: true,
    borderRadius: true,
    borderStyle: true,
    borderWidth: true,
    boxShadow: true,
    boxSizing: true,
    clear: true,
    container: true,
    cursor: true,
    display: true,
    divideColor: true,
    divideStyle: true,
    divideWidth: true,
    fill: true,
    flex: true,
    flexDirection: true,
    flexGrow: true,
    flexShrink: true,
    flexWrap: true,
    fontFamily: true,
    fontSize: true,
    fontSmoothing: true,
    fontStyle: true,
    fontWeight: true,
    gap: true,
    gradientColorStops: true,
    gridAutoColumns: true,
    gridAutoFlow: true,
    gridAutoRows: true,
    gridColumn: true,
    gridColumnEnd: true,
    gridColumnStart: true,
    gridRow: true,
    gridRowEnd: true,
    gridRowStart: true,
    gridTemplateColumns: true,
    gridTemplateRows: true,
    height: true,
    inset: true,
    justifyContent: true,
    justifyItems: true,
    justifySelf: true,
    letterSpacing: true,
    lineHeight: true,
    listStylePosition: true,
    listStyleType: true,
    margin: true,
    maxHeight: true,
    maxWidth: true,
    minHeight: true,
    minWidth: true,
    mixBlendMode: true,
    objectFit: true,
    objectPosition: true,
    opacity: true,
    order: true,
    outline: true,
    overflow: true,
    overscrollBehavior: true,
    padding: true,
    placeContent: true,
    placeItems: true,
    placeSelf: true,
    placeholderColor: true,
    pointerEvents: true,
    position: true,
    resize: true,
    rotate: true,
    rounded: true,
    scale: true,
    scrollBehavior: true,
    scrollMargin: true,
    scrollPadding: true,
    scrollSnapAlign: true,
    scrollSnapStop: true,
    scrollSnapType: true,
    scrollbarWidth: true,
    skew: true,
    space: true,
    textAlign: true,
    textColor: true,
    textDecoration: true,
    textIndent: true,
    textOverflow: true,
    textTransform: true,
    textUnderlineOffset: true,
    transform: true,
    transformOrigin: true,
    transition: true,
    translate: true,
    userSelect: true,
    verticalAlign: true,
    visibility: true,
    whitespace: true,
    width: true,
    wordBreak: true,
    zIndex: true,
  },
}
