<template>
  <AuthForm
    title="欢迎回来"
    subtitle="登录您的账户以继续使用代码片段管理工具"
    submit-text="登录"
    loading-text="登录中..."
    :is-loading="isLoading"
    :is-form-valid="isFormValid"
    :error="error"
    @submit="handleLogin"
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
      />

      <FormField
        id="password"
        label="密码"
        type="password"
        v-model="form.password"
        placeholder="请输入密码"
        icon="M12,17A2,2 0 0,0 14,15C14,13.89 13.1,13 12,13A2,2 0 0,0 10,15A2,2 0 0,0 12,17M18,8A2,2 0 0,1 20,10V20A2,2 0 0,1 18,22H6A2,2 0 0,1 4,20V10C4,8.89 4.9,8 6,8H7V6A5,5 0 0,1 12,1A5,5 0 0,1 17,6V8H18M12,3A3,3 0 0,0 9,6V8H15V6A3,3 0 0,0 12,3Z"
        autocomplete="current-password"
        required
        :disabled="isLoading"
        :error-message="fieldErrors.password"
      />

      <!-- 记住我选项 -->
      <div class="login-options">
        <label class="checkbox-label">
          <input
            type="checkbox"
            v-model="form.rememberMe"
            :disabled="isLoading"
            class="checkbox-input"
          />
          <span class="checkbox-custom"></span>
          <span class="checkbox-text">记住我</span>
        </label>

        <router-link to="/forgot-password" class="forgot-link">
          忘记密码？
        </router-link>
      </div>
    </template>

    <template #footer>
      <p class="footer-text">
        还没有账户？
        <router-link to="/register" class="footer-link">立即注册</router-link>
      </p>
    </template>
  </AuthForm>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import AuthForm from '@/components/auth/AuthForm.vue'
import FormField from '@/components/auth/FormField.vue'

const router = useRouter()
const authStore = useAuthStore()

// 表单数据
const form = ref({
  username: '',
  password: '',
  rememberMe: false
})

// 状态管理
const error = ref('')
const isLoading = ref(false)
const fieldErrors = ref<Record<string, string>>({})

// 计算属性
const isFormValid = computed(() => {
  return form.value.username.trim() !== '' &&
         form.value.password.trim() !== '' &&
         Object.keys(fieldErrors.value).length === 0
})

/**
 * 验证表单字段
 */
function validateForm() {
  const errors: Record<string, string> = {}

  // 验证用户名
  if (!form.value.username.trim()) {
    errors.username = '请输入用户名'
  } else if (form.value.username.length < 3) {
    errors.username = '用户名至少需要3个字符'
  }

  // 验证密码
  if (!form.value.password.trim()) {
    errors.password = '请输入密码'
  } else if (form.value.password.length < 6) {
    errors.password = '密码至少需要6个字符'
  }

  fieldErrors.value = errors
  return Object.keys(errors).length === 0
}

/**
 * 处理登录
 */
async function handleLogin() {
  if (isLoading.value) return

  // 清除之前的错误
  error.value = ''
  fieldErrors.value = {}

  // 验证表单
  if (!validateForm()) {
    return
  }

  isLoading.value = true

  try {
    await authStore.login({
      username: form.value.username,
      password: form.value.password
    })

    // 登录成功，重定向到首页
    router.push('/')
  } catch (err: any) {
    console.error('登录失败:', err)

    // 处理不同类型的错误
    if (err.response?.status === 401) {
      error.value = '用户名或密码错误'
    } else if (err.response?.status === 429) {
      error.value = '登录尝试过于频繁，请稍后再试'
    } else if (err.response?.data?.message) {
      error.value = err.response.data.message
    } else if (err.message) {
      error.value = err.message
    } else {
      error.value = '登录失败，请检查网络连接后重试'
    }
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
/* 登录选项样式 */
.login-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  font-size: 0.875rem;
  color: #374151;
}

.checkbox-input {
  position: absolute;
  opacity: 0;
  pointer-events: none;
}

.checkbox-custom {
  width: 18px;
  height: 18px;
  border: 2px solid #d1d5db;
  border-radius: 4px;
  background-color: #ffffff;
  transition: all 0.3s ease;
  position: relative;
  flex-shrink: 0;
}

.checkbox-custom::after {
  content: '';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%) scale(0);
  width: 10px;
  height: 10px;
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

.forgot-link {
  color: #3b82f6;
  text-decoration: none;
  font-size: 0.875rem;
  transition: color 0.3s ease;
}

.forgot-link:hover {
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
  .login-options {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .checkbox-custom,
  .checkbox-custom::after,
  .forgot-link,
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
