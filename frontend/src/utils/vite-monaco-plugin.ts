import type { Plugin } from 'vite'
import { resolve } from 'path'

/**
 * Monaco Editor Vite 插件
 * 专门处理 Monaco Editor 的 Web Worker 配置和优化
 */

interface MonacoEditorPluginOptions {
  /** 是否启用 Web Worker */
  enableWorker?: boolean
  /** Monaco Editor 路径 */
  publicPath?: string
  /** 是否启用代码分割 */
  enableCodeSplitting?: boolean
  /** 是否启用语言包压缩 */
  enableLanguagePackCompression?: boolean
  /** 自定义 Worker 路径 */
  workerPaths?: Record<string, string>
  /** 需要包含的语言 */
  languages?: string[]
}

/**
 * 默认插件选项
 */
const defaultOptions: Required<MonacoEditorPluginOptions> = {
  enableWorker: true,
  publicPath: '/monaco-editor',
  enableCodeSplitting: true,
  enableLanguagePackCompression: true,
  workerPaths: {},
  languages: [
    'javascript',
    'typescript',
    'html',
    'css',
    'json',
    'python',
    'java',
    'csharp',
    'cpp',
    'sql',
    'markdown'
  ]
}

/**
 * Monaco Editor Vite 插件
 */
export function monacoEditorPlugin(options: MonacoEditorPluginOptions = {}): Plugin {
  const pluginOptions = { ...defaultOptions, ...options }
  
  return {
    name: 'vite-plugin-monaco-editor',
    enforce: 'pre',
    
    configResolved(config) {
      // 配置构建优化
      if (config.build.rollupOptions) {
        if (!config.build.rollupOptions.output) {
          config.build.rollupOptions.output = {}
        }
        
        const output = config.build.rollupOptions.output as any
        
        // 配置 Monaco Editor 相关的代码分割
        if (pluginOptions.enableCodeSplitting) {
          if (!output.manualChunks) {
            output.manualChunks = {}
          }
          
          // Monaco Editor 核心库
          output.manualChunks['monaco-editor-core'] = [
            'monaco-editor',
            '@monaco-editor/loader'
          ]
          
          // 语言支持
          pluginOptions.languages.forEach(lang => {
            output.manualChunks[`monaco-language-${lang}`] = [
              `monaco-editor/esm/vs/language/${lang}/${lang}`
            ]
          })
          
          // 基础功能
          output.manualChunks['monaco-editor-basic'] = [
            'monaco-editor/esm/vs/editor/editor.main',
            'monaco-editor/esm/vs/editor/editor.worker'
          ]
        }
      }
    },
    
    resolveId(id) {
      // 处理 Monaco Editor 的虚拟模块
      if (id.startsWith('monaco-editor:')) {
        return id
      }
      return null
    },
    
    load(id) {
      // 处理 Monaco Editor 虚拟模块
      if (id.startsWith('monaco-editor:')) {
        const [_, type, lang] = id.split(':')
        
        if (type === 'worker') {
          return generateWorkerCode(lang, pluginOptions)
        }
        
        if (type === 'config') {
          return generateConfigCode(pluginOptions)
        }
      }
      return null
    },
    
    transform(code, id) {
      // 处理 Monaco Editor 相关的模块导入
      if (id.includes('monaco-editor')) {
        // 替换 Worker 路径
        if (pluginOptions.enableWorker) {
          code = code.replace(
            /new Worker\([^)]+\)/g,
            (match) => {
              return match.replace(
                /".*?"/g,
                (path) => `"${pluginOptions.publicPath}/workers/editor.worker.js"`
              )
            }
          )
        }
        
        // 优化语言包加载
        if (pluginOptions.enableLanguagePackCompression) {
          code = code.replace(
            /import\('monaco-editor.*?\)/g,
            (match) => {
              return match.replace(
                'monaco-editor',
                `${pluginOptions.publicPath}/languages`
              )
            }
          )
        }
      }
      
      return {
        code,
        map: null
      }
    },
    
    generateBundle(options, bundle) {
      // 生成 Monaco Editor 相关的资源文件
      if (pluginOptions.enableWorker) {
        // 生成 Web Worker 文件
        this.emitFile({
          type: 'asset',
          fileName: 'monaco-editor/workers/editor.worker.js',
          source: generateWorkerFile('editor', pluginOptions)
        })
        
        // 生成语言特定的 Worker 文件
        pluginOptions.languages.forEach(lang => {
          this.emitFile({
            type: 'asset',
            fileName: `monaco-editor/workers/${lang}.worker.js`,
            source: generateWorkerFile(lang, pluginOptions)
          })
        })
      }
      
      // 生成配置文件
      this.emitFile({
        type: 'asset',
        fileName: 'monaco-editor/config.json',
        source: JSON.stringify({
          enableWorker: pluginOptions.enableWorker,
          publicPath: pluginOptions.publicPath,
          languages: pluginOptions.languages,
          enableCodeSplitting: pluginOptions.enableCodeSplitting
        }, null, 2)
      })
    },
    
    configureServer(server) {
      // 开发服务器配置
      server.middlewares.use((req, res, next) => {
        if (req.url?.startsWith(pluginOptions.publicPath)) {
          // 处理 Monaco Editor 相关的请求
          if (req.url.endsWith('/config.json')) {
            res.setHeader('Content-Type', 'application/json')
            res.end(JSON.stringify({
              enableWorker: pluginOptions.enableWorker,
              publicPath: pluginOptions.publicPath,
              languages: pluginOptions.languages
            }))
            return
          }
          
          // 处理 Worker 请求
          if (req.url.includes('.worker.js')) {
            const lang = req.url.match(/\/(\w+)\.worker\.js$/)?.[1]
            if (lang) {
              res.setHeader('Content-Type', 'application/javascript')
              res.end(generateWorkerFile(lang, pluginOptions))
              return
            }
          }
        }
        next()
      })
    }
  }
}

/**
 * 生成 Worker 代码
 */
function generateWorkerCode(lang: string, options: Required<MonacoEditorPluginOptions>): string {
  const workerPath = options.workerPaths[lang] || 
    `${options.publicPath}/workers/${lang}.worker.js`
  
  return `
// Monaco Editor Worker: ${lang}
// Generated by vite-plugin-monaco-editor

self.MonacoEnvironment = {
  getWorkerUrl: function(moduleId, label) {
    if (label === '${lang}') {
      return '${workerPath}'
    }
    return '${options.publicPath}/workers/editor.worker.js'
  },
  getWorker: function(moduleId, label) {
    if (label === '${lang}') {
      return new Worker('${workerPath}')
    }
    return new Worker('${options.publicPath}/workers/editor.worker.js')
  }
}

// 导出 Worker 相关函数
export function getWorkerUrl(moduleId: string, label: string) {
  return self.MonacoEnvironment.getWorkerUrl(moduleId, label)
}

export function getWorker(moduleId: string, label: string) {
  return self.MonacoEnvironment.getWorker(moduleId, label)
}
`
}

/**
 * 生成配置代码
 */
function generateConfigCode(options: Required<MonacoEditorPluginOptions>): string {
  return `
// Monaco Editor Configuration
// Generated by vite-plugin-monaco-editor

export const monacoConfig = {
  enableWorker: ${options.enableWorker},
  publicPath: '${options.publicPath}',
  languages: ${JSON.stringify(options.languages)},
  enableCodeSplitting: ${options.enableCodeSplitting},
  enableLanguagePackCompression: ${options.enableLanguagePackCompression}
}

export function getWorkerPath(language: string) {
  return ${JSON.stringify(options.workerPaths)}[language] || 
    '${options.publicPath}/workers/' + language + '.worker.js'
}

export function getLanguageConfig(language: string) {
  const configs = ${JSON.stringify(getLanguageConfigs(options.languages))}
  return configs[language] || null
}
`
}

/**
 * 生成 Worker 文件
 */
function generateWorkerFile(lang: string, options: Required<MonacoEditorPluginOptions>): string {
  const languageConfigs = getLanguageConfigs(options.languages)
  const config = languageConfigs[lang] || {}
  
  return `
// Monaco Editor Worker: ${lang}
// Generated by vite-plugin-monaco-editor

importScripts('${options.publicPath}/monaco-editor/min/vs/base/worker/workerMain.js')

// 语言特定的配置
const languageConfig = ${JSON.stringify(config)}

// 初始化语言特性
if (languageConfig.features) {
  languageConfig.features.forEach(feature => {
    self.monaco.languages[feature.type]?.[feature.method]?.(feature.options)
  })
}

// 导出语言配置
export const config = languageConfig

// 导出语言 ID
export const languageId = '${lang}'

// 错误处理
self.addEventListener('error', (event) => {
  console.error('Monaco Editor Worker Error (${lang}):', event.error)
  event.preventDefault()
})

// 消息处理
self.addEventListener('message', (event) => {
  const { type, data } = event.data
  
  try {
    switch (type) {
      case 'validate':
        // 处理代码验证
        const result = validateCode(data.code, '${lang}')
        self.postMessage({
          type: 'validate-result',
          id: data.id,
          result
        })
        break
        
      case 'format':
        // 处理代码格式化
        const formatted = formatCode(data.code, '${lang}', data.options)
        self.postMessage({
          type: 'format-result',
          id: data.id,
          result: formatted
        })
        break
        
      case 'complete':
        // 处理代码补全
        const completions = getCompletions(data.code, '${lang}', data.position)
        self.postMessage({
          type: 'complete-result',
          id: data.id,
          result: completions
        })
        break
        
      default:
        console.warn('Unknown message type:', type)
    }
  } catch (error) {
    self.postMessage({
      type: 'error',
      id: data.id,
      error: error.message
    })
  }
})

// 代码验证函数
function validateCode(code, language) {
  // 实现代码验证逻辑
  return {
    valid: true,
    errors: [],
    warnings: []
  }
}

// 代码格式化函数
function formatCode(code, language, options) {
  // 实现代码格式化逻辑
  return code
}

// 代码补全函数
function getCompletions(code, language, position) {
  // 实现代码补全逻辑
  return []
}
`
}

/**
 * 获取语言配置
 */
function getLanguageConfigs(languages: string[]): Record<string, any> {
  const configs: Record<string, any> = {
    javascript: {
      features: [
        {
          type: 'typescript',
          method: 'javascriptDefaults.setDiagnosticsOptions',
          options: {
            noSemanticValidation: false,
            noSyntaxValidation: false
          }
        }
      ]
    },
    typescript: {
      features: [
        {
          type: 'typescript',
          method: 'typescriptDefaults.setDiagnosticsOptions',
          options: {
            noSemanticValidation: false,
            noSyntaxValidation: false
          }
        }
      ]
    },
    html: {
      features: [
        {
          type: 'html',
          method: 'htmlDefaults.setDiagnosticsOptions',
          options: {
            validate: true,
            validateScripts: true,
            validateStyles: true
          }
        }
      ]
    },
    css: {
      features: [
        {
          type: 'css',
          method: 'cssDefaults.setDiagnosticsOptions',
          options: {
            validate: true,
            lint: {
              emptyRules: 'warning'
            }
          }
        }
      ]
    },
    json: {
      features: [
        {
          type: 'json',
          method: 'jsonDefaults.setDiagnosticsOptions',
          options: {
            validate: true,
            allowComments: true
          }
        }
      ]
    },
    python: {
      features: [
        {
          type: 'python',
          method: 'register',
          options: {
            validate: true,
            format: true
          }
        }
      ]
    },
    sql: {
      features: [
        {
          type: 'sql',
          method: 'register',
          options: {
            validate: true,
            suggest: true
          }
        }
      ]
    },
    markdown: {
      features: [
        {
          type: 'markdown',
          method: 'register',
          options: {
            validate: true,
            preview: true
          }
        }
      ]
    }
  }
  
  // 返回请求的语言配置
  const result: Record<string, any> = {}
  languages.forEach(lang => {
    if (configs[lang]) {
      result[lang] = configs[lang]
    }
  })
  
  return result
}

// 导出插件
export default monacoEditorPlugin