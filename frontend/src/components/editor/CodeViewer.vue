<template>
  <div class="code-viewer">
    <div class="viewer-header" v-if="showHeader">
      <div class="language-badge" v-if="language">
        {{ getLanguageLabel(language) }}
      </div>
      <div class="viewer-actions">
        <button @click="copyCode" class="copy-btn" :disabled="copying">
          <span v-if="copying">复制中...</span>
          <span v-else-if="copied">已复制!</span>
          <span v-else>复制代码</span>
        </button>
      </div>
    </div>

    <div ref="viewerContainer" class="viewer-container"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, nextTick } from 'vue'
import * as monaco from 'monaco-editor'
import loader from '@monaco-editor/loader'
import type { SupportedLanguage } from '@/types/editor'

// Props 定义
interface Props {
  code: string
  language?: string
  theme?: string
  height?: string
  showHeader?: boolean
  showLineNumbers?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  code: '',
  language: 'javascript',
  theme: 'vs',
  height: '300px',
  showHeader: true,
  showLineNumbers: true
})

// Emits 定义
interface Emits {
  'copy': [code: string]
}

const emit = defineEmits<Emits>()

// 响应式数据
const viewerContainer = ref<HTMLElement>()
const copying = ref(false)
const copied = ref(false)

let editor: monaco.editor.IStandaloneCodeEditor | null = null

// 支持的编程语言列表
const supportedLanguages: SupportedLanguage[] = [
  { value: 'javascript', label: 'JavaScript' },
  { value: 'typescript', label: 'TypeScript' },
  { value: 'python', label: 'Python' },
  { value: 'java', label: 'Java' },
  { value: 'csharp', label: 'C#' },
  { value: 'cpp', label: 'C++' },
  { value: 'c', label: 'C' },
  { value: 'html', label: 'HTML' },
  { value: 'css', label: 'CSS' },
  { value: 'scss', label: 'SCSS' },
  { value: 'json', label: 'JSON' },
  { value: 'xml', label: 'XML' },
  { value: 'yaml', label: 'YAML' },
  { value: 'markdown', label: 'Markdown' },
  { value: 'sql', label: 'SQL' },
  { value: 'shell', label: 'Shell' },
  { value: 'powershell', label: 'PowerShell' },
  { value: 'php', label: 'PHP' },
  { value: 'ruby', label: 'Ruby' },
  { value: 'go', label: 'Go' },
  { value: 'rust', label: 'Rust' },
  { value: 'swift', label: 'Swift' },
  { value: 'kotlin', label: 'Kotlin' },
  { value: 'dart', label: 'Dart' },
  { value: 'r', label: 'R' },
  { value: 'scala', label: 'Scala' },
  { value: 'perl', label: 'Perl' },
  { value: 'lua', label: 'Lua' }
]

/**
 * 获取语言显示标签
 */
const getLanguageLabel = (language: string): string => {
  const lang = supportedLanguages.find(l => l.value === language)
  return lang?.label || language.toUpperCase()
}

/**
 * 初始化代码查看器
 */
const initViewer = async () => {
  if (!viewerContainer.value) return

  try {
    // 配置 Monaco Editor 加载器
    loader.config({
      paths: {
        vs: 'https://cdn.jsdelivr.net/npm/monaco-editor@0.45.0/min/vs'
      }
    })

    const monacoInstance = await loader.init()

    // 创建只读编辑器实例
    editor = monacoInstance.editor.create(viewerContainer.value, {
      value: props.code,
      language: props.language,
      theme: props.theme,
      readOnly: true,
      automaticLayout: true,
      minimap: { enabled: false },
      scrollBeyondLastLine: false,
      fontSize: 14,
      lineNumbers: props.showLineNumbers ? 'on' : 'off',
      renderWhitespace: 'none',
      folding: false,
      glyphMargin: false,
      lineDecorationsWidth: 0,
      lineNumbersMinChars: 3,
      scrollbar: {
        vertical: 'auto',
        horizontal: 'auto',
        verticalScrollbarSize: 8,
        horizontalScrollbarSize: 8
      },
      overviewRulerLanes: 0,
      hideCursorInOverviewRuler: true,
      overviewRulerBorder: false,
      contextmenu: false
    })

  } catch (error) {
    console.error('Failed to initialize Monaco Viewer:', error)
  }
}

/**
 * 复制代码到剪贴板
 */
const copyCode = async () => {
  if (copying.value) return

  copying.value = true

  try {
    await navigator.clipboard.writeText(props.code)
    copied.value = true
    emit('copy', props.code)

    // 2秒后重置状态
    setTimeout(() => {
      copied.value = false
    }, 2000)

  } catch (error) {
    console.error('Failed to copy code:', error)
    // 降级处理：选择文本
    if (editor) {
      editor.focus()
      editor.setSelection(editor.getModel()?.getFullModelRange() || new monaco.Range(1, 1, 1, 1))
    }
  } finally {
    copying.value = false
  }
}

/**
 * 更新代码内容
 */
const updateCode = (newCode: string) => {
  if (editor && editor.getValue() !== newCode) {
    editor.setValue(newCode)
  }
}

/**
 * 更新语言
 */
const updateLanguage = (newLanguage: string) => {
  if (editor) {
    const model = editor.getModel()
    if (model) {
      monaco.editor.setModelLanguage(model, newLanguage)
    }
  }
}

/**
 * 更新主题
 */
const updateTheme = (newTheme: string) => {
  if (editor) {
    monaco.editor.setTheme(newTheme)
  }
}

// 监听 props 变化
watch(() => props.code, updateCode)
watch(() => props.language, updateLanguage)
watch(() => props.theme, updateTheme)

// 生命周期钩子
onMounted(async () => {
  await nextTick()
  await initViewer()
})

onUnmounted(() => {
  if (editor) {
    editor.dispose()
    editor = null
  }
})

// 暴露方法给父组件
defineExpose({
  copyCode,
  updateCode,
  updateLanguage,
  updateTheme
})
</script>

<style scoped>
.code-viewer {
  border: 1px solid #e1e5e9;
  border-radius: 6px;
  overflow: hidden;
  background: #fff;
}

.viewer-header {
  background: #f8f9fa;
  border-bottom: 1px solid #e1e5e9;
  padding: 8px 12px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.language-badge {
  background: #0969da;
  color: #fff;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 500;
  text-transform: uppercase;
}

.viewer-actions {
  display: flex;
  gap: 8px;
}

.copy-btn {
  padding: 4px 12px;
  background: #f6f8fa;
  color: #24292f;
  border: 1px solid #d0d7de;
  border-radius: 4px;
  font-size: 12px;
  cursor: pointer;
  transition: all 0.2s;
}

.copy-btn:hover:not(:disabled) {
  background: #f3f4f6;
  border-color: #8c959f;
}

.copy-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.viewer-container {
  height: v-bind(height);
  width: 100%;
}

/* 深色主题样式 */
.code-viewer.dark {
  border-color: #30363d;
  background: #0d1117;
}

.code-viewer.dark .viewer-header {
  background: #161b22;
  border-bottom-color: #30363d;
}

.code-viewer.dark .copy-btn {
  background: #21262d;
  color: #f0f6fc;
  border-color: #30363d;
}

.code-viewer.dark .copy-btn:hover:not(:disabled) {
  background: #30363d;
}
</style>
