<template>
  <div class="dialog-overlay" @click="handleOverlayClick">
    <div class="dialog" @click.stop>
      <div class="dialog-header">
        <h2>创建新用户</h2>
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

        <!-- 密码 -->
        <div class="form-group">
          <label for="password" class="form-label">
            密码 <span class="required">*</span>
          </label>
          <div class="password-input">
            <input
              id="password"
              v-model="form.password"
              :type="showPassword ? 'text' : 'password'"
              class="form-input"
              :class="{ error: errors.password }"
              placeholder="请输入密码"
              required
            />
            <button
              type="button"
              class="password-toggle"
              @click="showPassword = !showPassword"
            >
              <i :class="showPassword ? 'icon-eye-off' : 'icon-eye'"></i>
            </button>
          </div>
          <div v-if="errors.password" class="error-message">
            {{ errors.password }}
          </div>
          <div class="password-hint">
            密码至少8位，包含字母和数字
          </div>
        </div>

        <!-- 确认密码 -->
        <div class="form-group">
          <label for="confirmPassword" class="form-label">
            确认密码 <span class="required">*</span>
          </label>
          <input
            id="confirmPassword"
            v-model="form.confirmPassword"
            type="password"
            class="form-input"
            :class="{ error: errors.confirmPassword }"
            placeholder="请再次输入密码"
            required
          />
          <div v-if="errors.confirmPassword" class="error-message">
            {{ errors.confirmPassword }}
          </div>
        </div>

        <!-- 角色选择 -->
        <div class="form-group">
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
            :disabled="loading || !isFormValid"
          >
            <span v-if="loading" class="loading-spinner"></span>
            {{ loading ? '创建中...' : '创建用户' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive } from 'vue'
import { useUsersStore } from '@/stores/users'
import { UserRole } from '@/types'
import type { CreateUserRequest } from '@/services/userService'

// Emits
const emit = defineEmits<{
  close: []
  created: [user: any]
}>()

// 状态管理
const usersStore = useUsersStore()

// 响应式数据
const loading = ref(false)
const showPassword = ref(false)
const submitError = ref('')

// 表单数据
const form = reactive({
  username: '',
  email: '',
  password: '',
  confirmPassword: '',
  role: UserRole.Viewer
})

// 表单验证错误
const errors = reactive({
  username: '',
  email: '',
  password: '',
  confirmPassword: ''
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
 * 表单是否有效
 */
const isFormValid = computed(() => {
  return form.username.trim() &&
         form.email.trim() &&
         form.password.trim() &&
         form.confirmPassword.trim() &&
         !Object.values(errors).some(error => error)
})

// 方法
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

  // 验证密码
  if (!form.password.trim()) {
    errors.password = '密码不能为空'
    isValid = false
  } else if (form.password.length < 8) {
    errors.password = '密码至少8个字符'
    isValid = false
  } else if (!/(?=.*[a-zA-Z])(?=.*\d)/.test(form.password)) {
    errors.password = '密码必须包含字母和数字'
    isValid = false
  }

  // 验证确认密码
  if (!form.confirmPassword.trim()) {
    errors.confirmPassword = '请确认密码'
    isValid = false
  } else if (form.password !== form.confirmPassword) {
    errors.confirmPassword = '两次输入的密码不一致'
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
    const userData: CreateUserRequest = {
      username: form.username.trim(),
      email: form.email.trim(),
      password: form.password,
      role: form.role
    }

    const user = await usersStore.createUser(userData)
    if (user) {
      // 发出创建成功事件
      emit('created', user)
    } else {
      submitError.value = usersStore.error || '创建用户失败'
    }
  } catch (error: any) {
    submitError.value = error.response?.data?.message || '创建用户时发生错误'
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

.password-input {
  position: relative;
}

.password-toggle {
  position: absolute;
  right: 10px;
  top: 50%;
  transform: translateY(-50%);
  border: none;
  background: none;
  color: #666;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  transition: color 0.2s;
}

.password-toggle:hover {
  color: #1a1a1a;
}

.password-hint {
  margin-top: 4px;
  font-size: 12px;
  color: #666;
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