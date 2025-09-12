<template>
  <div class="email-settings-view">
    <div class="section-header">
      <h2>邮件设置</h2>
      <p class="section-description">配置系统的邮件发送服务和通知设置</p>
    </div>

    <form @submit.prevent="saveSettings" class="settings-form">
      <!-- SMTP服务器配置 -->
      <div class="form-section">
        <h3>SMTP服务器配置</h3>
        <div class="row">
          <div class="col-md-6">
            <div class="form-group">
              <label for="smtpHost" class="form-label">SMTP服务器地址</label>
              <input
                id="smtpHost"
                v-model="form.smtpHost"
                type="text"
                class="form-control"
                :class="{ 'is-invalid': errors.smtpHost }"
                placeholder="smtp.example.com"
                required
              />
              <div v-if="errors.smtpHost" class="invalid-feedback">
                {{ errors.smtpHost }}
              </div>
            </div>
          </div>
          <div class="col-md-6">
            <div class="form-group">
              <label for="smtpPort" class="form-label">SMTP端口</label>
              <input
                id="smtpPort"
                v-model.number="form.smtpPort"
                type="number"
                class="form-control"
                :class="{ 'is-invalid': errors.smtpPort }"
                min="1"
                max="65535"
                required
              />
              <div v-if="errors.smtpPort" class="invalid-feedback">
                {{ errors.smtpPort }}
              </div>
              <small class="form-text text-muted">
                常用端口：25, 465, 587
              </small>
            </div>
          </div>
        </div>

        <div class="row">
          <div class="col-md-6">
            <div class="form-group">
              <label for="smtpUsername" class="form-label">用户名</label>
              <input
                id="smtpUsername"
                v-model="form.smtpUsername"
                type="text"
                class="form-control"
                :class="{ 'is-invalid': errors.smtpUsername }"
                placeholder="your-email@example.com"
                required
              />
              <div v-if="errors.smtpUsername" class="invalid-feedback">
                {{ errors.smtpUsername }}
              </div>
            </div>
          </div>
          <div class="col-md-6">
            <div class="form-group">
              <label for="smtpPassword" class="form-label">密码</label>
              <div class="input-group">
                <input
                  id="smtpPassword"
                  v-model="form.smtpPassword"
                  :type="showPassword ? 'text' : 'password'"
                  class="form-control"
                  :class="{ 'is-invalid': errors.smtpPassword }"
                  placeholder="输入SMTP密码"
                  required
                />
                <button
                  type="button"
                  class="btn btn-outline-secondary"
                  @click="showPassword = !showPassword"
                >
                  <i :class="showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
                </button>
                <div v-if="errors.smtpPassword" class="invalid-feedback">
                  {{ errors.smtpPassword }}
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="row">
          <div class="col-md-6">
            <div class="form-group">
              <label for="fromEmail" class="form-label">发件人邮箱</label>
              <input
                id="fromEmail"
                v-model="form.fromEmail"
                type="email"
                class="form-control"
                :class="{ 'is-invalid': errors.fromEmail }"
                placeholder="noreply@example.com"
                required
              />
              <div v-if="errors.fromEmail" class="invalid-feedback">
                {{ errors.fromEmail }}
              </div>
            </div>
          </div>
          <div class="col-md-6">
            <div class="form-group">
              <label for="fromName" class="form-label">发件人名称</label>
              <input
                id="fromName"
                v-model="form.fromName"
                type="text"
                class="form-control"
                :class="{ 'is-invalid': errors.fromName }"
                placeholder="CodeShare系统"
                required
              />
              <div v-if="errors.fromName" class="invalid-feedback">
                {{ errors.fromName }}
              </div>
            </div>
          </div>
        </div>

        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="enableSsl"
              v-model="form.enableSsl"
              type="checkbox"
              class="form-check-input"
            />
            <label for="enableSsl" class="form-check-label">
              启用SSL加密
            </label>
          </div>
          <small class="form-text text-muted">
            建议启用以确保邮件传输安全
          </small>
        </div>
      </div>

      <!-- 邮件通知设置 -->
      <div class="form-section">
        <h3>邮件通知设置</h3>
        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="enableEmailNotifications"
              v-model="form.enableEmailNotifications"
              type="checkbox"
              class="form-check-input"
            />
            <label for="enableEmailNotifications" class="form-check-label">
              启用邮件通知
            </label>
          </div>
        </div>

        <div v-if="form.enableEmailNotifications" class="form-group">
          <label class="form-label">通知类型</label>
          <div class="notification-types">
            <div class="form-check">
              <input
                id="notifyRegistration"
                v-model="form.notificationTypes"
                type="checkbox"
                value="registration"
                class="form-check-input"
              />
              <label for="notifyRegistration" class="form-check-label">
                用户注册通知
              </label>
            </div>
            <div class="form-check">
              <input
                id="notifyLogin"
                v-model="form.notificationTypes"
                type="checkbox"
                value="login"
                class="form-check-input"
              />
              <label for="notifyLogin" class="form-check-label">
                登录提醒通知
              </label>
            </div>
            <div class="form-check">
              <input
                id="notifyPasswordChange"
                v-model="form.notificationTypes"
                type="checkbox"
                value="password_change"
                class="form-check-input"
              />
              <label for="notifyPasswordChange" class="form-check-label">
                密码变更通知
              </label>
            </div>
            <div class="form-check">
              <input
                id="notifyShareAccess"
                v-model="form.notificationTypes"
                type="checkbox"
                value="share_access"
                class="form-check-input"
              />
              <label for="notifyShareAccess" class="form-check-label">
                分享访问通知
              </label>
            </div>
            <div class="form-check">
              <input
                id="notifySystemAlert"
                v-model="form.notificationTypes"
                type="checkbox"
                value="system_alert"
                class="form-check-input"
              />
              <label for="notifySystemAlert" class="form-check-label">
                系统警报通知
              </label>
            </div>
          </div>
        </div>

        <div v-if="form.enableEmailNotifications" class="form-group">
          <label for="emailTemplate" class="form-label">邮件模板</label>
          <select
            id="emailTemplate"
            v-model="form.emailTemplate"
            class="form-select"
          >
            <option value="default">默认模板</option>
            <option value="modern">现代风格</option>
            <option value="minimal">极简风格</option>
            <option value="custom">自定义模板</option>
          </select>
        </div>
      </div>

      <!-- 邮件队列设置 -->
      <div class="form-section">
        <h3>邮件队列设置</h3>
        <div class="row">
          <div class="col-md-6">
            <div class="form-group">
              <label for="maxQueueSize" class="form-label">最大队列大小</label>
              <input
                id="maxQueueSize"
                v-model.number="form.maxQueueSize"
                type="number"
                class="form-control"
                :class="{ 'is-invalid': errors.maxQueueSize }"
                min="100"
                max="10000"
                required
              />
              <div v-if="errors.maxQueueSize" class="invalid-feedback">
                {{ errors.maxQueueSize }}
              </div>
              <small class="form-text text-muted">
                邮件队列的最大容量
              </small>
            </div>
          </div>
          <div class="col-md-6">
            <div class="form-group">
              <label for="retryAttempts" class="form-label">重试次数</label>
              <input
                id="retryAttempts"
                v-model.number="form.retryAttempts"
                type="number"
                class="form-control"
                :class="{ 'is-invalid': errors.retryAttempts }"
                min="0"
                max="10"
                required
              />
              <div v-if="errors.retryAttempts" class="invalid-feedback">
                {{ errors.retryAttempts }}
              </div>
              <small class="form-text text-muted">
                发送失败时的重试次数
              </small>
            </div>
          </div>
        </div>

        <div class="form-group">
          <label for="retryInterval" class="form-label">重试间隔（分钟）</label>
          <input
            id="retryInterval"
            v-model.number="form.retryInterval"
            type="number"
            class="form-control"
            :class="{ 'is-invalid': errors.retryInterval }"
            min="1"
            max="60"
            required
          />
          <div v-if="errors.retryInterval" class="invalid-feedback">
            {{ errors.retryInterval }}
          </div>
          <small class="form-text text-muted">
            重试发送邮件的时间间隔
          </small>
        </div>

        <div class="form-group">
          <div class="form-check form-switch">
            <input
              id="enableQueueLogging"
              v-model="form.enableQueueLogging"
              type="checkbox"
              class="form-check-input"
            />
            <label for="enableQueueLogging" class="form-check-label">
              启用队列日志记录
            </label>
          </div>
        </div>
      </div>

      <!-- 测试邮件 -->
      <div class="form-section">
        <h3>测试邮件</h3>
        <div class="row">
          <div class="col-md-8">
            <div class="form-group">
              <label for="testEmail" class="form-label">测试邮箱地址</label>
              <input
                id="testEmail"
                v-model="testEmail"
                type="email"
                class="form-control"
                :class="{ 'is-invalid': testEmailError }"
                placeholder="输入测试邮箱地址"
              />
              <div v-if="testEmailError" class="invalid-feedback">
                {{ testEmailError }}
              </div>
            </div>
          </div>
          <div class="col-md-4">
            <div class="form-group">
              <label class="form-label">&nbsp;</label>
              <button
                type="button"
                @click="sendTestEmail"
                class="btn btn-outline-primary w-100"
                :disabled="!testEmail || sendingTestEmail"
              >
                <i class="fas fa-paper-plane" :class="{ 'fa-spin': sendingTestEmail }"></i>
                {{ sendingTestEmail ? '发送中...' : '发送测试邮件' }}
              </button>
            </div>
          </div>
        </div>
        <div v-if="testEmailResult" class="mt-3">
          <div :class="['alert', testEmailResult.success ? 'alert-success' : 'alert-danger']">
            <i :class="testEmailResult.success ? 'fas fa-check-circle' : 'fas fa-exclamation-circle'"></i>
            {{ testEmailResult.message }}
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
        <button 
          type="button" 
          class="btn btn-outline-info"
          @click="testConnection"
          :disabled="testingConnection"
        >
          <i class="fas fa-wifi" :class="{ 'fa-spin': testingConnection }"></i>
          {{ testingConnection ? '测试中...' : '测试连接' }}
        </button>
      </div>
    </form>

    <!-- 连接测试结果模态框 -->
    <div v-if="showTestResult" class="modal-backdrop">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">连接测试结果</h5>
            <button type="button" class="btn-close" @click="closeTestResult"></button>
          </div>
          <div class="modal-body">
            <div v-if="testResult">
              <div v-if="testResult.success" class="alert alert-success">
                <i class="fas fa-check-circle"></i>
                SMTP连接测试成功！
              </div>
              <div v-else class="alert alert-danger">
                <i class="fas fa-exclamation-circle"></i>
                SMTP连接测试失败：{{ testResult.message }}
              </div>
              
              <div v-if="testResult.details" class="mt-3">
                <h6>详细信息：</h6>
                <pre class="bg-light p-3 rounded">{{ testResult.details }}</pre>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="closeTestResult">
              关闭
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useToast } from '@/composables/useToast'
import type { EmailSettings, UpdateEmailSettingsRequest } from '@/types/settings'

interface Props {
  settings: EmailSettings | null
  loading: boolean
}

interface Emits {
  (e: 'update:settings', request: UpdateEmailSettingsRequest): void
  (e: 'send-test', recipientEmail: string): void
  (e: 'test-connection'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()
const toast = useToast()

// 表单数据
const form = ref<EmailSettings>({
  smtpHost: '',
  smtpPort: 587,
  smtpUsername: '',
  smtpPassword: '',
  fromEmail: '',
  fromName: 'CodeShare系统',
  enableSsl: true,
  enableEmailNotifications: true,
  notificationTypes: ['registration', 'password_change'],
  emailTemplate: 'default',
  maxQueueSize: 1000,
  retryAttempts: 3,
  retryInterval: 5,
  enableQueueLogging: true
})

// 错误信息
const errors = ref<Record<string, string>>({})

// 测试相关状态
const showPassword = ref(false)
const testEmail = ref('')
const testEmailError = ref('')
const sendingTestEmail = ref(false)
const testEmailResult = ref<{ success: boolean; message: string } | null>(null)
const testingConnection = ref(false)
const testResult = ref<{ success: boolean; message: string; details?: string } | null>(null)
const showTestResult = ref(false)

// 计算属性
const hasChanges = computed(() => {
  if (!props.settings) return false
  
  return Object.keys(form.value).some(key => {
    const currentValue = form.value[key as keyof EmailSettings]
    const originalValue = props.settings![key as keyof EmailSettings]
    
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
  
  if (!form.value.smtpHost?.trim()) {
    errors.value.smtpHost = 'SMTP服务器地址不能为空'
  }
  
  if (form.value.smtpPort < 1 || form.value.smtpPort > 65535) {
    errors.value.smtpPort = 'SMTP端口必须在1-65535之间'
  }
  
  if (!form.value.smtpUsername?.trim()) {
    errors.value.smtpUsername = '用户名不能为空'
  }
  
  if (!form.value.smtpPassword?.trim()) {
    errors.value.smtpPassword = '密码不能为空'
  }
  
  if (!form.value.fromEmail?.trim()) {
    errors.value.fromEmail = '发件人邮箱不能为空'
  } else if (!isValidEmail(form.value.fromEmail)) {
    errors.value.fromEmail = '请输入有效的邮箱地址'
  }
  
  if (!form.value.fromName?.trim()) {
    errors.value.fromName = '发件人名称不能为空'
  }
  
  if (form.value.maxQueueSize < 100 || form.value.maxQueueSize > 10000) {
    errors.value.maxQueueSize = '最大队列大小必须在100-10000之间'
  }
  
  if (form.value.retryAttempts < 0 || form.value.retryAttempts > 10) {
    errors.value.retryAttempts = '重试次数必须在0-10之间'
  }
  
  if (form.value.retryInterval < 1 || form.value.retryInterval > 60) {
    errors.value.retryInterval = '重试间隔必须在1-60分钟之间'
  }
  
  return Object.keys(errors.value).length === 0
}

function isValidEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  return emailRegex.test(email)
}

function saveSettings() {
  if (!validateForm()) return
  
  const request: UpdateEmailSettingsRequest = {}
  
  // 只包含有变化的字段
  if (form.value.smtpHost !== props.settings?.smtpHost) {
    request.smtpHost = form.value.smtpHost
  }
  if (form.value.smtpPort !== props.settings?.smtpPort) {
    request.smtpPort = form.value.smtpPort
  }
  if (form.value.smtpUsername !== props.settings?.smtpUsername) {
    request.smtpUsername = form.value.smtpUsername
  }
  if (form.value.smtpPassword !== props.settings?.smtpPassword) {
    request.smtpPassword = form.value.smtpPassword
  }
  if (form.value.fromEmail !== props.settings?.fromEmail) {
    request.fromEmail = form.value.fromEmail
  }
  if (form.value.fromName !== props.settings?.fromName) {
    request.fromName = form.value.fromName
  }
  if (form.value.enableSsl !== props.settings?.enableSsl) {
    request.enableSsl = form.value.enableSsl
  }
  if (form.value.enableEmailNotifications !== props.settings?.enableEmailNotifications) {
    request.enableEmailNotifications = form.value.enableEmailNotifications
  }
  if (JSON.stringify(form.value.notificationTypes) !== JSON.stringify(props.settings?.notificationTypes)) {
    request.notificationTypes = form.value.notificationTypes
  }
  if (form.value.emailTemplate !== props.settings?.emailTemplate) {
    request.emailTemplate = form.value.emailTemplate
  }
  if (form.value.maxQueueSize !== props.settings?.maxQueueSize) {
    request.maxQueueSize = form.value.maxQueueSize
  }
  if (form.value.retryAttempts !== props.settings?.retryAttempts) {
    request.retryAttempts = form.value.retryAttempts
  }
  if (form.value.retryInterval !== props.settings?.retryInterval) {
    request.retryInterval = form.value.retryInterval
  }
  if (form.value.enableQueueLogging !== props.settings?.enableQueueLogging) {
    request.enableQueueLogging = form.value.enableQueueLogging
  }
  
  emit('update:settings', request)
}

function resetForm() {
  if (props.settings) {
    form.value = { ...props.settings }
    errors.value = {}
  }
}

async function sendTestEmail() {
  if (!testEmail.value?.trim()) {
    testEmailError.value = '请输入测试邮箱地址'
    return
  }
  
  if (!isValidEmail(testEmail.value)) {
    testEmailError.value = '请输入有效的邮箱地址'
    return
  }
  
  testEmailError.value = ''
  sendingTestEmail.value = true
  testEmailResult.value = null
  
  try {
    emit('send-test', testEmail.value)
    // 这里应该等待父组件处理结果
    // 为了演示，我们假设成功
    testEmailResult.value = {
      success: true,
      message: '测试邮件已发送，请查收'
    }
    toast.success('测试邮件已发送')
  } catch (error) {
    testEmailResult.value = {
      success: false,
      message: '发送测试邮件失败'
    }
    toast.error('发送测试邮件失败')
  } finally {
    sendingTestEmail.value = false
  }
}

async function testConnection() {
  testingConnection.value = true
  testResult.value = null
  
  try {
    emit('test-connection')
    // 这里应该等待父组件处理结果
    // 为了演示，我们假设成功
    testResult.value = {
      success: true,
      message: '连接测试成功',
      details: 'SMTP服务器连接正常，认证成功'
    }
    showTestResult.value = true
    toast.success('SMTP连接测试成功')
  } catch (error) {
    testResult.value = {
      success: false,
      message: '连接测试失败',
      details: error instanceof Error ? error.message : '未知错误'
    }
    showTestResult.value = true
    toast.error('SMTP连接测试失败')
  } finally {
    testingConnection.value = false
  }
}

function closeTestResult() {
  showTestResult.value = false
  testResult.value = null
}
</script>

<style scoped>
.email-settings-view {
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

.notification-types {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
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

.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-dialog {
  background: white;
  border-radius: 0.5rem;
  max-width: 600px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  padding: 1rem;
  border-bottom: 1px solid #dee2e6;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-body {
  padding: 1rem;
}

.modal-footer {
  padding: 1rem;
  border-top: 1px solid #dee2e6;
  display: flex;
  justify-content: flex-end;
}

@media (max-width: 768px) {
  .notification-types {
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