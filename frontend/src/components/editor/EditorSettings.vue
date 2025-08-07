<template>
  <div class="editor-settings">
    <div class="settings-header">
      <h3>编辑器设置</h3>
      <button @click="resetToDefaults" class="reset-btn">
        重置默认
      </button>
    </div>

    <div class="settings-content">
      <div class="setting-group">
        <label class="setting-label">主题</label>
        <select v-model="localSettings.theme" @change="onSettingChange" class="setting-select">
          <option value="vs">浅色主题</option>
          <option value="vs-dark">深色主题</option>
          <option value="hc-black">高对比度</option>
        </select>
      </div>

      <div class="setting-group">
        <label class="setting-label">字体大小</label>
        <div class="font-size-control">
          <input
            v-model.number="localSettings.fontSize"
            @change="onSettingChange"
            type="range"
            min="10"
            max="24"
            step="1"
            class="font-size-slider"
          />
          <span class="font-size-value">{{ localSettings.fontSize }}px</span>
        </div>
      </div>

      <div class="setting-group">
        <label class="setting-label">Tab 大小</label>
        <select v-model.number="localSettings.tabSize" @change="onSettingChange" class="setting-select">
          <option :value="2">2 空格</option>
          <option :value="4">4 空格</option>
          <option :value="8">8 空格</option>
        </select>
      </div>

      <div class="setting-group">
        <label class="setting-label">自动换行</label>
        <select v-model="localSettings.wordWrap" @change="onSettingChange" class="setting-select">
          <option value="off">关闭</option>
          <option value="on">开启</option>
          <option value="bounded">边界换行</option>
        </select>
      </div>

      <div class="setting-group">
        <div class="checkbox-group">
          <label class="checkbox-label">
            <input
              v-model="localSettings.insertSpaces"
              @change="onSettingChange"
              type="checkbox"
              class="setting-checkbox"
            />
            <span class="checkbox-text">使用空格代替 Tab</span>
          </label>
        </div>
      </div>

      <div class="setting-group">
        <div class="checkbox-group">
          <label class="checkbox-label">
            <input
              v-model="localSettings.minimap!.enabled"
              @change="onSettingChange"
              type="checkbox"
              class="setting-checkbox"
            />
            <span class="checkbox-text">显示代码缩略图</span>
          </label>
        </div>
      </div>

      <div class="setting-group">
        <div class="checkbox-group">
          <label class="checkbox-label">
            <input
              v-model="localSettings.folding"
              @change="onSettingChange"
              type="checkbox"
              class="setting-checkbox"
            />
            <span class="checkbox-text">启用代码折叠</span>
          </label>
        </div>
      </div>

      <div class="setting-group">
        <label class="setting-label">行号显示</label>
        <select v-model="localSettings.lineNumbers" @change="onSettingChange" class="setting-select">
          <option value="on">显示</option>
          <option value="off">隐藏</option>
          <option value="relative">相对行号</option>
          <option value="interval">间隔显示</option>
        </select>
      </div>

      <div class="setting-group">
        <label class="setting-label">空白字符显示</label>
        <select v-model="localSettings.renderWhitespace" @change="onSettingChange" class="setting-select">
          <option value="none">不显示</option>
          <option value="boundary">边界</option>
          <option value="selection">选中时</option>
          <option value="trailing">尾随空格</option>
          <option value="all">全部</option>
        </select>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue'
import type { EditorOptions } from '@/types/editor'

// Props 定义
interface Props {
  settings: EditorOptions
}

const props = defineProps<Props>()

// Emits 定义
interface Emits {
  'update:settings': [settings: EditorOptions]
}

const emit = defineEmits<Emits>()

// 默认设置
const defaultSettings: EditorOptions = {
  theme: 'vs',
  fontSize: 14,
  tabSize: 2,
  insertSpaces: true,
  wordWrap: 'on',
  minimap: { enabled: true },
  lineNumbers: 'on',
  renderWhitespace: 'selection',
  folding: true,
  showFoldingControls: 'always'
}

// 本地设置状态
const localSettings = reactive<EditorOptions>({
  ...defaultSettings,
  ...props.settings
})

/**
 * 设置变更处理
 */
const onSettingChange = () => {
  emit('update:settings', { ...localSettings })
}

/**
 * 重置为默认设置
 */
const resetToDefaults = () => {
  Object.assign(localSettings, defaultSettings)
  onSettingChange()
}

// 监听外部设置变化
watch(() => props.settings, (newSettings) => {
  Object.assign(localSettings, newSettings)
}, { deep: true })
</script>

<style scoped>
.editor-settings {
  background: #fff;
  border: 1px solid #e1e5e9;
  border-radius: 6px;
  overflow: hidden;
}

.settings-header {
  background: #f8f9fa;
  border-bottom: 1px solid #e1e5e9;
  padding: 12px 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.settings-header h3 {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
  color: #24292f;
}

.reset-btn {
  padding: 4px 12px;
  background: #f6f8fa;
  color: #24292f;
  border: 1px solid #d0d7de;
  border-radius: 4px;
  font-size: 12px;
  cursor: pointer;
  transition: all 0.2s;
}

.reset-btn:hover {
  background: #f3f4f6;
  border-color: #8c959f;
}

.settings-content {
  padding: 16px;
  max-height: 400px;
  overflow-y: auto;
}

.setting-group {
  margin-bottom: 16px;
}

.setting-group:last-child {
  margin-bottom: 0;
}

.setting-label {
  display: block;
  font-size: 12px;
  font-weight: 500;
  color: #24292f;
  margin-bottom: 6px;
}

.setting-select {
  width: 100%;
  padding: 6px 8px;
  border: 1px solid #d0d7de;
  border-radius: 4px;
  background: #fff;
  font-size: 12px;
}

.setting-select:focus {
  outline: none;
  border-color: #0969da;
  box-shadow: 0 0 0 2px rgba(9, 105, 218, 0.1);
}

.font-size-control {
  display: flex;
  align-items: center;
  gap: 12px;
}

.font-size-slider {
  flex: 1;
  height: 4px;
  background: #e1e5e9;
  border-radius: 2px;
  outline: none;
  -webkit-appearance: none;
}

.font-size-slider::-webkit-slider-thumb {
  -webkit-appearance: none;
  width: 16px;
  height: 16px;
  background: #0969da;
  border-radius: 50%;
  cursor: pointer;
}

.font-size-slider::-moz-range-thumb {
  width: 16px;
  height: 16px;
  background: #0969da;
  border-radius: 50%;
  cursor: pointer;
  border: none;
}

.font-size-value {
  font-size: 12px;
  color: #656d76;
  min-width: 32px;
  text-align: right;
}

.checkbox-group {
  display: flex;
  align-items: center;
}

.checkbox-label {
  display: flex;
  align-items: center;
  cursor: pointer;
  font-size: 12px;
  color: #24292f;
}

.setting-checkbox {
  margin-right: 8px;
  width: 16px;
  height: 16px;
  cursor: pointer;
}

.checkbox-text {
  user-select: none;
}

/* 深色主题样式 */
.editor-settings.dark {
  background: #0d1117;
  border-color: #30363d;
}

.editor-settings.dark .settings-header {
  background: #161b22;
  border-bottom-color: #30363d;
}

.editor-settings.dark .settings-header h3 {
  color: #f0f6fc;
}

.editor-settings.dark .reset-btn {
  background: #21262d;
  color: #f0f6fc;
  border-color: #30363d;
}

.editor-settings.dark .reset-btn:hover {
  background: #30363d;
}

.editor-settings.dark .setting-label {
  color: #f0f6fc;
}

.editor-settings.dark .setting-select {
  background: #21262d;
  border-color: #30363d;
  color: #f0f6fc;
}

.editor-settings.dark .font-size-value {
  color: #8b949e;
}

.editor-settings.dark .checkbox-label {
  color: #f0f6fc;
}
</style>
