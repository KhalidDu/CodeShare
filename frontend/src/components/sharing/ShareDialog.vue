<template>
  <div class="dialog-overlay" @click="$emit('cancel')">
    <div class="dialog share-dialog" @click.stop>
      <div class="dialog-header">
        <div class="dialog-icon">
          <i class="icon-share"></i>
        </div>
        <h2>分享代码片段</h2>
        <button class="dialog-close" @click="$emit('cancel')">
          <i class="icon-x"></i>
        </button>
      </div>

      <div class="dialog-content">
        <!-- 分享设置表单 -->
        <div v-if="!shareToken" class="share-form">
          <div class="form-group">
            <label class="form-label">分享权限</label>
            <select v-model="form.permission" class="form-select">
              <option value="VIEW">仅查看</option>
              <option value="EDIT">可编辑</option>
            </select>
            <small class="form-hint">设置访问者对此代码片段的权限</small>
          </div>

          <div class="form-group">
            <label class="form-label">有效期</label>
            <select v-model="form.expiryOption" class="form-select" @change="handleExpiryChange">
              <option value="never">永不过期</option>
              <option value="1h">1小时</option>
              <option value="24h">24小时</option>
              <option value="7d">7天</option>
              <option value="30d">30天</option>
              <option value="custom">自定义</option>
            </select>
          </div>

          <div v-if="form.expiryOption === 'custom'" class="form-group">
            <label class="form-label">自定义过期时间</label>
            <input
              v-model="form.expiresAt"
              type="datetime-local"
              class="form-input"
              :min="minDateTime"
            />
          </div>

          <div class="form-group">
            <label class="form-label">访问限制</label>
            <div class="checkbox-group">
              <label class="checkbox-label">
                <input
                  v-model="form.hasPassword"
                  type="checkbox"
                  @change="handlePasswordChange"
                />
                <span>密码保护</span>
              </label>
              <label class="checkbox-label">
                <input
                  v-model="form.enableAccessLimit"
                  type="checkbox"
                  @change="handleAccessLimitChange"
                />
                <span>限制访问次数</span>
              </label>
              <label class="checkbox-label">
                <input
                  v-model="form.allowDownload"
                  type="checkbox"
                />
                <span>允许下载</span>
              </label>
            </div>
          </div>

          <div v-if="form.hasPassword" class="form-group">
            <label class="form-label">访问密码</label>
            <div class="password-input-group">
              <input
                v-model="form.password"
                :type="showPassword ? 'text' : 'password'"
                class="form-input"
                placeholder="输入访问密码"
              />
              <button
                type="button"
                class="password-toggle"
                @click="showPassword = !showPassword"
              >
                <i :class="showPassword ? 'icon-eye-off' : 'icon-eye'"></i>
              </button>
            </div>
            <small class="form-hint">访问者需要输入密码才能查看代码</small>
          </div>

          <div v-if="form.enableAccessLimit" class="form-group">
            <label class="form-label">最大访问次数</label>
            <input
              v-model.number="form.maxAccessCount"
              type="number"
              class="form-input"
              min="1"
              max="999"
              placeholder="输入最大访问次数"
            />
            <small class="form-hint">达到此次数后分享链接将失效</small>
          </div>

          <!-- 错误提示 -->
          <div v-if="error" class="error-message">
            <i class="icon-alert-circle"></i>
            {{ error }}
          </div>
        </div>

        <!-- 分享结果展示 -->
        <div v-else class="share-result">
          <div class="success-message">
            <i class="icon-check-circle"></i>
            <span>分享链接已创建成功！</span>
          </div>

          <div class="share-link-container">
            <div class="share-link-input">
              <input
                :value="shareLink"
                type="text"
                readonly
                class="form-input"
                @click="copyShareLink"
              />
              <button
                type="button"
                class="btn btn-primary"
                @click="copyShareLink"
              >
                <i class="icon-copy"></i>
                复制链接
              </button>
            </div>
            <small v-if="copied" class="success-text">链接已复制到剪贴板</small>
          </div>

          <!-- 二维码显示 -->
          <div class="qr-section">
            <div class="qr-code-container">
              <canvas ref="qrCanvas" class="qr-code"></canvas>
            </div>
            <button
              type="button"
              class="btn btn-secondary"
              @click="downloadQRCode"
            >
              <i class="icon-download"></i>
              下载二维码
            </button>
          </div>

          <!-- 分享选项 -->
          <div class="share-options">
            <h3>分享到</h3>
            <div class="share-buttons">
              <button
                type="button"
                class="share-option-btn"
                @click="shareToSocial('twitter')"
              >
                <i class="icon-twitter"></i>
                Twitter
              </button>
              <button
                type="button"
                class="share-option-btn"
                @click="shareToSocial('facebook')"
              >
                <i class="icon-facebook"></i>
                Facebook
              </button>
              <button
                type="button"
                class="share-option-btn"
                @click="shareToSocial('linkedin')"
              >
                <i class="icon-linkedin"></i>
                LinkedIn
              </button>
              <button
                type="button"
                class="share-option-btn"
                @click="shareToSocial('email')"
              >
                <i class="icon-mail"></i>
                邮件
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="dialog-actions">
        <button
          v-if="!shareToken"
          type="button"
          class="btn btn-secondary"
          @click="$emit('cancel')"
        >
          取消
        </button>
        <button
          v-if="!shareToken"
          type="button"
          class="btn btn-primary"
          :disabled="isSubmitting"
          @click="handleCreateShare"
        >
          <i v-if="isSubmitting" class="icon-loader loading"></i>
          {{ isSubmitting ? '创建中...' : '创建分享链接' }}
        </button>
        <button
          v-else
          type="button"
          class="btn btn-primary"
          @click="$emit('cancel')"
        >
          完成
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick, watch } from 'vue'
import { shareService } from '@/services/shareService'
import type { CreateShareTokenRequest, ShareToken } from '@/types'
import { SharePermission } from '@/types/share'

interface Props {
  /** 代码片段ID */
  snippetId: string
  /** 代码片段标题 */
  snippetTitle?: string
  /** 默认分享权限 */
  defaultPermission?: SharePermission
  /** 是否显示对话框 */
  visible?: boolean
}

interface Emits {
  (e: 'cancel'): void
  (e: 'share-created', shareToken: ShareToken): void
}

const props = withDefaults(defineProps<Props>(), {
  defaultPermission: 'VIEW' as SharePermission,
  visible: false
})

const emit = defineEmits<Emits>()

// 响应式状态
const isSubmitting = ref(false)
const error = ref('')
const shareToken = ref<ShareToken | null>(null)
const copied = ref(false)
const showPassword = ref(false)
const qrCanvas = ref<HTMLCanvasElement | null>(null)

// 表单数据
const form = ref({
  permission: props.defaultPermission,
  expiryOption: 'never',
  expiresAt: '',
  hasPassword: false,
  password: '',
  enableAccessLimit: false,
  maxAccessCount: 10,
  allowDownload: true
})

// 计算属性
const minDateTime = computed(() => {
  const now = new Date()
  now.setHours(now.getHours() + 1)
  return now.toISOString().slice(0, 16)
})

const shareLink = computed(() => {
  if (!shareToken.value) return ''
  return shareService.getShareLink(shareToken.value.token)
})

// 方法
const handleExpiryChange = () => {
  if (form.value.expiryOption === 'custom') {
    // 设置默认过期时间为24小时后
    const expiresAt = new Date()
    expiresAt.setHours(expiresAt.getHours() + 24)
    form.value.expiresAt = expiresAt.toISOString().slice(0, 16)
  } else {
    form.value.expiresAt = ''
  }
}

const handlePasswordChange = () => {
  if (!form.value.hasPassword) {
    form.value.password = ''
  }
}

const handleAccessLimitChange = () => {
  if (!form.value.enableAccessLimit) {
    form.value.maxAccessCount = 10
  }
}

const validateForm = (): string | null => {
  if (!props.snippetId) {
    return '代码片段ID不能为空'
  }

  if (form.value.expiryOption === 'custom' && !form.value.expiresAt) {
    return '请设置过期时间'
  }

  if (form.value.hasPassword && !form.value.password) {
    return '请设置访问密码'
  }

  if (form.value.enableAccessLimit && form.value.maxAccessCount <= 0) {
    return '最大访问次数必须大于0'
  }

  return null
}

const handleCreateShare = async () => {
  // 表单验证
  const validationError = validateForm()
  if (validationError) {
    error.value = validationError
    return
  }

  isSubmitting.value = true
  error.value = ''

  try {
    // 构建过期时间
    let expiresAt: string | undefined
    if (form.value.expiryOption === 'custom') {
      expiresAt = new Date(form.value.expiresAt).toISOString()
    } else if (form.value.expiryOption !== 'never') {
      const expiresAtDate = new Date()
      switch (form.value.expiryOption) {
        case '1h':
          expiresAtDate.setHours(expiresAtDate.getHours() + 1)
          break
        case '24h':
          expiresAtDate.setHours(expiresAtDate.getHours() + 24)
          break
        case '7d':
          expiresAtDate.setDate(expiresAtDate.getDate() + 7)
          break
        case '30d':
          expiresAtDate.setDate(expiresAtDate.getDate() + 30)
          break
      }
      expiresAt = expiresAtDate.toISOString()
    }

    // 构建分享请求
    const shareRequest: CreateShareTokenRequest = {
      snippetId: props.snippetId,
      permission: form.value.permission,
      expiresAt,
      password: form.value.hasPassword ? form.value.password : undefined,
      maxAccessCount: form.value.enableAccessLimit ? form.value.maxAccessCount : undefined,
      allowDownload: form.value.allowDownload
    }

    // 检查用户是否可以创建分享令牌
    const canCreate = await shareService.canCreateShareToken(props.snippetId)
    if (!canCreate.canCreate) {
      throw new Error(canCreate.reason || '无法创建分享链接')
    }

    // 创建分享令牌
    const createdToken = await shareService.createShareToken(shareRequest)
    shareToken.value = createdToken

    // 生成二维码
    await nextTick()
    await generateQRCode()

    // 触发成功事件
    emit('share-created', createdToken)

  } catch (err) {
    console.error('创建分享失败:', err)
    error.value = err instanceof Error ? err.message : '创建分享失败，请稍后重试'
  } finally {
    isSubmitting.value = false
  }
}

const generateQRCode = async () => {
  if (!qrCanvas.value || !shareLink.value) return

  try {
    // 动态导入QRCode库
    const QRCode = (await import('qrcode')).default
    await QRCode.toCanvas(qrCanvas.value, shareLink.value, {
      width: 200,
      margin: 2,
      color: {
        dark: '#000000',
        light: '#FFFFFF'
      }
    })
  } catch (err) {
    console.error('生成二维码失败:', err)
  }
}

const copyShareLink = async () => {
  if (!shareLink.value) return

  try {
    if (navigator.clipboard && window.isSecureContext) {
      await navigator.clipboard.writeText(shareLink.value)
    } else {
      // 降级处理
      const textArea = document.createElement('textarea')
      textArea.value = shareLink.value
      textArea.style.position = 'fixed'
      textArea.style.left = '-999999px'
      textArea.style.top = '-999999px'
      document.body.appendChild(textArea)
      textArea.focus()
      textArea.select()
      document.execCommand('copy')
      document.body.removeChild(textArea)
    }

    copied.value = true
    setTimeout(() => {
      copied.value = false
    }, 2000)
  } catch (err) {
    console.error('复制链接失败:', err)
  }
}

const downloadQRCode = () => {
  if (!qrCanvas.value) return

  try {
    const link = document.createElement('a')
    link.download = `qrcode-${shareToken.value?.token || 'share'}.png`
    link.href = qrCanvas.value.toDataURL()
    link.click()
  } catch (err) {
    console.error('下载二维码失败:', err)
  }
}

const shareToSocial = (platform: string) => {
  if (!shareLink.value || !props.snippetTitle) return

  const text = `查看我分享的代码片段: ${props.snippetTitle}`
  const encodedText = encodeURIComponent(text)
  const encodedUrl = encodeURIComponent(shareLink.value)

  let shareUrl = ''
  switch (platform) {
    case 'twitter':
      shareUrl = `https://twitter.com/intent/tweet?text=${encodedText}&url=${encodedUrl}`
      break
    case 'facebook':
      shareUrl = `https://www.facebook.com/sharer/sharer.php?u=${encodedUrl}`
      break
    case 'linkedin':
      shareUrl = `https://www.linkedin.com/sharing/share-offsite/?url=${encodedUrl}`
      break
    case 'email':
      shareUrl = `mailto:?subject=${encodeURIComponent('查看代码片段')}&body=${encodedText}%0A%0A${encodedUrl}`
      break
  }

  if (shareUrl) {
    window.open(shareUrl, '_blank', 'width=600,height=400')
  }
}

// 重置表单
const resetForm = () => {
  form.value = {
    permission: props.defaultPermission,
    expiryOption: 'never',
    expiresAt: '',
    hasPassword: false,
    password: '',
    enableAccessLimit: false,
    maxAccessCount: 10,
    allowDownload: true
  }
  error.value = ''
  shareToken.value = null
  copied.value = false
  showPassword.value = false
}

// 监听visible变化
watch(() => props.visible, (newValue) => {
  if (newValue) {
    resetForm()
  }
})

// 组件挂载时初始化
onMounted(() => {
  if (props.visible) {
    resetForm()
  }
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

.share-dialog {
  background: white;
  border-radius: 12px;
  width: 100%;
  max-width: 500px;
  max-height: 90vh;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.dialog-header {
  display: flex;
  align-items: center;
  padding: 24px 24px 16px 24px;
  border-bottom: 1px solid #e5e7eb;
}

.dialog-icon {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: #dbeafe;
  color: #3b82f6;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 16px;
  flex-shrink: 0;
}

.dialog-header h2 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #1a1a1a;
  flex: 1;
}

.dialog-close {
  background: none;
  border: none;
  font-size: 20px;
  color: #6b7280;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  transition: all 0.2s;
}

.dialog-close:hover {
  background: #f3f4f6;
  color: #374151;
}

.dialog-content {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
}

.dialog-actions {
  display: flex;
  gap: 12px;
  padding: 16px 24px 24px 24px;
  border-top: 1px solid #e5e7eb;
  justify-content: flex-end;
}

/* 表单样式 */
.form-group {
  margin-bottom: 20px;
}

.form-label {
  display: block;
  font-weight: 500;
  color: #374151;
  margin-bottom: 6px;
}

.form-input,
.form-select {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  transition: border-color 0.2s;
}

.form-input:focus,
.form-select:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-hint {
  font-size: 12px;
  color: #6b7280;
  margin-top: 4px;
}

.checkbox-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.checkbox-label {
  display: flex;
  align-items: center;
  cursor: pointer;
  font-size: 14px;
  color: #374151;
}

.checkbox-label input[type="checkbox"] {
  margin-right: 8px;
  width: 16px;
  height: 16px;
  accent-color: #3b82f6;
}

.password-input-group {
  position: relative;
}

.password-toggle {
  position: absolute;
  right: 12px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #6b7280;
  cursor: pointer;
  padding: 4px;
}

.password-toggle:hover {
  color: #374151;
}

/* 错误和成功消息 */
.error-message {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 6px;
  color: #dc2626;
  font-size: 14px;
  margin-top: 16px;
}

.success-message {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px;
  background: #dcfce7;
  border: 1px solid #bbf7d0;
  border-radius: 6px;
  color: #16a34a;
  font-size: 14px;
  margin-bottom: 20px;
}

.success-text {
  color: #16a34a;
  font-size: 12px;
  margin-top: 4px;
}

/* 分享结果样式 */
.share-link-container {
  margin-bottom: 24px;
}

.share-link-input {
  display: flex;
  gap: 8px;
  align-items: center;
}

.share-link-input .form-input {
  flex: 1;
  background: #f9fafb;
  font-family: monospace;
  font-size: 12px;
}

.qr-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  margin-bottom: 24px;
  padding: 20px;
  background: #f9fafb;
  border-radius: 8px;
}

.qr-code-container {
  display: flex;
  justify-content: center;
  align-items: center;
}

.qr-code {
  border: 1px solid #e5e7eb;
  border-radius: 8px;
}

.share-options {
  margin-top: 24px;
}

.share-options h3 {
  font-size: 16px;
  font-weight: 600;
  color: #374151;
  margin-bottom: 12px;
}

.share-buttons {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 8px;
}

.share-option-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 8px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  background: white;
  color: #374151;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
}

.share-option-btn:hover {
  background: #f3f4f6;
  border-color: #9ca3af;
}

/* 按钮样式 */
.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.btn-secondary {
  background: #f3f4f6;
  color: #374151;
  border: 1px solid #d1d5db;
}

.btn-secondary:hover {
  background: #e5e7eb;
}

.btn-primary {
  background: #3b82f6;
  color: white;
}

.btn-primary:hover {
  background: #2563eb;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.loading {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

/* 响应式设计 */
@media (max-width: 480px) {
  .share-dialog {
    margin: 10px;
    max-width: none;
  }

  .share-buttons {
    grid-template-columns: 1fr;
  }

  .dialog-actions {
    flex-direction: column;
  }

  .btn {
    width: 100%;
  }
}

/* 暗色主题支持 */
@media (prefers-color-scheme: dark) {
  .share-dialog {
    background: #1f2937;
    color: #f9fafb;
  }

  .dialog-header {
    border-bottom-color: #374151;
  }

  .dialog-header h2 {
    color: #f9fafb;
  }

  .dialog-close {
    color: #9ca3af;
  }

  .dialog-close:hover {
    background: #374151;
    color: #f9fafb;
  }

  .dialog-content {
    color: #f9fafb;
  }

  .dialog-actions {
    border-top-color: #374151;
  }

  .form-label {
    color: #f9fafb;
  }

  .form-input,
  .form-select {
    background: #374151;
    border-color: #4b5563;
    color: #f9fafb;
  }

  .form-input:focus,
  .form-select:focus {
    border-color: #60a5fa;
    box-shadow: 0 0 0 3px rgba(96, 165, 250, 0.1);
  }

  .form-hint {
    color: #9ca3af;
  }

  .checkbox-label {
    color: #f9fafb;
  }

  .password-toggle {
    color: #9ca3af;
  }

  .password-toggle:hover {
    color: #f9fafb;
  }

  .share-link-input .form-input {
    background: #374151;
    color: #f9fafb;
  }

  .qr-section {
    background: #374151;
  }

  .qr-code {
    border-color: #4b5563;
  }

  .share-options h3 {
    color: #f9fafb;
  }

  .share-option-btn {
    background: #374151;
    border-color: #4b5563;
    color: #f9fafb;
  }

  .share-option-btn:hover {
    background: #4b5563;
    border-color: #6b7280;
  }

  .btn-secondary {
    background: #374151;
    color: #f9fafb;
    border-color: #4b5563;
  }

  .btn-secondary:hover {
    background: #4b5563;
  }
}
</style>