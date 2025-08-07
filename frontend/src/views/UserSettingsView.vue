<template>
  <div class="user-settings">
    <!-- 页面标题 -->
    <div class="header">
      <div class="title-section">
        <h1>个人设置</h1>
        <p class="subtitle">管理您的账户信息和偏好设置</p>
      </div>
    </div>

    <div class="settings-container">
      <!-- 设置导航 -->
      <div class="settings-nav">
        <button
          v-for="tab in settingsTabs"
          :key="tab.key"
          class="nav-item"
          :class="{ active: activeTab === tab.key }"
          @click="activeTab = tab.key"
        >
          <i :class="tab.icon"></i>
          <span>{{ tab.label }}</span>
        </button>
      </div>

      <!-- 设置内容 -->
      <div class="settings-content">
        <!-- 个人资料 -->
        <div v-if="activeTab === 'profile'" class="settings-section">
          <div class="section-header">
            <h2>个人资料</h2>
            <p>更新您的基本信息</p>
          </div>

          <form @submit.prevent="updateProfile" class="profile-form">
            <!-- 用户头像 -->
            <div class="avatar-section">
              <div class="avatar">
                <i class="icon-user"></i>
              </div>
              <div class="avatar-info">
                <h3>{{ user?.username }}</h3>
                <p>{{ getRoleText(user?.role) }}</p>
              </div>
            </div>

            <!-- 用户名 -->
            <div class="form-group">
              <label for="username" class="form-label">
                用户名 <span class="required">*</span>
              </label>
              <input
                id="username"
                v-model="profileForm.username"
                type="text"
                class="form-input"
                :class="{ error: profileErrors.username }"
                placeholder="请输入用户名"
                required
              />
              <div v-if="profileErrors.username" class="error-message">
                {{ profileErrors.username }}
              </div>
            </div>

            <!-- 邮箱 -->
            <div class="form-group">
              <label for="email" class="form-label">
                邮箱地址 <span class="required">*</span>
              </label>
              <input
                id="email"
                v-model="profileForm.email"
                type="email"
                class="form-input"
                :class="{ error: profileErrors.email }"
                placeholder="请输入邮箱地址"
                required
              />
              <div v-if="profileErrors.email" class="error-message">
                {{ profileErrors.email }}
              </div>
            </div>

            <!-- 账户信息 -->
            <div class="account-info">
              <div class="info-item">
                <span class="info-label">用户ID:</span>
                <span class="info-value">{{ user?.id }}</span>
              </div>
              <div class="info-item">
                <span class="info-label">注册时间:</span>
                <span class="info-value">{{ formatDate(user?.createdAt) }}</span>
              </div>
              <div class="info-item">
                <span class="info-label">账户状态:</span>
                <span class="info-value status" :class="{ active: user?.isActive }">
                  {{ user?.isActive ? '正常' : '禁用' }}
                </span>
              </div>
            </div>

            <!-- 错误提示 -->
            <div v-if="profileError" class="submit-error">
              <i class="icon-alert-circle"></i>
              {{ profileError }}
            </div>

            <!-- 操作按钮 -->
            <div class="form-actions">
              <button
                type="button"
                class="btn btn-secondary"
                @click="resetProfileForm"
                :disabled="profileLoading"
              >
                重置
              </button>
              <button
                type="submit"
                class="btn btn-primary"
                :disabled="profileLoading || !hasProfileChanges"
              >
                <span v-if="profileLoading" class="loading-spinner"></span>
                {{ profileLoading ? '保存中...' : '保存更改' }}
              </button>
            </div>
          </form>
        </div>

        <!-- 安全设置 -->
        <div v-if="activeTab === 'security'" class="settings-section">
          <div class="section-header">
            <h2>安全设置</h2>
            <p>管理您的密码和安全选项</p>
          </div>

          <form @submit.prevent="changePassword" class="security-form">
            <!-- 当前密码 -->
            <div class="form-group">
              <label for="currentPassword" class="form-label">
                当前密码 <span class="required">*</span>
              </label>
              <div class="password-input">
                <input
                  id="currentPassword"
                  v-model="passwordForm.currentPassword"
                  :type="showCurrentPassword ? 'text' : 'password'"
                  class="form-input"
                  :class="{ error: passwordErrors.currentPassword }"
                  placeholder="请输入当前密码"
                  required
                />
                <button
                  type="button"
                  class="password-toggle"
                  @click="showCurrentPassword = !showCurrentPassword"
                >
                  <i :class="showCurrentPassword ? 'icon-eye-off' : 'icon-eye'"></i>
                </button>
              </div>
              <div v-if="passwordErrors.currentPassword" class="error-message">
                {{ passwordErrors.currentPassword }}
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
                  v-model="passwordForm.newPassword"
                  :type="showNewPassword ? 'text' : 'password'"
                  class="form-input"
                  :class="{ error: passwordErrors.newPassword }"
                  placeholder="请输入新密码"
                  required
                />
                <button
                  type="button"
                  class="password-toggle"
                  @click="showNewPassword = !showNewPassword"
                >
                  <i :class="showNewPassword ? 'icon-eye-off' : 'icon-eye'"></i>
                </button>
              </div>
              <div v-if="passwordErrors.newPassword" class="error-message">
                {{ passwordErrors.newPassword }}
              </div>
              <div class="password-hint">
                密码至少8位，包含字母和数字
              </div>
            </div>

            <!-- 确认新密码 -->
            <div class="form-group">
              <label for="confirmPassword" class="form-label">
                确认新密码 <span class="required">*</span>
              </label>
              <input
                id="confirmPassword"
                v-model="passwordForm.confirmPassword"
                type="password"
                class="form-input"
                :class="{ error: passwordErrors.confirmPassword }"
                placeholder="请再次输入新密码"
                required
              />
              <div v-if="passwordErrors.confirmPassword" class="error-message">
                {{ passwordErrors.confirmPassword }}
              </div>
            </div>

            <!-- 密码强度指示器 -->
            <div v-if="passwordForm.newPassword" class="password-strength">
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
            <div v-if="passwordError" class="submit-error">
              <i class="icon-alert-circle"></i>
              {{ passwordError }}
            </div>

            <!-- 操作按钮 -->
            <div class="form-actions">
              <button
                type="button"
                class="btn btn-secondary"
                @click="resetPasswordForm"
                :disabled="passwordLoading"
              >
                重置
              </button>
              <button
                type="submit"
                class="btn btn-primary"
                :disabled="passwordLoading || !isPasswordFormValid"
              >
                <span v-if="passwordLoading" class="loading-spinner"></span>
                {{ passwordLoading ? '修改中...' : '修改密码' }}
              </button>
            </div>
          </form>
        </div>

        <!-- 偏好设置 -->
        <div v-if="activeTab === 'preferences'" class="settings-section">
          <div class="section-header">
            <h2>偏好设置</h2>
            <p>自定义您的使用体验</p>
          </div>

          <form @submit.prevent="updatePreferences" class="preferences-form">
            <!-- 主题设置 -->
            <div class="preference-group">
              <h3>界面主题</h3>
              <div class="theme-options">
                <label
                  v-for="theme in themeOptions"
                  :key="theme.value"
                  class="theme-option"
                  :class="{ selected: preferences.theme === theme.value }"
                >
                  <input
                    v-model="preferences.theme"
                    type="radio"
                    :value="theme.value"
                    class="theme-radio"
                  />
                  <div class="theme-preview" :class="theme.value">
                    <div class="preview-header"></div>
                    <div class="preview-content">
                      <div class="preview-line"></div>
                      <div class="preview-line short"></div>
                    </div>
                  </div>
                  <span class="theme-name">{{ theme.label }}</span>
                </label>
              </div>
            </div>

            <!-- 编辑器设置 -->
            <div class="preference-group">
              <h3>编辑器设置</h3>
              <div class="editor-settings">
                <div class="setting-item">
                  <label class="setting-label">字体大小</label>
                  <select v-model="preferences.fontSize" class="setting-select">
                    <option value="12">12px</option>
                    <option value="14">14px</option>
                    <option value="16">16px</option>
                    <option value="18">18px</option>
                    <option value="20">20px</option>
                  </select>
                </div>
                <div class="setting-item">
                  <label class="setting-label">Tab 大小</label>
                  <select v-model="preferences.tabSize" class="setting-select">
                    <option value="2">2 空格</option>
                    <option value="4">4 空格</option>
                    <option value="8">8 空格</option>
                  </select>
                </div>
                <div class="setting-item">
                  <label class="setting-toggle">
                    <input
                      v-model="preferences.wordWrap"
                      type="checkbox"
                      class="toggle-checkbox"
                    />
                    <div class="toggle-slider"></div>
                    <span>自动换行</span>
                  </label>
                </div>
                <div class="setting-item">
                  <label class="setting-toggle">
                    <input
                      v-model="preferences.minimap"
                      type="checkbox"
                      class="toggle-checkbox"
                    />
                    <div class="toggle-slider"></div>
                    <span>显示小地图</span>
                  </label>
                </div>
              </div>
            </div>

            <!-- 通知设置 -->
            <div class="preference-group">
              <h3>通知设置</h3>
              <div class="notification-settings">
                <div class="setting-item">
                  <label class="setting-toggle">
                    <input
                      v-model="preferences.emailNotifications"
                      type="checkbox"
                      class="toggle-checkbox"
                    />
                    <div class="toggle-slider"></div>
                    <span>邮件通知</span>
                  </label>
                  <p class="setting-description">接收重要更新和通知邮件</p>
                </div>
                <div class="setting-item">
                  <label class="setting-toggle">
                    <input
                      v-model="preferences.browserNotifications"
                      type="checkbox"
                      class="toggle-checkbox"
                    />
                    <div class="toggle-slider"></div>
                    <span>浏览器通知</span>
                  </label>
                  <p class="setting-description">在浏览器中显示通知</p>
                </div>
              </div>
            </div>

            <!-- 操作按钮 -->
            <div class="form-actions">
              <button
                type="button"
                class="btn btn-secondary"
                @click="resetPreferences"
                :disabled="preferencesLoading"
              >
                重置为默认
              </button>
              <button
                type="submit"
                class="btn btn-primary"
                :disabled="preferencesLoading"
              >
                <span v-if="preferencesLoading" class="loading-spinner"></span>
                {{ preferencesLoading ? '保存中...' : '保存设置' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Toast 通知 -->
    <Toast
      v-if="toast.show"
      :type="toast.type"
      :message="toast.message"
      @close="hideToast"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { userService, type UpdateUserRequest } from '@/services/userService'
import { authService } from '@/services/authService'
import { UserRole, type User } from '@/types'
import Toast from '@/components/common/Toast.vue'

// 状态管理
const authStore = useAuthStore()

// 响应式数据
const activeTab = ref('profile')
const profileLoading = ref(false)
const passwordLoading = ref(false)
const preferencesLoading = ref(false)
const profileError = ref('')
const passwordError = ref('')
const showCurrentPassword = ref(false)
const showNewPassword = ref(false)

// 用户信息
const user = computed(() => authStore.user)

// 个人资料表单
const profileForm = reactive({
  username: '',
  email: ''
})

const originalProfile = reactive({
  username: '',
  email: ''
})

const profileErrors = reactive({
  username: '',
  email: ''
})

// 密码表单
const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const passwordErrors = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

// 偏好设置
const preferences = reactive({
  theme: 'light',
  fontSize: 14,
  tabSize: 4,
  wordWrap: true,
  minimap: true,
  emailNotifications: true,
  browserNotifications: false
})

// Toast 通知
const toast = ref({
  show: false,
  type: 'success' as 'success' | 'error' | 'warning' | 'info',
  message: ''
})

// 设置标签
const settingsTabs = [
  { key: 'profile', label: '个人资料', icon: 'icon-user' },
  { key: 'security', label: '安全设置', icon: 'icon-shield' },
  { key: 'preferences', label: '偏好设置', icon: 'icon-settings' }
]

// 主题选项
const themeOptions = [
  { value: 'light', label: '浅色主题' },
  { value: 'dark', label: '深色主题' },
  { value: 'auto', label: '跟随系统' }
]

// 计算属性
/**
 * 个人资料是否有变更
 */
const hasProfileChanges = computed(() => {
  return profileForm.username !== originalProfile.username ||
         profileForm.email !== originalProfile.email
})

/**
 * 密码表单是否有效
 */
const isPasswordFormValid = computed(() => {
  return passwordForm.currentPassword.trim() &&
         passwordForm.newPassword.trim() &&
         passwordForm.confirmPassword.trim() &&
         !Object.values(passwordErrors).some(error => error)
})

/**
 * 密码强度计算
 */
const passwordStrength = computed(() => {
  const password = passwordForm.newPassword
  if (!password) {
    return { width: '0%', class: '', text: '' }
  }

  let score = 0

  if (password.length >= 8) score += 1
  if (/[a-z]/.test(password)) score += 1
  if (/[A-Z]/.test(password)) score += 1
  if (/\d/.test(password)) score += 1
  if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) score += 1

  if (score <= 2) {
    return { width: '25%', class: 'weak', text: '弱' }
  } else if (score === 3) {
    return { width: '50%', class: 'medium', text: '中等' }
  } else if (score === 4) {
    return { width: '75%', class: 'strong', text: '强' }
  } else {
    return { width: '100%', class: 'very-strong', text: '很强' }
  }
})

// 方法
/**
 * 初始化表单数据
 */
function initializeForms() {
  if (user.value) {
    profileForm.username = user.value.username
    profileForm.email = user.value.email
    
    originalProfile.username = user.value.username
    originalProfile.email = user.value.email
  }

  // 从本地存储加载偏好设置
  const savedPreferences = localStorage.getItem('userPreferences')
  if (savedPreferences) {
    Object.assign(preferences, JSON.parse(savedPreferences))
  }
}

/**
 * 获取角色文本
 */
function getRoleText(role?: UserRole): string {
  switch (role) {
    case UserRole.Admin:
      return '管理员'
    case UserRole.Editor:
      return '编辑者'
    case UserRole.Viewer:
      return '查看者'
    default:
      return '查看者'
  }
}

/**
 * 格式化日期
 */
function formatDate(dateString?: string): string {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

/**
 * 验证个人资料表单
 */
function validateProfileForm(): boolean {
  Object.keys(profileErrors).forEach(key => {
    profileErrors[key as keyof typeof profileErrors] = ''
  })

  let isValid = true

  if (!profileForm.username.trim()) {
    profileErrors.username = '用户名不能为空'
    isValid = false
  } else if (profileForm.username.length < 3) {
    profileErrors.username = '用户名至少3个字符'
    isValid = false
  }

  if (!profileForm.email.trim()) {
    profileErrors.email = '邮箱地址不能为空'
    isValid = false
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(profileForm.email)) {
    profileErrors.email = '请输入有效的邮箱地址'
    isValid = false
  }

  return isValid
}

/**
 * 验证密码表单
 */
function validatePasswordForm(): boolean {
  Object.keys(passwordErrors).forEach(key => {
    passwordErrors[key as keyof typeof passwordErrors] = ''
  })

  let isValid = true

  if (!passwordForm.currentPassword.trim()) {
    passwordErrors.currentPassword = '请输入当前密码'
    isValid = false
  }

  if (!passwordForm.newPassword.trim()) {
    passwordErrors.newPassword = '请输入新密码'
    isValid = false
  } else if (passwordForm.newPassword.length < 8) {
    passwordErrors.newPassword = '密码至少8个字符'
    isValid = false
  } else if (!/(?=.*[a-zA-Z])(?=.*\d)/.test(passwordForm.newPassword)) {
    passwordErrors.newPassword = '密码必须包含字母和数字'
    isValid = false
  }

  if (!passwordForm.confirmPassword.trim()) {
    passwordErrors.confirmPassword = '请确认新密码'
    isValid = false
  } else if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    passwordErrors.confirmPassword = '两次输入的密码不一致'
    isValid = false
  }

  return isValid
}

/**
 * 更新个人资料
 */
async function updateProfile() {
  if (!validateProfileForm() || !user.value) return

  profileLoading.value = true
  profileError.value = ''

  try {
    const updateData: UpdateUserRequest = {}

    if (profileForm.username !== originalProfile.username) {
      updateData.username = profileForm.username.trim()
    }
    if (profileForm.email !== originalProfile.email) {
      updateData.email = profileForm.email.trim()
    }

    const updatedUser = await userService.updateUser(user.value.id, updateData)
    
    // 更新认证状态中的用户信息
    authStore.updateUser(updatedUser)
    
    // 更新原始数据
    originalProfile.username = updatedUser.username
    originalProfile.email = updatedUser.email

    showToast('success', '个人资料更新成功')
  } catch (error: any) {
    profileError.value = error.response?.data?.message || '更新个人资料失败'
  } finally {
    profileLoading.value = false
  }
}

/**
 * 修改密码
 */
async function changePassword() {
  if (!validatePasswordForm()) return

  passwordLoading.value = true
  passwordError.value = ''

  try {
    await authService.changePassword({
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword
    })

    resetPasswordForm()
    showToast('success', '密码修改成功')
  } catch (error: any) {
    passwordError.value = error.response?.data?.message || '修改密码失败'
  } finally {
    passwordLoading.value = false
  }
}

/**
 * 更新偏好设置
 */
async function updatePreferences() {
  preferencesLoading.value = true

  try {
    // 保存到本地存储
    localStorage.setItem('userPreferences', JSON.stringify(preferences))
    
    // 应用主题设置
    applyTheme(preferences.theme)
    
    showToast('success', '偏好设置保存成功')
  } catch (error) {
    showToast('error', '保存偏好设置失败')
  } finally {
    preferencesLoading.value = false
  }
}

/**
 * 应用主题设置
 */
function applyTheme(theme: string) {
  const html = document.documentElement
  
  if (theme === 'dark') {
    html.classList.add('dark')
  } else if (theme === 'light') {
    html.classList.remove('dark')
  } else if (theme === 'auto') {
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
    if (prefersDark) {
      html.classList.add('dark')
    } else {
      html.classList.remove('dark')
    }
  }
}

/**
 * 重置个人资料表单
 */
function resetProfileForm() {
  if (user.value) {
    profileForm.username = user.value.username
    profileForm.email = user.value.email
  }
  Object.keys(profileErrors).forEach(key => {
    profileErrors[key as keyof typeof profileErrors] = ''
  })
  profileError.value = ''
}

/**
 * 重置密码表单
 */
function resetPasswordForm() {
  passwordForm.currentPassword = ''
  passwordForm.newPassword = ''
  passwordForm.confirmPassword = ''
  Object.keys(passwordErrors).forEach(key => {
    passwordErrors[key as keyof typeof passwordErrors] = ''
  })
  passwordError.value = ''
}

/**
 * 重置偏好设置
 */
function resetPreferences() {
  Object.assign(preferences, {
    theme: 'light',
    fontSize: 14,
    tabSize: 4,
    wordWrap: true,
    minimap: true,
    emailNotifications: true,
    browserNotifications: false
  })
}

/**
 * 显示Toast通知
 */
function showToast(type: typeof toast.value.type, message: string) {
  toast.value = { show: true, type, message }
}

/**
 * 隐藏Toast通知
 */
function hideToast() {
  toast.value.show = false
}

// 生命周期
onMounted(() => {
  initializeForms()
  applyTheme(preferences.theme)
})
</script>

<style scoped>
.user-settings {
  padding: 24px;
  max-width: 1200px;
  margin: 0 auto;
}

/* 页面标题 */
.header {
  margin-bottom: 32px;
}

.title-section h1 {
  margin: 0 0 8px 0;
  font-size: 28px;
  font-weight: 600;
  color: #1a1a1a;
}

.subtitle {
  margin: 0;
  color: #666;
  font-size: 16px;
}

/* 设置容器 */
.settings-container {
  display: grid;
  grid-template-columns: 240px 1fr;
  gap: 32px;
  align-items: start;
}

/* 设置导航 */
.settings-nav {
  background: white;
  border-radius: 12px;
  padding: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border: 1px solid #e5e7eb;
  position: sticky;
  top: 24px;
}

.nav-item {
  width: 100%;
  padding: 12px 16px;
  border: none;
  background: none;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 14px;
  color: #666;
  text-align: left;
}

.nav-item:hover {
  background: #f3f4f6;
  color: #1a1a1a;
}

.nav-item.active {
  background: #3b82f6;
  color: white;
}

.nav-item i {
  font-size: 16px;
}

/* 设置内容 */
.settings-content {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border: 1px solid #e5e7eb;
}

.settings-section {
  padding: 32px;
}

.section-header {
  margin-bottom: 32px;
  padding-bottom: 16px;
  border-bottom: 1px solid #e5e7eb;
}

.section-header h2 {
  margin: 0 0 8px 0;
  font-size: 20px;
  font-weight: 600;
  color: #1a1a1a;
}

.section-header p {
  margin: 0;
  color: #666;
  font-size: 14px;
}

/* 头像部分 */
.avatar-section {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 32px;
  padding: 20px;
  background: #f9fafb;
  border-radius: 12px;
}

.avatar {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: linear-gradient(135deg, #3b82f6, #1d4ed8);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 24px;
}

.avatar-info h3 {
  margin: 0 0 4px 0;
  font-size: 18px;
  font-weight: 600;
  color: #1a1a1a;
}

.avatar-info p {
  margin: 0;
  color: #666;
  font-size: 14px;
}

/* 表单样式 */
.form-group {
  margin-bottom: 24px;
}

.form-label {
  display: block;
  margin-bottom: 8px;
  font-size: 14px;
  font-weight: 500;
  color: #374151;
}

.required {
  color: #dc2626;
}

.form-input {
  width: 100%;
  padding: 12px 16px;
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
  right: 12px;
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
  margin-top: 6px;
  font-size: 12px;
  color: #666;
}

.error-message {
  margin-top: 6px;
  font-size: 12px;
  color: #dc2626;
}

/* 账户信息 */
.account-info {
  background: #f9fafb;
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 24px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
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

.info-value.status {
  font-family: inherit;
  font-size: 14px;
  font-weight: 500;
  color: #dc2626;
}

.info-value.status.active {
  color: #10b981;
}

/* 密码强度 */
.password-strength {
  margin-bottom: 24px;
  padding: 16px;
  background: #f9fafb;
  border-radius: 8px;
}

.strength-label {
  font-size: 12px;
  color: #666;
  margin-bottom: 8px;
}

.strength-bar {
  height: 4px;
  background: #e5e7eb;
  border-radius: 2px;
  overflow: hidden;
  margin-bottom: 8px;
}

.strength-fill {
  height: 100%;
  transition: width 0.3s ease;
}

.strength-fill.weak { background: #dc2626; }
.strength-fill.medium { background: #f59e0b; }
.strength-fill.strong { background: #10b981; }
.strength-fill.very-strong { background: #059669; }

.strength-text {
  font-size: 12px;
  font-weight: 500;
}

.strength-text.weak { color: #dc2626; }
.strength-text.medium { color: #f59e0b; }
.strength-text.strong { color: #10b981; }
.strength-text.very-strong { color: #059669; }

/* 偏好设置 */
.preference-group {
  margin-bottom: 32px;
}

.preference-group h3 {
  margin: 0 0 16px 0;
  font-size: 16px;
  font-weight: 600;
  color: #1a1a1a;
}

/* 主题选项 */
.theme-options {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 16px;
}

.theme-option {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 16px;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.theme-option:hover {
  border-color: #3b82f6;
}

.theme-option.selected {
  border-color: #3b82f6;
  background: #eff6ff;
}

.theme-radio {
  display: none;
}

.theme-preview {
  width: 60px;
  height: 40px;
  border-radius: 4px;
  overflow: hidden;
  border: 1px solid #d1d5db;
}

.theme-preview.light {
  background: white;
}

.theme-preview.dark {
  background: #1f2937;
}

.theme-preview.auto {
  background: linear-gradient(90deg, white 50%, #1f2937 50%);
}

.preview-header {
  height: 8px;
  background: #f3f4f6;
}

.theme-preview.dark .preview-header {
  background: #374151;
}

.preview-content {
  padding: 4px;
}

.preview-line {
  height: 2px;
  background: #d1d5db;
  margin-bottom: 2px;
  border-radius: 1px;
}

.preview-line.short {
  width: 60%;
}

.theme-preview.dark .preview-line {
  background: #6b7280;
}

.theme-name {
  font-size: 12px;
  font-weight: 500;
  color: #374151;
}

/* 编辑器设置 */
.editor-settings {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.setting-item {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.setting-label {
  font-size: 14px;
  font-weight: 500;
  color: #374151;
}

.setting-select {
  padding: 8px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  background: white;
}

.setting-toggle {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
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

/* 通知设置 */
.notification-settings .setting-item {
  padding: 16px 0;
  border-bottom: 1px solid #e5e7eb;
}

.notification-settings .setting-item:last-child {
  border-bottom: none;
}

.setting-description {
  margin: 4px 0 0 56px;
  font-size: 12px;
  color: #666;
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
  margin-bottom: 24px;
}

/* 操作按钮 */
.form-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  padding-top: 24px;
  border-top: 1px solid #e5e7eb;
}

.btn {
  padding: 12px 24px;
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
@media (max-width: 768px) {
  .user-settings {
    padding: 16px;
  }

  .settings-container {
    grid-template-columns: 1fr;
    gap: 20px;
  }

  .settings-nav {
    position: static;
    display: flex;
    overflow-x: auto;
    padding: 4px;
  }

  .nav-item {
    white-space: nowrap;
    min-width: auto;
  }

  .settings-section {
    padding: 24px 20px;
  }

  .theme-options {
    grid-template-columns: repeat(3, 1fr);
  }

  .editor-settings {
    grid-template-columns: 1fr;
  }

  .form-actions {
    flex-direction: column-reverse;
  }

  .btn {
    width: 100%;
    justify-content: center;
  }
}
</style>