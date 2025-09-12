import type { Monaco } from '@monaco-editor/loader'

/**
 * Monaco Editor 基础配置
 * 提供编辑器的全局配置选项
 */
export interface MonacoConfig {
  /** 编辑器主题 */
  theme?: string
  /** 编程语言 */
  language?: string
  /** 是否只读 */
  readOnly?: boolean
  /** 是否显示行号 */
  lineNumbers?: 'on' | 'off' | 'relative' | 'interval'
  /** 是否启用代码折叠 */
  folding?: boolean
  /** 是否启用小地图 */
  minimap?: {
    enabled?: boolean
    side?: 'right' | 'left'
    showSlider?: 'always' | 'mouseover'
  }
  /** 是否启用自动滚动 */
  automaticLayout?: boolean
  /** 是否启用多光标编辑 */
  multiCursorModifier?: 'ctrlCmd' | 'alt'
  /** 字体大小 */
  fontSize?: number
  /** 字体系列 */
  fontFamily?: string
  /** 行高 */
  lineHeight?: number
  /** 是否启用单词建议 */
  wordBasedSuggestions?: boolean
  /** 是否启用参数提示 */
  parameterHints?: {
    enabled?: boolean
    cycle?: boolean
  }
  /** 是否启用快速建议 */
  quickSuggestions?: {
    other?: boolean
    comments?: boolean
    strings?: boolean
  }
  /** 是否启用代码格式化 */
  formatOnPaste?: boolean
  formatOnType?: boolean
  /** 是否显示滚动条 */
  scrollbar?: {
    vertical?: 'auto' | 'visible' | 'hidden'
    horizontal?: 'auto' | 'visible' | 'hidden'
    verticalScrollbarSize?: number
    horizontalScrollbarSize?: number
  }
  /** 是否启用渲染空格 */
  renderWhitespace?: 'none' | 'boundary' | 'selection' | 'trailing' | 'all'
  /** 是否启用渲染控制字符 */
  renderControlCharacters?: boolean
  /** 是否启用渲染行号 */
  renderLineHighlight?: 'none' | 'gutter' | 'line' | 'all'
  /** 是否启用选择高亮 */
  selectionHighlight?: boolean
  /** 是否启用光标高亮 */
  occurrencesHighlight?: boolean
  /** 是否启用代码镜头 */
  codeLens?: boolean
  /** 是否启用光标平滑动画 */
  cursorSmoothCaretAnimation?: boolean
  /** 是否启用光标闪烁 */
  cursorBlinking?: 'blink' | 'smooth' | 'phase' | 'expand' | 'solid'
}

/**
 * 默认的 Monaco Editor 配置
 */
export const defaultMonacoConfig: MonacoConfig = {
  theme: 'vs-dark',
  language: 'javascript',
  readOnly: false,
  lineNumbers: 'on',
  folding: true,
  minimap: {
    enabled: true,
    side: 'right',
    showSlider: 'mouseover'
  },
  automaticLayout: true,
  multiCursorModifier: 'ctrlCmd',
  fontSize: 14,
  fontFamily: 'Consolas, Monaco, "Courier New", monospace',
  lineHeight: 1.6,
  wordBasedSuggestions: true,
  parameterHints: {
    enabled: true,
    cycle: true
  },
  quickSuggestions: {
    other: true,
    comments: false,
    strings: false
  },
  formatOnPaste: true,
  formatOnType: false,
  scrollbar: {
    vertical: 'auto',
    horizontal: 'auto',
    verticalScrollbarSize: 10,
    horizontalScrollbarSize: 10
  },
  renderWhitespace: 'selection',
  renderControlCharacters: false,
  renderLineHighlight: 'all',
  selectionHighlight: true,
  occurrencesHighlight: true,
  codeLens: false,
  cursorSmoothCaretAnimation: false,
  cursorBlinking: 'blink'
}

/**
 * 代码查看器专用配置 (只读模式)
 */
export const viewerConfig: MonacoConfig = {
  ...defaultMonacoConfig,
  readOnly: true,
  minimap: {
    enabled: true,
    side: 'right',
    showSlider: 'always'
  },
  wordBasedSuggestions: false,
  quickSuggestions: {
    other: false,
    comments: false,
    strings: false
  },
  formatOnPaste: false,
  formatOnType: false,
  selectionHighlight: false,
  occurrencesHighlight: false
}

/**
 * 代码编辑器专用配置 (编辑模式)
 */
export const editorConfig: MonacoConfig = {
  ...defaultMonacoConfig,
  readOnly: false,
  minimap: {
    enabled: true,
    side: 'right',
    showSlider: 'mouseover'
  },
  wordBasedSuggestions: true,
  quickSuggestions: {
    other: true,
    comments: true,
    strings: true
  },
  formatOnPaste: true,
  formatOnType: true,
  selectionHighlight: true,
  occurrencesHighlight: true
}

/**
 * 移动端专用配置
 */
export const mobileConfig: MonacoConfig = {
  ...viewerConfig,
  fontSize: 12,
  lineHeight: 1.4,
  minimap: {
    enabled: false,
    side: 'right',
    showSlider: 'mouseover'
  },
  scrollbar: {
    vertical: 'visible',
    horizontal: 'visible',
    verticalScrollbarSize: 8,
    horizontalScrollbarSize: 8
  }
}

/**
 * 获取编程语言对应的 Monaco Editor 语言 ID
 */
export function getMonacoLanguage(language: string): string {
  const languageMap: Record<string, string> = {
    'javascript': 'javascript',
    'typescript': 'typescript',
    'python': 'python',
    'java': 'java',
    'csharp': 'csharp',
    'cpp': 'cpp',
    'c': 'c',
    'html': 'html',
    'css': 'css',
    'scss': 'scss',
    'less': 'less',
    'vue': 'html',
    'react': 'javascript',
    'jsx': 'javascript',
    'tsx': 'typescript',
    'angular': 'typescript',
    'php': 'php',
    'ruby': 'ruby',
    'go': 'go',
    'rust': 'rust',
    'swift': 'swift',
    'kotlin': 'kotlin',
    'dart': 'dart',
    'sql': 'sql',
    'shell': 'shell',
    'bash': 'shell',
    'powershell': 'powershell',
    'json': 'json',
    'xml': 'xml',
    'yaml': 'yaml',
    'yml': 'yaml',
    'markdown': 'markdown',
    'md': 'markdown',
    'text': 'plaintext'
  }

  return languageMap[language.toLowerCase()] || 'plaintext'
}

/**
 * 获取编辑器配置
 */
export function getEditorConfig(
  type: 'viewer' | 'editor' | 'mobile',
  overrides?: Partial<MonacoConfig>
): MonacoConfig {
  const baseConfig = type === 'viewer' 
    ? viewerConfig 
    : type === 'mobile' 
      ? mobileConfig 
      : editorConfig

  return {
    ...baseConfig,
    ...overrides
  }
}

/**
 * Monaco Editor 初始化配置
 */
export const monacoInitializationOptions = {
  'vs/nls': {
    availableLanguages: {
      '*': 'zh-cn'
    }
  },
  // 配置 Web Worker
  'vs/editor/common/services/editorWorker': {
    // 启用 Web Worker
    enableModelAutoValidation: true,
    // 启用语法检查
    enableSemanticValidation: true,
    // 启用语法高亮
    enableSyntaxValidation: true
  },
  // 配置编辑器选项
  'vs/editor/editor.main': {
    // 启用自动检测缩进
    detectIndentation: true,
    // 启用 Tab 大小
    tabSize: 2,
    // 启用空格缩进
    insertSpaces: true,
    // 启用行尾自动添加分号
    semicolons: 'ignore',
    // 启用引号类型
    quotes: 'single',
    // 启用代码格式化
    formatOnSave: false,
    // 启用代码检查
    lintOnSave: true,
    // 启用自动修复
    fixOnSave: true
  }
}

/**
 * Monaco Editor 主题配置
 */
export const themeConfig = {
  // 默认主题
  'vs': {
    base: 'vs',
    inherit: true,
    rules: [],
    colors: {}
  },
  // 深色主题
  'vs-dark': {
    base: 'vs-dark',
    inherit: true,
    rules: [],
    colors: {}
  },
  // 高对比度主题
  'hc-black': {
    base: 'hc-black',
    inherit: true,
    rules: [],
    colors: {}
  }
}

/**
 * 获取主题配置
 */
export function getThemeConfig(theme: string) {
  return themeConfig[theme as keyof typeof themeConfig] || themeConfig['vs']
}

/**
 * Monaco Editor 扩展配置
 */
export const extensionConfig = {
  // 启用 JavaScript/TypeScript 支持
  javascript: {
    validate: {
      enable: true,
      semanticValidation: true,
      syntaxValidation: true
    },
    format: {
      enable: true,
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
    }
  },
  // 启用 Python 支持
  python: {
    validate: {
      enable: true,
      semanticValidation: true,
      syntaxValidation: true
    },
    format: {
      enable: true
    }
  },
  // 启用 Java 支持
  java: {
    validate: {
      enable: true,
      semanticValidation: true,
      syntaxValidation: true
    },
    format: {
      enable: true
    }
  }
}

/**
 * 获取语言扩展配置
 */
export function getLanguageExtensionConfig(language: string) {
  const languageKey = language.toLowerCase()
  return extensionConfig[languageKey as keyof typeof extensionConfig] || {}
}