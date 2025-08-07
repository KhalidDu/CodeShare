<template>
  <div class="dialog-overlay" @click="handleOverlayClick">
    <div class="dialog" @click.stop>
      <div class="dialog-header">
        <h2>编辑用户</h2>
        <button class="close-btn" @click="$emit('close')">
          <i class="icon-x"></i>
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="dialog-content">
        <!-- 用户名 -->
        <div class="form-group">
          <label for="username" class="form-label">
            用户名 <span class="required">*</span>
          </label>
          <input
            id="username"
            v-model="form.username"
            type="text"
            class="form-input"
            :class="{ error: errors.username }"
            placeholder="请输入用户名"
            required
          />
          <div v-if="errors.username" class="error-message">
            {{ errors.username }}
          </div>
        </div>

        <!-- 邮箱 -->
        <div class="form-group">
          <label for="email" class="form-label">
            邮箱地址 <span class="required">*</span>
          </label>
          <input
            id="email"
            v-model="form.email"
            type="email"
            class="form-input"
            :class="{ error: errors.email }"
            placeholder="请输入邮箱地址"
            required
          />
          <div v-if="errors.email" class="error-message">
            {{ errors.email }}
          </div>
        </div>

        <!-- 角色选择 (仅管理员可修改) -->
        <div v-if="canEditRole" class="form-group">
          <label class="form-label">
            用户角色 <span class="required">*</span>
          </label>
          <div class="role-options">
            <label
              v-for="role in roleOptions"
              :key="role.value"
              class="role-option"
              :class="{ selected: form.role === role.value }"
            >
              <input
                v-model="form.role"
                type="radio"
                :value="role.value"
                class="role-radio"
              />
              <div class="role-content">
                <div class="role-header">
                  <i :class="role.icon"></i>
                  <span class="role-name">{{ role.label }}</span>
                </div>
                <div class="role-description">{{ role.description }}</div>
              </div>
            </label>
          </div>
        </div>

        <!-- 用户状态 (仅管理员可修改) -->
        <div v-if="canEditStatus" class="form-group">
          <label class="form-label">用户状态</label>
          <div class="status-toggle">
            <label class="toggle-option">
              <input
                v-model="form.isActive"
                type="checkbox"
                class="toggle-checkbox"
              />
              <div class="toggle-slider"></div>
              <span class="toggle-label">
                {{ form.isActive ? '启用' : '禁用' }}
              </span>
            </label>
            <div class="status-description">
              {{ form.isActive ? '用户可以正常登录和使用系统' : '用户将无法登录系统' }}
            </div>
          </div>
        </div>

        <!-- 用户信息 -->
        <div class="user-info">
          <div class="info-item">
            <span class="info-label">用户ID:</span>
            <span class="info-value">{{ user.id }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">创建时间:</span>
            <span class="info-value">{{ formatDate(user.createdAt) }}</span>
          </div>
        </div>

        <!-- 错误提示 -->
        <div v-if="submitError" class="submit-error">
          <i class="icon-alert-circle"></i>
          {{ submitError }}
        </div>

        <!-- 操作按钮 -->
        <div class="dialog-actions">
          <button
            type="button"
            class="btn btn-secondary"
            @click="$emit('close')"
            :disabled="loading"
          >
            取消
          </button>
          <button
            type="submit"
            class="btn btn-primary"
            :disabled="loading || !isFormValid || !hasChanges"
          >
            <span v-if="loading" class="loading-spinner"></span>
            {{ loading ? '保存中...' : '保存更改' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted } from 'vue'
import { useUsersStore } from '@/stores/users'
import { useAuthStore } from '@/stores/auth'
import { UserRole, type User } from '@/types'
import type { UpdateUserRequest } from '@/services/userService'

// Props
interface Props {
  user: User
}

const props = defineProps<Props>()

// Emits
const emit = defineEmits<{
  close: []
  updated: [user: User]
}>()

// 状态管理
const usersStore = useUsersStore()
const authStore = useAuthStore()

// 响应式数据
const loading = ref(false)
const submitError = ref('')

// 表单数据
const form = reactive({
  username: '',
  email: '',
  role: UserRole.Viewer,
  isActive: true
})

// 原始数据 (用于检测变更)
const originalData = reactive({
  username: '',
  email: '',
  role: UserRole.Viewer,
  isActive: true
})

// 表单验证错误
const errors = reactive({
  username: '',
  email: ''
})

// 角色选项
const roleOptions = [
  {
    value: UserRole.Viewer,
    label: '查看者',
    icon: 'icon-eye',
    description: '只能查看和复制公开的代码片段'
  },
  {
    value: UserRole.Editor,
    label: '编辑者',
    icon: 'icon-edit',
    description: '可以创建、编辑和管理自己的代码片段'
  },
  {
    value: UserRole.Admin,
    label: '管理员',
    icon: 'icon-shield',
    description: '拥有完整的系统访问权限，包括用户管理'
  }
]

// 计算属性
/**
 * 是否可以编辑角色 (仅管理员)
 */
const canEditRole = computed(() => {
  return authStore.user?.role === UserRole.Admin
})

/**
 * 是否可以编辑状态 (仅管理员，且不能禁用自己)
 */
const canEditStatus = computed(() => {
  return authStore.user?.role === UserRole.Admin && authStore.user?.id !== props.user.id
})

/**
 * 表单是否有效
 */
const isFormValid = computed(() => {
  return form.username.trim() &&
         form.email.trim() &&
         !Object.values(errors).some(error => error)
})

/**
 * 是否有变更
 */
const hasChanges = computed(() => {
  return form.username !== originalData.username ||
         form.email !== originalData.email ||
         form.role !== originalData.role ||
         form.isActive !== originalData.isActive
})

// 方法
/**
 * 初始化表单数据
 */
function initializeForm() {
  form.username = props.user.username
  form.email = props.user.email
  form.role = props.user.role
  form.isActive = props.user.isActive

  // 保存原始数据
  originalData.username = props.user.username
  originalData.email = props.user.email
  originalData.role = props.user.role
  originalData.isActive = props.user.isActive
}

/**
 * 验证表单
 */
function validateForm(): boolean {
  // 清除之前的错误
  Object.keys(errors).forEach(key => {
    errors[key as keyof typeof errors] = ''
  })

  let isValid = true

  // 验证用户名
  if (!form.username.trim()) {
    errors.username = '用户名不能为空'
    isValid = false
  } else if (form.username.length < 3) {
    errors.username = '用户名至少3个字符'
    isValid = false
  } else if (!/^[a-zA-Z0-9_-]+$/.test(form.username)) {
    errors.username = '用户名只能包含字母、数字、下划线和连字符'
    isValid = false
  }

  // 验证邮箱
  if (!form.email.trim()) {
    errors.email = '邮箱地址不能为空'
    isValid = false
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
    errors.email = '请输入有效的邮箱地址'
    isValid = false
  }

  return isValid
}

/**
 * 处理表单提交
 */
async function handleSubmit() {
  if (!validateForm()) {
    return
  }

  loading.value = true
  submitError.value = ''

  try {
    const updateData: UpdateUserRequest = {}

    // 只包含有变更的字段
    if (form.username !== originalData.username) {
      updateData.username = form.username.trim()
    }
    if (form.email !== originalData.email) {
      updateData.email = form.email.trim()
    }
    if (canEditRole.value && form.role !== originalData.role) {
      updateData.role = form.role
    }
    if (canEditStatus.value && form.isActive !== originalData.isActive) {
      updateData.isActive = form.isActive
    }

    const updatedUser = await usersStore.updateUser(props.user.id, updateData)
    if (updatedUser) {
      // 发出更新成功事件
      emit('updated', updatedUser)
    } else {
      submitError.value = usersStore.error || '更新用户失败'
    }
  } catch (error: any) {
    submitError.value = error.response?.data?.message || '更新用户时发生错误'
  } finally {
    loading.value = false
  }
}

/**
 * 处理遮罩层点击
 */
function handleOverlayClick() {
  if (!loading.value) {
    emit('close')
  }
}

/**
 * 格式化日期
 */
function formatDate(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}



// 生命周期
onMounted(() => {
  initializeForm()
})
</script>

<style scoped>
.dialog-overlay {
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

.dialog {
  background: white;
  border-radius: 12px;
  width: 100%;
  max-width: 500px;
  max-height: 90vh;
  overflow: hidden;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
}

.dialog-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid #e5e7eb;
}

.dialog-header h2 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #1a1a1a;
}

.close-btn {
  width: 32px;
  height: 32px;
  border: none;
  background: none;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #666;
  transition: all 0.2s;
}

.close-btn:hover {
  background: #f3f4f6;
  color: #1a1a1a;
}

.dialog-content {
  padding: 24px;
  max-height: calc(90vh - 80px);
  overflow-y: auto;
}

/* 表单样式 */
.form-group {
  margin-bottom: 20px;
}

.form-label {
  display: block;
  margin-bottom: 6px;
  font-size: 14px;
  font-weight: 500;
  color: #374151;
}

.required {
  color: #dc2626;
}

.form-input {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.2s;
  box-sizing: border-box;
}

.form-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-input.error {
  border-color: #dc2626;
}

.error-message {
  margin-top: 4px;
  font-size: 12px;
  color: #dc2626;
}

/* 角色选择 */
.role-options {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.role-option {
  border: 1px solid #d1d5db;
  border-radius: 8px;
  padding: 16px;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: flex-start;
  gap: 12px;
}

.role-option:hover {
  border-color: #3b82f6;
  background: #f8fafc;
}

.role-option.selected {
  border-color: #3b82f6;
  background: #eff6ff;
}

.role-radio {
  margin-top: 2px;
}

.role-content {
  flex: 1;
}

.role-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 4px;
}

.role-name {
  font-weight: 500;
  color: #1a1a1a;
}

.role-description {
  font-size: 13px;
  color: #666;
  line-height: 1.4;
}

/* 状态切换 */
.status-toggle {
  border: 1px solid #d1d5db;
  border-radius: 8px;
  padding: 16px;
}

.toggle-option {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  margin-bottom: 8px;
}

.toggle-checkbox {
  display: none;
}

.toggle-slider {
  width: 44px;
  height: 24px;
  background: #d1d5db;
  border-radius: 12px;
  position: relative;
  transition: background 0.2s;
}

.toggle-slider::after {
  content: '';
  position: absolute;
  top: 2px;
  left: 2px;
  width: 20px;
  height: 20px;
  background: white;
  border-radius: 50%;
  transition: transform 0.2s;
}

.toggle-checkbox:checked + .toggle-slider {
  background: #3b82f6;
}

.toggle-checkbox:checked + .toggle-slider::after {
  transform: translateX(20px);
}

.toggle-label {
  font-weight: 500;
  color: #1a1a1a;
}

.status-description {
  font-size: 13px;
  color: #666;
  line-height: 1.4;
}

/* 用户信息 */
.user-info {
  background: #f9fafb;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 20px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
  font-size: 14px;
}

.info-item:last-child {
  margin-bottom: 0;
}

.info-label {
  color: #666;
  font-weight: 500;
}

.info-value {
  color: #1a1a1a;
  font-family: 'Courier New', monospace;
  font-size: 12px;
}

/* 提交错误 */
.submit-error {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
  color: #dc2626;
  font-size: 14px;
  margin-bottom: 20px;
}

/* 操作按钮 */
.dialog-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  padding-top: 20px;
  border-top: 1px solid #e5e7eb;
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

.loading-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top: 2px solid white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* 响应式设计 */
@media (max-width: 480px) {
  .dialog {
    margin: 10px;
    max-width: none;
  }

  .dialog-content {
    padding: 20px;
  }

  .dialog-actions {
    flex-direction: column-reverse;
  }

  .btn {
    width: 100%;
    justify-content: center;
  }
}
</style>