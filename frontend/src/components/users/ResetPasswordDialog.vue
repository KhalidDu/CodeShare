<template>
  <div class="dialog-overlay" @click="handleOverlayClick">
    <div class="dialog" @click.stop>
      <div class="dialog-header">
        <h2>重置密码</h2>
        <button class="close-btn" @click="$emit('close')">
          <i class="icon-x"></i>
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="dialog-content">
        <!-- 用户信息 -->
        <div class="user-info">
          <div class="user-avatar">
            <i class="icon-user"></i>
          </div>
          <div class="user-details">
            <h3>{{ user.username }}</h3>
            <p>{{ user.email }}</p>
          </div>
        </div>

        <div class="warning-message">
          <i class="icon-alert-triangle"></i>
          <div>
            <p><strong>注意：</strong>重置密码后，用户需要使用新密码重新登录。</p>
            <p>请确保将新密码安全地传达给用户。</p>
          </div>
        </div>

        <!-- 新密码 -->
        <div class="form-group">
          <label for="newPassword" class="form-label">
            新密码 <span class="required">*</span>
          </label>
          <div class="password-input">
            <input
              id="newPassword"
              v-model="form.newPassword"
              :type="showPassword ? 'text' : 'password'"
              class="form-input"
              :class="{ error: errors.newPassword }"
              placeholder="请输入新密码"
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
          <div v-if="errors.newPassword" class="error-message">
            {{ errors.newPassword }}
          </div>
          <div class="password-hint">
            密码至少8位，包含字母和数字
          </div>
        </div>

        <!-- 确认密码 -->
        <div class="form-group">
          <label for="confirmPassword" class="form-label">
            确认新密码 <span class="required">*</span>
          </label>
          <input
            id="confirmPassword"
            v-model="form.confirmPassword"
            type="password"
            class="form-input"
            :class="{ error: errors.confirmPassword }"
            placeholder="请再次输入新密码"
            required
          />
          <div v-if="errors.confirmPassword" class="error-message">
            {{ errors.confirmPassword }}
          </div>
        </div>

        <!-- 密码强度指示器 -->
        <div v-if="form.newPassword" class="password-strength">
          <div class="strength-label">密码强度:</div>
          <div class="strength-bar">
            <div 
              class="strength-fill" 
              :class="passwordStrength.class"
              :style="{ width: passwordStrength.width }"
            ></div>
          </div>
          <div class="strength-text" :class="passwordStrength.class">
            {{ passwordStrength.text }}
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
            class="btn btn-danger"
            :disabled="loading || !isFormValid"
          >
            <span v-if="loading" class="loading-spinner"></span>
            {{ loading ? '重置中...' : '重置密码' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive } from 'vue'
import { useUsersStore } from '@/stores/users'
import type { User } from '@/types'

// Props
interface Props {
  user: User
}

const props = defineProps<Props>()

// Emits
const emit = defineEmits<{
  close: []
  reset: []
}>()

// 状态管理
const usersStore = useUsersStore()

// 响应式数据
const loading = ref(false)
const showPassword = ref(false)
const submitError = ref('')

// 表单数据
const form = reactive({
  newPassword: '',
  confirmPassword: ''
})

// 表单验证错误
const errors = reactive({
  newPassword: '',
  confirmPassword: ''
})

// 计算属性
/**
 * 表单是否有效
 */
const isFormValid = computed(() => {
  return form.newPassword.trim() &&
         form.confirmPassword.trim() &&
         !Object.values(errors).some(error => error)
})

/**
 * 密码强度计算
 */
const passwordStrength = computed(() => {
  const password = form.newPassword
  if (!password) {
    return { width: '0%', class: '', text: '' }
  }

  let score = 0
  const feedback = []

  // 长度检查
  if (password.length >= 8) score += 1
  else feedback.push('至少8位')

  // 包含小写字母
  if (/[a-z]/.test(password)) score += 1
  else feedback.push('包含小写字母')

  // 包含大写字母
  if (/[A-Z]/.test(password)) score += 1
  else feedback.push('包含大写字母')

  // 包含数字
  if (/\d/.test(password)) score += 1
  else feedback.push('包含数字')

  // 包含特殊字符
  if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) score += 1
  else feedback.push('包含特殊字符')

  // 根据得分返回强度
  if (score <= 2) {
    return {
      width: '25%',
      class: 'weak',
      text: '弱'
    }
  } else if (score === 3) {
    return {
      width: '50%',
      class: 'medium',
      text: '中等'
    }
  } else if (score === 4) {
    return {
      width: '75%',
      class: 'strong',
      text: '强'
    }
  } else {
    return {
      width: '100%',
      class: 'very-strong',
      text: '很强'
    }
  }
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

  // 验证新密码
  if (!form.newPassword.trim()) {
    errors.newPassword = '新密码不能为空'
    isValid = false
  } else if (form.newPassword.length < 8) {
    errors.newPassword = '密码至少8个字符'
    isValid = false
  } else if (!/(?=.*[a-zA-Z])(?=.*\d)/.test(form.newPassword)) {
    errors.newPassword = '密码必须包含字母和数字'
    isValid = false
  }

  // 验证确认密码
  if (!form.confirmPassword.trim()) {
    errors.confirmPassword = '请确认新密码'
    isValid = false
  } else if (form.newPassword !== form.confirmPassword) {
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
    const success = await usersStore.resetPassword(props.user.id, form.newPassword)
    if (success) {
      // 发出重置成功事件
      emit('reset')
    } else {
      submitError.value = usersStore.error || '重置密码失败'
    }
  } catch (error: any) {
    submitError.value = error.response?.data?.message || '重置密码时发生错误'
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
  max-width: 480px;
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

/* 用户信息 */
.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  background: #f9fafb;
  border-radius: 8px;
  margin-bottom: 20px;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: linear-gradient(135deg, #3b82f6, #1d4ed8);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 16px;
}

.user-details h3 {
  margin: 0 0 4px 0;
  font-size: 16px;
  font-weight: 600;
  color: #1a1a1a;
}

.user-details p {
  margin: 0;
  font-size: 14px;
  color: #666;
}

/* 警告消息 */
.warning-message {
  display: flex;
  gap: 12px;
  padding: 16px;
  background: #fef3c7;
  border: 1px solid #fbbf24;
  border-radius: 8px;
  margin-bottom: 20px;
}

.warning-message i {
  color: #d97706;
  font-size: 20px;
  flex-shrink: 0;
  margin-top: 2px;
}

.warning-message p {
  margin: 0 0 8px 0;
  font-size: 14px;
  color: #92400e;
  line-height: 1.4;
}

.warning-message p:last-child {
  margin-bottom: 0;
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

/* 密码强度 */
.password-strength {
  margin-bottom: 20px;
  padding: 12px;
  background: #f9fafb;
  border-radius: 8px;
}

.strength-label {
  font-size: 12px;
  color: #666;
  margin-bottom: 6px;
}

.strength-bar {
  height: 4px;
  background: #e5e7eb;
  border-radius: 2px;
  overflow: hidden;
  margin-bottom: 6px;
}

.strength-fill {
  height: 100%;
  transition: width 0.3s ease;
}

.strength-fill.weak {
  background: #dc2626;
}

.strength-fill.medium {
  background: #f59e0b;
}

.strength-fill.strong {
  background: #10b981;
}

.strength-fill.very-strong {
  background: #059669;
}

.strength-text {
  font-size: 12px;
  font-weight: 500;
}

.strength-text.weak {
  color: #dc2626;
}

.strength-text.medium {
  color: #f59e0b;
}

.strength-text.strong {
  color: #10b981;
}

.strength-text.very-strong {
  color: #059669;
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

.btn-danger {
  background: #dc2626;
  color: white;
}

.btn-danger:hover:not(:disabled) {
  background: #b91c1c;
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