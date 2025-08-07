<template>
  <div class="auth-container">
    <div class="auth-card">
      <!-- 头部 -->
      <div class="auth-header">
        <div class="auth-logo">
          <svg class="logo-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M9.4 16.6L4.8 12l4.6-4.6L8 6l-6 6 6 6 1.4-1.4zm5.2 0L19.2 12l-4.6-4.6L16 6l6 6-6 6-1.4-1.4z"/>
          </svg>
          <span class="logo-text">代码片段管理</span>
        </div>
        <h1 class="auth-title">{{ title }}</h1>
        <p class="auth-subtitle" v-if="subtitle">{{ subtitle }}</p>
      </div>

      <!-- 表单内容 -->
      <div class="auth-body">
        <form @submit.prevent="handleSubmit" class="auth-form">
          <slot name="fields"></slot>

          <!-- 提交按钮 -->
          <button
            type="submit"
            class="auth-submit-btn"
            :disabled="isLoading || !isFormValid"
            :class="{ loading: isLoading }"
          >
            <svg v-if="isLoading" class="loading-spinner" viewBox="0 0 24 24">
              <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none" opacity="0.25"/>
              <path fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"/>
            </svg>
            <span>{{ isLoading ? loadingText : submitText }}</span>
          </button>
        </form>

        <!-- 底部链接 -->
        <div class="auth-footer" v-if="$slots.footer">
          <slot name="footer"></slot>
        </div>

        <!-- 错误消息 -->
        <div v-if="error" class="auth-message auth-error">
          <svg class="message-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z"/>
          </svg>
          {{ error }}
        </div>

        <!-- 成功消息 -->
        <div v-if="success" class="auth-message auth-success">
          <svg class="message-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M17,9L16.25,8.25L11,13.5L7.75,10.25L7,11L11,15L17,9Z"/>
          </svg>
          {{ success }}
        </div>
      </div>
    </div>

    <!-- 背景装饰 -->
    <div class="auth-background">
      <div class="bg-shape bg-shape-1"></div>
      <div class="bg-shape bg-shape-2"></div>
      <div class="bg-shape bg-shape-3"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  title: string
  subtitle?: string
  submitText: string
  loadingText: string
  isLoading?: boolean
  isFormValid?: boolean
  error?: string
  success?: string
}

interface Emits {
  (e: 'submit'): void
}

withDefaults(defineProps<Props>(), {
  isLoading: false,
  isFormValid: true
})

defineEmits<Emits>()

defineSlots<{
  fields(): any
  footer(): any
}>()

/**
 * 处理表单提交
 */
function handleSubmit() {
  // 由父组件处理具体的提交逻辑
}
</script>

<style scoped>
.auth-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  position: relative;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  overflow: hidden;
}

.auth-card {
  background: white;
  border-radius: 16px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 420px;
  position: relative;
  z-index: 10;
  overflow: hidden;
}

/* 头部样式 */
.auth-header {
  padding: 2rem 2rem 1rem 2rem;
  text-align: center;
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  border-bottom: 1px solid #dee2e6;
}

.auth-logo {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  margin-bottom: 1rem;
  color: #007bff;
}

.logo-icon {
  width: 32px;
  height: 32px;
}

.logo-text {
  font-size: 1.125rem;
  font-weight: 600;
}

.auth-title {
  font-size: 1.75rem;
  font-weight: 700;
  color: #212529;
  margin: 0 0 0.5rem 0;
}

.auth-subtitle {
  color: #6c757d;
  margin: 0;
  font-size: 0.875rem;
  line-height: 1.4;
}

/* 表单主体 */
.auth-body {
  padding: 2rem;
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

/* 提交按钮 */
.auth-submit-btn {
  width: 100%;
  padding: 0.875rem 1rem;
  background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  position: relative;
  overflow: hidden;
}

.auth-submit-btn:hover:not(:disabled) {
  background: linear-gradient(135deg, #0056b3 0%, #004085 100%);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 123, 255, 0.3);
}

.auth-submit-btn:active:not(:disabled) {
  transform: translateY(0);
}

.auth-submit-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.auth-submit-btn.loading {
  pointer-events: none;
}

.loading-spinner {
  width: 20px;
  height: 20px;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* 底部链接 */
.auth-footer {
  margin-top: 1.5rem;
  text-align: center;
  padding-top: 1.5rem;
  border-top: 1px solid #dee2e6;
}

/* 消息样式 */
.auth-message {
  margin-top: 1rem;
  padding: 0.875rem 1rem;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  line-height: 1.4;
}

.auth-error {
  background-color: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.auth-success {
  background-color: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.message-icon {
  width: 18px;
  height: 18px;
  flex-shrink: 0;
}

/* 背景装饰 */
.auth-background {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 1;
}

.bg-shape {
  position: absolute;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.1);
  animation: float 6s ease-in-out infinite;
}

.bg-shape-1 {
  width: 200px;
  height: 200px;
  top: 10%;
  left: -5%;
  animation-delay: 0s;
}

.bg-shape-2 {
  width: 150px;
  height: 150px;
  top: 60%;
  right: -5%;
  animation-delay: 2s;
}

.bg-shape-3 {
  width: 100px;
  height: 100px;
  bottom: 20%;
  left: 20%;
  animation-delay: 4s;
}

@keyframes float {
  0%, 100% { transform: translateY(0px) rotate(0deg); }
  50% { transform: translateY(-20px) rotate(180deg); }
}

/* 响应式设计 */
@media (max-width: 480px) {
  .auth-container {
    padding: 0.5rem;
  }

  .auth-card {
    border-radius: 12px;
  }

  .auth-header {
    padding: 1.5rem 1.5rem 1rem 1.5rem;
  }

  .auth-body {
    padding: 1.5rem;
  }

  .auth-title {
    font-size: 1.5rem;
  }

  .logo-text {
    display: none;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .auth-submit-btn {
    transition: none;
  }

  .bg-shape {
    animation: none;
  }

  .loading-spinner {
    animation: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .auth-card {
    border: 2px solid #000;
  }

  .auth-header {
    border-bottom-width: 2px;
  }

  .auth-submit-btn {
    border: 2px solid #000;
  }
}
</style>
