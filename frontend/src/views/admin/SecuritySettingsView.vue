<template>
  <div class="security-settings-view">
    <div class="section-header">
      <h2>安全设置</h2>
      <p class="section-description">配置系统的安全策略和访问控制</p>
    </div>

    <form @submit.prevent="saveSettings" class="settings-form">
      <!-- 密码策略 -->
      <div class="form-section">
        <h3>密码策略</h3>
        <div class="row">
          <div class="col-md-6">
            <div class="form-group">
              <label for="minPasswordLength" class="form-label">最小密码长度</label>
              <input
                id="minPasswordLength"
                v-model.number="form.minPasswordLength"
                type="number"
                class="form-control"
                :class="{ 'is-invalid': errors.minPasswordLength }"
                min="6"
                max="50"
                required
              />
              <div v-if="errors.minPasswordLength" class="invalid-feedback">
                {{ errors.minPasswordLength }}
              </div>
              <small class="form-text text-muted">
                建议最小长度为8个字符
              </small>
            </div>
          </div>
          <div class="col-md-6">
            <div class="form-group">
              <label for="maxPasswordLength" class="form-label">最大密码长度</label>
              <input
                id="maxPasswordLength"
                v-model.number="form.maxPasswordLength"
                type="number"
                class="form-control"
                :class="{ 'is-invalid': errors.maxPasswordLength }"
                min="8"
                max="128"
                required
              />
              <div v-if="errors.maxPasswordLength" class="invalid-feedback">
                {{ errors.maxPasswordLength }}
              </div>
            </div>
          </div>
        </div>

        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="requireUppercase"
              v-model="form.requireUppercase"
              type="checkbox"
              class="form-check-input"
            />
            <label for="requireUppercase" class="form-check-label">
              要求大写字母
            </label>
          </div>
        </div>

        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="requireLowercase"
              v-model="form.requireLowercase"
              type="checkbox"
              class="form-check-input"
            />
            <label for="requireLowercase" class="form-check-label">
              要求小写字母
            </label>
          </div>
        </div>

        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="requireNumbers"
              v-model="form.requireNumbers"
              type="checkbox"
              class="form-check-input"
            />
            <label for="requireNumbers" class="form-check-label">
              要求数字
            </label>
          </div>
        </div>

        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="requireSpecialChars"
              v-model="form.requireSpecialChars"
              type="checkbox"
              class="form-check-input"
            />
            <label for="requireSpecialChars" class="form-check-label">
              要求特殊字符
            </label>
          </div>
          <small class="form-text text-muted">
            特殊字符包括：!@#$%^&*()_+-=[]{}|;:,.<>?
          </small>
        </div>
      </div>

      <!-- 账户锁定策略 -->
      <div class="form-section">
        <h3>账户锁定策略</h3>
        <div class="row">
          <div class="col-md-6">
            <div class="form-group">
              <label for="maxLoginAttempts" class="form-label">最大登录尝试次数</label>
              <input
                id="maxLoginAttempts"
                v-model.number="form.maxLoginAttempts"
                type="number"
                class="form-control"
                :class="{ 'is-invalid': errors.maxLoginAttempts }"
                min="1"
                max="20"
                required
              />
              <div v-if="errors.maxLoginAttempts" class="invalid-feedback">
                {{ errors.maxLoginAttempts }}
              </div>
              <small class="form-text text-muted">
                超过此次数后账户将被锁定
              </small>
            </div>
          </div>
          <div class="col-md-6">
            <div class="form-group">
              <label for="lockoutDuration" class="form-label">锁定持续时间（分钟）</label>
              <input
                id="lockoutDuration"
                v-model.number="form.lockoutDuration"
                type="number"
                class="form-control"
                :class="{ 'is-invalid': errors.lockoutDuration }"
                min="1"
                max="1440"
                required
              />
              <div v-if="errors.lockoutDuration" class="invalid-feedback">
                {{ errors.lockoutDuration }}
              </div>
              <small class="form-text text-muted">
                账户锁定后需要等待的时间
              </small>
            </div>
          </div>
        </div>
      </div>

      <!-- 会话管理 -->
      <div class="form-section">
        <h3>会话管理</h3>
        <div class="row">
          <div class="col-md-6">
            <div class="form-group">
              <label for="sessionTimeout" class="form-label">会话超时时间（分钟）</label>
              <input
                id="sessionTimeout"
                v-model.number="form.sessionTimeout"
                type="number"
                class="form-control"
                :class="{ 'is-invalid': errors.sessionTimeout }"
                min="5"
                max="1440"
                required
              />
              <div v-if="errors.sessionTimeout" class="invalid-feedback">
                {{ errors.sessionTimeout }}
              </div>
              <small class="form-text text-muted">
                用户无操作后自动登出的时间
              </small>
            </div>
          </div>
          <div class="col-md-6">
            <div class="form-group">
              <label for="maxConcurrentSessions" class="form-label">最大并发会话数</label>
              <input
                id="maxConcurrentSessions"
                v-model.number="form.maxConcurrentSessions"
                type="number"
                class="form-control"
                :class="{ 'is-invalid': errors.maxConcurrentSessions }"
                min="1"
                max="10"
                required
              />
              <div v-if="errors.maxConcurrentSessions" class="invalid-feedback">
                {{ errors.maxConcurrentSessions }}
              </div>
              <small class="form-text text-muted">
                单用户同时登录的设备数量限制
              </small>
            </div>
          </div>
        </div>

        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="rememberMeEnabled"
              v-model="form.rememberMeEnabled"
              type="checkbox"
              class="form-check-input"
            />
            <label for="rememberMeEnabled" class="form-check-label">
              启用"记住我"功能
            </label>
          </div>
        </div>

        <div v-if="form.rememberMeEnabled" class="form-group">
          <label for="rememberMeDuration" class="form-label">记住持续时间（天）</label>
          <input
            id="rememberMeDuration"
            v-model.number="form.rememberMeDuration"
            type="number"
            class="form-control"
            :class="{ 'is-invalid': errors.rememberMeDuration }"
            min="1"
            max="365"
            required
          />
          <div v-if="errors.rememberMeDuration" class="invalid-feedback">
            {{ errors.rememberMeDuration }}
          </div>
        </div>
      </div>

      <!-- 双因素认证 -->
      <div class="form-section">
        <h3>双因素认证</h3>
        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="twoFactorEnabled"
              v-model="form.twoFactorEnabled"
              type="checkbox"
              class="form-check-input"
            />
            <label for="twoFactorEnabled" class="form-check-label">
              启用双因素认证
            </label>
          </div>
          <small class="form-text text-muted">
            启用后用户登录时需要二次验证
          </small>
        </div>

        <div v-if="form.twoFactorEnabled" class="form-group">
          <label for="twoFactorMethods" class="form-label">可用的验证方式</label>
          <div class="form-check">
            <input
              id="methodEmail"
              v-model="form.twoFactorMethods"
              type="checkbox"
              value="email"
              class="form-check-input"
            />
            <label for="methodEmail" class="form-check-label">
              邮箱验证码
            </label>
          </div>
          <div class="form-check">
            <input
              id="methodSms"
              v-model="form.twoFactorMethods"
              type="checkbox"
              value="sms"
              class="form-check-input"
            />
            <label for="methodSms" class="form-check-label">
              短信验证码
            </label>
          </div>
          <div class="form-check">
            <input
              id="methodApp"
              v-model="form.twoFactorMethods"
              type="checkbox"
              value="app"
              class="form-check-input"
            />
            <label for="methodApp" class="form-check-label">
              认证应用
            </label>
          </div>
        </div>
      </div>

      <!-- 安全日志 -->
      <div class="form-section">
        <h3>安全日志</h3>
        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="enableSecurityLogging"
              v-model="form.enableSecurityLogging"
              type="checkbox"
              class="form-check-input"
            />
            <label for="enableSecurityLogging" class="form-check-label">
              启用安全日志记录
            </label>
          </div>
        </div>

        <div v-if="form.enableSecurityLogging" class="form-group">
          <label for="logRetentionDays" class="form-label">日志保留天数</label>
          <input
            id="logRetentionDays"
            v-model.number="form.logRetentionDays"
            type="number"
            class="form-control"
            :class="{ 'is-invalid': errors.logRetentionDays }"
            min="7"
            max="365"
            required
          />
          <div v-if="errors.logRetentionDays" class="invalid-feedback">
            {{ errors.logRetentionDays }}
          </div>
          <small class="form-text text-muted">
            超过此天数的日志将被自动删除
          </small>
        </div>

        <div class="form-group">
          <label class="form-label">记录的安全事件</label>
          <div class="form-check">
            <input
              id="logLoginAttempts"
              v-model="form.securityLogEvents"
              type="checkbox"
              value="login_attempts"
              class="form-check-input"
            />
            <label for="logLoginAttempts" class="form-check-label">
              登录尝试
            </label>
          </div>
          <div class="form-check">
            <input
              id="logPasswordChanges"
              v-model="form.securityLogEvents"
              type="checkbox"
              value="password_changes"
              class="form-check-input"
            />
            <label for="logPasswordChanges" class="form-check-label">
              密码变更
            </label>
          </div>
          <div class="form-check">
            <input
              id="logPermissionChanges"
              v-model="form.securityLogEvents"
              type="checkbox"
              value="permission_changes"
              class="form-check-input"
            />
            <label for="logPermissionChanges" class="form-check-label">
              权限变更
            </label>
          </div>
          <div class="form-check">
            <input
              id="logAccountLockouts"
              v-model="form.securityLogEvents"
              type="checkbox"
              value="account_lockouts"
              class="form-check-input"
            />
            <label for="logAccountLockouts" class="form-check-label">
              账户锁定
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
import type { SecuritySettings, UpdateSecuritySettingsRequest } from '@/types/settings'

interface Props {
  settings: SecuritySettings | null
  loading: boolean
}

interface Emits {
  (e: 'update:settings', request: UpdateSecuritySettingsRequest): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// 表单数据
const form = ref<SecuritySettings>({
  minPasswordLength: 8,
  maxPasswordLength: 32,
  requireUppercase: true,
  requireLowercase: true,
  requireNumbers: true,
  requireSpecialChars: false,
  maxLoginAttempts: 5,
  lockoutDuration: 30,
  sessionTimeout: 30,
  maxConcurrentSessions: 3,
  rememberMeEnabled: true,
  rememberMeDuration: 7,
  twoFactorEnabled: false,
  twoFactorMethods: [],
  enableSecurityLogging: true,
  logRetentionDays: 90,
  securityLogEvents: ['login_attempts', 'password_changes']
})

// 错误信息
const errors = ref<Record<string, string>>({})

// 计算属性
const hasChanges = computed(() => {
  if (!props.settings) return false
  
  return Object.keys(form.value).some(key => {
    const currentValue = form.value[key as keyof SecuritySettings]
    const originalValue = props.settings![key as keyof SecuritySettings]
    
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
  
  if (form.value.minPasswordLength < 6) {
    errors.value.minPasswordLength = '最小密码长度不能少于6个字符'
  }
  
  if (form.value.maxPasswordLength < form.value.minPasswordLength) {
    errors.value.maxPasswordLength = '最大密码长度不能小于最小密码长度'
  }
  
  if (form.value.maxPasswordLength > 128) {
    errors.value.maxPasswordLength = '最大密码长度不能超过128个字符'
  }
  
  if (form.value.maxLoginAttempts < 1 || form.value.maxLoginAttempts > 20) {
    errors.value.maxLoginAttempts = '最大登录尝试次数必须在1-20之间'
  }
  
  if (form.value.lockoutDuration < 1 || form.value.lockoutDuration > 1440) {
    errors.value.lockoutDuration = '锁定持续时间必须在1-1440分钟之间'
  }
  
  if (form.value.sessionTimeout < 5 || form.value.sessionTimeout > 1440) {
    errors.value.sessionTimeout = '会话超时时间必须在5-1440分钟之间'
  }
  
  if (form.value.maxConcurrentSessions < 1 || form.value.maxConcurrentSessions > 10) {
    errors.value.maxConcurrentSessions = '最大并发会话数必须在1-10之间'
  }
  
  if (form.value.rememberMeEnabled && (form.value.rememberMeDuration < 1 || form.value.rememberMeDuration > 365)) {
    errors.value.rememberMeDuration = '记住持续时间必须在1-365天之间'
  }
  
  if (form.value.enableSecurityLogging && (form.value.logRetentionDays < 7 || form.value.logRetentionDays > 365)) {
    errors.value.logRetentionDays = '日志保留天数必须在7-365天之间'
  }
  
  return Object.keys(errors.value).length === 0
}

function saveSettings() {
  if (!validateForm()) return
  
  const request: UpdateSecuritySettingsRequest = {}
  
  // 只包含有变化的字段
  if (form.value.minPasswordLength !== props.settings?.minPasswordLength) {
    request.minPasswordLength = form.value.minPasswordLength
  }
  if (form.value.maxPasswordLength !== props.settings?.maxPasswordLength) {
    request.maxPasswordLength = form.value.maxPasswordLength
  }
  if (form.value.requireUppercase !== props.settings?.requireUppercase) {
    request.requireUppercase = form.value.requireUppercase
  }
  if (form.value.requireLowercase !== props.settings?.requireLowercase) {
    request.requireLowercase = form.value.requireLowercase
  }
  if (form.value.requireNumbers !== props.settings?.requireNumbers) {
    request.requireNumbers = form.value.requireNumbers
  }
  if (form.value.requireSpecialChars !== props.settings?.requireSpecialChars) {
    request.requireSpecialChars = form.value.requireSpecialChars
  }
  if (form.value.maxLoginAttempts !== props.settings?.maxLoginAttempts) {
    request.maxLoginAttempts = form.value.maxLoginAttempts
  }
  if (form.value.lockoutDuration !== props.settings?.lockoutDuration) {
    request.lockoutDuration = form.value.lockoutDuration
  }
  if (form.value.sessionTimeout !== props.settings?.sessionTimeout) {
    request.sessionTimeout = form.value.sessionTimeout
  }
  if (form.value.maxConcurrentSessions !== props.settings?.maxConcurrentSessions) {
    request.maxConcurrentSessions = form.value.maxConcurrentSessions
  }
  if (form.value.rememberMeEnabled !== props.settings?.rememberMeEnabled) {
    request.rememberMeEnabled = form.value.rememberMeEnabled
  }
  if (form.value.rememberMeDuration !== props.settings?.rememberMeDuration) {
    request.rememberMeDuration = form.value.rememberMeDuration
  }
  if (form.value.twoFactorEnabled !== props.settings?.twoFactorEnabled) {
    request.twoFactorEnabled = form.value.twoFactorEnabled
  }
  if (JSON.stringify(form.value.twoFactorMethods) !== JSON.stringify(props.settings?.twoFactorMethods)) {
    request.twoFactorMethods = form.value.twoFactorMethods
  }
  if (form.value.enableSecurityLogging !== props.settings?.enableSecurityLogging) {
    request.enableSecurityLogging = form.value.enableSecurityLogging
  }
  if (form.value.logRetentionDays !== props.settings?.logRetentionDays) {
    request.logRetentionDays = form.value.logRetentionDays
  }
  if (JSON.stringify(form.value.securityLogEvents) !== JSON.stringify(props.settings?.securityLogEvents)) {
    request.securityLogEvents = form.value.securityLogEvents
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
.security-settings-view {
  max-width: 900px;
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

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
  padding-top: 2rem;
  border-top: 1px solid #e9ecef;
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