<template>
  <div class="create-share-dialog-overlay" @click="$emit('close')">
    <div class="create-share-dialog" @click.stop>
      <div class="dialog-header">
        <h2 class="dialog-title">创建分享链接</h2>
        <button @click="$emit('close')" class="close-btn">
          <i class="icon-times"></i>
        </button>
      </div>

      <div class="dialog-body">
        <!-- 选择代码片段 -->
        <div class="form-group">
          <label class="form-label">选择代码片段</label>
          <select 
            v-model="selectedSnippetId" 
            class="form-select"
            :class="{ 'error': errors.snippetId }"
          >
            <option value="">请选择要分享的代码片段</option>
            <option 
              v-for="snippet in snippets" 
              :key="snippet.id" 
              :value="snippet.id"
            >
              {{ snippet.title || '无标题' }} ({{ snippet.language }})
            </option>
          </select>
          <div v-if="errors.snippetId" class="error-message">
            {{ errors.snippetId }}
          </div>
        </div>

        <!-- 权限设置 -->
        <div class="form-group">
          <label class="form-label">访问权限</label>
          <div class="permission-options">
            <label class="permission-option">
              <input
                type="radio"
                v-model="permission"
                value="VIEW"
                class="permission-radio"
              />
              <div class="permission-content">
                <div class="permission-title">仅查看</div>
                <div class="permission-description">用户只能查看代码，无法编辑</div>
              </div>
            </label>
            <label class="permission-option">
              <input
                type="radio"
                v-model="permission"
                value="EDIT"
                class="permission-radio"
              />
              <div class="permission-content">
                <div class="permission-title">可编辑</div>
                <div class="permission-description">用户可以查看和编辑代码</div>
              </div>
            </label>
          </div>
        </div>

        <!-- 访问限制 -->
        <div class="form-group">
          <label class="form-label">访问限制</label>
          <div class="access-limits">
            <!-- 过期时间 -->
            <div class="limit-group">
              <label class="limit-label">过期时间</label>
              <select 
                v-model="expiresAt" 
                class="limit-select"
                @change="handleExpiresAtChange"
              >
                <option value="">永不过期</option>
                <option value="1">1小时后</option>
                <option value="24">1天后</option>
                <option value="168">1周后</option>
                <option value="720">1月后</option>
                <option value="custom">自定义</option>
              </select>
            </div>

            <!-- 最大访问次数 -->
            <div class="limit-group">
              <label class="limit-label">最大访问次数</label>
              <input
                v-model.number="maxAccessCount"
                type="number"
                min="1"
                placeholder="无限制"
                class="limit-input"
              />
            </div>
          </div>
        </div>

        <!-- 自定义过期时间 -->
        <div v-if="showCustomExpiresAt" class="form-group">
          <label class="form-label">自定义过期时间</label>
          <input
            v-model="customExpiresAt"
            type="datetime-local"
            class="form-input"
            :min="minExpiresAt"
          />
        </div>

        <!-- 密码保护 -->
        <div class="form-group">
          <label class="form-label">密码保护</label>
          <div class="password-protection">
            <label class="switch">
              <input
                type="checkbox"
                v-model="enablePassword"
                class="switch-input"
              />
              <span class="switch-slider"></span>
            </label>
            <span class="switch-label">启用密码保护</span>
          </div>
          <div v-if="enablePassword" class="password-input-group">
            <input
              v-model="password"
              type="password"
              placeholder="请输入访问密码"
              class="form-input"
              :class="{ 'error': errors.password }"
            />
            <div v-if="errors.password" class="error-message">
              {{ errors.password }}
            </div>
          </div>
        </div>

        <!-- 分享描述 -->
        <div class="form-group">
          <label class="form-label">分享描述</label>
          <textarea
            v-model="description"
            placeholder="添加分享描述（可选）"
            class="form-textarea"
            rows="3"
          ></textarea>
        </div>
      </div>

      <div class="dialog-footer">
        <button 
          @click="$emit('close')" 
          class="btn btn-secondary"
          :disabled="isCreating"
        >
          取消
        </button>
        <button 
          @click="handleCreate" 
          class="btn btn-primary"
          :disabled="isCreating || !isValid"
        >
          <span v-if="isCreating" class="spinner"></span>
          {{ isCreating ? '创建中...' : '创建分享链接' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useCodeSnippetsStore } from '@/stores/codeSnippets'
import { useShareStore } from '@/stores/share'
import { useToastStore } from '@/stores/toast'
import { SharePermission } from '@/types/share'

interface Props {
  visible?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  visible: true
})

const emit = defineEmits<{
  close: []
  created: [shareToken: any]
}>()

// Store注入
const snippetsStore = useCodeSnippetsStore()
const shareStore = useShareStore()
const toastStore = useToastStore()

// 响应式数据
const selectedSnippetId = ref('')
const permission = ref<SharePermission>(SharePermission.VIEW)
const expiresAt = ref('')
const maxAccessCount = ref<number | null>(null)
const enablePassword = ref(false)
const password = ref('')
const description = ref('')
const showCustomExpiresAt = ref(false)
const customExpiresAt = ref('')
const isCreating = ref(false)

// 错误信息
const errors = ref({
  snippetId: '',
  password: ''
})

// 代码片段列表
const snippets = ref([])

// 计算属性
const isValid = computed(() => {
  return selectedSnippetId.value && 
         (!enablePassword.value || password.value.length >= 6)
})

const minExpiresAt = computed(() => {
  const now = new Date()
  now.setHours(now.getHours() + 1)
  return now.toISOString().slice(0, 16)
})

// 生命周期
onMounted(async () => {
  await loadSnippets()
})

// 监听器
watch(enablePassword, (newValue) => {
  if (!newValue) {
    password.value = ''
    errors.value.password = ''
  }
})

// 方法
async function loadSnippets() {
  try {
    await snippetsStore.fetchSnippets()
    snippets.value = snippetsStore.snippets
  } catch (error) {
    toastStore.error('加载代码片段失败')
  }
}

function handleExpiresAtChange() {
  showCustomExpiresAt.value = expiresAt.value === 'custom'
}

function validateForm(): boolean {
  errors.value = {
    snippetId: '',
    password: ''
  }

  if (!selectedSnippetId.value) {
    errors.value.snippetId = '请选择要分享的代码片段'
    return false
  }

  if (enablePassword.value && password.value.length < 6) {
    errors.value.password = '密码长度至少6位'
    return false
  }

  return true
}

async function handleCreate() {
  if (!validateForm()) return

  isCreating.value = true
  try {
    let expiresAtValue: string | null = null

    if (expiresAt.value && expiresAt.value !== 'custom') {
      const hours = parseInt(expiresAt.value)
      const expiresAtDate = new Date()
      expiresAtDate.setHours(expiresAtDate.getHours() + hours)
      expiresAtValue = expiresAtDate.toISOString()
    } else if (showCustomExpiresAt.value && customExpiresAt.value) {
      expiresAtValue = new Date(customExpiresAt.value).toISOString()
    }

    const shareToken = await shareStore.createShareToken({
      snippetId: selectedSnippetId.value,
      permission: permission.value,
      expiresAt: expiresAtValue,
      maxAccessCount: maxAccessCount.value || undefined,
      password: enablePassword.value ? password.value : undefined,
      description: description.value || undefined
    })

    emit('created', shareToken)
    toastStore.success('分享链接创建成功')
  } catch (error) {
    toastStore.error('创建分享链接失败')
  } finally {
    isCreating.value = false
  }
}
</script>

<style scoped>
.create-share-dialog-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
}

.create-share-dialog {
  background: white;
  border-radius: 12px;
  max-width: 600px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
}

.dialog-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #e5e7eb;
}

.dialog-title {
  font-size: 20px;
  font-weight: 600;
  color: #1f2937;
  margin: 0;
}

.close-btn {
  background: none;
  border: none;
  color: #6b7280;
  cursor: pointer;
  padding: 8px;
  border-radius: 6px;
  transition: all 0.2s;
}

.close-btn:hover {
  background: #f3f4f6;
  color: #374151;
}

.dialog-body {
  padding: 20px;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 20px;
  border-top: 1px solid #e5e7eb;
}

.form-group {
  margin-bottom: 20px;
}

.form-label {
  display: block;
  font-weight: 500;
  color: #374151;
  margin-bottom: 8px;
}

.form-select,
.form-input,
.form-textarea {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  transition: border-color 0.2s;
}

.form-select:focus,
.form-input:focus,
.form-textarea:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-select.error,
.form-input.error,
.form-textarea.error {
  border-color: #ef4444;
}

.error-message {
  color: #ef4444;
  font-size: 12px;
  margin-top: 4px;
}

.permission-options {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.permission-option {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 12px;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.permission-option:hover {
  border-color: #3b82f6;
  background: #f0f9ff;
}

.permission-option:has(.permission-radio:checked) {
  border-color: #3b82f6;
  background: #eff6ff;
}

.permission-radio {
  margin-top: 2px;
}

.permission-content {
  flex: 1;
}

.permission-title {
  font-weight: 500;
  color: #1f2937;
  margin-bottom: 2px;
}

.permission-description {
  font-size: 12px;
  color: #6b7280;
}

.access-limits {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.limit-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.limit-label {
  font-weight: 500;
  color: #374151;
  font-size: 14px;
}

.limit-select,
.limit-input {
  padding: 8px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
}

.password-protection {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.switch {
  position: relative;
  display: inline-block;
  width: 48px;
  height: 24px;
}

.switch-input {
  opacity: 0;
  width: 0;
  height: 0;
}

.switch-slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #d1d5db;
  transition: 0.4s;
  border-radius: 24px;
}

.switch-slider:before {
  position: absolute;
  content: "";
  height: 16px;
  width: 16px;
  left: 4px;
  bottom: 4px;
  background-color: white;
  transition: 0.4s;
  border-radius: 50%;
}

.switch-input:checked + .switch-slider {
  background-color: #3b82f6;
}

.switch-input:checked + .switch-slider:before {
  transform: translateX(24px);
}

.switch-label {
  font-weight: 500;
  color: #374151;
}

.password-input-group {
  margin-top: 12px;
}

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-secondary {
  background: #f3f4f6;
  color: #374151;
  border: 1px solid #d1d5db;
}

.btn-secondary:hover:not(:disabled) {
  background: #e5e7eb;
}

.btn-primary {
  background: #3b82f6;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #2563eb;
}

.spinner {
  width: 16px;
  height: 16px;
  border: 2px solid #ffffff;
  border-top: 2px solid transparent;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* 响应式设计 */
@media (max-width: 640px) {
  .create-share-dialog-overlay {
    padding: 10px;
  }

  .create-share-dialog {
    max-width: 100%;
  }

  .access-limits {
    grid-template-columns: 1fr;
  }

  .dialog-footer {
    flex-direction: column;
  }

  .btn {
    width: 100%;
    justify-content: center;
  }
}
</style>