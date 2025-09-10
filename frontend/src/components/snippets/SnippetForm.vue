<template>
  <div class="snippet-form">
    <form @submit.prevent="onSubmit" class="form">
      <div class="form-header">
        <h2>{{ isEditing ? '编辑代码片段' : '创建代码片段' }}</h2>
        <div class="form-actions">
          <button
            type="button"
            @click="onCancel"
            class="btn btn-secondary"
            :disabled="loading"
          >
            取消
          </button>
          <button
            type="submit"
            class="btn btn-primary"
            :disabled="loading || !isFormValid"
          >
            {{ loading ? '保存中...' : (isEditing ? '更新' : '创建') }}
          </button>
        </div>
      </div>

      <div class="form-body">
        <div class="form-group">
          <label for="title" class="form-label">标题 *</label>
          <input
            id="title"
            v-model="formData.title"
            type="text"
            class="form-input"
            :class="{ error: errors.title }"
            placeholder="请输入代码片段标题"
            maxlength="200"
            required
          />
          <span v-if="errors.title" class="error-message">{{ errors.title }}</span>
        </div>

        <div class="form-group">
          <label for="description" class="form-label">描述</label>
          <textarea
            id="description"
            v-model="formData.description"
            class="form-textarea"
            :class="{ error: errors.description }"
            placeholder="请输入代码片段描述"
            rows="3"
            maxlength="1000"
          ></textarea>
          <span v-if="errors.description" class="error-message">{{ errors.description }}</span>
        </div>

        <div class="form-group">
          <label class="form-label">代码 *</label>
          <CodeEditor
            v-model="formData.code"
            :language="formData.language"
            :theme="editorTheme"
            :height="editorHeight"
            @language-change="onLanguageChange"
            class="code-editor"
            :class="{ error: errors.code }"
          />
          <span v-if="errors.code" class="error-message">{{ errors.code }}</span>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label for="language" class="form-label">编程语言 *</label>
            <select
              id="language"
              v-model="formData.language"
              class="form-select"
              :class="{ error: errors.language }"
              required
            >
              <option value="">选择语言</option>
              <option
                v-for="lang in supportedLanguages"
                :key="lang.value"
                :value="lang.value"
              >
                {{ lang.label }}
              </option>
            </select>
            <span v-if="errors.language" class="error-message">{{ errors.language }}</span>
          </div>

          <div class="form-group">
            <label class="form-label">可见性</label>
            <div class="visibility-options">
              <label class="radio-label">
                <input
                  v-model="formData.isPublic"
                  type="radio"
                  :value="false"
                  class="radio-input"
                />
                <span class="radio-text">私有</span>
              </label>
              <label class="radio-label">
                <input
                  v-model="formData.isPublic"
                  type="radio"
                  :value="true"
                  class="radio-input"
                />
                <span class="radio-text">公开</span>
              </label>
            </div>
          </div>
        </div>

        <div class="form-group">
          <TagSelector
            v-model="formData.tags"
            :disabled="loading"
          />
        </div>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import CodeEditor from '@/components/editor/CodeEditor.vue'
import TagSelector from '@/components/snippets/TagSelector.vue'
import { useEditorSettings } from '@/composables/useEditorSettings'
import type { CodeSnippet, CreateSnippetRequest, UpdateSnippetRequest, Tag, SupportedLanguage } from '@/types'

// Props 定义
interface Props {
  snippet?: CodeSnippet
  isEditing?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  isEditing: false
})

// Emits 定义
interface Emits {
  'submit': [data: any] // 使用 any 类型以支持两种请求类型
  'cancel': []
}

const emit = defineEmits<Emits>()

// 路由
const router = useRouter()

// 编辑器设置
const { settings } = useEditorSettings()

// 响应式数据
const loading = ref(false)
const editorHeight = ref('400px')

// 表单数据
const formData = reactive({
  title: '',
  description: '',
  code: '',
  language: 'javascript',
  isPublic: false,
  tags: [] as Tag[]
})

// 表单验证错误
const errors = reactive({
  title: '',
  description: '',
  code: '',
  language: ''
})

// 支持的编程语言
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
  { value: 'dart', label: 'Dart' }
]

// 计算属性
const editorTheme = computed(() => settings.value.theme || 'vs')

const isFormValid = computed(() => {
  return formData.title.trim() &&
         formData.code.trim() &&
         formData.language &&
         !Object.values(errors).some(error => error)
})

/**
 * 初始化表单数据
 */
const initializeForm = () => {
  if (props.snippet) {
    formData.title = props.snippet.title
    formData.description = props.snippet.description
    formData.code = props.snippet.code
    formData.language = props.snippet.language
    formData.isPublic = props.snippet.isPublic
    formData.tags = [...props.snippet.tags]
  }
}

/**
 * 验证表单
 */
const validateForm = (): boolean => {
  // 清空之前的错误
  Object.keys(errors).forEach(key => {
    errors[key as keyof typeof errors] = ''
  })

  let isValid = true

  // 验证标题
  if (!formData.title.trim()) {
    errors.title = '标题不能为空'
    isValid = false
  } else if (formData.title.length > 200) {
    errors.title = '标题长度不能超过200个字符'
    isValid = false
  }

  // 验证代码
  if (!formData.code.trim()) {
    errors.code = '代码内容不能为空'
    isValid = false
  }

  // 验证语言
  if (!formData.language) {
    errors.language = '请选择编程语言'
    isValid = false
  }

  // 验证描述长度
  if (formData.description && formData.description.length > 1000) {
    errors.description = '描述长度不能超过1000个字符'
    isValid = false
  }

  return isValid
}

/**
 * 语言变化处理
 */
const onLanguageChange = (language: string) => {
  formData.language = language
}

/**
 * 表单提交处理
 */
const onSubmit = async () => {
  if (!validateForm()) {
    return
  }

  loading.value = true

  try {
    const submitData = {
      title: formData.title.trim(),
      description: formData.description.trim(),
      code: formData.code,
      language: formData.language,
      isPublic: formData.isPublic,
      tags: formData.tags.map(tag => tag.name)
    }

    emit('submit', submitData)
  } catch (error) {
    console.error('Form submission error:', error)
  } finally {
    loading.value = false
  }
}

/**
 * 取消处理
 */
const onCancel = () => {
  emit('cancel')
}

// 监听 snippet 变化
watch(() => props.snippet, () => {
  initializeForm()
}, { immediate: true, deep: true })

// 生命周期
onMounted(() => {
  initializeForm()
})
</script>

<style scoped>
.snippet-form {
  max-width: 1000px;
  margin: 0 auto;
  background: #fff;
  border: 1px solid #e1e5e9;
  border-radius: 8px;
  overflow: hidden;
}

.form-header {
  background: #f6f8fa;
  border-bottom: 1px solid #e1e5e9;
  padding: var(--spacing-lg) var(--spacing-xl);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.form-header h2 {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: #24292f;
}

.form-actions {
  display: flex;
  gap: var(--spacing-sm);
}

.btn {
  padding: var(--spacing-sm) var(--spacing-md);
  border: 1px solid;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background: #0969da;
  color: #fff;
  border-color: #0969da;
}

.btn-primary:hover:not(:disabled) {
  background: #0860ca;
  border-color: #0860ca;
}

.btn-secondary {
  background: #f6f8fa;
  color: #24292f;
  border-color: #d0d7de;
}

.btn-secondary:hover:not(:disabled) {
  background: #f3f4f6;
  border-color: #8c959f;
}

.form-body {
  padding: var(--spacing-xl);
}

.form-group {
  margin-bottom: var(--spacing-lg);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-lg);
}

.form-label {
  display: block;
  font-size: 14px;
  font-weight: 500;
  color: #24292f;
  margin-bottom: var(--spacing-sm);
}

.form-input,
.form-textarea,
.form-select {
  width: 100%;
  padding: var(--spacing-sm) var(--spacing-md);
  border: 1px solid #d0d7de;
  border-radius: 6px;
  font-size: 14px;
  background: #fff;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.form-input:focus,
.form-textarea:focus,
.form-select:focus {
  outline: none;
  border-color: #0969da;
  box-shadow: 0 0 0 2px rgba(9, 105, 218, 0.1);
}

.form-input.error,
.form-textarea.error,
.form-select.error,
.code-editor.error {
  border-color: #d1242f;
}

.form-textarea {
  resize: vertical;
  min-height: 80px;
}

.visibility-options {
  display: flex;
  gap: var(--spacing-md);
  margin-top: var(--spacing-sm);
}

.radio-label {
  display: flex;
  align-items: center;
  cursor: pointer;
  font-size: 14px;
}

.radio-input {
  margin-right: var(--spacing-sm);
  cursor: pointer;
}

.radio-text {
  color: #24292f;
}

.error-message {
  display: block;
  color: #d1242f;
  font-size: 12px;
  margin-top: var(--spacing-xs);
}

.code-editor {
  border-radius: 6px;
  overflow: hidden;
}

@media (max-width: 768px) {
  .form-header {
    flex-direction: column;
    gap: var(--spacing-sm);
    align-items: stretch;
  }

  .form-actions {
    justify-content: flex-end;
  }

  .form-body {
    padding: var(--spacing-lg);
  }

  .form-row {
    grid-template-columns: 1fr;
    gap: var(--spacing-md);
  }
}

/* 深色主题样式 */
.snippet-form.dark {
  background: #0d1117;
  border-color: #30363d;
}

.snippet-form.dark .form-header {
  background: #161b22;
  border-bottom-color: #30363d;
}

.snippet-form.dark .form-header h2 {
  color: #f0f6fc;
}

.snippet-form.dark .form-label {
  color: #f0f6fc;
}

.snippet-form.dark .form-input,
.snippet-form.dark .form-textarea,
.snippet-form.dark .form-select {
  background: #21262d;
  border-color: #30363d;
  color: #f0f6fc;
}

.snippet-form.dark .radio-text {
  color: #f0f6fc;
}
</style>
