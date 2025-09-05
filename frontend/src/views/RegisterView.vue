<template>
  <AuthForm
    title="创建账户"
    subtitle="注册新账户以开始使用代码片段管理工具"
    submit-text="注册"
    loading-text="注册中..."
    :is-loading="isLoading"
    :is-form-valid="isFormValid"
    :error="error"
    :success="success"
    @submit="handleRegister"
  >
    <template #fields>
      <FormField
        id="username"
        label="用户名"
        type="text"
        v-model="form.username"
        placeholder="请输入用户名"
        icon="M12,4A4,4 0 0,1 16,8A4,4 0 0,1 12,12A4,4 0 0,1 8,8A4,4 0 0,1 12,4M12,14C16.42,14 20,15.79 20,18V20H4V18C4,15.79 7.58,14 12,14Z"
        autocomplete="username"
        required
        :disabled="isLoading"
        :error-message="fieldErrors.username"
        help-text="用户名需要3-20个字符，只能包含字母、数字和下划线"
      />

      <FormField
        id="email"
        label="邮箱地址"
        type="email"
        v-model="form.email"
        placeholder="请输入邮箱地址"
        icon="M20,8L12,13L4,8V6L12,11L20,6M20,4H4C2.89,4 2,4.89 2,6V18A2,2 0 0,0 4,20H20A2,2 0 0,0 22,18V6C22,5.11 21.1,4 20,4Z"
        autocomplete="email"
        required
        :disabled="isLoading"
        :error-message="fieldErrors.email"
        help-text="请输入有效的邮箱地址"
      />

      <FormField
        id="password"
        label="密码"
        type="password"
        v-model="form.password"
        placeholder="请输入密码"
        icon="M12,17A2,2 0 0,0 14,15C14,13.89 13.1,13 12,13A2,2 0 0,0 10,15A2,2 0 0,0 12,17M18,8A2,2 0 0,1 20,10V20A2,2 0 0,1 18,22H6A2,2 0 0,1 4,20V10C4,8.89 4.9,8 6,8H7V6A5,5 0 0,1 12,1A5,5 0 0,1 17,6V8H18M12,3A3,3 0 0,0 9,6V8H15V6A3,3 0 0,0 12,3Z"
        autocomplete="new-password"
        required
        :disabled="isLoading"
        :error-message="fieldErrors.password"
        help-text="密码至少需要6个字符，建议包含字母、数字和特殊字符"
      />

      <FormField
        id="confirmPassword"
        label="确认密码"
        type="password"
        v-model="form.confirmPassword"
        placeholder="请再次输入密码"
        icon="M12,17A2,2 0 0,0 14,15C14,13.89 13.1,13 12,13A2,2 0 0,0 10,15A2,2 0 0,0 12,17M18,8A2,2 0 0,1 20,10V20A2,2 0 0,1 18,22H6A2,2 0 0,1 4,20V10C4,8.89 4.9,8 6,8H7V6A5,5 0 0,1 12,1A5,5 0 0,1 17,6V8H18M12,3A3,3 0 0,0 9,6V8H15V6A3,3 0 0,0 12,3Z"
        autocomplete="new-password"
        required
        :disabled="isLoading"
        :error-message="fieldErrors.confirmPassword"
      />

      <!-- 服务条款同意 -->
      <div class="terms-agreement">
        <label class="checkbox-label">
          <input
            type="checkbox"
            v-model="form.agreeToTerms"
            :disabled="isLoading"
            class="checkbox-input"
            required
          />
          <span class="checkbox-custom"></span>
          <span class="checkbox-text">
            我已阅读并同意
            <a href="#" @click.prevent="showTerms" class="terms-link">服务条款</a>
            和
            <a href="#" @click.prevent="showPrivacy" class="terms-link">隐私政策</a>
          </span>
        </label>
      </div>
    </template>

    <template #footer>
      <p class="footer-text">
        已有账户？
        <router-link to="/login" class="footer-link">立即登录</router-link>
      </p>
    </template>
  </AuthForm>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import AuthForm from '@/components/auth/AuthForm.vue'
import FormField from '@/components/auth/FormField.vue'

const router = useRouter()
const authStore = useAuthStore()

// 表单数据
const form = ref({
  username: '',
  email: '',
  password: '',
  confirmPassword: '',
  agreeToTerms: false
})

// 状态管理
const error = ref('')
const success = ref('')
const isLoading = ref(false)
const fieldErrors = ref<Record<string, string>>({})

// 计算属性
const isFormValid = computed(() => {
  return form.value.username.trim() !== '' &&
         form.value.email.trim() !== '' &&
         form.value.password.trim() !== '' &&
         form.value.confirmPassword.trim() !== '' &&
         form.value.agreeToTerms &&
         Object.keys(fieldErrors.value).length === 0
})

/**
 * 验证邮箱格式
 */
function isValidEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  return emailRegex.test(email)
}

/**
 * 验证用户名格式
 */
function isValidUsername(username: string): boolean {
  const usernameRegex = /^[a-zA-Z0-9_]{3,20}$/
  return usernameRegex.test(username)
}

/**
 * 验证密码强度
 */
function validatePasswordStrength(password: string): string | null {
  if (password.length < 6) {
    return '密码至少需要6个字符'
  }
  if (password.length > 128) {
    return '密码不能超过128个字符'
  }
  return null
}

/**
 * 验证表单字段
 */
function validateForm() {
  const errors: Record<string, string> = {}

  // 验证用户名
  if (!form.value.username.trim()) {
    errors.username = '请输入用户名'
  } else if (!isValidUsername(form.value.username)) {
    errors.username = '用户名只能包含字母、数字和下划线，长度3-20个字符'
  }

  // 验证邮箱
  if (!form.value.email.trim()) {
    errors.email = '请输入邮箱地址'
  } else if (!isValidEmail(form.value.email)) {
    errors.email = '请输入有效的邮箱地址'
  }

  // 验证密码
  if (!form.value.password.trim()) {
    errors.password = '请输入密码'
  } else {
    const passwordError = validatePasswordStrength(form.value.password)
    if (passwordError) {
      errors.password = passwordError
    }
  }

  // 验证确认密码
  if (!form.value.confirmPassword.trim()) {
    errors.confirmPassword = '请确认密码'
  } else if (form.value.password !== form.value.confirmPassword) {
    errors.confirmPassword = '两次输入的密码不一致'
  }

  fieldErrors.value = errors
  return Object.keys(errors).length === 0
}

/**
 * 实时验证确认密码
 */
watch([() => form.value.password, () => form.value.confirmPassword], () => {
  if (form.value.confirmPassword && form.value.password !== form.value.confirmPassword) {
    fieldErrors.value.confirmPassword = '两次输入的密码不一致'
  } else if (fieldErrors.value.confirmPassword === '两次输入的密码不一致') {
    delete fieldErrors.value.confirmPassword
  }
})

/**
 * 处理注册
 */
async function handleRegister() {
  if (isLoading.value) return

  // 清除之前的错误和成功消息
  error.value = ''
  success.value = ''
  fieldErrors.value = {}

  // 验证表单
  if (!validateForm()) {
    return
  }

  // 检查服务条款同意
  if (!form.value.agreeToTerms) {
    error.value = '请先同意服务条款和隐私政策'
    return
  }

  isLoading.value = true

  try {
    await authStore.register({
      username: form.value.username,
      email: form.value.email,
      password: form.value.password,
      confirmPassword: form.value.confirmPassword
    })

    success.value = '注册成功！正在跳转到登录页面...'

    // 延迟跳转，让用户看到成功消息
    setTimeout(() => {
      router.push('/login')
    }, 2000)
  } catch (err: any) {
    console.error('注册失败:', err)

    // 处理不同类型的错误
    if (err.response?.status === 409) {
      error.value = '用户名或邮箱已存在'
    } else if (err.response?.status === 422) {
      error.value = '输入信息格式不正确，请检查后重试'
    } else if (err.response?.data?.message) {
      error.value = err.response.data.message
    } else if (err.message) {
      error.value = err.message
    } else {
      error.value = '注册失败，请检查网络连接后重试'
    }
  } finally {
    isLoading.value = false
  }
}

/**
 * 显示服务条款
 */
function showTerms() {
  // 这里可以打开服务条款模态框或跳转到服务条款页面
  console.log('显示服务条款')
}

/**
 * 显示隐私政策
 */
function showPrivacy() {
  // 这里可以打开隐私政策模态框或跳转到隐私政策页面
  console.log('显示隐私政策')
}
</script>

<style scoped>
/* 服务条款同意样式 */
.terms-agreement {
  margin-top: 0.25rem;
}

.checkbox-label {
  display: flex;
  align-items: flex-start;
  gap: 0.375rem;
  cursor: pointer;
  font-size: 0.8125rem;
  color: #374151;
  line-height: 1.4;
}

.checkbox-input {
  position: absolute;
  opacity: 0;
  pointer-events: none;
}

.checkbox-custom {
  width: 16px;
  height: 16px;
  border: 2px solid #d1d5db;
  border-radius: 4px;
  background-color: #ffffff;
  transition: all 0.3s ease;
  position: relative;
  flex-shrink: 0;
  margin-top: 0.125rem;
}

.checkbox-custom::after {
  content: '';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%) scale(0);
  width: 8px;
  height: 8px;
  background-color: #3b82f6;
  border-radius: 2px;
  transition: transform 0.2s ease;
}

.checkbox-input:checked + .checkbox-custom {
  border-color: #3b82f6;
  background-color: #3b82f6;
}

.checkbox-input:checked + .checkbox-custom::after {
  transform: translate(-50%, -50%) scale(1);
  background-color: white;
}

.checkbox-input:focus + .checkbox-custom {
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.checkbox-text {
  user-select: none;
}

.terms-link {
  color: #3b82f6;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.3s ease;
}

.terms-link:hover {
  color: #2563eb;
  text-decoration: underline;
}

/* 底部链接样式 */
.footer-text {
  margin: 0;
  font-size: 0.875rem;
  color: #6b7280;
}

.footer-link {
  color: #3b82f6;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.3s ease;
}

.footer-link:hover {
  color: #2563eb;
  text-decoration: underline;
}

/* 响应式设计 */
@media (max-width: 480px) {
  .checkbox-label {
    font-size: 0.8125rem;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .checkbox-custom,
  .checkbox-custom::after,
  .terms-link,
  .footer-link {
    transition: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .checkbox-custom {
    border-width: 2px;
  }

  .checkbox-input:checked + .checkbox-custom {
    border-width: 2px;
  }
}
</style>
