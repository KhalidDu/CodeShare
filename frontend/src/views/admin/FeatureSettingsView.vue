<template>
  <div class="feature-settings-view">
    <div class="section-header">
      <h2>功能设置</h2>
      <p class="section-description">控制系统的功能模块和特性开关</p>
    </div>

    <form @submit.prevent="saveSettings" class="settings-form">
      <!-- 代码片段功能 -->
      <div class="form-section">
        <h3>代码片段功能</h3>
        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="enableCodeSnippets"
              v-model="form.enableCodeSnippets"
              type="checkbox"
              class="form-check-input"
            />
            <label for="enableCodeSnippets" class="form-check-label">
              启用代码片段功能
            </label>
          </div>
          <small class="form-text text-muted">
            允许用户创建、编辑和分享代码片段
          </small>
        </div>

        <div v-if="form.enableCodeSnippets" class="form-group">
          <label for="maxSnippetLength" class="form-label">最大代码片段长度（字符）</label>
          <input
            id="maxSnippetLength"
            v-model.number="form.maxSnippetLength"
            type="number"
            class="form-control"
            :class="{ 'is-invalid': errors.maxSnippetLength }"
            min="1000"
            max="1000000"
            required
          />
          <div v-if="errors.maxSnippetLength" class="invalid-feedback">
            {{ errors.maxSnippetLength }}
          </div>
          <small class="form-text text-muted">
            单个代码片段的最大字符数限制
          </small>
        </div>

        <div v-if="form.enableCodeSnippets" class="form-group">
          <label for="maxSnippetsPerUser" class="form-label">用户最大片段数量</label>
          <input
            id="maxSnippetsPerUser"
            v-model.number="form.maxSnippetsPerUser"
            type="number"
            class="form-control"
            :class="{ 'is-invalid': errors.maxSnippetsPerUser }"
            min="10"
            max="10000"
            required
          />
          <div v-if="errors.maxSnippetsPerUser" class="invalid-feedback">
            {{ errors.maxSnippetsPerUser }}
          </div>
          <small class="form-text text-muted">
            单个用户可以创建的最大代码片段数量
          </small>
        </div>

        <div v-if="form.enableCodeSnippets" class="form-group">
          <label class="form-label">支持的编程语言</label>
          <div class="language-list">
            <div v-for="language in availableLanguages" :key="language.value" class="form-check">
              <input
                :id="`lang-${language.value}`"
                v-model="form.supportedLanguages"
                type="checkbox"
                :value="language.value"
                class="form-check-input"
              />
              <label :for="`lang-${language.value}`" class="form-check-label">
                {{ language.label }}
              </label>
            </div>
          </div>
        </div>
      </div>

      <!-- 分享功能 -->
      <div class="form-section">
        <h3>分享功能</h3>
        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="enableSharing"
              v-model="form.enableSharing"
              type="checkbox"
              class="form-check-input"
            />
            <label for="enableSharing" class="form-check-label">
              启用分享功能
            </label>
          </div>
          <small class="form-text text-muted">
            允许用户通过链接分享代码片段
          </small>
        </div>

        <div v-if="form.enableSharing" class="form-group">
          <label for="shareTokenExpiryHours" class="form-label">分享链接有效期（小时）</label>
          <input
            id="shareTokenExpiryHours"
            v-model.number="form.shareTokenExpiryHours"
            type="number"
            class="form-control"
            :class="{ 'is-invalid': errors.shareTokenExpiryHours }"
            min="1"
            max="8760"
            required
          />
          <div v-if="errors.shareTokenExpiryHours" class="invalid-feedback">
            {{ errors.shareTokenExpiryHours }}
          </div>
          <small class="form-text text-muted">
            分享链接的有效时间，0表示永久有效
          </small>
        </div>

        <div v-if="form.enableSharing" class="form-group">
          <label for="maxSharesPerSnippet" class="form-label">单个片段最大分享次数</label>
          <input
            id="maxSharesPerSnippet"
            v-model.number="form.maxSharesPerSnippet"
            type="number"
            class="form-control"
            :class="{ 'is-invalid': errors.maxSharesPerSnippet }"
            min="1"
            max="1000"
            required
          />
          <div v-if="errors.maxSharesPerSnippet" class="invalid-feedback">
            {{ errors.maxSharesPerSnippet }}
          </div>
        </div>

        <div v-if="form.enableSharing" class="form-group">
          <div class="form-check form-switch">
            <input
              id="enableSharePassword"
              v-model="form.enableSharePassword"
              type="checkbox"
              class="form-check-input"
            />
            <label for="enableSharePassword" class="form-check-label">
              启用分享密码保护
            </label>
          </div>
        </div>

        <div v-if="form.enableSharing && form.enableSharePassword" class="form-group">
          <div class="form-check form-switch">
            <input
              id="requireSharePassword"
              v-model="form.requireSharePassword"
              type="checkbox"
              class="form-check-input"
            />
            <label for="requireSharePassword" class="form-check-label">
              强制要求分享密码
            </label>
          </div>
        </div>
      </div>

      <!-- 剪贴板功能 -->
      <div class="form-section">
        <h3>剪贴板功能</h3>
        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="enableClipboard"
              v-model="form.enableClipboard"
              type="checkbox"
              class="form-check-input"
            />
            <label for="enableClipboard" class="form-check-label">
              启用剪贴板历史功能
            </label>
          </div>
          <small class="form-text text-muted">
            允许用户保存和管理剪贴板历史记录
          </small>
        </div>

        <div v-if="form.enableClipboard" class="form-group">
          <label for="maxClipboardItems" class="form-label">最大剪贴板项目数</label>
          <input
            id="maxClipboardItems"
            v-model.number="form.maxClipboardItems"
            type="number"
            class="form-control"
            :class="{ 'is-invalid': errors.maxClipboardItems }"
            min="10"
            max="1000"
            required
          />
          <div v-if="errors.maxClipboardItems" class="invalid-feedback">
            {{ errors.maxClipboardItems }}
          </div>
          <small class="form-text text-muted">
            单个用户保存的最大剪贴板项目数量
          </small>
        </div>

        <div v-if="form.enableClipboard" class="form-group">
          <label for="clipboardRetentionDays" class="form-label">剪贴板保留天数</label>
          <input
            id="clipboardRetentionDays"
            v-model.number="form.clipboardRetentionDays"
            type="number"
            class="form-control"
            :class="{ 'is-invalid': errors.clipboardRetentionDays }"
            min="1"
            max="365"
            required
          />
          <div v-if="errors.clipboardRetentionDays" class="invalid-feedback">
            {{ errors.clipboardRetentionDays }}
          </div>
          <small class="form-text text-muted">
            剪贴板项目的保留时间，超时自动删除
          </small>
        </div>
      </div>

      <!-- 文件上传功能 -->
      <div class="form-section">
        <h3>文件上传功能</h3>
        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="enableFileUpload"
              v-model="form.enableFileUpload"
              type="checkbox"
              class="form-check-input"
            />
            <label for="enableFileUpload" class="form-check-label">
              启用文件上传功能
            </label>
          </div>
          <small class="form-text text-muted">
            允许用户在代码片段中附加文件
          </small>
        </div>

        <div v-if="form.enableFileUpload" class="form-group">
          <label for="maxFileSize" class="form-label">最大文件大小（MB）</label>
          <input
            id="maxFileSize"
            v-model.number="form.maxFileSize"
            type="number"
            class="form-control"
            :class="{ 'is-invalid': errors.maxFileSize }"
            min="1"
            max="100"
            required
          />
          <div v-if="errors.maxFileSize" class="invalid-feedback">
            {{ errors.maxFileSize }}
          </div>
        </div>

        <div v-if="form.enableFileUpload" class="form-group">
          <label for="allowedFileTypes" class="form-label">允许的文件类型</label>
          <div class="file-type-list">
            <div v-for="fileType in availableFileTypes" :key="fileType.value" class="form-check">
              <input
                :id="`file-${fileType.value}`"
                v-model="form.allowedFileTypes"
                type="checkbox"
                :value="fileType.value"
                class="form-check-input"
              />
              <label :for="`file-${fileType.value}`" class="form-check-label">
                {{ fileType.label }}
              </label>
            </div>
          </div>
        </div>

        <div v-if="form.enableFileUpload" class="form-group">
          <label for="maxFilesPerSnippet" class="form-label">单个片段最大文件数</label>
          <input
            id="maxFilesPerSnippet"
            v-model.number="form.maxFilesPerSnippet"
            type="number"
            class="form-control"
            :class="{ 'is-invalid': errors.maxFilesPerSnippet }"
            min="1"
            max="20"
            required
          />
          <div v-if="errors.maxFilesPerSnippet" class="invalid-feedback">
            {{ errors.maxFilesPerSnippet }}
          </div>
        </div>
      </div>

      <!-- 搜索功能 -->
      <div class="form-section">
        <h3>搜索功能</h3>
        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="enableSearch"
              v-model="form.enableSearch"
              type="checkbox"
              class="form-check-input"
            />
            <label for="enableSearch" class="form-check-label">
              启用全局搜索功能
            </label>
          </div>
        </div>

        <div v-if="form.enableSearch" class="form-group">
          <div class="form-check form-switch">
            <input
              id="enableFullTextSearch"
              v-model="form.enableFullTextSearch"
              type="checkbox"
              class="form-check-input"
            />
            <label for="enableFullTextSearch" class="form-check-label">
              启用全文搜索
            </label>
          </div>
          <small class="form-text text-muted">
            支持在代码内容中进行全文搜索
          </small>
        </div>

        <div v-if="form.enableSearch" class="form-group">
          <div class="form-check form-switch">
            <input
              id="enableSearchIndexing"
              v-model="form.enableSearchIndexing"
              type="checkbox"
              class="form-check-input"
            />
            <label for="enableSearchIndexing" class="form-check-label">
              启用搜索索引
            </label>
          </div>
          <small class="form-text text-muted">
            提高搜索性能，但会增加系统资源消耗
          </small>
        </div>
      </div>

      <!-- API功能 -->
      <div class="form-section">
        <h3>API功能</h3>
        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="enableApi"
              v-model="form.enableApi"
              type="checkbox"
              class="form-check-input"
            />
            <label for="enableApi" class="form-check-label">
              启用REST API
            </label>
          </div>
          <small class="form-text text-muted">
            允许通过API访问系统功能
          </small>
        </div>

        <div v-if="form.enableApi" class="form-group">
          <label for="apiRateLimit" class="form-label">API调用频率限制（次/分钟）</label>
          <input
            id="apiRateLimit"
            v-model.number="form.apiRateLimit"
            type="number"
            class="form-control"
            :class="{ 'is-invalid': errors.apiRateLimit }"
            min="1"
            max="10000"
            required
          />
          <div v-if="errors.apiRateLimit" class="invalid-feedback">
            {{ errors.apiRateLimit }}
          </div>
          <small class="form-text text-muted">
            每分钟允许的API调用次数
          </small>
        </div>

        <div v-if="form.enableApi" class="form-group">
          <div class="form-check form-switch">
            <input
              id="enableApiKeyAuth"
              v-model="form.enableApiKeyAuth"
              type="checkbox"
              class="form-check-input"
            />
            <label for="enableApiKeyAuth" class="form-check-label">
              启用API密钥认证
            </label>
          </div>
        </div>
      </div>

      <!-- 操作按钮 -->
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
import type { FeatureSettings, UpdateFeatureSettingsRequest } from '@/types/settings'

interface Props {
  settings: FeatureSettings | null
  loading: boolean
}

interface Emits {
  (e: 'update:settings', request: UpdateFeatureSettingsRequest): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// 可用的编程语言
const availableLanguages = [
  { value: 'javascript', label: 'JavaScript' },
  { value: 'typescript', label: 'TypeScript' },
  { value: 'python', label: 'Python' },
  { value: 'java', label: 'Java' },
  { value: 'csharp', label: 'C#' },
  { value: 'cpp', label: 'C++' },
  { value: 'go', label: 'Go' },
  { value: 'rust', label: 'Rust' },
  { value: 'php', label: 'PHP' },
  { value: 'ruby', label: 'Ruby' },
  { value: 'swift', label: 'Swift' },
  { value: 'kotlin', label: 'Kotlin' },
  { value: 'sql', label: 'SQL' },
  { value: 'html', label: 'HTML' },
  { value: 'css', label: 'CSS' },
  { value: 'json', label: 'JSON' },
  { value: 'xml', label: 'XML' },
  { value: 'yaml', label: 'YAML' },
  { value: 'markdown', label: 'Markdown' },
  { value: 'bash', label: 'Bash' },
  { value: 'powershell', label: 'PowerShell' },
  { value: 'dockerfile', label: 'Dockerfile' }
]

// 可用的文件类型
const availableFileTypes = [
  { value: 'image', label: '图片文件 (jpg, png, gif, svg)' },
  { value: 'document', label: '文档文件 (pdf, doc, docx)' },
  { value: 'code', label: '代码文件 (js, py, java, cpp)' },
  { value: 'archive', label: '压缩文件 (zip, rar, 7z)' },
  { value: 'text', label: '文本文件 (txt, md, csv)' }
]

// 表单数据
const form = ref<FeatureSettings>({
  enableCodeSnippets: true,
  maxSnippetLength: 50000,
  maxSnippetsPerUser: 100,
  supportedLanguages: ['javascript', 'python', 'java', 'csharp', 'cpp'],
  enableSharing: true,
  shareTokenExpiryHours: 168,
  maxSharesPerSnippet: 10,
  enableSharePassword: true,
  requireSharePassword: false,
  enableClipboard: true,
  maxClipboardItems: 50,
  clipboardRetentionDays: 30,
  enableFileUpload: true,
  maxFileSize: 10,
  allowedFileTypes: ['image', 'document', 'code', 'text'],
  maxFilesPerSnippet: 5,
  enableSearch: true,
  enableFullTextSearch: true,
  enableSearchIndexing: true,
  enableApi: true,
  apiRateLimit: 100,
  enableApiKeyAuth: true
})

// 错误信息
const errors = ref<Record<string, string>>({})

// 计算属性
const hasChanges = computed(() => {
  if (!props.settings) return false
  
  return Object.keys(form.value).some(key => {
    const currentValue = form.value[key as keyof FeatureSettings]
    const originalValue = props.settings![key as keyof FeatureSettings]
    
    // 处理数组的比较
    if (Array.isArray(currentValue) && Array.isArray(originalValue)) {
      return JSON.stringify(currentValue) !== JSON.stringify(originalValue)
    }
    
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
  
  if (form.value.enableCodeSnippets) {
    if (form.value.maxSnippetLength < 1000 || form.value.maxSnippetLength > 1000000) {
      errors.value.maxSnippetLength = '最大代码片段长度必须在1000-1000000字符之间'
    }
    
    if (form.value.maxSnippetsPerUser < 10 || form.value.maxSnippetsPerUser > 10000) {
      errors.value.maxSnippetsPerUser = '用户最大片段数量必须在10-10000之间'
    }
    
    if (form.value.supportedLanguages.length === 0) {
      errors.value.supportedLanguages = '至少需要选择一种支持的编程语言'
    }
  }
  
  if (form.value.enableSharing) {
    if (form.value.shareTokenExpiryHours < 1 || form.value.shareTokenExpiryHours > 8760) {
      errors.value.shareTokenExpiryHours = '分享链接有效期必须在1-8760小时之间'
    }
    
    if (form.value.maxSharesPerSnippet < 1 || form.value.maxSharesPerSnippet > 1000) {
      errors.value.maxSharesPerSnippet = '单个片段最大分享次数必须在1-1000之间'
    }
  }
  
  if (form.value.enableClipboard) {
    if (form.value.maxClipboardItems < 10 || form.value.maxClipboardItems > 1000) {
      errors.value.maxClipboardItems = '最大剪贴板项目数必须在10-1000之间'
    }
    
    if (form.value.clipboardRetentionDays < 1 || form.value.clipboardRetentionDays > 365) {
      errors.value.clipboardRetentionDays = '剪贴板保留天数必须在1-365天之间'
    }
  }
  
  if (form.value.enableFileUpload) {
    if (form.value.maxFileSize < 1 || form.value.maxFileSize > 100) {
      errors.value.maxFileSize = '最大文件大小必须在1-100MB之间'
    }
    
    if (form.value.allowedFileTypes.length === 0) {
      errors.value.allowedFileTypes = '至少需要选择一种允许的文件类型'
    }
    
    if (form.value.maxFilesPerSnippet < 1 || form.value.maxFilesPerSnippet > 20) {
      errors.value.maxFilesPerSnippet = '单个片段最大文件数必须在1-20之间'
    }
  }
  
  if (form.value.enableApi) {
    if (form.value.apiRateLimit < 1 || form.value.apiRateLimit > 10000) {
      errors.value.apiRateLimit = 'API调用频率限制必须在1-10000次/分钟之间'
    }
  }
  
  return Object.keys(errors.value).length === 0
}

function saveSettings() {
  if (!validateForm()) return
  
  const request: UpdateFeatureSettingsRequest = {}
  
  // 只包含有变化的字段
  if (form.value.enableCodeSnippets !== props.settings?.enableCodeSnippets) {
    request.enableCodeSnippets = form.value.enableCodeSnippets
  }
  if (form.value.maxSnippetLength !== props.settings?.maxSnippetLength) {
    request.maxSnippetLength = form.value.maxSnippetLength
  }
  if (form.value.maxSnippetsPerUser !== props.settings?.maxSnippetsPerUser) {
    request.maxSnippetsPerUser = form.value.maxSnippetsPerUser
  }
  if (JSON.stringify(form.value.supportedLanguages) !== JSON.stringify(props.settings?.supportedLanguages)) {
    request.supportedLanguages = form.value.supportedLanguages
  }
  if (form.value.enableSharing !== props.settings?.enableSharing) {
    request.enableSharing = form.value.enableSharing
  }
  if (form.value.shareTokenExpiryHours !== props.settings?.shareTokenExpiryHours) {
    request.shareTokenExpiryHours = form.value.shareTokenExpiryHours
  }
  if (form.value.maxSharesPerSnippet !== props.settings?.maxSharesPerSnippet) {
    request.maxSharesPerSnippet = form.value.maxSharesPerSnippet
  }
  if (form.value.enableSharePassword !== props.settings?.enableSharePassword) {
    request.enableSharePassword = form.value.enableSharePassword
  }
  if (form.value.requireSharePassword !== props.settings?.requireSharePassword) {
    request.requireSharePassword = form.value.requireSharePassword
  }
  if (form.value.enableClipboard !== props.settings?.enableClipboard) {
    request.enableClipboard = form.value.enableClipboard
  }
  if (form.value.maxClipboardItems !== props.settings?.maxClipboardItems) {
    request.maxClipboardItems = form.value.maxClipboardItems
  }
  if (form.value.clipboardRetentionDays !== props.settings?.clipboardRetentionDays) {
    request.clipboardRetentionDays = form.value.clipboardRetentionDays
  }
  if (form.value.enableFileUpload !== props.settings?.enableFileUpload) {
    request.enableFileUpload = form.value.enableFileUpload
  }
  if (form.value.maxFileSize !== props.settings?.maxFileSize) {
    request.maxFileSize = form.value.maxFileSize
  }
  if (JSON.stringify(form.value.allowedFileTypes) !== JSON.stringify(props.settings?.allowedFileTypes)) {
    request.allowedFileTypes = form.value.allowedFileTypes
  }
  if (form.value.maxFilesPerSnippet !== props.settings?.maxFilesPerSnippet) {
    request.maxFilesPerSnippet = form.value.maxFilesPerSnippet
  }
  if (form.value.enableSearch !== props.settings?.enableSearch) {
    request.enableSearch = form.value.enableSearch
  }
  if (form.value.enableFullTextSearch !== props.settings?.enableFullTextSearch) {
    request.enableFullTextSearch = form.value.enableFullTextSearch
  }
  if (form.value.enableSearchIndexing !== props.settings?.enableSearchIndexing) {
    request.enableSearchIndexing = form.value.enableSearchIndexing
  }
  if (form.value.enableApi !== props.settings?.enableApi) {
    request.enableApi = form.value.enableApi
  }
  if (form.value.apiRateLimit !== props.settings?.apiRateLimit) {
    request.apiRateLimit = form.value.apiRateLimit
  }
  if (form.value.enableApiKeyAuth !== props.settings?.enableApiKeyAuth) {
    request.enableApiKeyAuth = form.value.enableApiKeyAuth
  }
  
  emit('update:settings', request)
}

function resetForm() {
  if (props.settings) {
    form.value = { ...props.settings }
    errors.value = {}
  }
}
</script>

<style scoped>
.feature-settings-view {
  max-width: 1000px;
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

.form-check {
  margin-bottom: 0.5rem;
}

.form-check-label {
  font-weight: 500;
  color: #495057;
}

.language-list,
.file-type-list {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 0.5rem;
  margin-top: 0.5rem;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
  padding-top: 2rem;
  border-top: 1px solid #e9ecef;
}

@media (max-width: 768px) {
  .language-list,
  .file-type-list {
    grid-template-columns: 1fr;
  }
  
  .form-actions {
    flex-direction: column;
  }
  
  .form-actions .btn {
    width: 100%;
  }
}
</style>