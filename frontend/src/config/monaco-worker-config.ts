import { loader } from '@monaco-editor/react'
import type { Monaco } from '@monaco-editor/loader'

/**
 * Monaco Editor Web Worker 配置
 * 负责配置和管理 Web Worker 的初始化和加载
 */

/**
 * Web Worker 配置接口
 */
export interface WebWorkerConfig {
  /** 是否启用 Web Worker */
  enabled: boolean
  /** Web Worker 路径 */
  workerPath?: string
  /** 是否启用语法检查 */
  enableValidation: boolean
  /** 是否启用代码格式化 */
  enableFormatting: boolean
  /** 是否启用语义高亮 */
  enableSemanticHighlighting: boolean
  /** 是否启用代码补全 */
  enableCompletion: boolean
  /** 是否启用错误检查 */
  enableErrorChecking: boolean
  /** 是否启用性能监控 */
  enablePerformanceMonitoring: boolean
  /** 最大文件大小 (字节) */
  maxFileSize?: number
  /** 超时时间 (毫秒) */
  timeout?: number
}

/**
 * 默认 Web Worker 配置
 */
export const defaultWebWorkerConfig: WebWorkerConfig = {
  enabled: true,
  enableValidation: true,
  enableFormatting: true,
  enableSemanticHighlighting: true,
  enableCompletion: true,
  enableErrorChecking: true,
  enablePerformanceMonitoring: false,
  maxFileSize: 10 * 1024 * 1024, // 10MB
  timeout: 30000 // 30秒
}

/**
 * Monaco Editor Worker 路径配置
 */
export const workerPaths = {
  // 编辑器核心 Worker
  editorWorkerService: () => import('monaco-editor/esm/vs/editor/editor.worker'),
  // JSON Worker
  jsonWorker: () => import('monaco-editor/esm/vs/language/json/json.worker'),
  // CSS Worker
  cssWorker: () => import('monaco-editor/esm/vs/language/css/css.worker'),
  // HTML Worker
  htmlWorker: () => import('monaco-editor/esm/vs/language/html/html.worker'),
  // TypeScript/JavaScript Worker
  tsWorker: () => import('monaco-editor/esm/vs/language/typescript/ts.worker'),
  // Python Worker (如果需要)
  pythonWorker: () => import('monaco-editor/esm/vs/language/python/python.worker'),
  // 通用 Worker
  defaultWorker: () => import('monaco-editor/esm/vs/editor/editor.worker')
}

/**
 * 初始化 Monaco Editor 和 Web Worker
 */
export async function initializeMonaco(config: Partial<WebWorkerConfig> = {}) {
  const workerConfig = { ...defaultWebWorkerConfig, ...config }

  if (!workerConfig.enabled) {
    console.warn('Web Worker is disabled, Monaco Editor will run in main thread')
    return
  }

  try {
    // 配置 Monaco Editor
    loader.config({
      paths: {
        vs: '/monaco-editor/min/vs'
      },
      'vs/nls': {
        availableLanguages: {
          '*': 'zh-cn'
        }
      }
    })

    // 初始化 Monaco Editor
    const monaco = await loader.init()

    // 配置 Web Worker
    configureWebWorkers(monaco, workerConfig)

    // 注册语言支持
    registerLanguageSupport(monaco)

    // 配置性能监控
    if (workerConfig.enablePerformanceMonitoring) {
      configurePerformanceMonitoring(monaco)
    }

    console.log('Monaco Editor initialized successfully with Web Worker support')
    return monaco

  } catch (error) {
    console.error('Failed to initialize Monaco Editor:', error)
    throw error
  }
}

/**
 * 配置 Web Worker
 */
function configureWebWorkers(monaco: Monaco, config: WebWorkerConfig) {
  // 配置 Worker 路径
  window.MonacoEnvironment = {
    getWorker(moduleId: string, label: string) {
      // 根据模块 ID 选择对应的 Worker
      if (moduleId.includes('json')) {
        return new Worker(new URL('monaco-editor/esm/vs/language/json/json.worker', import.meta.url))
      }
      if (moduleId.includes('css')) {
        return new Worker(new URL('monaco-editor/esm/vs/language/css/css.worker', import.meta.url))
      }
      if (moduleId.includes('html')) {
        return new Worker(new URL('monaco-editor/esm/vs/language/html/html.worker', import.meta.url))
      }
      if (moduleId.includes('typescript') || moduleId.includes('javascript')) {
        return new Worker(new URL('monaco-editor/esm/vs/language/typescript/ts.worker', import.meta.url))
      }
      if (moduleId.includes('python')) {
        return new Worker(new URL('monaco-editor/esm/vs/language/python/python.worker', import.meta.url))
      }
      // 默认 Worker
      return new Worker(new URL('monaco-editor/esm/vs/editor/editor.worker', import.meta.url))
    }
  }

  // 配置编辑器选项
  monaco.editor.setTheme('vs-dark')
  
  // 配置语言特性
  if (config.enableValidation) {
    configureValidation(monaco)
  }

  if (config.enableFormatting) {
    configureFormatting(monaco)
  }

  if (config.enableSemanticHighlighting) {
    configureSemanticHighlighting(monaco)
  }

  if (config.enableCompletion) {
    configureCompletion(monaco)
  }

  if (config.enableErrorChecking) {
    configureErrorChecking(monaco)
  }
}

/**
 * 注册语言支持
 */
function registerLanguageSupport(monaco: Monaco) {
  // 注册 JavaScript 支持
  monaco.languages.register({
    id: 'javascript',
    extensions: ['.js', '.jsx', '.mjs'],
    aliases: ['JavaScript', 'javascript', 'js'],
    mimetypes: ['text/javascript']
  })

  // 注册 TypeScript 支持
  monaco.languages.register({
    id: 'typescript',
    extensions: ['.ts', '.tsx'],
    aliases: ['TypeScript', 'typescript', 'ts'],
    mimetypes: ['text/typescript']
  })

  // 注册 Python 支持
  monaco.languages.register({
    id: 'python',
    extensions: ['.py', '.pyw'],
    aliases: ['Python', 'python', 'py'],
    mimetypes: ['text/x-python']
  })

  // 注册 HTML 支持
  monaco.languages.register({
    id: 'html',
    extensions: ['.html', '.htm'],
    aliases: ['HTML', 'html', 'htm'],
    mimetypes: ['text/html']
  })

  // 注册 CSS 支持
  monaco.languages.register({
    id: 'css',
    extensions: ['.css', '.scss', '.less'],
    aliases: ['CSS', 'css', 'scss', 'less'],
    mimetypes: ['text/css']
  })

  // 注册 JSON 支持
  monaco.languages.register({
    id: 'json',
    extensions: ['.json', '.jsonc'],
    aliases: ['JSON', 'json'],
    mimetypes: ['application/json']
  })

  // 注册 SQL 支持
  monaco.languages.register({
    id: 'sql',
    extensions: ['.sql'],
    aliases: ['SQL', 'sql'],
    mimetypes: ['text/x-sql']
  })

  // 注册 Shell 支持
  monaco.languages.register({
    id: 'shell',
    extensions: ['.sh', '.bash', '.zsh'],
    aliases: ['Shell', 'shell', 'bash', 'zsh'],
    mimetypes: ['text/x-shellscript']
  })

  // 注册 Markdown 支持
  monaco.languages.register({
    id: 'markdown',
    extensions: ['.md', '.markdown'],
    aliases: ['Markdown', 'markdown', 'md'],
    mimetypes: ['text/x-markdown']
  })
}

/**
 * 配置验证功能
 */
function configureValidation(monaco: Monaco) {
  // 配置 JavaScript/TypeScript 验证
  monaco.languages.typescript.javascriptDefaults.setDiagnosticsOptions({
    noSemanticValidation: false,
    noSyntaxValidation: false,
    diagnosticCodesToIgnore: []
  })

  monaco.languages.typescript.typescriptDefaults.setDiagnosticsOptions({
    noSemanticValidation: false,
    noSyntaxValidation: false,
    diagnosticCodesToIgnore: []
  })

  // 配置 JSON 验证
  monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
    validate: true,
    allowComments: true,
    schemas: []
  })

  // 配置 CSS 验证
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

/**
 * 配置格式化功能
 */
function configureFormatting(monaco: Monaco) {
  // 配置 JavaScript/TypeScript 格式化
  monaco.languages.typescript.javascriptDefaults.setFormatOnTypeOptions({
    insertSpaceAfterCommaDelimiter: true,
    insertSpaceAfterConstructor: true,
    insertSpaceAfterSemicolonInForStatements: true,
    insertSpaceBeforeAndAfterBinaryOperators: true,
    insertSpaceAfterKeywordsInControlFlowStatements: true,
    insertSpaceAfterFunctionKeywordForAnonymousFunctions: true,
    insertSpaceBeforeFunctionParenthesis: false,
    insertSpaceAfterOpeningAndBeforeClosingNonemptyParenthesis: false,
    insertSpaceAfterOpeningAndBeforeClosingNonemptyBrackets: false,
    insertSpaceAfterOpeningAndBeforeClosingNonemptyBraces: true,
    insertSpaceAfterOpeningAndBeforeClosingTemplateStringBraces: false,
    insertSpaceAfterOpeningAndBeforeClosingEmptyBraces: false,
    insertSpaceBeforeFunctionParenthesis: false,
    placeOpenBraceOnNewLineForControlBlocks: false,
    placeOpenBraceOnNewLineForFunctions: false
  })

  monaco.languages.typescript.typescriptDefaults.setFormatOnTypeOptions({
    insertSpaceAfterCommaDelimiter: true,
    insertSpaceAfterConstructor: true,
    insertSpaceAfterSemicolonInForStatements: true,
    insertSpaceBeforeAndAfterBinaryOperators: true,
    insertSpaceAfterKeywordsInControlFlowStatements: true,
    insertSpaceAfterFunctionKeywordForAnonymousFunctions: true,
    insertSpaceBeforeFunctionParenthesis: false,
    insertSpaceAfterOpeningAndBeforeClosingNonemptyParenthesis: false,
    insertSpaceAfterOpeningAndBeforeClosingNonemptyBrackets: false,
    insertSpaceAfterOpeningAndBeforeClosingNonemptyBraces: true,
    insertSpaceAfterOpeningAndBeforeClosingTemplateStringBraces: false,
    insertSpaceAfterOpeningAndBeforeClosingEmptyBraces: false,
    insertSpaceBeforeFunctionParenthesis: false,
    placeOpenBraceOnNewLineForControlBlocks: false,
    placeOpenBraceOnNewLineForFunctions: false
  })
}

/**
 * 配置语义高亮
 */
function configureSemanticHighlighting(monaco: Monaco) {
  // 启用语义高亮
  monaco.editor.defineTheme('vs-dark', {
    base: 'vs-dark',
    inherit: true,
    rules: [],
    colors: {
      'editor.background': '#1e1e1e',
      'editor.foreground': '#d4d4d4',
      'editor.lineHighlightBackground': '#2d2d2d',
      'editor.selectionBackground': '#264f78',
      'editor.inactiveSelectionBackground': '#3a3d41',
      'editor.wordHighlightBackground': '#4a4a4a',
      'editor.wordHighlightStrongBackground': '#6a6a6a',
      'editor.findMatchBackground': '#515c6a',
      'editor.findMatchHighlightBackground': '#4a4a4a'
    }
  })
}

/**
 * 配置代码补全
 */
function configureCompletion(monaco: Monaco) {
  // 配置 JavaScript/TypeScript 代码补全
  monaco.languages.typescript.javascriptDefaults.setCompilerOptions({
    target: monaco.languages.typescript.ScriptTarget.ES2020,
    allowNonTsExtensions: true,
    moduleResolution: monaco.languages.typescript.ModuleResolutionKind.NodeJs,
    module: monaco.languages.typescript.ModuleKind.ESNext,
    noEmit: true,
    allowJs: true,
    jsx: monaco.languages.typescript.JsxEmit.React,
    reactNamespace: 'React',
    baseUrl: '.',
    paths: {
      '@/*': ['src/*']
    }
  })

  monaco.languages.typescript.typescriptDefaults.setCompilerOptions({
    target: monaco.languages.typescript.ScriptTarget.ES2020,
    allowNonTsExtensions: true,
    moduleResolution: monaco.languages.typescript.ModuleResolutionKind.NodeJs,
    module: monaco.languages.typescript.ModuleKind.ESNext,
    noEmit: true,
    allowJs: false,
    jsx: monaco.languages.typescript.JsxEmit.React,
    reactNamespace: 'React',
    baseUrl: '.',
    paths: {
      '@/*': ['src/*']
    }
  })
}

/**
 * 配置错误检查
 */
function configureErrorChecking(monaco: Monaco) {
  // 配置错误检查规则
  monaco.languages.typescript.javascriptDefaults.setDiagnosticsOptions({
    noSemanticValidation: false,
    noSyntaxValidation: false,
    diagnosticCodesToIgnore: [
      // 忽略一些常见的警告
      '6133', // unused variable
      '6196', // unused parameter
      '6138'  // unused import
    ]
  })

  monaco.languages.typescript.typescriptDefaults.setDiagnosticsOptions({
    noSemanticValidation: false,
    noSyntaxValidation: false,
    diagnosticCodesToIgnore: [
      '6133', // unused variable
      '6196', // unused parameter
      '6138'  // unused import
    ]
  })
}

/**
 * 配置性能监控
 */
function configurePerformanceMonitoring(monaco: Monaco) {
  // 监控编辑器性能
  monaco.editor.onDidCreateEditor((editor) => {
    console.log('Editor created:', editor.getId())
    
    // 监控渲染性能
    editor.onDidChangeModelContent(() => {
      const start = performance.now()
      // 这里可以添加性能监控逻辑
      const end = performance.now()
      if (end - start > 100) {
        console.warn('Editor content change took too long:', end - start, 'ms')
      }
    })
  })
}

/**
 * 检查文件大小限制
 */
export function checkFileSize(content: string, config: WebWorkerConfig): boolean {
  if (!config.maxFileSize) return true
  
  const size = new Blob([content]).size
  return size <= config.maxFileSize
}

/**
 * 获取 Web Worker 状态
 */
export function getWebWorkerStatus(): {
  enabled: boolean
  supported: boolean
  activeWorkers: number
} {
  return {
    enabled: defaultWebWorkerConfig.enabled,
    supported: typeof Worker !== 'undefined',
    activeWorkers: (window as any).monacoWorkers?.length || 0
  }
}

/**
 * 清理 Web Worker
 */
export function cleanupWebWorkers() {
  if (typeof Worker !== 'undefined') {
    // 终止所有 Worker
    const workers = (window as any).monacoWorkers || []
    workers.forEach((worker: Worker) => {
      worker.terminate()
    })
    
    // 清理引用
    delete (window as any).monacoWorkers
  }
}