import { initializeMonaco } from '@/config/monaco-worker-config'
import { getMonacoLanguage, getEditorConfig } from '@/config/monaco-config'
import { EditorUtils } from '@/utils/editor-utils'

/**
 * Monaco Editor 全局初始化脚本
 * 在应用启动时初始化 Monaco Editor 和相关配置
 */

/**
 * 全局 Monaco Editor 配置
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
  customLanguages?: Record<string, any>
  /** 自定义主题配置 */
  customThemes?: Record<string, any>
}

/**
 * 默认全局配置
 */
const defaultGlobalConfig: GlobalMonacoConfig = {
  defaultTheme: 'vs-dark',
  defaultLanguage: 'javascript',
  enableWorker: true,
  enablePerformanceMonitoring: false,
  maxFileSize: 10 * 1024 * 1024, // 10MB
  timeout: 30000, // 30秒
  customLanguages: {},
  customThemes: {}
}

/**
 * Monaco Editor 全局状态
 */
class MonacoEditorGlobal {
  private static instance: MonacoEditorGlobal
  private initialized = false
  private initializing = false
  private config: GlobalMonacoConfig
  private monacoInstance: any = null
  private initializationPromise: Promise<any> | null = null

  private constructor(config: GlobalMonacoConfig = {}) {
    this.config = { ...defaultGlobalConfig, ...config }
  }

  static getInstance(config?: GlobalMonacoConfig): MonacoEditorGlobal {
    if (!MonacoEditorGlobal.instance) {
      MonacoEditorGlobal.instance = new MonacoEditorGlobal(config)
    }
    return MonacoEditorGlobal.instance
  }

  /**
   * 初始化 Monaco Editor
   */
  async initialize(): Promise<any> {
    if (this.initialized) {
      return this.monacoInstance
    }

    if (this.initializing) {
      return this.initializationPromise
    }

    this.initializing = true
    this.initializationPromise = this.doInitialize()
    
    try {
      this.monacoInstance = await this.initializationPromise
      this.initialized = true
      this.initializing = false
      return this.monacoInstance
    } catch (error) {
      this.initializing = false
      throw error
    }
  }

  /**
   * 执行初始化
   */
  private async doInitialize(): Promise<any> {
    console.log('Initializing Monaco Editor...')

    try {
      // 初始化 Monaco Editor 和 Web Worker
      const monaco = await initializeMonaco({
        enabled: this.config.enableWorker,
        enablePerformanceMonitoring: this.config.enablePerformanceMonitoring,
        maxFileSize: this.config.maxFileSize,
        timeout: this.config.timeout
      })

      // 注册自定义语言
      this.registerCustomLanguages(monaco)

      // 注册自定义主题
      this.registerCustomThemes(monaco)

      // 配置全局编辑器选项
      this.configureGlobalOptions(monaco)

      // 设置性能监控
      if (this.config.enablePerformanceMonitoring) {
        this.setupPerformanceMonitoring(monaco)
      }

      // 注册全局快捷键
      this.registerGlobalShortcuts(monaco)

      console.log('Monaco Editor initialized successfully')
      return monaco

    } catch (error) {
      console.error('Failed to initialize Monaco Editor:', error)
      throw error
    }
  }

  /**
   * 注册自定义语言
   */
  private registerCustomLanguages(monaco: any): void {
    if (!this.config.customLanguages) return

    Object.entries(this.config.customLanguages).forEach(([languageId, config]) => {
      try {
        monaco.languages.register({
          id: languageId,
          extensions: config.extensions || [],
          aliases: config.aliases || [languageId],
          mimetypes: config.mimetypes || []
        })

        // 注册语言特性
        if (config.features) {
          config.features.forEach((feature: any) => {
            if (monaco.languages[feature.type]?.[feature.method]) {
              monaco.languages[feature.type][feature.method](feature.options)
            }
          })
        }

        console.log(`Custom language '${languageId}' registered successfully`)

      } catch (error) {
        console.error(`Failed to register custom language '${languageId}':`, error)
      }
    })
  }

  /**
   * 注册自定义主题
   */
  private registerCustomThemes(monaco: any): void {
    if (!this.config.customThemes) return

    Object.entries(this.config.customThemes).forEach(([themeId, themeConfig]) => {
      try {
        monaco.editor.defineTheme(themeId, themeConfig)
        console.log(`Custom theme '${themeId}' registered successfully`)
      } catch (error) {
        console.error(`Failed to register custom theme '${themeId}':`, error)
      }
    })
  }

  /**
   * 配置全局编辑器选项
   */
  private configureGlobalOptions(monaco: any): void {
    // 设置默认主题
    monaco.editor.setTheme(this.config.defaultTheme || 'vs-dark')

    // 配置全局编辑器选项
    monaco.editor.setOptions({
      minimap: {
        enabled: true,
        side: 'right',
        showSlider: 'mouseover'
      },
      scrollBeyondLastLine: false,
      automaticLayout: true,
      fontSize: 14,
      lineHeight: 1.6,
      fontFamily: 'Consolas, Monaco, "Courier New", monospace',
      wordWrap: 'on',
      lineNumbers: 'on',
      renderWhitespace: 'selection',
      renderControlCharacters: false,
      renderIndentGuides: true,
      selectOnLineNumbers: true,
      matchBrackets: 'always',
      autoIndent: 'advanced',
      formatOnPaste: true,
      formatOnType: false,
      tabSize: 2,
      insertSpaces: true,
      detectIndentation: true,
      trimAutoWhitespace: true,
      suggestOnTriggerCharacters: true,
      quickSuggestions: {
        other: true,
        comments: false,
        strings: false
      },
      parameterHints: {
        enabled: true
      },
      wordBasedSuggestions: true,
      semanticHighlighting: true,
      codeLens: false,
      contextmenu: true,
      mouseWheelZoom: false,
      multiCursorModifier: 'ctrlCmd',
      accessibilitySupport: 'auto'
    })

    // 配置语言特定选项
    this.configureLanguageSpecificOptions(monaco)
  }

  /**
   * 配置语言特定选项
   */
  private configureLanguageSpecificOptions(monaco: any): void {
    // JavaScript/TypeScript 配置
    if (monaco.languages.typescript) {
      monaco.languages.typescript.javascriptDefaults.setCompilerOptions({
        target: monaco.languages.typescript.ScriptTarget.ES2020,
        allowNonTsExtensions: true,
        moduleResolution: monaco.languages.typescript.ModuleResolutionKind.NodeJs,
        module: monaco.languages.typescript.ModuleKind.ESNext,
        noEmit: true,
        allowJs: true,
        jsx: monaco.languages.typescript.JsxEmit.React,
        reactNamespace: 'React'
      })

      monaco.languages.typescript.typescriptDefaults.setCompilerOptions({
        target: monaco.languages.typescript.ScriptTarget.ES2020,
        allowNonTsExtensions: true,
        moduleResolution: monaco.languages.typescript.ModuleResolutionKind.NodeJs,
        module: monaco.languages.typescript.ModuleKind.ESNext,
        noEmit: true,
        allowJs: false,
        jsx: monaco.languages.typescript.JsxEmit.React,
        reactNamespace: 'React'
      })
    }

    // JSON 配置
    if (monaco.languages.json) {
      monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
        validate: true,
        allowComments: true,
        schemas: []
      })
    }

    // HTML 配置
    if (monaco.languages.html) {
      monaco.languages.html.htmlDefaults.setDiagnosticsOptions({
        validate: true,
        validateScripts: true,
        validateStyles: true
      })
    }

    // CSS 配置
    if (monaco.languages.css) {
      monaco.languages.css.cssDefaults.setDiagnosticsOptions({
        validate: true,
        lint: {
          emptyRules: 'warning',
          importStatement: 'warning',
          boxModel: 'warning',
          universalSelector: 'warning',
          zeroUnits: 'warning',
          duplicateProperties: 'warning'
        }
      })
    }
  }

  /**
   * 设置性能监控
   */
  private setupPerformanceMonitoring(monaco: any): void {
    let lastUpdateTime = performance.now()
    let updateCount = 0

    monaco.editor.onDidCreateEditor((editor: any) => {
      const editorId = `editor-${Date.now()}`
      
      // 监控编辑器性能
      editor.onDidChangeModelContent(() => {
        updateCount++
        const now = performance.now()
        
        if (now - lastUpdateTime > 5000) { // 每5秒统计一次
          const updatesPerSecond = updateCount / ((now - lastUpdateTime) / 1000)
          
          if (updatesPerSecond > 10) {
            console.warn(`Editor ${editorId} has high update frequency: ${updatesPerSecond.toFixed(2)} updates/sec`)
          }
          
          lastUpdateTime = now
          updateCount = 0
        }
      })

      // 监控内存使用
      setInterval(() => {
        const memoryUsage = (performance as any).memory
        if (memoryUsage) {
          const usedHeapSize = memoryUsage.usedJSHeapSize / 1024 / 1024 // MB
          const totalHeapSize = memoryUsage.totalJSHeapSize / 1024 / 1024 // MB
          
          if (usedHeapSize > 100) { // 超过100MB时发出警告
            console.warn(`High memory usage detected: ${usedHeapSize.toFixed(2)}MB / ${totalHeapSize.toFixed(2)}MB`)
          }
        }
      }, 10000) // 每10秒检查一次
    })
  }

  /**
   * 注册全局快捷键
   */
  private registerGlobalShortcuts(monaco: any): void {
    // 添加全局快捷键处理
    document.addEventListener('keydown', (event) => {
      // Ctrl/Cmd + S: 保存
      if ((event.ctrlKey || event.metaKey) && event.key === 's') {
        event.preventDefault()
        this.handleGlobalSave()
      }

      // Ctrl/Cmd + F: 查找
      if ((event.ctrlKey || event.metaKey) && event.key === 'f') {
        event.preventDefault()
        this.handleGlobalFind()
      }

      // Ctrl/Cmd + Shift + F: 替换
      if ((event.ctrlKey || event.metaKey) && event.shiftKey && event.key === 'f') {
        event.preventDefault()
        this.handleGlobalReplace()
      }

      // F11: 全屏
      if (event.key === 'F11') {
        event.preventDefault()
        this.handleFullscreen()
      }
    })
  }

  /**
   * 处理全局保存
   */
  private handleGlobalSave(): void {
    console.log('Global save action triggered')
    // 触发全局保存事件
    window.dispatchEvent(new CustomEvent('monaco-global-save'))
  }

  /**
   * 处理全局查找
   */
  private handleGlobalFind(): void {
    console.log('Global find action triggered')
    // 触发全局查找事件
    window.dispatchEvent(new CustomEvent('monaco-global-find'))
  }

  /**
   * 处理全局替换
   */
  private handleGlobalReplace(): void {
    console.log('Global replace action triggered')
    // 触发全局替换事件
    window.dispatchEvent(new CustomEvent('monaco-global-replace'))
  }

  /**
   * 处理全屏
   */
  private handleFullscreen(): void {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen()
    } else {
      document.exitFullscreen()
    }
  }

  /**
   * 获取 Monaco 实例
   */
  getMonacoInstance(): any {
    return this.monacoInstance
  }

  /**
   * 检查是否已初始化
   */
  isInitialized(): boolean {
    return this.initialized
  }

  /**
   * 获取配置
   */
  getConfig(): GlobalMonacoConfig {
    return { ...this.config }
  }

  /**
   * 更新配置
   */
  updateConfig(newConfig: Partial<GlobalMonacoConfig>): void {
    this.config = { ...this.config, ...newConfig }
    
    // 如果已初始化，应用新配置
    if (this.initialized && this.monacoInstance) {
      this.applyNewConfig(this.monacoInstance)
    }
  }

  /**
   * 应用新配置
   */
  private applyNewConfig(monaco: any): void {
    // 更新主题
    if (this.config.defaultTheme) {
      monaco.editor.setTheme(this.config.defaultTheme)
    }

    // 更新全局选项
    monaco.editor.setOptions({
      fontSize: 14,
      lineHeight: 1.6,
      fontFamily: 'Consolas, Monaco, "Courier New", monospace'
    })
  }

  /**
   * 销毁实例
   */
  destroy(): void {
    if (this.initialized) {
      // 清理所有编辑器实例
      EditorUtils.destroyAllEditors()
      
      // 清理 Web Worker
      if (typeof Worker !== 'undefined') {
        const workers = (window as any).monacoWorkers || []
        workers.forEach((worker: Worker) => {
          worker.terminate()
        })
        delete (window as any).monacoWorkers
      }

      this.initialized = false
      this.initializing = false
      this.monacoInstance = null
      this.initializationPromise = null
    }
  }
}

/**
 * 全局 Monaco Editor 管理器
 */
export const monacoGlobal = MonacoEditorGlobal.getInstance()

/**
 * 初始化 Monaco Editor 的便捷函数
 */
export async function initializeMonacoEditor(config?: GlobalMonacoConfig): Promise<any> {
  const instance = MonacoEditorGlobal.getInstance(config)
  return await instance.initialize()
}

/**
 * 获取 Monaco Editor 实例
 */
export function getMonacoInstance(): any {
  return monacoGlobal.getMonacoInstance()
}

/**
 * 检查 Monaco Editor 是否已初始化
 */
export function isMonacoInitialized(): boolean {
  return monacoGlobal.isInitialized()
}

/**
 * 获取 Monaco Editor 配置
 */
export function getMonacoConfig(): GlobalMonacoConfig {
  return monacoGlobal.getConfig()
}

/**
 * 更新 Monaco Editor 配置
 */
export function updateMonacoConfig(newConfig: Partial<GlobalMonacoConfig>): void {
  monacoGlobal.updateConfig(newConfig)
}

/**
 * 销毁 Monaco Editor
 */
export function destroyMonacoEditor(): void {
  monacoGlobal.destroy()
}

// 默认导出
export default monacoGlobal