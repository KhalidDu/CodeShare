<template>
  <div class="theme-editor" :class="editorClasses">
    <!-- 编辑器头部 -->
    <div class="editor-header">
      <div class="header-content">
        <h2 class="editor-title">
          <i class="fas fa-palette mr-2"></i>
          主题编辑器
        </h2>
        <p class="editor-description">
          自定义应用程序的外观主题，创建专属的视觉风格
        </p>
      </div>
      
      <!-- 操作按钮 -->
      <div class="header-actions">
        <button
          @click="resetTheme"
          class="action-btn reset-btn"
          :disabled="!hasChanges"
        >
          <i class="fas fa-undo mr-1"></i>
          重置
        </button>
        
        <button
          @click="previewTheme"
          class="action-btn preview-btn"
        >
          <i class="fas fa-eye mr-1"></i>
          预览
        </button>
        
        <button
          @click="saveTheme"
          class="action-btn save-btn"
          :disabled="!hasChanges || !isValid"
        >
          <i class="fas fa-save mr-1"></i>
          保存
        </button>
      </div>
    </div>

    <!-- 主题选择和基本信息 -->
    <div class="theme-info-section">
      <div class="section-header">
        <h3 class="section-title">基本信息</h3>
      </div>
      
      <div class="info-content">
        <!-- 基础主题选择 -->
        <div class="form-group">
          <label class="form-label">基础主题</label>
          <select v-model="baseTheme" class="form-select" @change="onBaseThemeChange">
            <option v-for="theme in defaultThemes" :key="theme.id" :value="theme.id">
              {{ theme.name }}
            </option>
          </select>
        </div>
        
        <!-- 主题名称 -->
        <div class="form-group">
          <label class="form-label">主题名称</label>
          <input
            v-model="editingTheme.name"
            type="text"
            class="form-input"
            placeholder="输入主题名称"
            @input="validateForm"
          />
        </div>
        
        <!-- 主题描述 -->
        <div class="form-group">
          <label class="form-label">主题描述</label>
          <textarea
            v-model="editingTheme.description"
            class="form-textarea"
            placeholder="输入主题描述"
            rows="2"
            @input="validateForm"
          ></textarea>
        </div>
      </div>
    </div>

    <!-- 选项卡导航 -->
    <div class="editor-tabs">
      <div class="tabs-nav">
        <button
          v-for="tab in tabs"
          :key="tab.id"
          class="tab-btn"
          :class="{ active: activeTab === tab.id }"
          @click="activeTab = tab.id"
        >
          <i :class="tab.icon"></i>
          {{ tab.name }}
        </button>
      </div>
    </div>

    <!-- 选项卡内容 -->
    <div class="editor-content">
      <!-- 颜色配置 -->
      <div v-if="activeTab === 'colors'" class="tab-content">
        <div class="color-config">
          <div class="color-sections">
            <!-- 主要颜色 -->
            <div class="color-section">
              <h4 class="section-subtitle">主要颜色</h4>
              <div class="color-palette">
                <div
                  v-for="(color, shade) in editingTheme.colors.primary"
                  :key="`primary-${shade}`"
                  class="color-item"
                >
                  <div
                    class="color-preview"
                    :style="{ backgroundColor: color }"
                  ></div>
                  <input
                    v-model="editingTheme.colors.primary[shade]"
                    type="color"
                    class="color-input"
                    @input="onColorChange"
                  />
                  <span class="color-label">{{ shade }}</span>
                </div>
              </div>
            </div>

            <!-- 状态颜色 -->
            <div class="color-section">
              <h4 class="section-subtitle">状态颜色</h4>
              <div class="status-colors">
                <div
                  v-for="statusType in ['success', 'warning', 'error', 'info']"
                  :key="statusType"
                  class="status-color-group"
                >
                  <h5 class="status-label">{{ getStatusLabel(statusType) }}</h5>
                  <div class="color-palette compact">
                    <div
                      v-for="(color, shade) in editingTheme.colors[statusType]"
                      :key="`${statusType}-${shade}`"
                      class="color-item compact"
                    >
                      <div
                        class="color-preview small"
                        :style="{ backgroundColor: color }"
                      ></div>
                      <input
                        v-model="editingTheme.colors[statusType][shade]"
                        type="color"
                        class="color-input small"
                        @input="onColorChange"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- 中性颜色 -->
            <div class="color-section">
              <h4 class="section-subtitle">中性颜色</h4>
              <div class="color-palette">
                <div
                  v-for="(color, shade) in editingTheme.colors.gray"
                  :key="`gray-${shade}`"
                  class="color-item"
                >
                  <div
                    class="color-preview"
                    :style="{ backgroundColor: color }"
                  ></div>
                  <input
                    v-model="editingTheme.colors.gray[shade]"
                    type="color"
                    class="color-input"
                    @input="onColorChange"
                  />
                  <span class="color-label">{{ shade }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 排版配置 -->
      <div v-if="activeTab === 'typography'" class="tab-content">
        <div class="typography-config">
          <div class="typography-sections">
            <!-- 字体设置 -->
            <div class="typography-section">
              <h4 class="section-subtitle">字体设置</h4>
              <div class="form-group">
                <label class="form-label">字体族</label>
                <input
                  v-model="editingTheme.typography.fontFamily"
                  type="text"
                  class="form-input"
                  @input="onChange"
                />
              </div>
            </div>

            <!-- 字体大小 -->
            <div class="typography-section">
              <h4 class="section-subtitle">字体大小</h4>
              <div class="font-sizes">
                <div
                  v-for="(size, name) in editingTheme.typography.fontSize"
                  :key="`size-${name}`"
                  class="font-size-item"
                >
                  <label class="size-label">{{ name }}</label>
                  <input
                    v-model="editingTheme.typography.fontSize[name]"
                    type="text"
                    class="size-input"
                    @input="onChange"
                  />
                </div>
              </div>
            </div>

            <!-- 行高 -->
            <div class="typography-section">
              <h4 class="section-subtitle">行高</h4>
              <div class="line-heights">
                <div
                  v-for="(height, type) in editingTheme.typography.lineHeight"
                  :key="`height-${type}`"
                  class="line-height-item"
                >
                  <label class="height-label">{{ type }}</label>
                  <input
                    v-model="editingTheme.typography.lineHeight[type]"
                    type="text"
                    class="height-input"
                    @input="onChange"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 间距配置 -->
      <div v-if="activeTab === 'spacing'" class="tab-content">
        <div class="spacing-config">
          <div class="spacing-grid">
            <div
              v-for="(value, size) in editingTheme.spacing"
              :key="`spacing-${size}`"
              class="spacing-item"
            >
              <label class="spacing-label">{{ size }}</label>
              <input
                v-model="editingTheme.spacing[size]"
                type="text"
                class="spacing-input"
                @input="onChange"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- 圆角配置 -->
      <div v-if="activeTab === 'borderRadius'" class="tab-content">
        <div class="border-radius-config">
          <div class="radius-grid">
            <div
              v-for="(value, size) in editingTheme.borderRadius"
              :key="`radius-${size}`"
              class="radius-item"
            >
              <label class="radius-label">{{ size }}</label>
              <input
                v-model="editingTheme.borderRadius[size]"
                type="text"
                class="radius-input"
                @input="onChange"
              />
              <div
                class="radius-preview"
                :style="{ borderRadius: value }"
              ></div>
            </div>
          </div>
        </div>
      </div>

      <!-- 阴影配置 -->
      <div v-if="activeTab === 'shadows'" class="tab-content">
        <div class="shadows-config">
          <div class="shadow-items">
            <div
              v-for="(value, size) in editingTheme.shadows"
              :key="`shadow-${size}`"
              class="shadow-item"
            >
              <label class="shadow-label">{{ size }}</label>
              <textarea
                v-model="editingTheme.shadows[size]"
                class="shadow-input"
                rows="2"
                @input="onChange"
              ></textarea>
              <div
                class="shadow-preview"
                :style="{ boxShadow: value }"
              ></div>
            </div>
          </div>
        </div>
      </div>

      <!-- 动画配置 -->
      <div v-if="activeTab === 'animations'" class="tab-content">
        <div class="animations-config">
          <div class="animation-sections">
            <!-- 持续时间 -->
            <div class="animation-section">
              <h4 class="section-subtitle">持续时间</h4>
              <div class="duration-items">
                <div
                  v-for="(value, speed) in editingTheme.animations.duration"
                  :key="`duration-${speed}`"
                  class="duration-item"
                >
                  <label class="duration-label">{{ speed }}</label>
                  <input
                    v-model="editingTheme.animations.duration[speed]"
                    type="text"
                    class="duration-input"
                    @input="onChange"
                  />
                </div>
              </div>
            </div>

            <!-- 缓动函数 -->
            <div class="animation-section">
              <h4 class="section-subtitle">缓动函数</h4>
              <div class="easing-items">
                <div
                  v-for="(value, type) in editingTheme.animations.easing"
                  :key="`easing-${type}`"
                  class="easing-item"
                >
                  <label class="easing-label">{{ type }}</label>
                  <select
                    v-model="editingTheme.animations.easing[type]"
                    class="easing-select"
                    @change="onChange"
                  >
                    <option value="linear">linear</option>
                    <option value="ease">ease</option>
                    <option value="ease-in">ease-in</option>
                    <option value="ease-out">ease-out</option>
                    <option value="ease-in-out">ease-in-out</option>
                    <option value="cubic-bezier(0.4, 0, 0.2, 1)">cubic-bezier(0.4, 0, 0.2, 1)</option>
                    <option value="cubic-bezier(0.4, 0, 0.6, 1)">cubic-bezier(0.4, 0, 0.6, 1)</option>
                  </select>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 主题预览 -->
    <div class="theme-preview-section">
      <div class="section-header">
        <h3 class="section-title">主题预览</h3>
      </div>
      
      <div class="preview-content">
        <div class="preview-container" :style="previewStyle">
          <div class="preview-header">
            <h4 class="preview-title">{{ editingTheme.name || '未命名主题' }}</h4>
            <p class="preview-description">{{ editingTheme.description || '主题描述' }}</p>
          </div>
          
          <div class="preview-body">
            <div class="preview-colors">
              <div
                v-for="colorType in ['primary', 'success', 'warning', 'error']"
                :key="colorType"
                class="preview-color-item"
                :style="{ backgroundColor: editingTheme.colors[colorType][500] }"
              >
                {{ colorType }}
              </div>
            </div>
            
            <div class="preview-text">
              <h5 :style="{ fontFamily: editingTheme.typography.fontFamily }">
                标题文本示例
              </h5>
              <p :style="{ 
                fontFamily: editingTheme.typography.fontFamily,
                fontSize: editingTheme.typography.fontSize.base,
                lineHeight: editingTheme.typography.lineHeight.normal
              }">
                这是一段正文文本示例，用于预览当前主题的排版效果。您可以看到字体、大小、行高等设置的实际效果。
              </p>
            </div>
            
            <div class="preview-components">
              <button
                class="preview-btn"
                :style="{
                  backgroundColor: editingTheme.colors.primary[500],
                  color: editingTheme.colors.gray[50],
                  borderRadius: editingTheme.borderRadius.md,
                  fontFamily: editingTheme.typography.fontFamily
                }"
              >
                主要按钮
              </button>
              
              <div
                class="preview-card"
                :style="{
                  backgroundColor: editingTheme.colors.gray[50],
                  borderRadius: editingTheme.borderRadius.lg,
                  boxShadow: editingTheme.shadows.md,
                  padding: editingTheme.spacing.lg
                }"
              >
                <h6 :style="{ 
                  fontFamily: editingTheme.typography.fontFamily,
                  color: editingTheme.colors.gray[900]
                }">
                  卡片示例
                </h6>
                <p :style="{ 
                  fontFamily: editingTheme.typography.fontFamily,
                  color: editingTheme.colors.gray[600],
                  fontSize: editingTheme.typography.fontSize.sm
                }">
                  这是一个卡片组件的预览效果。
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 错误提示 -->
    <div v-if="errors.length > 0" class="error-section">
      <div class="error-content">
        <i class="fas fa-exclamation-triangle error-icon"></i>
        <div class="error-messages">
          <div v-for="error in errors" :key="error" class="error-message">
            {{ error }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useCustomTheme, type ThemeConfig } from '@/composables/useCustomTheme'
import { cn } from '@/lib/utils'

// 主题编辑器接口
interface Props {
  visible?: boolean
  themeId?: string
  onClose?: () => void
  onSave?: (theme: ThemeConfig) => void
}

const props = withDefaults(defineProps<Props>(), {
  visible: false,
  themeId: ''
})

const emit = defineEmits<{
  'update:visible': [visible: boolean]
  'close': []
  'save': [theme: ThemeConfig]
}>()

// 使用自定义主题系统
const { allThemes, defaultThemes, createCustomTheme, validateTheme } = useCustomTheme()

// 响应式状态
const activeTab = ref('colors')
const baseTheme = ref('light')
const originalTheme = ref<ThemeConfig | null>(null)
const editingTheme = ref<ThemeConfig>(getDefaultTheme())
const errors = ref<string[]>([])
const hasChanges = ref(false)

// 选项卡配置
const tabs = [
  { id: 'colors', name: '颜色', icon: 'fas fa-palette' },
  { id: 'typography', name: '排版', icon: 'fas fa-font' },
  { id: 'spacing', name: '间距', icon: 'fas fa-expand-arrows-alt' },
  { id: 'borderRadius', name: '圆角', icon: 'fas fa-square' },
  { id: 'shadows', name: '阴影', icon: 'fas fa-cloud' },
  { id: 'animations', name: '动画', icon: 'fas fa-magic' }
]

// 计算属性
const editorClasses = computed(() => {
  return [
    'theme-editor-container',
    'bg-white dark:bg-gray-800 rounded-lg shadow-xl',
    'max-w-6xl mx-auto p-6'
  ].join(' ')
})

const isValid = computed(() => {
  return errors.value.length === 0 && editingTheme.value.name.trim() !== ''
})

const previewStyle = computed(() => {
  return {
    '--primary-color': editingTheme.value.colors.primary[500],
    '--primary-bg': editingTheme.value.colors.primary[50],
    '--text-primary': editingTheme.value.colors.gray[900],
    '--text-secondary': editingTheme.value.colors.gray[600],
    '--border-color': editingTheme.value.colors.gray[200],
    '--font-family': editingTheme.value.typography.fontFamily,
    '--border-radius': editingTheme.value.borderRadius.md,
    '--shadow': editingTheme.value.shadows.md,
    '--spacing': editingTheme.value.spacing.md
  } as React.CSSProperties
})

// 获取默认主题配置
function getDefaultTheme(): ThemeConfig {
  return {
    id: '',
    name: '',
    description: '',
    colors: {
      primary: {
        50: '#eff6ff', 100: '#dbeafe', 200: '#bfdbfe', 300: '#93c5fd',
        400: '#60a5fa', 500: '#3b82f6', 600: '#2563eb', 700: '#1d4ed8',
        800: '#1e40af', 900: '#1e3a8a', 950: '#172554'
      },
      secondary: {
        50: '#f8fafc', 100: '#f1f5f9', 200: '#e2e8f0', 300: '#cbd5e1',
        400: '#94a3b8', 500: '#64748b', 600: '#475569', 700: '#334155',
        800: '#1e293b', 900: '#0f172a', 950: '#020617'
      },
      success: {
        50: '#f0fdf4', 100: '#dcfce7', 200: '#bbf7d0', 300: '#86efac',
        400: '#4ade80', 500: '#22c55e', 600: '#16a34a', 700: '#15803d',
        800: '#166534', 900: '#14532d', 950: '#052e16'
      },
      warning: {
        50: '#fffbeb', 100: '#fef3c7', 200: '#fde68a', 300: '#fcd34d',
        400: '#fbbf24', 500: '#f59e0b', 600: '#d97706', 700: '#b45309',
        800: '#92400e', 900: '#78350f', 950: '#451a03'
      },
      error: {
        50: '#fef2f2', 100: '#fee2e2', 200: '#fecaca', 300: '#fca5a5',
        400: '#f87171', 500: '#ef4444', 600: '#dc2626', 700: '#b91c1c',
        800: '#991b1b', 900: '#7f1d1d', 950: '#450a0a'
      },
      info: {
        50: '#f0f9ff', 100: '#e0f2fe', 200: '#bae6fd', 300: '#7dd3fc',
        400: '#38bdf8', 500: '#0ea5e9', 600: '#0284c7', 700: '#0369a1',
        800: '#075985', 900: '#0c4a6e', 950: '#082f49'
      },
      gray: {
        50: '#f9fafb', 100: '#f3f4f6', 200: '#e5e7eb', 300: '#d1d5db',
        400: '#9ca3af', 500: '#6b7280', 600: '#4b5563', 700: '#374151',
        800: '#1f2937', 900: '#111827', 950: '#030712'
      }
    },
    typography: {
      fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
      fontSize: {
        xs: '0.75rem', sm: '0.875rem', base: '1rem', lg: '1.125rem',
        xl: '1.25rem', '2xl': '1.5rem', '3xl': '1.875rem', '4xl': '2.25rem'
      },
      lineHeight: {
        tight: '1.25', normal: '1.5', relaxed: '1.625', loose: '2'
      },
      letterSpacing: {
        tight: '-0.025em', normal: '0', wide: '0.025em'
      }
    },
    spacing: {
      xs: '0.5rem', sm: '0.75rem', md: '1rem', lg: '1.5rem',
      xl: '2rem', '2xl': '2.5rem', '3xl': '3rem', '4xl': '4rem'
    },
    borderRadius: {
      none: '0', sm: '0.125rem', md: '0.375rem', lg: '0.5rem',
      xl: '0.75rem', '2xl': '1rem', '3xl': '1.5rem', full: '9999px'
    },
    shadows: {
      sm: '0 1px 2px 0 rgb(0 0 0 / 0.05)',
      md: '0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)',
      lg: '0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)',
      xl: '0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1)',
      '2xl': '0 25px 50px -12px rgb(0 0 0 / 0.25)'
    },
    animations: {
      duration: {
        fast: '150ms', normal: '300ms', slow: '500ms'
      },
      easing: {
        ease: 'ease', easeIn: 'ease-in', easeOut: 'ease-out', easeInOut: 'ease-in-out'
      }
    }
  }
}

// 获取状态标签
function getStatusLabel(type: string): string {
  const labels: Record<string, string> = {
    success: '成功',
    warning: '警告',
    error: '错误',
    info: '信息'
  }
  return labels[type] || type
}

// 基础主题变化处理
function onBaseThemeChange() {
  const baseThemeConfig = allThemes.value.find(t => t.id === baseTheme.value)
  if (baseThemeConfig) {
    editingTheme.value = createCustomTheme(baseTheme.value, {
      name: editingTheme.value.name,
      description: editingTheme.value.description
    })
    hasChanges.value = true
  }
}

// 颜色变化处理
function onColorChange() {
  hasChanges.value = true
  validateForm()
}

// 通用变化处理
function onChange() {
  hasChanges.value = true
  validateForm()
}

// 表单验证
function validateForm() {
  errors.value = validateTheme(editingTheme.value)
}

// 重置主题
function resetTheme() {
  if (originalTheme.value) {
    editingTheme.value = JSON.parse(JSON.stringify(originalTheme.value))
    hasChanges.value = false
    errors.value = []
  }
}

// 预览主题
function previewTheme() {
  // 临时应用主题进行预览
  const style = document.createElement('style')
  style.id = 'theme-preview'
  style.textContent = generateThemeCSS(editingTheme.value)
  
  // 移除之前的预览样式
  const existingStyle = document.getElementById('theme-preview')
  if (existingStyle) {
    existingStyle.remove()
  }
  
  document.head.appendChild(style)
  
  // 3秒后移除预览
  setTimeout(() => {
    style.remove()
  }, 3000)
}

// 保存主题
function saveTheme() {
  if (!isValid.value) return
  
  const finalTheme = {
    ...editingTheme.value,
    id: editingTheme.value.id || `custom-${Date.now()}`
  }
  
  emit('save', finalTheme)
  emit('update:visible', false)
  emit('close')
}

// 生成主题CSS
function generateThemeCSS(theme: ThemeConfig): string {
  let css = ':root {\n'
  
  // 颜色变量
  Object.entries(theme.colors).forEach(([colorName, colorValues]) => {
    Object.entries(colorValues).forEach(([shade, value]) => {
      css += `  --color-${colorName}-${shade}: ${value};\n`
    })
  })
  
  // 排版变量
  css += `  --font-family: ${theme.typography.fontFamily};\n`
  Object.entries(theme.typography.fontSize).forEach(([size, value]) => {
    css += `  --font-size-${size}: ${value};\n`
  })
  
  css += '}\n'
  
  return css
}

// 监听属性变化
watch(() => props.themeId, (newThemeId) => {
  if (newThemeId) {
    const theme = allThemes.value.find(t => t.id === newThemeId)
    if (theme) {
      originalTheme.value = JSON.parse(JSON.stringify(theme))
      editingTheme.value = JSON.parse(JSON.stringify(theme))
      baseTheme.value = theme.id.startsWith('custom-') ? 'light' : theme.id
      hasChanges.value = false
      errors.value = []
    }
  }
}, { immediate: true })

// 组件挂载
onMounted(() => {
  if (props.themeId) {
    const theme = allThemes.value.find(t => t.id === props.themeId)
    if (theme) {
      originalTheme.value = JSON.parse(JSON.stringify(theme))
      editingTheme.value = JSON.parse(JSON.stringify(theme))
      baseTheme.value = theme.id.startsWith('custom-') ? 'light' : theme.id
    }
  }
})
</script>

<style scoped>
.theme-editor-container {
  max-height: 90vh;
  overflow-y: auto;
}

/* 头部样式 */
.editor-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #e5e7eb;
}

.header-content h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 0.5rem 0;
}

.editor-description {
  color: #6b7280;
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 0.5rem;
}

.action-btn {
  padding: 0.5rem 1rem;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  font-weight: 500;
  border: none;
  cursor: pointer;
  transition: all 0.2s;
}

.reset-btn {
  background-color: #f3f4f6;
  color: #374151;
}

.reset-btn:hover:not(:disabled) {
  background-color: #e5e7eb;
}

.preview-btn {
  background-color: #dbeafe;
  color: #1e40af;
}

.preview-btn:hover {
  background-color: #bfdbfe;
}

.save-btn {
  background-color: #3b82f6;
  color: white;
}

.save-btn:hover:not(:disabled) {
  background-color: #2563eb;
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 主题信息部分 */
.theme-info-section {
  margin-bottom: 2rem;
}

.section-header {
  margin-bottom: 1rem;
}

.section-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0;
}

.info-content {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
  margin-bottom: 0.5rem;
}

.form-input,
.form-select,
.form-textarea {
  padding: 0.5rem 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  transition: border-color 0.2s;
}

.form-input:focus,
.form-select:focus,
.form-textarea:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

/* 选项卡样式 */
.editor-tabs {
  margin-bottom: 1.5rem;
}

.tabs-nav {
  display: flex;
  border-bottom: 1px solid #e5e7eb;
}

.tab-btn {
  padding: 0.75rem 1rem;
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  font-size: 0.875rem;
  font-weight: 500;
  color: #6b7280;
  cursor: pointer;
  transition: all 0.2s;
}

.tab-btn:hover {
  color: #374151;
}

.tab-btn.active {
  color: #3b82f6;
  border-bottom-color: #3b82f6;
}

.tab-btn i {
  margin-right: 0.5rem;
}

/* 选项卡内容 */
.tab-content {
  min-height: 400px;
}

/* 颜色配置 */
.color-config {
  display: grid;
  gap: 2rem;
}

.color-sections {
  display: grid;
  gap: 1.5rem;
}

.section-subtitle {
  font-size: 1rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 1rem 0;
}

.color-palette {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(80px, 1fr));
  gap: 1rem;
}

.color-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
}

.color-preview {
  width: 40px;
  height: 40px;
  border-radius: 0.375rem;
  border: 1px solid #e5e7eb;
}

.color-input {
  width: 100%;
  height: 30px;
  border: 1px solid #d1d5db;
  border-radius: 0.25rem;
  cursor: pointer;
}

.color-label {
  font-size: 0.75rem;
  color: #6b7280;
}

.color-palette.compact {
  grid-template-columns: repeat(auto-fit, minmax(60px, 1fr));
}

.color-item.compact {
  gap: 0.25rem;
}

.color-preview.small {
  width: 30px;
  height: 30px;
}

.color-input.small {
  width: 100%;
  height: 25px;
}

.status-colors {
  display: grid;
  gap: 1rem;
}

.status-color-group {
  padding: 1rem;
  background-color: #f9fafb;
  border-radius: 0.5rem;
}

.status-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: #374151;
  margin: 0 0 0.75rem 0;
}

/* 排版配置 */
.typography-config {
  display: grid;
  gap: 2rem;
}

.typography-sections {
  display: grid;
  gap: 1.5rem;
}

.font-sizes,
.line-heights {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1rem;
}

.font-size-item,
.line-height-item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.size-label,
.height-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
}

.size-input,
.height-input {
  padding: 0.5rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  font-size: 0.875rem;
}

/* 间距配置 */
.spacing-config {
  padding: 1rem;
}

.spacing-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 1rem;
}

.spacing-item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.spacing-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
}

.spacing-input {
  padding: 0.5rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  font-size: 0.875rem;
}

/* 圆角配置 */
.border-radius-config {
  padding: 1rem;
}

.radius-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 1rem;
}

.radius-item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.radius-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
}

.radius-input {
  padding: 0.5rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  font-size: 0.875rem;
}

.radius-preview {
  width: 40px;
  height: 20px;
  background-color: #e5e7eb;
  margin: 0 auto;
}

/* 阴影配置 */
.shadows-config {
  padding: 1rem;
}

.shadow-items {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.shadow-item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.shadow-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
}

.shadow-input {
  padding: 0.5rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  resize: vertical;
}

.shadow-preview {
  width: 40px;
  height: 40px;
  background-color: white;
  border-radius: 0.375rem;
  margin: 0 auto;
}

/* 动画配置 */
.animations-config {
  display: grid;
  gap: 2rem;
}

.animation-sections {
  display: grid;
  gap: 1.5rem;
}

.duration-items,
.easing-items {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1rem;
}

.duration-item,
.easing-item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.duration-label,
.easing-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
}

.duration-input,
.easing-select {
  padding: 0.5rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  font-size: 0.875rem;
}

/* 主题预览 */
.theme-preview-section {
  margin-top: 2rem;
  padding-top: 2rem;
  border-top: 1px solid #e5e7eb;
}

.preview-content {
  padding: 1rem;
  background-color: #f9fafb;
  border-radius: 0.5rem;
}

.preview-container {
  padding: 1.5rem;
  background: white;
  border-radius: 0.5rem;
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1);
}

.preview-header {
  margin-bottom: 1.5rem;
  text-align: center;
}

.preview-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 0.5rem 0;
}

.preview-description {
  color: #6b7280;
  margin: 0;
}

.preview-body {
  display: grid;
  gap: 1.5rem;
}

.preview-colors {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(100px, 1fr));
  gap: 0.5rem;
}

.preview-color-item {
  padding: 0.75rem;
  color: white;
  text-align: center;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  font-weight: 500;
}

.preview-text {
  padding: 1rem;
  background-color: #f9fafb;
  border-radius: 0.375rem;
}

.preview-text h5 {
  margin: 0 0 0.5rem 0;
  font-size: 1.125rem;
  font-weight: 600;
}

.preview-text p {
  margin: 0;
  font-size: 0.875rem;
  line-height: 1.5;
}

.preview-components {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.preview-btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 0.375rem;
  font-weight: 500;
  cursor: pointer;
}

.preview-card {
  flex: 1;
  min-width: 200px;
}

.preview-card h6 {
  margin: 0 0 0.5rem 0;
  font-size: 1rem;
  font-weight: 600;
}

.preview-card p {
  margin: 0;
  font-size: 0.875rem;
  line-height: 1.5;
}

/* 错误提示 */
.error-section {
  margin-top: 1rem;
  padding: 1rem;
  background-color: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 0.375rem;
}

.error-content {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
}

.error-icon {
  color: #dc2626;
  margin-top: 0.125rem;
}

.error-messages {
  flex: 1;
}

.error-message {
  color: #dc2626;
  font-size: 0.875rem;
  margin: 0 0 0.25rem 0;
}

.error-message:last-child {
  margin-bottom: 0;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .theme-editor-container {
    padding: 1rem;
  }
  
  .editor-header {
    flex-direction: column;
    gap: 1rem;
  }
  
  .header-actions {
    width: 100%;
    justify-content: flex-end;
  }
  
  .info-content {
    grid-template-columns: 1fr;
  }
  
  .color-palette {
    grid-template-columns: repeat(auto-fit, minmax(60px, 1fr));
  }
  
  .font-sizes,
  .line-heights,
  .spacing-grid,
  .radius-grid {
    grid-template-columns: repeat(auto-fit, minmax(100px, 1fr));
  }
  
  .shadow-items {
    grid-template-columns: 1fr;
  }
  
  .duration-items,
  .easing-items {
    grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  }
  
  .preview-components {
    flex-direction: column;
  }
}

/* 深色模式 */
.dark .theme-editor-container {
  background-color: #1f2937;
}

.dark .editor-header {
  border-bottom-color: #374151;
}

.dark .header-content h2 {
  color: #f9fafb;
}

.dark .editor-description {
  color: #9ca3af;
}

.dark .section-title,
.dark .section-subtitle {
  color: #f9fafb;
}

.dark .form-label,
.dark .size-label,
.dark .height-label,
.dark .spacing-label,
.dark .radius-label,
.dark .shadow-label,
.dark .duration-label,
.dark .easing-label,
.dark .status-label {
  color: #d1d5db;
}

.dark .form-input,
.dark .form-select,
.dark .form-textarea,
.dark .size-input,
.dark .height-input,
.dark .spacing-input,
.dark .radius-input,
.dark .shadow-input,
.dark .duration-input,
.dark .easing-select {
  background-color: #374151;
  border-color: #4b5563;
  color: #f9fafb;
}

.dark .form-input:focus,
.dark .form-select:focus,
.dark .form-textarea:focus,
.dark .size-input:focus,
.dark .height-input:focus,
.dark .spacing-input:focus,
.dark .radius-input:focus,
.dark .shadow-input:focus,
.dark .duration-input:focus,
.dark .easing-select:focus {
  border-color: #60a5fa;
  box-shadow: 0 0 0 3px rgba(96, 165, 250, 0.1);
}

.dark .tabs-nav {
  border-bottom-color: #374151;
}

.dark .tab-btn {
  color: #9ca3af;
}

.dark .tab-btn:hover {
  color: #d1d5db;
}

.dark .tab-btn.active {
  color: #60a5fa;
  border-bottom-color: #60a5fa;
}

.dark .reset-btn {
  background-color: #374151;
  color: #d1d5db;
}

.dark .reset-btn:hover:not(:disabled) {
  background-color: #4b5563;
}

.dark .preview-btn {
  background-color: #1e3a8a;
  color: #bfdbfe;
}

.dark .preview-btn:hover {
  background-color: #1e40af;
}

.dark .status-color-group {
  background-color: #374151;
}

.dark .preview-content {
  background-color: #374151;
}

.dark .preview-container {
  background-color: #1f2937;
}

.dark .preview-title {
  color: #f9fafb;
}

.dark .preview-description {
  color: #9ca3af;
}

.dark .preview-text {
  background-color: #374151;
}

.dark .preview-text h5 {
  color: #f9fafb;
}

.dark .preview-text p {
  color: #d1d5db;
}

.dark .error-section {
  background-color: #7f1d1d;
  border-color: #991b1b;
}

.dark .error-icon {
  color: #f87171;
}

.dark .error-message {
  color: #fca5a5;
}
</style>