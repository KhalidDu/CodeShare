<template>
  <div class="settings-panel-overlay" @click="handleOverlayClick">
    <div class="settings-panel" @click.stop>
      <!-- 面板头部 -->
      <div class="settings-header">
        <h2 class="settings-title">编辑器设置</h2>
        <button @click="$emit('close')" class="close-btn">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>
      
      <!-- 设置内容 -->
      <div class="settings-content">
        <!-- 主题设置 -->
        <div class="settings-section">
          <h3 class="section-title">主题设置</h3>
          <div class="theme-options">
            <button
              v-for="theme in themeOptions"
              :key="theme.id"
              @click="selectTheme(theme.id)"
              class="theme-option"
              :class="{ active: localSettings.theme === theme.id }"
            >
              <div class="theme-preview" :style="getThemePreviewStyle(theme.id)">
                <div class="preview-line preview-line-1"></div>
                <div class="preview-line preview-line-2"></div>
                <div class="preview-line preview-line-3"></div>
              </div>
              <span class="theme-name">{{ theme.name }}</span>
            </button>
          </div>
        </div>
        
        <!-- 显示设置 -->
        <div class="settings-section">
          <h3 class="section-title">显示设置</h3>
          <div class="settings-options">
            <!-- 行号显示 -->
            <div class="setting-item">
              <label class="setting-label">
                <span class="setting-text">显示行号</span>
                <input
                  v-model="localSettings.showLineNumbers"
                  type="checkbox"
                  class="setting-checkbox"
                  @change="handleSettingChange"
                />
              </label>
            </div>
            
            <!-- 小地图显示 -->
            <div class="setting-item">
              <label class="setting-label">
                <span class="setting-text">显示小地图</span>
                <input
                  v-model="localSettings.showMinimap"
                  type="checkbox"
                  class="setting-checkbox"
                  @change="handleSettingChange"
                />
              </label>
            </div>
            
            <!-- 自动换行 -->
            <div class="setting-item">
              <label class="setting-label">
                <span class="setting-text">自动换行</span>
                <select
                  v-model="localSettings.wordWrap"
                  class="setting-select"
                  @change="handleSettingChange"
                >
                  <option value="on">开启</option>
                  <option value="off">关闭</option>
                  <option value="wordWrapColumn">按列宽</option>
                  <option value="bounded">有界</option>
                </select>
              </label>
            </div>
          </div>
        </div>
        
        <!-- 字体设置 -->
        <div class="settings-section">
          <h3 class="section-title">字体设置</h3>
          <div class="settings-options">
            <!-- 字体大小 -->
            <div class="setting-item">
              <label class="setting-label">
                <span class="setting-text">字体大小</span>
                <input
                  v-model.number="localSettings.fontSize"
                  type="range"
                  min="10"
                  max="24"
                  class="setting-range"
                  @change="handleSettingChange"
                />
                <span class="setting-value">{{ localSettings.fontSize }}px</span>
              </label>
            </div>
            
            <!-- 字体族 -->
            <div class="setting-item">
              <label class="setting-label">
                <span class="setting-text">字体族</span>
                <select
                  v-model="localSettings.fontFamily"
                  class="setting-select"
                  @change="handleSettingChange"
                >
                  <option value="Consolas, Monaco, 'Courier New', monospace">Consolas</option>
                  <option value="'Fira Code', 'Fira Mono', monospace">Fira Code</option>
                  <option value="'Source Code Pro', monospace">Source Code Pro</option>
                  <option value="'JetBrains Mono', monospace">JetBrains Mono</option>
                  <option value="'Ubuntu Mono', monospace">Ubuntu Mono</option>
                </select>
              </label>
            </div>
            
            <!-- 行高 -->
            <div class="setting-item">
              <label class="setting-label">
                <span class="setting-text">行高</span>
                <input
                  v-model.number="localSettings.lineHeight"
                  type="range"
                  min="1.0"
                  max="2.0"
                  step="0.1"
                  class="setting-range"
                  @change="handleSettingChange"
                />
                <span class="setting-value">{{ localSettings.lineHeight }}</span>
              </label>
            </div>
          </div>
        </div>
        
        <!-- 编辑器设置 -->
        <div class="settings-section">
          <h3 class="section-title">编辑器设置</h3>
          <div class="settings-options">
            <!-- Tab 大小 -->
            <div class="setting-item">
              <label class="setting-label">
                <span class="setting-text">Tab 大小</span>
                <input
                  v-model.number="localSettings.tabSize"
                  type="range"
                  min="2"
                  max="8"
                  class="setting-range"
                  @change="handleSettingChange"
                />
                <span class="setting-value">{{ localSettings.tabSize }}</span>
              </label>
            </div>
            
            <!-- 插入空格 -->
            <div class="setting-item">
              <label class="setting-label">
                <span class="setting-text">插入空格</span>
                <input
                  v-model="localSettings.insertSpaces"
                  type="checkbox"
                  class="setting-checkbox"
                  @change="handleSettingChange"
                />
              </label>
            </div>
            
            <!-- 自动缩进 -->
            <div class="setting-item">
              <label class="setting-label">
                <span class="setting-text">自动缩进</span>
                <input
                  v-model="localSettings.autoIndent"
                  type="checkbox"
                  class="setting-checkbox"
                  @change="handleSettingChange"
                />
              </label>
            </div>
          </div>
        </div>
        
        <!-- 高级设置 -->
        <div class="settings-section">
          <h3 class="section-title">高级设置</h3>
          <div class="settings-options">
            <!-- 代码折叠 -->
            <div class="setting-item">
              <label class="setting-label">
                <span class="setting-text">代码折叠</span>
                <input
                  v-model="localSettings.folding"
                  type="checkbox"
                  class="setting-checkbox"
                  @change="handleSettingChange"
                />
              </label>
            </div>
            
            <!-- 括号匹配 -->
            <div class="setting-item">
              <label class="setting-label">
                <span class="setting-text">括号匹配</span>
                <input
                  v-model="localSettings.matchBrackets"
                  type="checkbox"
                  class="setting-checkbox"
                  @change="handleSettingChange"
                />
              </label>
            </div>
            
            <!-- 渲染空白字符 -->
            <div class="setting-item">
              <label class="setting-label">
                <span class="setting-text">渲染空白字符</span>
                <select
                  v-model="localSettings.renderWhitespace"
                  class="setting-select"
                  @change="handleSettingChange"
                >
                  <option value="none">不显示</option>
                  <option value="boundary">边界</option>
                  <option value="selection">选中</option>
                  <option value="trailing">结尾</option>
                  <option value="all">全部</option>
                </select>
              </label>
            </div>
          </div>
        </div>
      </div>
      
      <!-- 面板底部 -->
      <div class="settings-footer">
        <button @click="resetSettings" class="btn btn-secondary">
          重置默认
        </button>
        <button @click="saveSettings" class="btn btn-primary">
          保存设置
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'

// Props
interface Props {
  visible: boolean
  settings: {
    theme: string
    showLineNumbers: boolean
    showMinimap: boolean
    wordWrap: 'on' | 'off' | 'wordWrapColumn' | 'bounded'
    fontSize?: number
    fontFamily?: string
    lineHeight?: number
    tabSize?: number
    insertSpaces?: boolean
    autoIndent?: boolean
    folding?: boolean
    matchBrackets?: boolean
    renderWhitespace?: string
  }
}

const props = defineProps<Props>()

// Emits
interface Emits {
  (e: 'close'): void
  (e: 'settings-change', settings: Props['settings']): void
}

const emit = defineEmits<Emits>()

// 响应式状态
const localSettings = ref<Props['settings']>({ ...props.settings })

// 主题选项
const themeOptions = [
  { id: 'vs', name: '明亮' },
  { id: 'vs-dark', name: '深色' },
  { id: 'hc-black', name: '高对比度' }
]

// 计算属性
const defaultSettings = computed(() => ({
  theme: 'vs-dark',
  showLineNumbers: true,
  showMinimap: true,
  wordWrap: 'on' as const,
  fontSize: 14,
  fontFamily: 'Consolas, Monaco, "Courier New", monospace',
  lineHeight: 1.6,
  tabSize: 2,
  insertSpaces: true,
  autoIndent: true,
  folding: true,
  matchBrackets: true,
  renderWhitespace: 'selection'
}))

// 方法
/**
 * 选择主题
 */
function selectTheme(themeId: string) {
  localSettings.value.theme = themeId
  handleSettingChange()
}

/**
 * 处理设置变化
 */
function handleSettingChange() {
  emit('settings-change', { ...localSettings.value })
}

/**
 * 重置设置
 */
function resetSettings() {
  localSettings.value = { ...defaultSettings.value }
  handleSettingChange()
}

/**
 * 保存设置
 */
function saveSettings() {
  handleSettingChange()
  emit('close')
}

/**
 * 处理遮罩点击
 */
function handleOverlayClick() {
  emit('close')
}

/**
 * 获取主题预览样式
 */
function getThemePreviewStyle(themeId: string) {
  const styles: Record<string, any> = {
    'vs': {
      backgroundColor: '#ffffff',
      borderColor: '#e4e4e4'
    },
    'vs-dark': {
      backgroundColor: '#1e1e1e',
      borderColor: '#404040'
    },
    'hc-black': {
      backgroundColor: '#000000',
      borderColor: '#ffffff'
    }
  }
  
  return styles[themeId] || styles['vs-dark']
}

// 监听 Props 变化
watch(() => props.settings, (newSettings) => {
  localSettings.value = { ...newSettings }
}, { deep: true })

// 监听可见性变化
watch(() => props.visible, (isVisible) => {
  if (isVisible) {
    // 当面板打开时，滚动到顶部
    document.querySelector('.settings-content')?.scrollTo({
      top: 0,
      behavior: 'smooth'
    })
  }
})
</script>

<style scoped>
/* 设置面板遮罩 */
.settings-panel-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  backdrop-filter: blur(4px);
}

/* 设置面板 */
.settings-panel {
  background: #2d2d2d;
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  width: 90%;
  max-width: 600px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4);
  overflow: hidden;
}

/* 面板头部 */
.settings-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  background: rgba(30, 30, 30, 0.9);
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
}

.settings-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #ffffff;
  margin: 0;
}

.close-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: none;
  border-radius: 6px;
  background: transparent;
  color: #d4d4d4;
  cursor: pointer;
  transition: all 0.2s ease;
}

.close-btn:hover {
  background: rgba(255, 255, 255, 0.1);
  color: #ffffff;
}

/* 设置内容 */
.settings-content {
  flex: 1;
  overflow-y: auto;
  padding: 1.5rem;
}

.settings-section {
  margin-bottom: 2rem;
}

.settings-section:last-child {
  margin-bottom: 0;
}

.section-title {
  font-size: 1rem;
  font-weight: 600;
  color: #ffffff;
  margin: 0 0 1rem 0;
}

/* 主题选项 */
.theme-options {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 1rem;
}

.theme-option {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem;
  border: 2px solid transparent;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.05);
  cursor: pointer;
  transition: all 0.2s ease;
}

.theme-option:hover {
  background: rgba(255, 255, 255, 0.1);
}

.theme-option.active {
  border-color: #007acc;
  background: rgba(0, 122, 204, 0.1);
}

.theme-preview {
  width: 60px;
  height: 40px;
  border-radius: 4px;
  border: 1px solid;
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: 4px;
}

.preview-line {
  height: 2px;
  border-radius: 1px;
  background: #007acc;
}

.preview-line-1 {
  width: 100%;
}

.preview-line-2 {
  width: 70%;
}

.preview-line-3 {
  width: 90%;
}

.theme-name {
  font-size: 0.875rem;
  color: #d4d4d4;
  font-weight: 500;
}

/* 设置选项 */
.settings-options {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.setting-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.75rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 8px;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.setting-label {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  cursor: pointer;
}

.setting-text {
  font-size: 0.875rem;
  color: #d4d4d4;
  font-weight: 500;
}

.setting-checkbox {
  width: 18px;
  height: 18px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 4px;
  background: transparent;
  cursor: pointer;
  transition: all 0.2s ease;
}

.setting-checkbox:checked {
  background: #007acc;
  border-color: #007acc;
}

.setting-checkbox:hover {
  border-color: rgba(255, 255, 255, 0.5);
}

.setting-select {
  padding: 0.5rem;
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 4px;
  background: rgba(30, 30, 30, 0.9);
  color: #d4d4d4;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.2s ease;
}

.setting-select:hover {
  border-color: rgba(255, 255, 255, 0.5);
}

.setting-select:focus {
  outline: none;
  border-color: #007acc;
}

.setting-range {
  flex: 1;
  margin: 0 1rem;
  height: 6px;
  border-radius: 3px;
  background: rgba(255, 255, 255, 0.1);
  outline: none;
  cursor: pointer;
}

.setting-range::-webkit-slider-thumb {
  appearance: none;
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background: #007acc;
  cursor: pointer;
  transition: all 0.2s ease;
}

.setting-range::-webkit-slider-thumb:hover {
  background: #005a9e;
}

.setting-value {
  font-size: 0.875rem;
  color: #007acc;
  font-weight: 500;
  min-width: 40px;
  text-align: right;
}

/* 面板底部 */
.settings-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding: 1.5rem;
  background: rgba(30, 30, 30, 0.9);
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-primary {
  background: #007acc;
  color: white;
}

.btn-primary:hover {
  background: #005a9e;
}

.btn-secondary {
  background: rgba(255, 255, 255, 0.1);
  color: #d4d4d4;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.btn-secondary:hover {
  background: rgba(255, 255, 255, 0.15);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .settings-panel {
    width: 95%;
    max-width: none;
    border-radius: 0;
    height: 100vh;
    max-height: none;
  }
  
  .settings-header {
    padding: 1rem;
  }
  
  .settings-content {
    padding: 1rem;
  }
  
  .settings-footer {
    padding: 1rem;
  }
  
  .theme-options {
    grid-template-columns: repeat(auto-fit, minmax(100px, 1fr));
    gap: 0.75rem;
  }
  
  .theme-preview {
    width: 50px;
    height: 35px;
  }
  
  .setting-item {
    padding: 0.5rem;
  }
  
  .setting-text {
    font-size: 0.8125rem;
  }
  
  .btn {
    padding: 0.5rem 1rem;
    font-size: 0.8125rem;
  }
}

@media (max-width: 480px) {
  .theme-options {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .settings-footer {
    flex-direction: column;
  }
  
  .btn {
    width: 100%;
  }
}

/* 滚动条样式 */
.settings-content::-webkit-scrollbar {
  width: 6px;
}

.settings-content::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.05);
}

.settings-content::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.2);
  border-radius: 3px;
}

.settings-content::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.3);
}

/* 动画效果 */
.settings-panel {
  animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* 无障碍支持 */
.close-btn:focus,
.theme-option:focus,
.setting-checkbox:focus,
.setting-select:focus,
.setting-range:focus,
.btn:focus {
  outline: 2px solid #007acc;
  outline-offset: 2px;
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .settings-panel {
    border-width: 2px;
  }
  
  .setting-item {
    border-width: 2px;
  }
  
  .theme-option.active {
    border-width: 3px;
  }
}

/* 暗色模式适配 */
@media (prefers-color-scheme: dark) {
  .settings-panel {
    background: #1a1a1a;
  }
  
  .settings-header,
  .settings-footer {
    background: rgba(20, 20, 20, 0.9);
  }
}
</style>