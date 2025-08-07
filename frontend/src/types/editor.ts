/**
 * 代码编辑器相关类型定义
 */

// 支持的编程语言
export interface SupportedLanguage {
  value: string
  label: string
}

// 编辑器主题
export type EditorTheme = 'vs' | 'vs-dark' | 'hc-black'

// 编辑器配置选项
export interface EditorOptions {
  language?: string
  theme?: EditorTheme
  readonly?: boolean
  height?: string
  fontSize?: number
  tabSize?: number
  insertSpaces?: boolean
  wordWrap?: 'on' | 'off' | 'wordWrapColumn' | 'bounded'
  minimap?: {
    enabled: boolean
  }
  lineNumbers?: 'on' | 'off' | 'relative' | 'interval'
  renderWhitespace?: 'none' | 'boundary' | 'selection' | 'trailing' | 'all'
  folding?: boolean
  showFoldingControls?: 'always' | 'mouseover'
}

// 编辑器实例方法
export interface EditorInstance {
  setValue: (value: string) => void
  getValue: () => string
  setLanguage: (language: string) => void
  setTheme: (theme: string) => void
  focus: () => void
  formatCode: () => Promise<void>
}

// 代码片段编辑器事件
export interface EditorEvents {
  'update:modelValue': (value: string) => void
  'language-change': (language: string) => void
  'theme-change': (theme: string) => void
}
