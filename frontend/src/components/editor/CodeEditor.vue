<template>
  <div class="code-editor">
    <div class="editor-toolbar">
      <div class="editor-controls">
        <select v-model="selectedLanguage" @change="onLanguageChange" class="language-select">
          <option value="">选择语言</option>
          <option v-for="lang in supportedLanguages" :key="lang.value" :value="lang.value">
            {{ lang.label }}
          </option>
        </select>

        <select v-model="selectedTheme" @change="onThemeChange" class="theme-select">
          <option value="vs">浅色主题</option>
          <option value="vs-dark">深色主题</option>
          <option value="hc-black">高对比度</option>
        </select>

        <button @click="formatCode" class="format-btn" :disabled="!canFormat">
          格式化代码
        </button>
      </div>
    </div>

    <div ref="editorContainer" class="editor-container"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, nextTick } from 'vue'
import * as monaco from 'monaco-editor'
import loader from '@monaco-editor/loader'

// Props 定义
interface Props {
  modelValue?: string
  language?: string
  theme?: string
  readonly?: boolean
  height?: string
  options?: monaco.editor.IStandaloneEditorConstructionOptions
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '',
  language: 'javascript',
  theme: 'vs',
  readonly: false,
  height: '400px',
  options: () => ({})
})

// Emits 定义
interface Emits {
  'update:modelValue': [value: string]
  'language-change': [language: string]
  'theme-change': [theme: string]
}

const emit = defineEmits<Emits>()

// 响应式数据
const editorContainer = ref<HTMLElement>()
const selectedLanguage = ref(props.language)
const selectedTheme = ref(props.theme)
const canFormat = ref(false)

let editor: monaco.editor.IStandaloneCodeEditor | null = null

// 支持的编程语言列表
const supportedLanguages = [
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
 * 初始化 Monaco Editor
 */
const initEditor = async () => {
  if (!editorContainer.value) return

  try {
    // 配置 Monaco Editor 加载器
    loader.config({
      paths: {
        vs: 'https://cdn.jsdelivr.net/npm/monaco-editor@0.45.0/min/vs'
      }
    })

    const monacoInstance = await loader.init()

    // 创建编辑器实例
    editor = monacoInstance.editor.create(editorContainer.value, {
      value: props.modelValue,
      language: selectedLanguage.value,
      theme: selectedTheme.value,
      readOnly: props.readonly,
      automaticLayout: true,
      minimap: { enabled: true },
      scrollBeyondLastLine: false,
      fontSize: 14,
      lineNumbers: 'on',
      renderWhitespace: 'selection',
      tabSize: 2,
      insertSpaces: true,
      wordWrap: 'on',
      folding: true,
      foldingStrategy: 'indentation',
      showFoldingControls: 'always',
      ...props.options
    })

    // 监听内容变化
    editor.onDidChangeModelContent(() => {
      if (editor) {
        const value = editor.getValue()
        emit('update:modelValue', value)
      }
    })

    // 更新格式化按钮状态
    updateFormatButtonState()

  } catch (error) {
    console.error('Failed to initialize Monaco Editor:', error)
  }
}

/**
 * 更新格式化按钮状态
 */
const updateFormatButtonState = () => {
  const formattableLanguages = [
    'javascript', 'typescript', 'json', 'html', 'css', 'scss', 'xml'
  ]
  canFormat.value = formattableLanguages.includes(selectedLanguage.value)
}

/**
 * 语言变化处理
 */
const onLanguageChange = () => {
  if (editor) {
    const model = editor.getModel()
    if (model) {
      monaco.editor.setModelLanguage(model, selectedLanguage.value)
    }
  }
  updateFormatButtonState()
  emit('language-change', selectedLanguage.value)
}

/**
 * 主题变化处理
 */
const onThemeChange = () => {
  if (editor) {
    monaco.editor.setTheme(selectedTheme.value)
  }
  emit('theme-change', selectedTheme.value)
}

/**
 * 格式化代码
 */
const formatCode = async () => {
  if (!editor || !canFormat.value) return

  try {
    await editor.getAction('editor.action.formatDocument')?.run()
  } catch (error) {
    console.error('Failed to format code:', error)
  }
}

/**
 * 设置编辑器内容
 */
const setValue = (value: string) => {
  if (editor && editor.getValue() !== value) {
    editor.setValue(value)
  }
}

/**
 * 获取编辑器内容
 */
const getValue = (): string => {
  return editor?.getValue() || ''
}

/**
 * 设置编辑器语言
 */
const setLanguage = (language: string) => {
  selectedLanguage.value = language
  onLanguageChange()
}

/**
 * 设置编辑器主题
 */
const setTheme = (theme: string) => {
  selectedTheme.value = theme
  onThemeChange()
}

/**
 * 聚焦编辑器
 */
const focus = () => {
  editor?.focus()
}

// 监听 props 变化
watch(() => props.modelValue, (newValue) => {
  if (editor && editor.getValue() !== newValue) {
    setValue(newValue)
  }
})

watch(() => props.language, (newLanguage) => {
  if (newLanguage !== selectedLanguage.value) {
    setLanguage(newLanguage)
  }
})

watch(() => props.theme, (newTheme) => {
  if (newTheme !== selectedTheme.value) {
    setTheme(newTheme)
  }
})

// 生命周期钩子
onMounted(async () => {
  await nextTick()
  await initEditor()
})

onUnmounted(() => {
  if (editor) {
    editor.dispose()
    editor = null
  }
})

// 暴露方法给父组件
defineExpose({
  setValue,
  getValue,
  setLanguage,
  setTheme,
  focus,
  formatCode
})
</script>

<style scoped>
.code-editor {
  border: 1px solid rgba(0, 0, 0, 0.06);
  border-radius: 16px;
  overflow: hidden;
  background: linear-gradient(135deg, #ffffff 0%, #fafbfc 100%);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
}

.editor-toolbar {
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  border-bottom: 1px solid rgba(0, 0, 0, 0.06);
  padding: 12px 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  backdrop-filter: blur(10px);
}

.editor-controls {
  display: flex;
  gap: 12px;
  align-items: center;
}

.language-select,
.theme-select {
  padding: 8px 12px;
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.9);
  font-size: 13px;
  font-weight: 500;
  min-width: 140px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  backdrop-filter: blur(10px);
}

.language-select:focus,
.theme-select:focus {
  outline: none;
  border-color: #007bff;
  box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
  background: rgba(255, 255, 255, 1);
}

.format-btn {
  padding: 8px 16px;
  background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
  color: #fff;
  border: none;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 2px 8px rgba(0, 123, 255, 0.2);
}

.format-btn:hover:not(:disabled) {
  background: linear-gradient(135deg, #0056b3 0%, #004085 100%);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 123, 255, 0.3);
}

.format-btn:disabled {
  background: linear-gradient(135deg, #8c959f 0%, #6c757d 100%);
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.editor-container {
  height: v-bind(height);
  width: 100%;
}

/* 深色主题样式 */
.code-editor.dark {
  border-color: #30363d;
  background: #0d1117;
}

.code-editor.dark .editor-toolbar {
  background: #161b22;
  border-bottom-color: #30363d;
}

.code-editor.dark .language-select,
.code-editor.dark .theme-select {
  background: #21262d;
  border-color: #30363d;
  color: #f0f6fc;
}
</style>
