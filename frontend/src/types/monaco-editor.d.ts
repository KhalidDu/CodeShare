// Monaco Editor 类型定义扩展
// 扩展 Monaco Editor 的 TypeScript 类型定义

import type * as Monaco from 'monaco-editor'

declare global {
  interface Window {
    /** Monaco Editor 全局实例 */
    monaco?: typeof Monaco
    /** Monaco Editor Web Worker 数组 */
    monacoWorkers?: Worker[]
    /** Monaco Editor 环境配置 */
    MonacoEnvironment?: {
      getWorkerUrl?: (moduleId: string, label: string) => string
      getWorker?: (moduleId: string, label: string) => Worker
    }
  }
}

/**
 * Monaco Editor 配置接口扩展
 */
export interface ExtendedMonacoConfig extends Monaco.editor.IStandaloneEditorConstructionOptions {
  /** 编辑器类型 */
  type?: 'viewer' | 'editor' | 'mobile'
  /** 编程语言 */
  language?: string
  /** 主题 */
  theme?: string
  /** 是否启用 Web Worker */
  enableWorker?: boolean
  /** 最大文件大小 */
  maxFileSize?: number
  /** 超时时间 */
  timeout?: number
  /** 自定义语言配置 */
  customLanguageConfig?: any
  /** 自定义主题配置 */
  customThemeConfig?: any
  /** 性能监控配置 */
  performanceMonitoring?: {
    enabled?: boolean
    threshold?: number
    interval?: number
  }
}

/**
 * Monaco Editor 实例扩展接口
 */
export interface ExtendedEditorInstance {
  /** 编辑器实例 */
  editor: Monaco.editor.IStandaloneCodeEditor
  /** 编辑器容器 */
  container: HTMLElement
  /** 编辑器配置 */
  options: ExtendedMonacoConfig
  /** 编辑器语言 */
  language: string
  /** 编辑器主题 */
  theme: string
  /** 创建时间 */
  createdAt: Date
  /** 最后更新时间 */
  updatedAt: Date
  /** 编辑器 ID */
  id: string
  /** 编辑器统计信息 */
  stats?: {
    totalChanges: number
    lastChangeTime: Date
    averageResponseTime: number
    memoryUsage?: number
  }
}

/**
 * Monaco Editor 事件处理器接口扩展
 */
export interface ExtendedEditorEventHandlers {
  /** 内容变化事件 */
  onContentChange?: (content: string, instance: ExtendedEditorInstance) => void
  /** 光标位置变化事件 */
  onCursorChange?: (position: Monaco.IPosition, instance: ExtendedEditorInstance) => void
  /** 选择变化事件 */
  onSelectionChange?: (selection: Monaco.ISelection, instance: ExtendedEditorInstance) => void
  /** 编辑器失去焦点事件 */
  onBlur?: (instance: ExtendedEditorInstance) => void
  /** 编辑器获得焦点事件 */
  onFocus?: (instance: ExtendedEditorInstance) => void
  /** 编辑器滚动事件 */
  onScrollChange?: (scrollTop: number, scrollLeft: number, instance: ExtendedEditorInstance) => void
  /** 编辑器大小变化事件 */
  onSizeChange?: (width: number, height: number, instance: ExtendedEditorInstance) => void
  /** 编辑器配置变化事件 */
  onOptionsChange?: (options: Monaco.IEditorOptions, instance: ExtendedEditorInstance) => void
  /** 编辑器销毁事件 */
  onDestroy?: (instance: ExtendedEditorInstance) => void
  /** 编辑器初始化完成事件 */
  onInitialized?: (instance: ExtendedEditorInstance) => void
  /** 编辑器错误事件 */
  onError?: (error: Error, instance: ExtendedEditorInstance) => void
  /** 编辑器性能事件 */
  onPerformance?: (stats: any, instance: ExtendedEditorInstance) => void
}

/**
 * Monaco Editor 语言配置接口
 */
export interface LanguageConfig {
  /** 语言 ID */
  id: string
  /** 语言名称 */
  name: string
  /** 文件扩展名 */
  extensions: string[]
  /** 语言别名 */
  aliases: string[]
  /** MIME 类型 */
  mimetypes: string[]
  /** 语言特性 */
  features: LanguageFeature[]
  /** 配置选项 */
  options?: any
}

/**
 * 语言特性接口
 */
export interface LanguageFeature {
  /** 特性类型 */
  type: string
  /** 特性方法 */
  method: string
  /** 特性选项 */
  options: any
  /** 是否启用 */
  enabled?: boolean
}

/**
 * Monaco Editor 主题配置接口
 */
export interface ThemeConfig {
  /** 主题 ID */
  id: string
  /** 主题名称 */
  name: string
  /** 基础主题 */
  base: 'vs' | 'vs-dark' | 'hc-black'
  /** 是否继承基础主题 */
  inherit: boolean
  /** 语法高亮规则 */
  rules: Monaco.editor.ITokenThemeRule[]
  /** 主题颜色 */
  colors: Record<string, string>
  /** 自定义样式 */
  customStyles?: Record<string, string>
}

/**
 * Monaco Editor 性能配置接口
 */
export interface PerformanceConfig {
  /** 是否启用性能监控 */
  enabled: boolean
  /** 性能阈值 (毫秒) */
  threshold: number
  /** 监控间隔 (毫秒) */
  interval: number
  /** 是否启用内存监控 */
  memoryMonitoring: boolean
  /** 是否启用 Web Worker 监控 */
  workerMonitoring: boolean
  /** 是否启用编辑器监控 */
  editorMonitoring: boolean
  /** 自定义监控指标 */
  customMetrics?: Record<string, {
    name: string
    description: string
    unit: string
    threshold?: number
  }>
}

/**
 * Monaco Editor Web Worker 配置接口
 */
export interface WorkerConfig {
  /** 是否启用 Web Worker */
  enabled: boolean
  /** Web Worker 路径 */
  workerPath?: string
  /** Worker 数量 */
  workerCount?: number
  /** Worker 超时时间 */
  timeout?: number
  /** Worker 重试次数 */
  retryCount?: number
  /** 自定义 Worker 处理器 */
  customHandlers?: Record<string, (data: any) => any>
}

/**
 * Monaco Editor 工具类接口
 */
export interface EditorUtils {
  /** 创建编辑器实例 */
  createEditor: (
    container: HTMLElement,
    options: ExtendedMonacoConfig,
    eventHandlers?: ExtendedEditorEventHandlers
  ) => Promise<ExtendedEditorInstance>
  /** 获取编辑器实例 */
  getEditor: (id: string) => ExtendedEditorInstance | undefined
  /** 获取所有编辑器实例 */
  getAllEditors: () => ExtendedEditorInstance[]
  /** 销毁编辑器实例 */
  destroyEditor: (id: string) => boolean
  /** 销毁所有编辑器实例 */
  destroyAllEditors: () => void
  /** 设置编辑器内容 */
  setContent: (id: string, content: string) => boolean
  /** 获取编辑器内容 */
  getContent: (id: string) => string | undefined
  /** 设置编辑器语言 */
  setLanguage: (id: string, language: string) => boolean
  /** 设置编辑器主题 */
  setTheme: (id: string, theme: string) => boolean
  /** 设置编辑器只读状态 */
  setReadOnly: (id: string, readOnly: boolean) => boolean
  /** 获取编辑器选中文本 */
  getSelectedText: (id: string) => string | undefined
  /** 获取编辑器选中范围 */
  getSelection: (id: string) => Monaco.ISelection | undefined
  /** 获取编辑器光标位置 */
  getCursorPosition: (id: string) => Monaco.IPosition | undefined
  /** 设置编辑器光标位置 */
  setCursorPosition: (id: string, position: Monaco.IPosition) => boolean
  /** 获取编辑器滚动位置 */
  getScrollPosition: (id: string) => { scrollTop: number; scrollLeft: number } | undefined
  /** 设置编辑器滚动位置 */
  setScrollPosition: (id: string, scrollTop: number, scrollLeft: number) => boolean
  /** 获取编辑器统计信息 */
  getEditorStats: (id: string) => {
    lineCount: number
    columnCount: number
    selectedLength: number
    totalLength: number
    readOnly: boolean
    language: string
    theme: string
  } | undefined
  /** 获取 Web Worker 状态 */
  getWorkerStatus: () => {
    enabled: boolean
    supported: boolean
    activeWorkers: number
  }
  /** 格式化代码 */
  formatCode: (id: string) => Promise<boolean>
  /** 查找下一个 */
  findNext: (id: string, searchTerm: string) => boolean
  /** 查找上一个 */
  findPrevious: (id: string, searchTerm: string) => boolean
  /** 替换文本 */
  replace: (id: string, searchText: string, replaceText: string) => boolean
  /** 跳转到指定行 */
  goToLine: (id: string, lineNumber: number, column?: number) => boolean
  /** 折叠所有代码 */
  foldAll: (id: string) => boolean
  /** 展开所有代码 */
  unfoldAll: (id: string) => boolean
  /** 获取编辑器实例统计 */
  getInstanceStats: () => {
    totalInstances: number
    activeInstances: number
    languages: Record<string, number>
    themes: Record<string, number>
    averageAge: number
  }
}

/**
 * Monaco Editor 全局配置接口
 */
export interface GlobalMonacoConfig {
  /** 默认主题 */
  defaultTheme?: string
  /** 默认语言 */
  defaultLanguage?: string
  /** 是否启用 Web Worker */
  enableWorker?: boolean
  /** 是否启用性能监控 */
  enablePerformanceMonitoring?: boolean
  /** 最大文件大小限制 */
  maxFileSize?: number
  /** 超时时间 */
  timeout?: number
  /** 自定义语言配置 */
  customLanguages?: Record<string, LanguageConfig>
  /** 自定义主题配置 */
  customThemes?: Record<string, ThemeConfig>
  /** 性能配置 */
  performanceConfig?: PerformanceConfig
  /** Web Worker 配置 */
  workerConfig?: WorkerConfig
}

/**
 * Monaco Editor 初始化选项接口
 */
export interface MonacoInitOptions {
  /** 语言包 URL */
  vs?: string
  /** 语言配置 */
  locale?: string
  /** 是否启用 AMD */
  getWorkerUrl?: (moduleId: string, label: string) => string
  /** 是否启用 Web Worker */
  getWorker?: (moduleId: string, label: string) => Worker
}

/**
 * Monaco Editor 错误类型
 */
export enum MonacoErrorType {
  INITIALIZATION_ERROR = 'INITIALIZATION_ERROR',
  WORKER_ERROR = 'WORKER_ERROR',
  CONFIGURATION_ERROR = 'CONFIGURATION_ERROR',
  PERFORMANCE_ERROR = 'PERFORMANCE_ERROR',
  LANGUAGE_ERROR = 'LANGUAGE_ERROR',
  THEME_ERROR = 'THEME_ERROR',
  UNKNOWN_ERROR = 'UNKNOWN_ERROR'
}

/**
 * Monaco Editor 错误接口
 */
export interface MonacoError extends Error {
  /** 错误类型 */
  type: MonacoErrorType
  /** 错误代码 */
  code?: string
  /** 错误详情 */
  details?: any
  /** 时间戳 */
  timestamp: Date
  /** 编辑器实例 ID */
  editorId?: string
}

/**
 * Monaco Editor 事件类型
 */
export enum MonacoEventType {
  EDITOR_CREATED = 'EDITOR_CREATED',
  EDITOR_DESTROYED = 'EDITOR_DESTROYED',
  CONTENT_CHANGED = 'CONTENT_CHANGED',
  CURSOR_CHANGED = 'CURSOR_CHANGED',
  SELECTION_CHANGED = 'SELECTION_CHANGED',
  FOCUS_CHANGED = 'FOCUS_CHANGED',
  SIZE_CHANGED = 'SIZE_CHANGED',
  SCROLL_CHANGED = 'SCROLL_CHANGED',
  OPTIONS_CHANGED = 'OPTIONS_CHANGED',
  THEME_CHANGED = 'THEME_CHANGED',
  LANGUAGE_CHANGED = 'LANGUAGE_CHANGED',
  ERROR_OCCURRED = 'ERROR_OCCURRED',
  PERFORMANCE_WARNING = 'PERFORMANCE_WARNING'
}

/**
 * Monaco Editor 事件数据接口
 */
export interface MonacoEventData {
  /** 事件类型 */
  type: MonacoEventType
  /** 事件数据 */
  data: any
  /** 编辑器实例 ID */
  editorId?: string
  /** 时间戳 */
  timestamp: Date
  /** 事件来源 */
  source?: string
}

/**
 * Monaco Editor 插件接口
 */
export interface MonacoPlugin {
  /** 插件名称 */
  name: string
  /** 插件版本 */
  version: string
  /** 插件描述 */
  description?: string
  /** 初始化函数 */
  initialize: (monaco: typeof Monaco) => Promise<void> | void
  /** 销毁函数 */
  destroy?: () => Promise<void> | void
  /** 配置选项 */
  options?: any
}

/**
 * Monaco Editor 自定义命令接口
 */
export interface MonacoCommand {
  /** 命令 ID */
  id: string
  /** 命令名称 */
  name: string
  /** 命令描述 */
  description?: string
  /** 命令处理函数 */
  handler: (editor: Monaco.editor.IStandaloneCodeEditor) => void
  /** 键盘快捷键 */
  keybindings?: Monaco.editor.IKeybindingOptions[]
  /** 上下文菜单 */
  contextMenu?: boolean
  /** 是否启用 */
  enabled?: boolean
}

/**
 * Monaco Editor 扩展类型
 */
export type MonacoEditorExtension = {
  /** 注册自定义命令 */
  registerCommand: (command: MonacoCommand) => void
  /** 注册自定义语言 */
  registerLanguage: (config: LanguageConfig) => void
  /** 注册自定义主题 */
  registerTheme: (config: ThemeConfig) => void
  /** 注册插件 */
  registerPlugin: (plugin: MonacoPlugin) => Promise<void>
  /** 获取所有命令 */
  getCommands: () => MonacoCommand[]
  /** 获取所有语言 */
  getLanguages: () => LanguageConfig[]
  /** 获取所有主题 */
  getThemes: () => ThemeConfig[]
  /** 获取所有插件 */
  getPlugins: () => MonacoPlugin[]
}

// 扩展 Monaco Editor 类型
declare module 'monaco-editor' {
  interface editor {
    /** 创建扩展编辑器 */
    createExtendedEditor: (
      container: HTMLElement,
      options: ExtendedMonacoConfig
    ) => ExtendedEditorInstance
    /** 获取编辑器工具 */
    getEditorUtils: () => EditorUtils
    /** 获取扩展功能 */
    getExtensions: () => MonacoEditorExtension
  }
}

// 导出所有类型
export {
  Monaco,
  type Monaco as MonacoNamespace,
  type editor as MonacoEditor,
  type languages as MonacoLanguages,
  type IStandaloneEditorConstructionOptions as MonacoEditorOptions,
  type IStandaloneCodeEditor as MonacoCodeEditor
} from 'monaco-editor'