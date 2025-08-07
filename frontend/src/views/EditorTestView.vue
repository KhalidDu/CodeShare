<template>
  <div class="editor-test-view">
    <PageTitle title="代码编辑器测试" />

    <div class="test-container">
      <div class="editor-section">
        <h3>代码编辑器</h3>
        <CodeEditor
          v-model="code"
          :language="selectedLanguage"
          :theme="selectedTheme"
          :height="editorHeight"
          @language-change="onLanguageChange"
          @theme-change="onThemeChange"
        />
      </div>

      <div class="viewer-section">
        <h3>代码查看器</h3>
        <CodeViewer
          :code="code"
          :language="selectedLanguage"
          :theme="selectedTheme"
          :height="viewerHeight"
          @copy="onCodeCopy"
        />
      </div>

      <div class="settings-section">
        <h3>编辑器设置</h3>
        <EditorSettings
          :settings="editorSettings"
          @update:settings="onSettingsUpdate"
        />
      </div>

      <div class="info-section">
        <h3>当前状态</h3>
        <div class="info-grid">
          <div class="info-item">
            <label>语言:</label>
            <span>{{ selectedLanguage }}</span>
          </div>
          <div class="info-item">
            <label>主题:</label>
            <span>{{ selectedTheme }}</span>
          </div>
          <div class="info-item">
            <label>代码长度:</label>
            <span>{{ code.length }} 字符</span>
          </div>
          <div class="info-item">
            <label>行数:</label>
            <span>{{ lineCount }} 行</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import PageTitle from '@/components/common/PageTitle.vue'
import CodeEditor from '@/components/editor/CodeEditor.vue'
import CodeViewer from '@/components/editor/CodeViewer.vue'
import EditorSettings from '@/components/editor/EditorSettings.vue'
import { useEditorSettings } from '@/composables/useEditorSettings'
import type { EditorOptions } from '@/types'

// 使用编辑器设置组合式函数
const { settings: editorSettings, updateSettings } = useEditorSettings()

// 响应式数据
const code = ref(`// 欢迎使用代码编辑器
function fibonacci(n) {
  if (n <= 1) {
    return n;
  }
  return fibonacci(n - 1) + fibonacci(n - 2);
}

// 计算斐波那契数列的前10项
for (let i = 0; i < 10; i++) {
  console.log(\`fibonacci(\${i}) = \${fibonacci(i)}\`);
}

// 这是一个示例代码片段
const greeting = "Hello, World!";
console.log(greeting);`)

const selectedLanguage = ref('javascript')
const selectedTheme = ref('vs')
const editorHeight = ref('400px')
const viewerHeight = ref('300px')

// 计算属性
const lineCount = computed(() => {
  return code.value.split('\n').length
})

/**
 * 语言变化处理
 */
const onLanguageChange = (language: string) => {
  selectedLanguage.value = language
  console.log('Language changed to:', language)
}

/**
 * 主题变化处理
 */
const onThemeChange = (theme: string) => {
  selectedTheme.value = theme
  console.log('Theme changed to:', theme)
}

/**
 * 代码复制处理
 */
const onCodeCopy = (copiedCode: string) => {
  console.log('Code copied:', copiedCode.substring(0, 50) + '...')
  // 这里可以添加复制统计逻辑
}

/**
 * 设置更新处理
 */
const onSettingsUpdate = (newSettings: EditorOptions) => {
  updateSettings(newSettings)

  // 同步主题设置
  if (newSettings.theme) {
    selectedTheme.value = newSettings.theme
  }

  console.log('Settings updated:', newSettings)
}
</script>

<style scoped>
.editor-test-view {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.test-container {
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-template-rows: auto auto;
  gap: 20px;
  margin-top: 20px;
}

.editor-section {
  grid-column: 1 / 2;
  grid-row: 1 / 2;
}

.viewer-section {
  grid-column: 2 / 3;
  grid-row: 1 / 2;
}

.settings-section {
  grid-column: 1 / 2;
  grid-row: 2 / 3;
}

.info-section {
  grid-column: 2 / 3;
  grid-row: 2 / 3;
}

.editor-section h3,
.viewer-section h3,
.settings-section h3,
.info-section h3 {
  margin: 0 0 12px 0;
  font-size: 16px;
  font-weight: 600;
  color: #24292f;
}

.info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: #f6f8fa;
  border: 1px solid #d0d7de;
  border-radius: 4px;
  font-size: 12px;
}

.info-item label {
  font-weight: 500;
  color: #656d76;
}

.info-item span {
  color: #24292f;
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
}

@media (max-width: 768px) {
  .test-container {
    grid-template-columns: 1fr;
    grid-template-rows: auto auto auto auto;
  }

  .editor-section {
    grid-column: 1 / 2;
    grid-row: 1 / 2;
  }

  .viewer-section {
    grid-column: 1 / 2;
    grid-row: 2 / 3;
  }

  .settings-section {
    grid-column: 1 / 2;
    grid-row: 3 / 4;
  }

  .info-section {
    grid-column: 1 / 2;
    grid-row: 4 / 5;
  }
}
</style>
