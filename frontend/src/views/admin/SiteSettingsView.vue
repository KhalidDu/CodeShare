<template>
  <div class="site-settings-view">
    <div class="section-header">
      <h2>站点设置</h2>
      <p class="section-description">配置网站的基本信息和外观设置</p>
    </div>

    <form @submit.prevent="saveSettings" class="settings-form">
      <div class="form-section">
        <h3>基本信息</h3>
        <div class="row">
          <div class="col-md-6">
            <div class="form-group">
              <label for="siteName" class="form-label">站点名称 *</label>
              <input
                id="siteName"
                v-model="form.siteName"
                type="text"
                class="form-control"
                :class="{ 'is-invalid': errors.siteName }"
                placeholder="请输入站点名称"
                required
              />
              <div v-if="errors.siteName" class="invalid-feedback">
                {{ errors.siteName }}
              </div>
            </div>
          </div>
          <div class="col-md-6">
            <div class="form-group">
              <label for="language" class="form-label">默认语言</label>
              <select
                id="language"
                v-model="form.language"
                class="form-select"
              >
                <option value="zh-CN">简体中文</option>
                <option value="zh-TW">繁体中文</option>
                <option value="en-US">English</option>
              </select>
            </div>
          </div>
        </div>

        <div class="form-group">
          <label for="siteDescription" class="form-label">站点描述</label>
          <textarea
            id="siteDescription"
            v-model="form.siteDescription"
            class="form-control"
            rows="3"
            placeholder="请输入站点描述"
          ></textarea>
        </div>
      </div>

      <div class="form-section">
        <h3>外观设置</h3>
        <div class="row">
          <div class="col-md-6">
            <div class="form-group">
              <label for="theme" class="form-label">主题风格</label>
              <select
                id="theme"
                v-model="form.theme"
                class="form-select"
              >
                <option value="light">浅色主题</option>
                <option value="dark">深色主题</option>
                <option value="auto">跟随系统</option>
              </select>
            </div>
          </div>
          <div class="col-md-6">
            <div class="form-group">
              <label for="pageSize" class="form-label">每页显示条数</label>
              <select
                id="pageSize"
                v-model="form.pageSize"
                class="form-select"
              >
                <option :value="10">10 条/页</option>
                <option :value="20">20 条/页</option>
                <option :value="50">50 条/页</option>
                <option :value="100">100 条/页</option>
              </select>
            </div>
          </div>
        </div>

        <div class="form-group">
          <label for="logoUrl" class="form-label">Logo 地址</label>
          <div class="input-group">
            <input
              id="logoUrl"
              v-model="form.logoUrl"
              type="url"
              class="form-control"
              :class="{ 'is-invalid': errors.logoUrl }"
              placeholder="https://example.com/logo.png"
            />
            <button 
              type="button"
              class="btn btn-outline-secondary"
              @click="uploadLogo"
            >
              <i class="fas fa-upload"></i>
              上传
            </button>
            <div v-if="errors.logoUrl" class="invalid-feedback">
              {{ errors.logoUrl }}
            </div>
          </div>
          <div v-if="form.logoUrl" class="mt-2">
            <img 
              :src="form.logoUrl" 
              alt="Logo预览" 
              class="logo-preview"
              @error="handleImageError"
            />
          </div>
        </div>
      </div>

      <div class="form-section">
        <h3>用户注册</h3>
        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="allowRegistration"
              v-model="form.allowRegistration"
              type="checkbox"
              class="form-check-input"
            />
            <label for="allowRegistration" class="form-check-label">
              允许新用户注册
            </label>
          </div>
          <small class="form-text text-muted">
            关闭此选项将禁止新用户注册，只有管理员可以创建用户账户
          </small>
        </div>
      </div>

      <div class="form-section">
        <h3>公告设置</h3>
        <div class="form-group">
          <label for="announcement" class="form-label">系统公告</label>
          <textarea
            id="announcement"
            v-model="form.announcement"
            class="form-control"
            rows="4"
            placeholder="请输入系统公告内容，支持HTML格式"
          ></textarea>
          <small class="form-text text-muted">
            公告将在网站首页显示，支持HTML格式
          </small>
        </div>
      </div>

      <div class="form-section">
        <h3>自定义代码</h3>
        <div class="form-group">
          <label for="customCss" class="form-label">自定义 CSS</label>
          <textarea
            id="customCss"
            v-model="form.customCss"
            class="form-control font-monospace"
            rows="6"
            placeholder="请输入自定义CSS代码"
          ></textarea>
        </div>
        
        <div class="form-group">
          <label for="customJs" class="form-label">自定义 JavaScript</label>
          <textarea
            id="customJs"
            v-model="form.customJs"
            class="form-control font-monospace"
            rows="6"
            placeholder="请输入自定义JavaScript代码"
          ></textarea>
          <small class="form-text text-muted">
            注意：自定义代码将在所有页面加载，请确保代码安全性
          </small>
        </div>
      </div>

      <div class="form-actions">
        <button 
          type="submit" 
          class="btn btn-primary"
          :disabled="loading || !hasChanges"
        >
          <i class="fas fa-save" :class="{ 'fa-spin': loading }"></i>
          保存设置
        </button>
        <button 
          type="button" 
          class="btn btn-outline-secondary"
          @click="resetForm"
          :disabled="loading"
        >
          <i class="fas fa-undo"></i>
          重置
        </button>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import type { SiteSettings, UpdateSiteSettingsRequest } from '@/types/settings'

interface Props {
  settings: SiteSettings | null
  loading: boolean
}

interface Emits {
  (e: 'update:settings', request: UpdateSiteSettingsRequest): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// 表单数据
const form = ref<SiteSettings>({
  siteName: '',
  siteDescription: '',
  logoUrl: '',
  theme: 'light',
  language: 'zh-CN',
  pageSize: 20,
  allowRegistration: true,
  announcement: '',
  customCss: '',
  customJs: ''
})

// 错误信息
const errors = ref<Record<string, string>>({})

// 计算属性
const hasChanges = computed(() => {
  if (!props.settings) return false
  
  return Object.keys(form.value).some(key => {
    const currentValue = form.value[key as keyof SiteSettings]
    const originalValue = props.settings![key as keyof SiteSettings]
    return currentValue !== originalValue
  })
})

// 监听props变化，更新表单
watch(() => props.settings, (newSettings) => {
  if (newSettings) {
    form.value = { ...newSettings }
    errors.value = {}
  }
}, { immediate: true })

// 方法
function validateForm(): boolean {
  errors.value = {}
  
  if (!form.value.siteName?.trim()) {
    errors.value.siteName = '站点名称不能为空'
  }
  
  if (form.value.siteName?.length > 100) {
    errors.value.siteName = '站点名称不能超过100个字符'
  }
  
  if (form.value.logoUrl && !isValidUrl(form.value.logoUrl)) {
    errors.value.logoUrl = '请输入有效的URL地址'
  }
  
  if (form.value.pageSize < 1 || form.value.pageSize > 200) {
    errors.value.pageSize = '每页显示条数必须在1-200之间'
  }
  
  return Object.keys(errors.value).length === 0
}

function isValidUrl(url: string): boolean {
  try {
    new URL(url)
    return true
  } catch {
    return false
  }
}

function saveSettings() {
  if (!validateForm()) return
  
  const request: UpdateSiteSettingsRequest = {}
  
  // 只包含有变化的字段
  if (form.value.siteName !== props.settings?.siteName) {
    request.siteName = form.value.siteName
  }
  if (form.value.siteDescription !== props.settings?.siteDescription) {
    request.siteDescription = form.value.siteDescription
  }
  if (form.value.logoUrl !== props.settings?.logoUrl) {
    request.logoUrl = form.value.logoUrl
  }
  if (form.value.theme !== props.settings?.theme) {
    request.theme = form.value.theme
  }
  if (form.value.language !== props.settings?.language) {
    request.language = form.value.language
  }
  if (form.value.pageSize !== props.settings?.pageSize) {
    request.pageSize = form.value.pageSize
  }
  if (form.value.allowRegistration !== props.settings?.allowRegistration) {
    request.allowRegistration = form.value.allowRegistration
  }
  if (form.value.announcement !== props.settings?.announcement) {
    request.announcement = form.value.announcement
  }
  if (form.value.customCss !== props.settings?.customCss) {
    request.customCss = form.value.customCss
  }
  if (form.value.customJs !== props.settings?.customJs) {
    request.customJs = form.value.customJs
  }
  
  emit('update:settings', request)
}

function resetForm() {
  if (props.settings) {
    form.value = { ...props.settings }
    errors.value = {}
  }
}

function uploadLogo() {
  // 这里应该实现文件上传逻辑
  alert('文件上传功能将在后续版本中实现')
}

function handleImageError(event: Event) {
  const img = event.target as HTMLImageElement
  img.style.display = 'none'
}
</script>

<style scoped>
.site-settings-view {
  max-width: 800px;
}

.section-header {
  margin-bottom: 2rem;
}

.section-header h2 {
  font-size: 1.5rem;
  font-weight: 600;
  margin: 0 0 0.5rem 0;
  color: #2c3e50;
}

.section-description {
  color: #6c757d;
  margin: 0;
}

.form-section {
  margin-bottom: 2rem;
  padding-bottom: 2rem;
  border-bottom: 1px solid #e9ecef;
}

.form-section:last-child {
  border-bottom: none;
  margin-bottom: 0;
  padding-bottom: 0;
}

.form-section h3 {
  font-size: 1.1rem;
  font-weight: 600;
  margin: 0 0 1.5rem 0;
  color: #495057;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-label {
  font-weight: 500;
  margin-bottom: 0.5rem;
  color: #495057;
}

.logo-preview {
  max-width: 200px;
  max-height: 60px;
  border: 1px solid #dee2e6;
  border-radius: 0.25rem;
  padding: 0.25rem;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
  padding-top: 2rem;
  border-top: 1px solid #e9ecef;
}

.font-monospace {
  font-family: 'Courier New', Courier, monospace;
  font-size: 0.875rem;
}

@media (max-width: 768px) {
  .form-actions {
    flex-direction: column;
  }
  
  .form-actions .btn {
    width: 100%;
  }
}
</style>