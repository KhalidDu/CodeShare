<template>
  <div class="theme-selector">
    <h3 class="selector-title">外观主题</h3>
    <p class="selector-description">
      选择应用程序的外观主题，您可以跟随系统设置或手动选择。
    </p>

    <div class="theme-options">
      <div
        v-for="theme in themes"
        :key="theme.value"
        class="theme-option"
        :class="{ active: selectedTheme === theme.value }"
        @click="selectTheme(theme.value)"
      >
        <div class="theme-preview" :class="`preview-${theme.value}`">
          <div class="theme-icon">
            <SystemIcon v-if="theme.value === 'system'" />
            <LightIcon v-else-if="theme.value === 'light'" />
            <DarkIcon v-else-if="theme.value === 'dark'" />
          </div>
        </div>
        <div class="theme-info">
          <h4 class="theme-name">{{ theme.name }}</h4>
          <p class="theme-desc">{{ theme.description }}</p>
          <span v-if="selectedTheme === theme.value" class="current-label">
            当前: {{ theme.name }}
          </span>
        </div>
        <div class="theme-radio">
          <input
            type="radio"
            :id="`theme-${theme.value}`"
            :value="theme.value"
            v-model="selectedTheme"
            class="radio-input"
          />
          <div class="radio-custom">
            <div class="radio-dot"></div>
          </div>
        </div>
      </div>
    </div>

    <div class="preview-section">
      <h4 class="preview-title">预览效果</h4>
      <div class="preview-container">
        <div class="preview-app" :class="`theme-${selectedTheme}`">
          <div class="preview-header">
            <div class="preview-logo">
              <svg viewBox="0 0 24 24" fill="currentColor">
                <path d="M9.4 16.6L4.8 12l4.6-4.6L8 6l-6 6 6 6 1.4-1.4zm5.2 0L19.2 12l-4.6-4.6L16 6l6 6-6 6-1.4-1.4z"/>
              </svg>
              SeekCode
            </div>
          </div>
          <div class="preview-content">
            <div class="preview-text">
              当前: {{ getCurrentThemeLabel() }}
            </div>
            <div class="preview-description">
              这是在当前主题下的界面预览效果
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import SystemIcon from '@/components/icons/SystemIcon.vue'
import LightIcon from '@/components/icons/LightIcon.vue'
import DarkIcon from '@/components/icons/DarkIcon.vue'

// 主题选项
const themes = [
  {
    value: 'system',
    name: '跟随系统',
    description: '自动跟随系统设置'
  },
  {
    value: 'light',
    name: '浅色',
    description: '明亮清新的界面'
  },
  {
    value: 'dark',
    name: '深色',
    description: '护眼的深色模式'
  }
]

interface Props {
  modelValue?: string
}

interface Emits {
  (e: 'update:modelValue', value: string): void
  (e: 'theme-change', theme: string): void
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: 'system'
})

const emit = defineEmits<Emits>()

const selectedTheme = ref(props.modelValue)

/**
 * 选择主题
 */
function selectTheme(theme: string) {
  selectedTheme.value = theme
  emit('update:modelValue', theme)
  emit('theme-change', theme)

  // 应用主题到文档
  applyTheme(theme)
}

/**
 * 应用主题
 */
function applyTheme(theme: string) {
  const root = document.documentElement

  // 移除现有主题类
  root.classList.remove('theme-light', 'theme-dark', 'theme-system')

  // 添加新主题类
  root.classList.add(`theme-${theme}`)

  // 保存到本地存储
  localStorage.setItem('theme', theme)
}

/**
 * 获取当前主题标签
 */
function getCurrentThemeLabel(): string {
  const theme = themes.find(t => t.value === selectedTheme.value)
  return theme?.name || '未知'
}

// 组件挂载时应用主题
onMounted(() => {
  applyTheme(selectedTheme.value)
})
</script>

<style scoped>
.theme-selector {
  max-width: 600px;
}

.selector-title {
  font-size: var(--text-xl);
  font-weight: var(--font-semibold);
  color: var(--gray-800);
  margin: 0 0 var(--space-2) 0;
}

.selector-description {
  color: var(--gray-600);
  font-size: var(--text-sm);
  line-height: var(--leading-relaxed);
  margin: 0 0 var(--space-6) 0;
}

.theme-options {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: var(--space-4);
  margin-bottom: var(--space-8);
}

.theme-option {
  background: var(--gradient-surface);
  border: 2px solid rgba(0, 0, 0, 0.06);
  border-radius: var(--radius-xl);
  padding: var(--space-5);
  cursor: pointer;
  transition: all var(--transition-normal);
  position: relative;
  overflow: hidden;
}

.theme-option::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(0, 123, 255, 0.05) 0%, rgba(0, 86, 179, 0.05) 100%);
  opacity: 0;
  transition: opacity var(--transition-normal);
}

.theme-option:hover {
  border-color: rgba(0, 123, 255, 0.2);
  transform: translateY(-2px);
  box-shadow: var(--shadow-lg);
}

.theme-option:hover::before {
  opacity: 1;
}

.theme-option.active {
  border-color: var(--primary-500);
  box-shadow: var(--shadow-primary);
  transform: translateY(-2px);
}

.theme-option.active::before {
  opacity: 1;
}

.theme-preview {
  width: 60px;
  height: 60px;
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: var(--space-4);
  position: relative;
  overflow: hidden;
}

.preview-system {
  background: linear-gradient(135deg, #6c757d 0%, #495057 100%);
}

.preview-light {
  background: linear-gradient(135deg, #ffc107 0%, #fd7e14 100%);
}

.preview-dark {
  background: linear-gradient(135deg, #343a40 0%, #212529 100%);
}

.theme-icon {
  width: 28px;
  height: 28px;
  color: white;
  filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.2));
}

.theme-info {
  flex: 1;
  margin-bottom: var(--space-3);
}

.theme-name {
  font-size: var(--text-base);
  font-weight: var(--font-semibold);
  color: var(--gray-800);
  margin: 0 0 var(--space-1) 0;
}

.theme-desc {
  font-size: var(--text-sm);
  color: var(--gray-600);
  line-height: var(--leading-normal);
  margin: 0;
}

.current-label {
  display: inline-block;
  background: var(--gradient-primary);
  color: white;
  font-size: var(--text-xs);
  font-weight: var(--font-semibold);
  padding: var(--space-1) var(--space-2);
  border-radius: var(--radius-md);
  margin-top: var(--space-2);
  box-shadow: var(--shadow-sm);
}

.theme-radio {
  position: absolute;
  top: var(--space-4);
  right: var(--space-4);
}

.radio-input {
  position: absolute;
  opacity: 0;
  pointer-events: none;
}

.radio-custom {
  width: 20px;
  height: 20px;
  border: 2px solid var(--gray-300);
  border-radius: var(--radius-full);
  background: white;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all var(--transition-normal);
  position: relative;
}

.radio-dot {
  width: 8px;
  height: 8px;
  background: var(--primary-500);
  border-radius: var(--radius-full);
  transform: scale(0);
  transition: transform var(--transition-normal);
}

.theme-option.active .radio-custom {
  border-color: var(--primary-500);
  background: var(--primary-50);
}

.theme-option.active .radio-dot {
  transform: scale(1);
}

/* 预览部分 */
.preview-section {
  margin-top: var(--space-8);
  padding-top: var(--space-6);
  border-top: 1px solid rgba(0, 0, 0, 0.06);
}

.preview-title {
  font-size: var(--text-lg);
  font-weight: var(--font-semibold);
  color: var(--gray-800);
  margin: 0 0 var(--space-4) 0;
}

.preview-container {
  background: var(--gray-100);
  border-radius: var(--radius-lg);
  padding: var(--space-4);
  overflow: hidden;
}

.preview-app {
  background: white;
  border-radius: var(--radius-md);
  overflow: hidden;
  box-shadow: var(--shadow-sm);
  transition: all var(--transition-normal);
}

.preview-app.theme-dark {
  background: #1a1a1a;
  color: #f5f5f5;
}

.preview-header {
  background: var(--gradient-primary);
  color: white;
  padding: var(--space-3) var(--space-4);
  display: flex;
  align-items: center;
}

.preview-app.theme-dark .preview-header {
  background: linear-gradient(135deg, #2d2d2d 0%, #404040 100%);
}

.preview-logo {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  font-weight: var(--font-semibold);
  font-size: var(--text-sm);
}

.preview-logo svg {
  width: 18px;
  height: 18px;
}

.preview-content {
  padding: var(--space-4);
}

.preview-text {
  font-weight: var(--font-medium);
  color: var(--primary-600);
  margin-bottom: var(--space-2);
}

.preview-app.theme-dark .preview-text {
  color: var(--primary-400);
}

.preview-description {
  font-size: var(--text-sm);
  color: var(--gray-600);
  line-height: var(--leading-normal);
}

.preview-app.theme-dark .preview-description {
  color: var(--gray-400);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .theme-options {
    grid-template-columns: 1fr;
    gap: var(--space-3);
  }

  .theme-option {
    padding: var(--space-4);
  }

  .theme-preview {
    width: 50px;
    height: 50px;
    margin-bottom: var(--space-3);
  }

  .theme-icon {
    width: 24px;
    height: 24px;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .theme-option,
  .radio-custom,
  .radio-dot,
  .preview-app {
    transition: none;
  }

  .theme-option:hover,
  .theme-option.active {
    transform: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .theme-option {
    border-width: 3px;
  }

  .radio-custom {
    border-width: 3px;
  }

  .preview-app {
    border: 2px solid var(--gray-800);
  }
}
</style>

<style>
/* 全局主题样式 */
.theme-light {
  --primary-bg: #ffffff;
  --secondary-bg: #f8f9fa;
  --text-primary: #212529;
  --text-secondary: #6c757d;
}

.theme-dark {
  --primary-bg: #1a1a1a;
  --secondary-bg: #2d2d2d;
  --text-primary: #f5f5f5;
  --text-secondary: #a3a3a3;
}

.theme-system {
  /* 跟随系统设置 */
}

@media (prefers-color-scheme: dark) {
  .theme-system {
    --primary-bg: #1a1a1a;
    --secondary-bg: #2d2d2d;
    --text-primary: #f5f5f5;
    --text-secondary: #a3a3a3;
  }
}

@media (prefers-color-scheme: light) {
  .theme-system {
    --primary-bg: #ffffff;
    --secondary-bg: #f8f9fa;
    --text-primary: #212529;
    --text-secondary: #6c757d;
  }
}
</style>
