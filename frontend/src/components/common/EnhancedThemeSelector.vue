<template>
  <div class="enhanced-theme-selector" :class="containerClasses">
    <div v-if="showLabel" class="selector-header">
      <h3 class="selector-title">{{ title }}</h3>
      <p class="selector-description">{{ description }}</p>
    </div>

    <div class="theme-options" :class="optionsClasses">
      <div
        v-for="theme in themes"
        :key="theme.value"
        class="theme-option"
        :class="[
          `theme-option-${variant}`,
          { active: selectedTheme === theme.value }
        ]"
        @click="selectTheme(theme.value)"
        @mouseenter="onHover(theme.value)"
        @mouseleave="onLeave"
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
          <transition :name="labelAnimation">
            <span v-if="selectedTheme === theme.value" class="current-label">
              {{ currentLabel }}
            </span>
          </transition>
        </div>
        
        <div class="theme-radio">
          <input
            :id="`theme-${theme.value}`"
            type="radio"
            :value="theme.value"
            v-model="selectedTheme"
            class="radio-input"
            @change="selectTheme(theme.value)"
          />
          <div class="radio-custom">
            <div class="radio-dot"></div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="showPreview" class="preview-section">
      <h4 class="preview-title">{{ previewTitle }}</h4>
      <div class="preview-container">
        <transition :name="previewAnimation">
          <div 
            :key="selectedTheme" 
            class="preview-app" 
            :class="`theme-${selectedTheme}`"
          >
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
                {{ getCurrentThemeLabel() }}
              </div>
              <div class="preview-description">
                {{ previewDescription }}
              </div>
            </div>
          </div>
        </transition>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
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

// Props 接口
interface Props {
  modelValue?: string
  size?: 'sm' | 'md' | 'lg'
  variant?: 'default' | 'minimal' | 'full'
  showLabel?: boolean
  showPreview?: boolean
  animation?: 'none' | 'fade' | 'slide' | 'bounce' | 'zoom' | 'elastic'
  title?: string
  description?: string
  currentLabel?: string
  previewTitle?: string
  previewDescription?: string
}

// Emits 接口
interface Emits {
  (e: 'update:modelValue', value: string): void
  (e: 'theme-change', theme: string): void
  (e: 'hover', theme: string | null): void
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: 'system',
  size: 'md',
  variant: 'default',
  showLabel: true,
  showPreview: true,
  animation: 'fade',
  title: '外观主题',
  description: '选择应用程序的外观主题，您可以跟随系统设置或手动选择。',
  currentLabel: '当前选择',
  previewTitle: '预览效果',
  previewDescription: '这是在当前主题下的界面预览效果'
})

const emit = defineEmits<Emits>()

const selectedTheme = ref(props.modelValue)
const hoveredTheme = ref<string | null>(null)

// 容器样式类
const containerClasses = computed(() => [
  `size-${props.size}`,
  `variant-${props.variant}`,
  `animation-${props.animation}`
])

// 选项样式类
const optionsClasses = computed(() => [
  `options-${props.size}`,
  `options-${props.variant}`
])

// 标签动画
const labelAnimation = computed(() => {
  if (props.animation === 'none') return 'none'
  return `label-${props.animation}`
})

// 预览动画
const previewAnimation = computed(() => {
  if (props.animation === 'none') return 'none'
  return `preview-${props.animation}`
})

// 选择主题
function selectTheme(theme: string) {
  selectedTheme.value = theme
  emit('update:modelValue', theme)
  emit('theme-change', theme)
  
  // 应用主题到文档
  applyTheme(theme)
}

// 应用主题
function applyTheme(theme: string) {
  const root = document.documentElement

  // 移除现有主题类
  root.classList.remove('theme-light', 'theme-dark', 'theme-system')

  // 添加新主题类
  root.classList.add(`theme-${theme}`)

  // 保存到本地存储
  localStorage.setItem('theme', theme)
}

// 获取当前主题标签
function getCurrentThemeLabel(): string {
  const theme = themes.find(t => t.value === selectedTheme.value)
  return theme?.name || '未知'
}

// 悬停事件
function onHover(theme: string) {
  hoveredTheme.value = theme
  emit('hover', theme)
}

// 离开事件
function onLeave() {
  hoveredTheme.value = null
  emit('hover', null)
}

// 监听 modelValue 变化
watch(() => props.modelValue, (newValue) => {
  if (newValue !== selectedTheme.value) {
    selectedTheme.value = newValue
    applyTheme(newValue)
  }
})

// 组件挂载时应用主题
onMounted(() => {
  applyTheme(selectedTheme.value)
})
</script>

<style scoped>
/* 基础样式 */
.enhanced-theme-selector {
  max-width: 600px;
  margin: 0 auto;
}

/* 尺寸变体 */
.size-sm {
  --scale-factor: 0.8;
}

.size-md {
  --scale-factor: 1;
}

.size-lg {
  --scale-factor: 1.2;
}

/* 样式变体 */
.variant-default {
  /* 默认样式 */
}

.variant-minimal {
  background: transparent;
  border: none;
  padding: 0;
}

.variant-full {
  background: var(--gradient-surface);
  border: 1px solid rgba(0, 0, 0, 0.06);
  border-radius: var(--radius-xl);
  padding: var(--space-6);
}

/* 头部样式 */
.selector-header {
  margin-bottom: var(--space-6);
}

.selector-title {
  font-size: calc(var(--text-xl) * var(--scale-factor));
  font-weight: var(--font-semibold);
  color: var(--gray-800);
  margin: 0 0 var(--space-2) 0;
}

.selector-description {
  color: var(--gray-600);
  font-size: calc(var(--text-sm) * var(--scale-factor));
  line-height: var(--leading-relaxed);
  margin: 0;
}

/* 选项容器 */
.theme-options {
  display: grid;
  gap: var(--space-4);
  margin-bottom: var(--space-8);
}

.options-sm {
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
}

.options-md {
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
}

.options-lg {
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
}

.options-minimal {
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-2);
}

/* 主题选项 */
.theme-option {
  background: var(--gradient-surface);
  border: 2px solid rgba(0, 0, 0, 0.06);
  border-radius: var(--radius-xl);
  padding: calc(var(--space-5) * var(--scale-factor));
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

/* 变体特定样式 */
.theme-option-minimal {
  background: transparent;
  border: 1px solid transparent;
  padding: calc(var(--space-3) * var(--scale-factor));
  border-radius: var(--radius-md);
}

.theme-option-minimal:hover {
  border-color: var(--primary-200);
  transform: none;
  box-shadow: none;
}

.theme-option-minimal.active {
  border-color: var(--primary-500);
  background: var(--primary-50);
  transform: none;
  box-shadow: none;
}

.theme-option-full {
  border-radius: var(--radius-lg);
}

/* 预览区域 */
.theme-preview {
  width: calc(60px * var(--scale-factor));
  height: calc(60px * var(--scale-factor));
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: calc(var(--space-4) * var(--scale-factor));
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
  width: calc(28px * var(--scale-factor));
  height: calc(28px * var(--scale-factor));
  color: white;
  filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.2));
}

/* 信息区域 */
.theme-info {
  flex: 1;
  margin-bottom: calc(var(--space-3) * var(--scale-factor));
}

.theme-name {
  font-size: calc(var(--text-base) * var(--scale-factor));
  font-weight: var(--font-semibold);
  color: var(--gray-800);
  margin: 0 0 var(--space-1) 0;
}

.theme-desc {
  font-size: calc(var(--text-sm) * var(--scale-factor));
  color: var(--gray-600);
  line-height: var(--leading-normal);
  margin: 0;
}

.current-label {
  display: inline-block;
  background: var(--gradient-primary);
  color: white;
  font-size: calc(var(--text-xs) * var(--scale-factor));
  font-weight: var(--font-semibold);
  padding: calc(var(--space-1) * var(--scale-factor)) calc(var(--space-2) * var(--scale-factor));
  border-radius: var(--radius-md);
  margin-top: calc(var(--space-2) * var(--scale-factor));
  box-shadow: var(--shadow-sm);
}

/* 单选按钮 */
.theme-radio {
  position: absolute;
  top: calc(var(--space-4) * var(--scale-factor));
  right: calc(var(--space-4) * var(--scale-factor));
}

.radio-input {
  position: absolute;
  opacity: 0;
  pointer-events: none;
}

.radio-custom {
  width: calc(20px * var(--scale-factor));
  height: calc(20px * var(--scale-factor));
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
  width: calc(8px * var(--scale-factor));
  height: calc(8px * var(--scale-factor));
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
  font-size: calc(var(--text-lg) * var(--scale-factor));
  font-weight: var(--font-semibold);
  color: var(--gray-800);
  margin: 0 0 var(--space-4) 0;
}

.preview-container {
  background: var(--gray-100);
  border-radius: var(--radius-lg);
  padding: var(--space-4);
  overflow: hidden;
  min-height: 120px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.preview-app {
  background: white;
  border-radius: var(--radius-md);
  overflow: hidden;
  box-shadow: var(--shadow-sm);
  transition: all var(--transition-normal);
  width: 100%;
  max-width: 300px;
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

/* 动画变体 */
.animation-fade .theme-option {
  animation: fadeIn 0.3s ease-out;
}

.animation-slide .theme-option {
  animation: slideIn 0.3s ease-out;
}

.animation-bounce .theme-option {
  animation: bounce 0.6s ease-out;
}

.animation-zoom .theme-option {
  animation: zoomIn 0.3s ease-out;
}

.animation-elastic .theme-option {
  animation: elastic 0.5s ease-out;
}

/* 标签动画 */
.label-fade-enter-active,
.label-fade-leave-active {
  transition: opacity var(--transition-normal), transform var(--transition-normal);
}

.label-fade-enter-from,
.label-fade-leave-to {
  opacity: 0;
  transform: scale(0.8);
}

.label-slide-enter-active,
.label-slide-leave-active {
  transition: all var(--transition-normal);
}

.label-slide-enter-from {
  opacity: 0;
  transform: translateX(-10px);
}

.label-slide-leave-to {
  opacity: 0;
  transform: translateX(10px);
}

.label-bounce-enter-active,
.label-bounce-leave-active {
  transition: all var(--transition-normal);
}

.label-bounce-enter-from {
  opacity: 0;
  transform: scale(0.5);
}

.label-bounce-leave-to {
  opacity: 0;
  transform: scale(1.2);
}

.label-zoom-enter-active,
.label-zoom-leave-active {
  transition: all var(--transition-normal);
}

.label-zoom-enter-from,
.label-zoom-leave-to {
  opacity: 0;
  transform: scale(0);
}

.label-elastic-enter-active,
.label-elastic-leave-active {
  transition: all var(--transition-normal);
}

.label-elastic-enter-from {
  opacity: 0;
  transform: scale(0) rotate(-180deg);
}

.label-elastic-leave-to {
  opacity: 0;
  transform: scale(1.5) rotate(180deg);
}

/* 预览动画 */
.preview-fade-enter-active,
.preview-fade-leave-active {
  transition: opacity var(--transition-normal);
}

.preview-fade-enter-from,
.preview-fade-leave-to {
  opacity: 0;
}

.preview-slide-enter-active,
.preview-slide-leave-active {
  transition: all var(--transition-normal);
}

.preview-slide-enter-from {
  opacity: 0;
  transform: translateX(20px);
}

.preview-slide-leave-to {
  opacity: 0;
  transform: translateX(-20px);
}

.preview-bounce-enter-active,
.preview-bounce-leave-active {
  transition: all var(--transition-normal);
}

.preview-bounce-enter-from {
  opacity: 0;
  transform: scale(0.8);
}

.preview-bounce-leave-to {
  opacity: 0;
  transform: scale(1.1);
}

.preview-zoom-enter-active,
.preview-zoom-leave-active {
  transition: all var(--transition-normal);
}

.preview-zoom-enter-from,
.preview-zoom-leave-to {
  opacity: 0;
  transform: scale(0.5);
}

.preview-elastic-enter-active,
.preview-elastic-leave-active {
  transition: all var(--transition-normal);
}

.preview-elastic-enter-from {
  opacity: 0;
  transform: scale(0) rotate(180deg);
}

.preview-elastic-leave-to {
  opacity: 0;
  transform: scale(1.2) rotate(-180deg);
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

  .selector-title {
    font-size: var(--text-lg);
  }

  .preview-app {
    max-width: 250px;
  }
}

@media (max-width: 480px) {
  .enhanced-theme-selector {
    padding: 0 var(--space-2);
  }

  .theme-option {
    padding: var(--space-3);
  }

  .theme-preview {
    width: 40px;
    height: 40px;
    margin-bottom: var(--space-2);
  }

  .theme-icon {
    width: 20px;
    height: 20px;
  }

  .preview-section {
    margin-top: var(--space-6);
    padding-top: var(--space-4);
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .theme-option,
  .radio-custom,
  .radio-dot,
  .preview-app,
  .label-fade-enter-active,
  .label-fade-leave-active,
  .label-slide-enter-active,
  .label-slide-leave-active,
  .label-bounce-enter-active,
  .label-bounce-leave-active,
  .label-zoom-enter-active,
  .label-zoom-leave-active,
  .label-elastic-enter-active,
  .label-elastic-leave-active,
  .preview-fade-enter-active,
  .preview-fade-leave-active,
  .preview-slide-enter-active,
  .preview-slide-leave-active,
  .preview-bounce-enter-active,
  .preview-bounce-leave-active,
  .preview-zoom-enter-active,
  .preview-zoom-leave-active,
  .preview-elastic-enter-active,
  .preview-elastic-leave-active {
    transition: none;
  }

  .theme-option:hover,
  .theme-option.active {
    transform: none;
  }

  .label-fade-enter-from,
  .label-fade-leave-to,
  .label-slide-enter-from,
  .label-slide-leave-to,
  .label-bounce-enter-from,
  .label-bounce-leave-to,
  .label-zoom-enter-from,
  .label-zoom-leave-to,
  .label-elastic-enter-from,
  .label-elastic-leave-to,
  .preview-fade-enter-from,
  .preview-fade-leave-to,
  .preview-slide-enter-from,
  .preview-slide-leave-to,
  .preview-bounce-enter-from,
  .preview-bounce-leave-to,
  .preview-zoom-enter-from,
  .preview-zoom-leave-to,
  .preview-elastic-enter-from,
  .preview-elastic-leave-to {
    opacity: 1;
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

  .current-label {
    border: 1px solid var(--primary-600);
  }
}

/* 焦点样式 */
.theme-option:focus-visible {
  outline: 2px solid var(--primary-500);
  outline-offset: 2px;
}

.radio-input:focus-visible + .radio-custom {
  outline: 2px solid var(--primary-500);
  outline-offset: 2px;
}

/* 全局主题样式（与原ThemeSelector保持兼容） */
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