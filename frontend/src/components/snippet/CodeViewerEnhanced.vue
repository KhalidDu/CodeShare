<template>
  <div class="code-viewer-enhanced">
    <!-- 编辑器工具栏 -->
    <div class="editor-toolbar">
      <div class="toolbar-left">
        <!-- 语言标签 -->
        <div class="language-badge">
          <div 
            class="language-dot"
            :style="{ backgroundColor: getLanguageColor(language) }"
          ></div>
          <span>{{ getLanguageDisplayName(language) }}</span>
        </div>
        
        <!-- 文件信息 -->
        <div class="file-info">
          <span class="file-size">{{ formatFileSize(content.length) }}</span>
          <span class="line-count">{{ getLineCount() }} 行</span>
        </div>
      </div>
      
      <div class="toolbar-right">
        <!-- 操作按钮 -->
        <div class="toolbar-actions">
          <!-- 行号切换 -->
          <button
            @click="toggleLineNumbers"
            class="toolbar-btn"
            :class="{ active: showLineNumbers }"
            title="切换行号显示"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
            </svg>
          </button>
          
          <!-- 小地图切换 -->
          <button
            @click="toggleMinimap"
            class="toolbar-btn"
            :class="{ active: showMinimap }"
            title="切换小地图"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7" />
            </svg>
          </button>
          
          <!-- 自动换行切换 -->
          <button
            @click="toggleWordWrap"
            class="toolbar-btn"
            :class="{ active: wordWrap !== 'off' }"
            title="切换自动换行"
            aria-label="切换自动换行"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h7" />
            </svg>
          </button>
          
          <!-- 代码折叠 -->
          <button
            @click="toggleFolding"
            class="toolbar-btn"
            :class="{ active: enableFolding }"
            title="切换代码折叠"
            aria-label="切换代码折叠"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
            </svg>
          </button>
          
          <!-- 缩进指南 -->
          <button
            @click="toggleIndentGuides"
            class="toolbar-btn"
            :class="{ active: showIndentGuides }"
            title="切换缩进指南"
            aria-label="切换缩进指南"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h7" />
            </svg>
          </button>
          
          <!-- 高亮当前行 -->
          <button
            @click="toggleHighlightLine"
            class="toolbar-btn"
            :class="{ active: highlightCurrentLine }"
            title="高亮当前行"
            aria-label="高亮当前行"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
            </svg>
          </button>
          
          <!-- 全屏切换 -->
          <button
            @click="toggleFullscreen"
            class="toolbar-btn"
            title="切换全屏"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 8V4m0 0h4M4 4l5 5m11-5h-4m4 0v4m0-4l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4" />
            </svg>
          </button>
          
          <!-- 复制代码 -->
          <button
            @click="copyCode"
            class="toolbar-btn"
            title="复制代码"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
            </svg>
          </button>
          
          <!-- 下载代码 -->
          <button
            @click="downloadCode"
            class="toolbar-btn"
            title="下载代码"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </button>
        </div>
      </div>
    </div>
    
    <!-- 编辑器容器 -->
    <div 
      ref="editorContainer"
      class="editor-container"
      :class="{ 
        'is-fullscreen': isFullscreen,
        'is-loading': isLoading 
      }"
    ></div>
    
    <!-- 状态栏 -->
    <div class="editor-status">
      <div class="status-left">
        <!-- 光标位置 -->
        <span class="cursor-position">
          行 {{ cursorPosition.lineNumber }}, 列 {{ cursorPosition.column }}
        </span>
        
        <!-- 选择信息 -->
        <span v-if="selectionInfo.text" class="selection-info">
          已选择 {{ selectionInfo.length }} 个字符
        </span>
      </div>
      
      <div class="status-right">
        <!-- 编码信息 -->
        <span class="encoding">UTF-8</span>
        
        <!-- 换行符 -->
        <span class="line-ending">LF</span>
      </div>
    </div>
    
    <!-- 加载遮罩 -->
    <div v-if="isLoading" class="loading-overlay">
      <div class="loading-content">
        <div class="loading-spinner"></div>
        <p class="loading-text">正在初始化编辑器...</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick, watch } from 'vue'
import { EditorUtils } from '@/utils/editor-utils'
import type { ExtendedEditorInstance, ExtendedEditorEventHandlers } from '@/types/monaco-editor'

// Props
interface Props {
  /** 代码内容 */
  code: string
  /** 编程语言 */
  language: string
  /** 编辑器主题 */
  theme: string
  /** 是否只读 */
  readOnly: boolean
  /** 是否显示行号 */
  showLineNumbers: boolean
  /** 是否显示小地图 */
  showMinimap: boolean
  /** 自动换行设置 */
  wordWrap: 'on' | 'off' | 'wordWrapColumn' | 'bounded'
  /** 是否启用代码折叠 */
  enableFolding: boolean
  /** 是否显示缩进指南 */
  showIndentGuides: boolean
  /** 是否高亮当前行 */
  highlightCurrentLine: boolean
  /** 是否显示空白字符 */
  renderWhitespace: 'none' | 'boundary' | 'selection' | 'all'
  /** 是否启用括号匹配 */
  matchBrackets: 'never' | 'near' | 'always'
}

const props = withDefaults(defineProps<Props>(), {
  code: '',
  language: 'javascript',
  theme: 'vs-dark',
  readOnly: true,
  showLineNumbers: true,
  showMinimap: true,
  wordWrap: 'on',
  enableFolding: true,
  showIndentGuides: true,
  highlightCurrentLine: true,
  renderWhitespace: 'selection',
  matchBrackets: 'always'
})

// Emits
interface Emits {
  (e: 'content-change', content: string): void
  (e: 'cursor-change', position: any): void
  (e: 'selection-change', selection: any): void
  (e: 'ready'): void
  (e: 'error', error: Error): void
  (e: 'toggle-line-numbers', enabled: boolean): void
  (e: 'toggle-minimap', enabled: boolean): void
  (e: 'toggle-word-wrap', mode: 'on' | 'off'): void
  (e: 'toggle-fullscreen', enabled: boolean): void
  (e: 'toggle-folding', enabled: boolean): void
  (e: 'toggle-indent-guides', enabled: boolean): void
  (e: 'toggle-highlight-line', enabled: boolean): void
  (e: 'copy-success'): void
  (e: 'copy-error', error: Error): void
  (e: 'download-success'): void
  (e: 'settings-change', settings: Partial<Props>): void
}

const emit = defineEmits<Emits>()

// 响应式状态
const editorContainer = ref<HTMLElement>()
const isLoading = ref(true)
const isFullscreen = ref(false)
const editorInstance = ref<ExtendedEditorInstance>()
const editorId = ref('')

// 编辑器状态
const cursorPosition = ref({ lineNumber: 1, column: 1 })
const selectionInfo = ref({ text: '', length: 0 })

// 计算属性
const editorConfig = computed(() => ({
  language: props.language,
  theme: props.theme,
  readOnly: props.readOnly,
  lineNumbers: props.showLineNumbers ? 'on' : 'off',
  minimap: {
    enabled: props.showMinimap,
    side: 'right',
    showSlider: 'mouseover'
  },
  wordWrap: props.wordWrap,
  automaticLayout: true,
  fontSize: 14,
  fontFamily: 'Consolas, Monaco, "Courier New", monospace',
  lineHeight: 1.6,
  scrollBeyondLastLine: false,
  renderWhitespace: props.renderWhitespace,
  renderControlCharacters: false,
  renderIndentGuides: props.showIndentGuides,
  selectOnLineNumbers: true,
  matchBrackets: props.matchBrackets,
  autoIndent: 'advanced',
  tabSize: 2,
  insertSpaces: true,
  detectIndentation: true,
  trimAutoWhitespace: true,
  folding: props.enableFolding,
  highlightCurrentLine: props.highlightCurrentLine,
  occurrenceHighlight: true,
  cursorBlinking: 'blink',
  cursorSmoothCaretAnimation: true,
  wordSegmenterLocales: 'en-us',
  smoothScrolling: true,
  mouseWheelZoom: true,
  multiCursorModifier: 'ctrlCmd',
  accessibilitySupport: 'auto'
}))

// 事件处理器
const eventHandlers: ExtendedEditorEventHandlers = {
  onContentChange: (content: string) => {
    emit('content-change', content)
  },
  onCursorChange: (position: any) => {
    cursorPosition.value = position
    emit('cursor-change', position)
  },
  onSelectionChange: (selection: any) => {
    if (selection) {
      selectionInfo.value = {
        text: getSelectedText(selection),
        length: getSelectionLength(selection)
      }
    } else {
      selectionInfo.value = { text: '', length: 0 }
    }
    emit('selection-change', selection)
  },
  onInitialized: (instance: ExtendedEditorInstance) => {
    isLoading.value = false
    editorId.value = instance.id
    emit('ready')
  },
  onError: (error: Error, instance: ExtendedEditorInstance) => {
    console.error('Editor error:', error)
    emit('error', error)
  }
}

// 方法
/**
 * 初始化编辑器
 */
async function initializeEditor() {
  if (!editorContainer.value) return

  try {
    isLoading.value = true
    
    // 创建编辑器实例
    const instance = await EditorUtils.createEditor(
      editorContainer.value,
      {
        ...editorConfig.value,
        value: props.code,
        type: props.readOnly ? 'viewer' : 'editor'
      },
      eventHandlers
    )

    editorInstance.value = instance
    
    // 设置初始内容
    if (props.code) {
      await nextTick()
      EditorUtils.setContent(instance.id, props.code)
    }

  } catch (error) {
    console.error('Failed to initialize editor:', error)
    emit('error', error as Error)
    isLoading.value = false
  }
}

/**
 * 销毁编辑器
 */
function destroyEditor() {
  if (editorId.value) {
    EditorUtils.destroyEditor(editorId.value)
    editorId.value = ''
    editorInstance.value = undefined
  }
}

/**
 * 更新编辑器配置
 */
async function updateEditorConfig() {
  if (!editorId.value) return

  try {
    // 更新主题
    EditorUtils.setTheme(editorId.value, props.theme)
    
    // 更新语言
    EditorUtils.setLanguage(editorId.value, props.language)
    
    // 更新只读状态
    EditorUtils.setReadOnly(editorId.value, props.readOnly)
    
    // 更新编辑器选项
    if (editorInstance.value?.editor) {
      editorInstance.value.editor.updateOptions({
        lineNumbers: props.showLineNumbers ? 'on' : 'off',
        minimap: {
          enabled: props.showMinimap,
          side: 'right',
          showSlider: 'mouseover'
        },
        wordWrap: props.wordWrap,
        folding: props.enableFolding,
        renderIndentGuides: props.showIndentGuides,
        renderWhitespace: props.renderWhitespace,
        matchBrackets: props.matchBrackets,
        highlightCurrentLine: props.highlightCurrentLine
      })
    }
    
  } catch (error) {
    console.error('Failed to update editor config:', error)
  }
}

/**
 * 切换行号显示
 */
function toggleLineNumbers() {
  const newValue = !props.showLineNumbers
  emit('toggle-line-numbers', newValue)
  emit('settings-change', { showLineNumbers: newValue })
}

/**
 * 切换小地图显示
 */
function toggleMinimap() {
  const newValue = !props.showMinimap
  emit('toggle-minimap', newValue)
  emit('settings-change', { showMinimap: newValue })
}

/**
 * 切换自动换行
 */
function toggleWordWrap() {
  const newValue = props.wordWrap === 'on' ? 'off' : 'on'
  emit('toggle-word-wrap', newValue)
  emit('settings-change', { wordWrap: newValue })
}

/**
 * 切换代码折叠
 */
function toggleFolding() {
  const newValue = !props.enableFolding
  emit('toggle-folding', newValue)
  emit('settings-change', { enableFolding: newValue })
}

/**
 * 切换缩进指南
 */
function toggleIndentGuides() {
  const newValue = !props.showIndentGuides
  emit('toggle-indent-guides', newValue)
  emit('settings-change', { showIndentGuides: newValue })
}

/**
 * 切换高亮当前行
 */
function toggleHighlightLine() {
  const newValue = !props.highlightCurrentLine
  emit('toggle-highlight-line', newValue)
  emit('settings-change', { highlightCurrentLine: newValue })
}

/**
 * 切换全屏
 */
function toggleFullscreen() {
  isFullscreen.value = !isFullscreen.value
  
  if (isFullscreen.value) {
    document.documentElement.requestFullscreen()
  } else {
    document.exitFullscreen()
  }
  
  emit('toggle-fullscreen', isFullscreen.value)
}

/**
 * 复制代码
 */
async function copyCode() {
  try {
    const codeToCopy = editorId.value && editorInstance.value?.editor
      ? EditorUtils.getContent(editorId.value)
      : props.code
    
    await navigator.clipboard.writeText(codeToCopy)
    emit('copy-success')
  } catch (error) {
    console.error('Failed to copy code:', error)
    emit('copy-error', error as Error)
  }
}

/**
 * 下载代码
 */
function downloadCode() {
  try {
    const codeToDownload = editorId.value && editorInstance.value?.editor
      ? EditorUtils.getContent(editorId.value)
      : props.code
    
    const blob = new Blob([codeToDownload], { 
      type: `text/${getFileExtension(props.language)}` 
    })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `code.${getFileExtension(props.language)}`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
    
    emit('download-success')
  } catch (error) {
    console.error('Failed to download code:', error)
    emit('error', error as Error)
  }
}

/**
 * 获取选中的文本
 */
function getSelectedText(selection: any): string {
  if (!editorInstance.value?.editor.getModel()) return ''
  
  const model = editorInstance.value.editor.getModel()
  return model?.getValueInRange(selection) || ''
}

/**
 * 获取选中的长度
 */
function getSelectionLength(selection: any): number {
  return getSelectedText(selection).length
}

/**
 * 获取文件扩展名
 */
function getFileExtension(language: string): string {
  const extensions: Record<string, string> = {
    javascript: 'js',
    typescript: 'ts',
    python: 'py',
    java: 'java',
    csharp: 'cs',
    cpp: 'cpp',
    html: 'html',
    css: 'css',
    vue: 'vue',
    json: 'json',
    xml: 'xml',
    yaml: 'yaml',
    markdown: 'md',
    sql: 'sql',
    shell: 'sh',
    plaintext: 'txt'
  }
  
  return extensions[language.toLowerCase()] || 'txt'
}

/**
 * 格式化文件大小
 */
function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 Bytes'
  
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

/**
 * 获取行数
 */
function getLineCount(): number {
  return props.code.split('\n').length
}

/**
 * 获取编程语言对应的颜色
 */
function getLanguageColor(language: string): string {
  const colors: Record<string, string> = {
    javascript: '#f7df1e',
    typescript: '#3178c6',
    python: '#3776ab',
    java: '#ed8b00',
    csharp: '#239120',
    cpp: '#00599c',
    html: '#e34f26',
    css: '#1572b6',
    vue: '#4fc08d',
    json: '#000000',
    xml: '#0060ac',
    yaml: '#cb171e',
    markdown: '#083fa1',
    sql: '#336791',
    shell: '#89e051',
    plaintext: '#6c757d'
  }
  
  return colors[language.toLowerCase()] || '#6c757d'
}

/**
 * 获取编程语言的显示名称
 */
function getLanguageDisplayName(language: string): string {
  const displayNames: Record<string, string> = {
    javascript: 'JavaScript',
    typescript: 'TypeScript',
    python: 'Python',
    java: 'Java',
    csharp: 'C#',
    cpp: 'C++',
    html: 'HTML',
    css: 'CSS',
    vue: 'Vue',
    json: 'JSON',
    xml: 'XML',
    yaml: 'YAML',
    markdown: 'Markdown',
    sql: 'SQL',
    shell: 'Shell',
    plaintext: 'Plain Text'
  }
  
  return displayNames[language.toLowerCase()] || language
}

// 监听 Props 变化
watch([() => props.theme, () => props.language, () => props.readOnly], () => {
  if (editorId.value) {
    updateEditorConfig()
  }
})

watch([
  () => props.showLineNumbers, 
  () => props.showMinimap, 
  () => props.wordWrap,
  () => props.enableFolding,
  () => props.showIndentGuides,
  () => props.highlightCurrentLine,
  () => props.renderWhitespace,
  () => props.matchBrackets
], () => {
  if (editorId.value && editorInstance.value?.editor) {
    editorInstance.value.editor.updateOptions({
      lineNumbers: props.showLineNumbers ? 'on' : 'off',
      minimap: {
        enabled: props.showMinimap,
        side: 'right',
        showSlider: 'mouseover'
      },
      wordWrap: props.wordWrap,
      folding: props.enableFolding,
      renderIndentGuides: props.showIndentGuides,
      renderWhitespace: props.renderWhitespace,
      matchBrackets: props.matchBrackets,
      highlightCurrentLine: props.highlightCurrentLine
    })
  }
})

watch(() => props.code, (newCode) => {
  if (editorId.value) {
    EditorUtils.setContent(editorId.value, newCode)
  }
})

// 生命周期
onMounted(() => {
  initializeEditor()
})

onUnmounted(() => {
  destroyEditor()
})

// 监听全屏状态变化
document.addEventListener('fullscreenchange', () => {
  isFullscreen.value = !!document.fullscreenElement
})

// 暴露方法供父组件调用
defineExpose({
  copyCode,
  downloadCode,
  toggleFullscreen,
  formatCode: async () => {
    if (editorId.value) {
      await EditorUtils.formatCode(editorId.value)
    }
  },
  getEditorStats: () => {
    if (editorId.value) {
      return EditorUtils.getEditorStats(editorId.value)
    }
    return null
  }
})
</script>

<style scoped>
.code-viewer-enhanced {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: var(--editor-bg-primary);
  border: 1px solid var(--editor-border-primary);
  border-radius: var(--snippet-radius-md);
  overflow: hidden;
  font-family: var(--snippet-font-family-primary);
}

/* 编辑器工具栏 */
.editor-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--snippet-spacing-md) var(--snippet-spacing-lg);
  background: var(--editor-bg-toolbar);
  border-bottom: 1px solid var(--editor-border-toolbar);
  backdrop-filter: var(--snippet-backdrop-blur-md);
  min-height: var(--snippet-toolbar-height);
}

.toolbar-left {
  display: flex;
  align-items: center;
  gap: var(--snippet-spacing-md);
}

.language-badge {
  display: inline-flex;
  align-items: center;
  gap: var(--snippet-spacing-sm);
  padding: var(--snippet-spacing-xs) var(--snippet-spacing-md);
  background: var(--snippet-bg-tertiary);
  border: 1px solid var(--snippet-border-secondary);
  border-radius: var(--snippet-radius-lg);
  font-size: var(--snippet-font-size-body-xs);
  font-weight: var(--snippet-font-weight-medium);
  color: var(--snippet-text-primary);
  transition: all var(--snippet-transition-normal);
}

.language-badge:hover {
  background: var(--snippet-bg-hover);
  transform: var(--snippet-transform-translate-y-hover);
}

.language-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}

.file-info {
  display: flex;
  gap: var(--snippet-spacing-md);
  font-size: var(--snippet-font-size-body-xs);
  color: var(--snippet-text-secondary);
}

.file-size, .line-count {
  display: flex;
  align-items: center;
  gap: var(--snippet-spacing-xs);
  font-family: var(--snippet-font-family-code);
}

.toolbar-right {
  display: flex;
  align-items: center;
}

.toolbar-actions {
  display: flex;
  gap: var(--snippet-spacing-xs);
}

.toolbar-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: var(--snippet-button-size-md);
  height: var(--snippet-button-size-md);
  padding: 0;
  border: 1px solid var(--snippet-border-secondary);
  border-radius: var(--snippet-radius-sm);
  background: var(--snippet-bg-secondary);
  color: var(--snippet-text-primary);
  cursor: pointer;
  transition: all var(--snippet-transition-normal);
  position: relative;
  overflow: hidden;
}

.toolbar-btn::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.1), transparent);
  transition: left var(--snippet-transition-normal);
}

.toolbar-btn:hover::before {
  left: 100%;
}

.toolbar-btn:hover {
  background: var(--snippet-bg-hover);
  border-color: var(--snippet-border-primary);
  transform: var(--snippet-transform-translate-y-hover);
  box-shadow: var(--snippet-shadow-sm);
}

.toolbar-btn:active {
  transform: var(--snippet-transform-translate-y-active);
}

.toolbar-btn.active {
  background: var(--snippet-bg-brand);
  border-color: var(--snippet-color-brand);
  color: white;
}

.toolbar-btn svg {
  width: 16px;
  height: 16px;
  flex-shrink: 0;
}

/* 编辑器容器 */
.editor-container {
  flex: 1;
  position: relative;
  min-height: var(--snippet-editor-min-height);
  background: var(--editor-bg-primary);
  border-bottom: 1px solid var(--editor-border-primary);
}

.editor-container.is-fullscreen {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: var(--snippet-z-index-fullscreen);
  border-radius: 0;
  border: none;
}

.editor-container.is-loading {
  pointer-events: none;
}

/* 状态栏 */
.editor-status {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--snippet-spacing-sm) var(--snippet-spacing-lg);
  background: var(--editor-bg-statusbar);
  border-top: 1px solid var(--editor-border-statusbar);
  font-size: var(--snippet-font-size-body-xs);
  color: var(--snippet-text-secondary);
  backdrop-filter: var(--snippet-backdrop-blur-sm);
  min-height: var(--snippet-statusbar-height);
}

.status-left {
  display: flex;
  align-items: center;
  gap: var(--snippet-spacing-md);
}

.status-right {
  display: flex;
  align-items: center;
  gap: var(--snippet-spacing-md);
}

.cursor-position, .selection-info {
  font-family: var(--snippet-font-family-code);
  color: var(--snippet-text-primary);
}

.encoding, .line-ending {
  font-family: var(--snippet-font-family-code);
  text-transform: uppercase;
  color: var(--snippet-text-tertiary);
}

/* 加载遮罩 */
.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: var(--snippet-bg-overlay);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: var(--snippet-z-index-modal);
  backdrop-filter: var(--snippet-backdrop-blur-lg);
}

.loading-content {
  text-align: center;
  padding: var(--snippet-spacing-xl);
  background: var(--snippet-bg-secondary);
  border-radius: var(--snippet-radius-lg);
  border: 1px solid var(--snippet-border-primary);
  box-shadow: var(--snippet-shadow-lg);
  min-width: 200px;
}

.loading-spinner {
  width: 32px;
  height: 32px;
  border: 3px solid var(--snippet-border-secondary);
  border-top: 3px solid var(--snippet-color-brand);
  border-radius: 50%;
  animation: var(--snippet-animation-spin);
  margin: 0 auto var(--snippet-spacing-md);
}

.loading-text {
  color: var(--snippet-text-primary);
  font-size: var(--snippet-font-size-body-sm);
  font-weight: var(--snippet-font-weight-medium);
}

/* 动画效果 */
@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 响应式设计 */
@media (max-width: 768px) {
  .code-viewer-enhanced {
    border-radius: var(--snippet-radius-sm);
  }
  
  .editor-toolbar {
    padding: var(--snippet-spacing-sm);
    flex-wrap: wrap;
    gap: var(--snippet-spacing-sm);
    min-height: auto;
  }
  
  .toolbar-left {
    order: 2;
    width: 100%;
    justify-content: space-between;
  }
  
  .toolbar-right {
    order: 1;
    width: 100%;
    justify-content: flex-end;
  }
  
  .file-info {
    gap: var(--snippet-spacing-sm);
  }
  
  .toolbar-btn {
    width: var(--snippet-button-size-sm);
    height: var(--snippet-button-size-sm);
  }
  
  .toolbar-btn svg {
    width: 14px;
    height: 14px;
  }
  
  .editor-status {
    padding: var(--snippet-spacing-sm);
    font-size: var(--snippet-font-size-body-xs);
    min-height: auto;
  }
  
  .status-left, .status-right {
    gap: var(--snippet-spacing-sm);
  }
  
  .loading-content {
    padding: var(--snippet-spacing-lg);
    min-width: 150px;
  }
}

@media (max-width: 480px) {
  .language-badge {
    padding: var(--snippet-spacing-xs) var(--snippet-spacing-sm);
    font-size: var(--snippet-font-size-body-xs);
  }
  
  .file-info {
    font-size: var(--snippet-font-size-body-xs);
    gap: var(--snippet-spacing-xs);
  }
  
  .toolbar-actions {
    gap: 2px;
  }
  
  .toolbar-btn {
    width: 28px;
    height: 28px;
  }
  
  .toolbar-btn svg {
    width: 12px;
    height: 12px;
  }
}

/* 深色模式支持 */
@media (prefers-color-scheme: dark) {
  .code-viewer-enhanced {
    border-color: var(--editor-border-primary-dark);
  }
  
  .editor-toolbar {
    background: var(--editor-bg-toolbar-dark);
    border-color: var(--editor-border-toolbar-dark);
  }
  
  .language-badge {
    background: var(--snippet-bg-tertiary-dark);
    border-color: var(--snippet-border-secondary-dark);
  }
  
  .toolbar-btn {
    background: var(--snippet-bg-secondary-dark);
    border-color: var(--snippet-border-secondary-dark);
  }
  
  .editor-status {
    background: var(--editor-bg-statusbar-dark);
    border-color: var(--editor-border-statusbar-dark);
  }
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .code-viewer-enhanced {
    border-width: 2px;
  }
  
  .toolbar-btn {
    border-width: 2px;
  }
  
  .language-badge {
    border-width: 2px;
  }
}

/* 减少动画偏好 */
@media (prefers-reduced-motion: reduce) {
  .toolbar-btn,
  .language-badge,
  .loading-spinner {
    animation-duration: 0.01ms !important;
    transition-duration: 0.01ms !important;
  }
}

/* 无障碍支持 */
.toolbar-btn:focus-visible {
  outline: 2px solid var(--snippet-color-brand);
  outline-offset: 2px;
}

.language-badge:focus-visible {
  outline: 2px solid var(--snippet-color-brand);
  outline-offset: 2px;
}

/* 打印样式 */
@media print {
  .code-viewer-enhanced {
    border: none;
    border-radius: 0;
    page-break-inside: avoid;
  }
  
  .editor-toolbar,
  .editor-status {
    display: none;
  }
  
  .editor-container {
    border: none;
    min-height: auto;
  }
  
  .loading-overlay {
    display: none;
  }
}
</style>