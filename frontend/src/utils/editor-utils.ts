import type { editor } from 'monaco-editor'
import { getMonacoLanguage, getEditorConfig } from '@/config/monaco-config'
import { getWebWorkerStatus, checkFileSize } from '@/config/monaco-worker-config'

/**
 * Monaco Editor 工具类
 * 提供编辑器的常用工具方法
 */

/**
 * 编辑器选项接口
 */
export interface EditorOptions extends editor.IStandaloneEditorConstructionOptions {
  /** 编辑器类型 */
  type?: 'viewer' | 'editor' | 'mobile'
  /** 编程语言 */
  language?: string
  /** 主题 */
  theme?: string
  /** 是否只读 */
  readOnly?: boolean
  /** 是否启用行号 */
  lineNumbers?: 'on' | 'off' | 'relative' | 'interval'
  /** 是否启用小地图 */
  minimap?: editor.IEditorMinimapOptions
  /** 字体大小 */
  fontSize?: number
  /** 字体系列 */
  fontFamily?: string
  /** 行高 */
  lineHeight?: number
  /** 是否启用自动滚动 */
  automaticLayout?: boolean
}

/**
 * 编辑器实例信息接口
 */
export interface EditorInstance {
  /** 编辑器实例 */
  editor: editor.IStandaloneCodeEditor
  /** 编辑器容器 */
  container: HTMLElement
  /** 编辑器配置 */
  options: EditorOptions
  /** 编辑器语言 */
  language: string
  /** 编辑器主题 */
  theme: string
  /** 创建时间 */
  createdAt: Date
  /** 最后更新时间 */
  updatedAt: Date
}

/**
 * 编辑器事件处理器接口
 */
export interface EditorEventHandlers {
  /** 内容变化事件 */
  onContentChange?: (content: string) => void
  /** 光标位置变化事件 */
  onCursorChange?: (position: editor.IPosition) => void
  /** 选择变化事件 */
  onSelectionChange?: (selection: editor.ISelection) => void
  /** 编辑器失去焦点事件 */
  onBlur?: () => void
  /** 编辑器获得焦点事件 */
  onFocus?: () => void
  /** 编辑器滚动事件 */
  onScrollChange?: (scrollTop: number, scrollLeft: number) => void
  /** 编辑器大小变化事件 */
  onSizeChange?: (width: number, height: number) => void
  /** 编辑器配置变化事件 */
  onOptionsChange?: (options: editor.IEditorOptions) => void
}

/**
 * 编辑器工具类
 */
export class EditorUtils {
  private static instances: Map<string, EditorInstance> = new Map()
  private static nextId = 1

  /**
   * 创建编辑器实例
   */
  static async createEditor(
    container: HTMLElement,
    options: EditorOptions,
    eventHandlers?: EditorEventHandlers
  ): Promise<EditorInstance> {
    const id = `editor-${this.nextId++}`
    
    // 检查容器是否有效
    if (!container || !(container instanceof HTMLElement)) {
      throw new Error('Invalid container element')
    }

    // 检查文件大小
    if (options.value && !checkFileSize(options.value, { maxFileSize: 10 * 1024 * 1024 })) {
      throw new Error('File size exceeds limit (10MB)')
    }

    // 动态导入 Monaco Editor
    const monaco = await import('monaco-editor')
    
    // 获取编辑器配置
    const editorConfig = this.getEditorConfig(options)
    
    // 创建编辑器
    const editor = monaco.editor.create(container, {
      ...editorConfig,
      value: options.value || '',
      language: getMonacoLanguage(options.language || 'javascript'),
      theme: options.theme || 'vs-dark',
      readOnly: options.readOnly || false,
      lineNumbers: options.lineNumbers || 'on',
      minimap: options.minimap || { enabled: true },
      fontSize: options.fontSize || 14,
      fontFamily: options.fontFamily || 'Consolas, Monaco, "Courier New", monospace',
      lineHeight: options.lineHeight || 1.6,
      automaticLayout: options.automaticLayout || true
    })

    // 绑定事件处理器
    this.bindEventHandlers(editor, eventHandlers)

    // 创建实例信息
    const instance: EditorInstance = {
      editor,
      container,
      options,
      language: options.language || 'javascript',
      theme: options.theme || 'vs-dark',
      createdAt: new Date(),
      updatedAt: new Date()
    }

    // 保存实例
    this.instances.set(id, instance)

    // 监听编辑器销毁
    editor.onDidDispose(() => {
      this.instances.delete(id)
    })

    return instance
  }

  /**
   * 获取编辑器配置
   */
  private static getEditorConfig(options: EditorOptions): editor.IStandaloneEditorConstructionOptions {
    const config = getEditorConfig(options.type || 'editor', options)
    
    return {
      ...config,
      language: getMonacoLanguage(options.language || 'javascript'),
      theme: options.theme || 'vs-dark',
      readOnly: options.readOnly || false,
      lineNumbers: options.lineNumbers || 'on',
      minimap: options.minimap || { enabled: true },
      fontSize: options.fontSize || 14,
      fontFamily: options.fontFamily || 'Consolas, Monaco, "Courier New", monospace',
      lineHeight: options.lineHeight || 1.6,
      automaticLayout: options.automaticLayout || true,
      scrollBeyondLastLine: false,
      automaticLayout: true,
      minimap: {
        enabled: true,
        side: 'right',
        showSlider: 'mouseover',
        renderCharacters: true
      },
      wordWrap: 'on',
      lineNumbersMinChars: 3,
      lineDecorationsWidth: 10,
      folding: true,
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
        comments: true,
        strings: true
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
    }
  }

  /**
   * 绑定事件处理器
   */
  private static bindEventHandlers(
    editor: editor.IStandaloneCodeEditor,
    handlers?: EditorEventHandlers
  ) {
    if (!handlers) return

    // 内容变化事件
    if (handlers.onContentChange) {
      editor.onDidChangeModelContent(() => {
        const content = editor.getValue()
        handlers.onContentChange!(content)
      })
    }

    // 光标位置变化事件
    if (handlers.onCursorChange) {
      editor.onDidChangeCursorPosition((e) => {
        handlers.onCursorChange!(e.position)
      })
    }

    // 选择变化事件
    if (handlers.onSelectionChange) {
      editor.onDidChangeCursorSelection((e) => {
        handlers.onSelectionChange!(e.selection)
      })
    }

    // 编辑器失去焦点事件
    if (handlers.onBlur) {
      editor.onDidBlurEditorWidget(() => {
        handlers.onBlur!()
      })
    }

    // 编辑器获得焦点事件
    if (handlers.onFocus) {
      editor.onDidFocusEditorWidget(() => {
        handlers.onFocus!()
      })
    }

    // 编辑器滚动事件
    if (handlers.onScrollChange) {
      editor.onDidScrollChange((e) => {
        handlers.onScrollChange!(e.scrollTop, e.scrollLeft)
      })
    }

    // 编辑器大小变化事件
    if (handlers.onSizeChange) {
      editor.onDidContentSizeChange((e) => {
        handlers.onSizeChange!(e.contentWidth, e.contentHeight)
      })
    }

    // 编辑器配置变化事件
    if (handlers.onOptionsChange) {
      editor.onDidChangeConfiguration((e) => {
        handlers.onOptionsChange!(editor.getOptions())
      })
    }
  }

  /**
   * 获取编辑器实例
   */
  static getEditor(id: string): EditorInstance | undefined {
    return this.instances.get(id)
  }

  /**
   * 获取所有编辑器实例
   */
  static getAllEditors(): EditorInstance[] {
    return Array.from(this.instances.values())
  }

  /**
   * 销毁编辑器实例
   */
  static destroyEditor(id: string): boolean {
    const instance = this.instances.get(id)
    if (instance) {
      instance.editor.dispose()
      this.instances.delete(id)
      return true
    }
    return false
  }

  /**
   * 销毁所有编辑器实例
   */
  static destroyAllEditors(): void {
    this.instances.forEach((instance) => {
      instance.editor.dispose()
    })
    this.instances.clear()
  }

  /**
   * 设置编辑器内容
   */
  static setContent(id: string, content: string): boolean {
    const instance = this.instances.get(id)
    if (instance) {
      instance.editor.setValue(content)
      instance.updatedAt = new Date()
      return true
    }
    return false
  }

  /**
   * 获取编辑器内容
   */
  static getContent(id: string): string | undefined {
    const instance = this.instances.get(id)
    return instance?.editor.getValue()
  }

  /**
   * 设置编辑器语言
   */
  static setLanguage(id: string, language: string): boolean {
    const instance = this.instances.get(id)
    if (instance) {
      const monacoLanguage = getMonacoLanguage(language)
      monaco.editor.setModelLanguage(instance.editor.getModel()!, monacoLanguage)
      instance.language = language
      instance.updatedAt = new Date()
      return true
    }
    return false
  }

  /**
   * 设置编辑器主题
   */
  static setTheme(id: string, theme: string): boolean {
    const instance = this.instances.get(id)
    if (instance) {
      instance.editor.setTheme(theme)
      instance.theme = theme
      instance.updatedAt = new Date()
      return true
    }
    return false
  }

  /**
   * 设置编辑器只读状态
   */
  static setReadOnly(id: string, readOnly: boolean): boolean {
    const instance = this.instances.get(id)
    if (instance) {
      instance.editor.updateOptions({ readOnly })
      instance.options.readOnly = readOnly
      instance.updatedAt = new Date()
      return true
    }
    return false
  }

  /**
   * 获取编辑器选中文本
   */
  static getSelectedText(id: string): string | undefined {
    const instance = this.instances.get(id)
    if (instance) {
      const selection = instance.editor.getSelection()
      if (selection) {
        const model = instance.editor.getModel()
        if (model) {
          return model.getValueInRange(selection)
        }
      }
    }
    return undefined
  }

  /**
   * 获取编辑器选中范围
   */
  static getSelection(id: string): editor.ISelection | undefined {
    const instance = this.instances.get(id)
    return instance?.editor.getSelection()
  }

  /**
   * 获取编辑器光标位置
   */
  static getCursorPosition(id: string): editor.IPosition | undefined {
    const instance = this.instances.get(id)
    return instance?.editor.getPosition()
  }

  /**
   * 设置编辑器光标位置
   */
  static setCursorPosition(id: string, position: editor.IPosition): boolean {
    const instance = this.instances.get(id)
    if (instance) {
      instance.editor.setPosition(position)
      return true
    }
    return false
  }

  /**
   * 获取编辑器滚动位置
   */
  static getScrollPosition(id: string): { scrollTop: number; scrollLeft: number } | undefined {
    const instance = this.instances.get(id)
    if (instance) {
      return {
        scrollTop: instance.editor.getScrollTop(),
        scrollLeft: instance.editor.getScrollLeft()
      }
    }
    return undefined
  }

  /**
   * 设置编辑器滚动位置
   */
  static setScrollPosition(id: string, scrollTop: number, scrollLeft: number): boolean {
    const instance = this.instances.get(id)
    if (instance) {
      instance.editor.setScrollTop(scrollTop)
      instance.editor.setScrollLeft(scrollLeft)
      return true
    }
    return false
  }

  /**
   * 获取编辑器统计信息
   */
  static getEditorStats(id: string): {
    lineCount: number
    columnCount: number
    selectedLength: number
    totalLength: number
    readOnly: boolean
    language: string
    theme: string
  } | undefined {
    const instance = this.instances.get(id)
    if (!instance) return undefined

    const model = instance.editor.getModel()
    const selection = instance.editor.getSelection()
    
    return {
      lineCount: model?.getLineCount() || 0,
      columnCount: model?.getLineMaxColumn(model.getLineCount()) || 0,
      selectedLength: selection ? model?.getValueInRange(selection).length || 0 : 0,
      totalLength: model?.getValueLength() || 0,
      readOnly: instance.options.readOnly || false,
      language: instance.language,
      theme: instance.theme
    }
  }

  /**
   * 获取 Web Worker 状态
   */
  static getWorkerStatus() {
    return getWebWorkerStatus()
  }

  /**
   * 格式化代码
   */
  static async formatCode(id: string): Promise<boolean> {
    const instance = this.instances.get(id)
    if (!instance) return false

    try {
      await instance.editor.getAction('editor.action.formatDocument')?.run()
      instance.updatedAt = new Date()
      return true
    } catch (error) {
      console.error('Failed to format code:', error)
      return false
    }
  }

  /**
   * 查找下一个
   */
  static findNext(id: string, searchTerm: string): boolean {
    const instance = this.instances.get(id)
    if (!instance) return false

    const result = instance.editor.getAction('actions.find')?.run()
    return !!result
  }

  /**
   * 查找上一个
   */
  static findPrevious(id: string, searchTerm: string): boolean {
    const instance = this.instances.get(id)
    if (!instance) return false

    const result = instance.editor.getAction('actions.findWithSelection')?.run()
    return !!result
  }

  /**
   * 替换文本
   */
  static replace(id: string, searchText: string, replaceText: string): boolean {
    const instance = this.instances.get(id)
    if (!instance) return false

    const result = instance.editor.getAction('editor.action.replace')?.run()
    return !!result
  }

  /**
   * 跳转到指定行
   */
  static goToLine(id: string, lineNumber: number, column?: number): boolean {
    const instance = this.instances.get(id)
    if (!instance) return false

    const position = {
      lineNumber,
      column: column || 1
    }
    
    instance.editor.setPosition(position)
    instance.editor.revealPosition(position)
    return true
  }

  /**
   * 折叠所有代码
   */
  static foldAll(id: string): boolean {
    const instance = this.instances.get(id)
    if (!instance) return false

    const result = instance.editor.getAction('editor.foldAll')?.run()
    return !!result
  }

  /**
   * 展开所有代码
   */
  static unfoldAll(id: string): boolean {
    const instance = this.instances.get(id)
    if (!instance) return false

    const result = instance.editor.getAction('editor.unfoldAll')?.run()
    return !!result
  }

  /**
   * 获取编辑器实例统计
   */
  static getInstanceStats(): {
    totalInstances: number
    activeInstances: number
    languages: Record<string, number>
    themes: Record<string, number>
    averageAge: number
  } {
    const instances = Array.from(this.instances.values())
    const now = Date.now()
    
    const languages: Record<string, number> = {}
    const themes: Record<string, number> = {}
    
    instances.forEach(instance => {
      languages[instance.language] = (languages[instance.language] || 0) + 1
      themes[instance.theme] = (themes[instance.theme] || 0) + 1
    })

    const averageAge = instances.length > 0
      ? instances.reduce((sum, instance) => sum + (now - instance.createdAt.getTime()), 0) / instances.length
      : 0

    return {
      totalInstances: instances.length,
      activeInstances: instances.filter(i => !i.options.readOnly).length,
      languages,
      themes,
      averageAge
    }
  }
}