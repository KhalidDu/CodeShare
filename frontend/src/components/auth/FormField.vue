<template>
  <div class="form-field" :class="{ error: hasError, focused: isFocused }">
    <label :for="id" class="field-label">
      <svg v-if="icon" class="label-icon" viewBox="0 0 24 24" fill="currentColor">
        <path :d="icon"/>
      </svg>
      {{ label }}
      <span v-if="required" class="required-mark">*</span>
    </label>

    <div class="field-input-wrapper">
      <input
        :id="id"
        :type="inputType"
        :value="modelValue"
        :placeholder="placeholder"
        :required="required"
        :disabled="disabled"
        :autocomplete="autocomplete"
        class="field-input"
        @input="handleInput"
        @focus="handleFocus"
        @blur="handleBlur"
      />

      <!-- 密码显示/隐藏切换按钮 -->
      <button
        v-if="type === 'password'"
        type="button"
        class="password-toggle"
        @click="togglePasswordVisibility"
        :title="showPassword ? '隐藏密码' : '显示密码'"
      >
        <svg class="toggle-icon" viewBox="0 0 24 24" fill="currentColor">
          <path v-if="showPassword" d="M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17M12,4.5C7,4.5 2.73,7.61 1,12C2.73,16.39 7,19.5 12,19.5C17,19.5 21.27,16.39 23,12C21.27,7.61 17,4.5 12,4.5Z"/>
          <path v-else d="M11.83,9L15,12.16C15,12.11 15,12.05 15,12A3,3 0 0,0 12,9C11.94,9 11.89,9 11.83,9M7.53,9.8L9.08,11.35C9.03,11.56 9,11.77 9,12A3,3 0 0,0 12,15C12.22,15 12.44,14.97 12.65,14.92L14.2,16.47C13.53,16.8 12.79,17 12,17A5,5 0 0,1 7,12C7,11.21 7.2,10.47 7.53,9.8M2,4.27L4.28,6.55L4.73,7C3.08,8.3 1.78,10 1,12C2.73,16.39 7,19.5 12,19.5C13.55,19.5 15.03,19.2 16.38,18.66L16.81,19.09L19.73,22L21,20.73L3.27,3M12,7A5,5 0 0,1 17,12C17,12.64 16.87,13.26 16.64,13.82L19.57,16.75C21.07,15.5 22.27,13.86 23,12C21.27,7.61 17,4.5 12,4.5C10.6,4.5 9.26,4.75 8,5.2L10.17,7.35C10.76,7.13 11.38,7 12,7Z"/>
        </svg>
      </button>
    </div>

    <!-- 错误消息 -->
    <div v-if="hasError" class="field-error">
      <svg class="error-icon" viewBox="0 0 24 24" fill="currentColor">
        <path d="M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z"/>
      </svg>
      {{ errorMessage }}
    </div>

    <!-- 帮助文本 -->
    <div v-if="helpText && !hasError" class="field-help">
      {{ helpText }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'

interface Props {
  id: string
  label: string
  type?: 'text' | 'email' | 'password'
  modelValue: string
  placeholder?: string
  required?: boolean
  disabled?: boolean
  autocomplete?: string
  icon?: string
  errorMessage?: string
  helpText?: string
}

interface Emits {
  (e: 'update:modelValue', value: string): void
}

const props = withDefaults(defineProps<Props>(), {
  type: 'text',
  required: false,
  disabled: false
})

const emit = defineEmits<Emits>()

// 响应式状态
const isFocused = ref(false)
const showPassword = ref(false)

// 计算属性
const hasError = computed(() => !!props.errorMessage)

const inputType = computed(() => {
  if (props.type === 'password') {
    return showPassword.value ? 'text' : 'password'
  }
  return props.type
})

/**
 * 处理输入事件
 */
function handleInput(event: Event) {
  const target = event.target as HTMLInputElement
  emit('update:modelValue', target.value)
}

/**
 * 处理焦点事件
 */
function handleFocus() {
  isFocused.value = true
}

/**
 * 处理失焦事件
 */
function handleBlur() {
  isFocused.value = false
}

/**
 * 切换密码显示状态
 */
function togglePasswordVisibility() {
  showPassword.value = !showPassword.value
}
</script>

<style scoped>
.form-field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

/* 标签样式 */
.field-label {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
  cursor: pointer;
}

.label-icon {
  width: 16px;
  height: 16px;
  color: #6b7280;
}

.required-mark {
  color: #ef4444;
  font-weight: 600;
}

/* 输入框包装器 */
.field-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

/* 输入框样式 */
.field-input {
  width: 100%;
  padding: 0.875rem 1rem;
  border: 2px solid #d1d5db;
  border-radius: 8px;
  font-size: 1rem;
  line-height: 1.5;
  color: #111827;
  background-color: #ffffff;
  transition: all 0.3s ease;
}

.field-input::placeholder {
  color: #9ca3af;
}

.field-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.field-input:disabled {
  background-color: #f9fafb;
  color: #6b7280;
  cursor: not-allowed;
}

/* 聚焦状态 */
.form-field.focused .field-label {
  color: #3b82f6;
}

.form-field.focused .label-icon {
  color: #3b82f6;
}

/* 错误状态 */
.form-field.error .field-input {
  border-color: #ef4444;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

.form-field.error .field-label {
  color: #ef4444;
}

.form-field.error .label-icon {
  color: #ef4444;
}

/* 密码切换按钮 */
.password-toggle {
  position: absolute;
  right: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #6b7280;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 4px;
  transition: color 0.3s ease;
}

.password-toggle:hover {
  color: #374151;
}

.password-toggle:focus {
  outline: none;
  color: #3b82f6;
}

.toggle-icon {
  width: 20px;
  height: 20px;
}

/* 为密码输入框留出空间 */
.form-field .field-input[type="password"],
.form-field .field-input[type="text"]:has(+ .password-toggle) {
  padding-right: 3rem;
}

/* 错误消息 */
.field-error {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  font-size: 0.8125rem;
  color: #ef4444;
  line-height: 1.4;
}

.error-icon {
  width: 14px;
  height: 14px;
  flex-shrink: 0;
}

/* 帮助文本 */
.field-help {
  font-size: 0.8125rem;
  color: #6b7280;
  line-height: 1.4;
}

/* 响应式设计 */
@media (max-width: 480px) {
  .field-input {
    padding: 0.75rem 0.875rem;
    font-size: 0.9375rem;
  }

  .password-toggle {
    right: 0.625rem;
  }

  .form-field .field-input[type="password"],
  .form-field .field-input[type="text"]:has(+ .password-toggle) {
    padding-right: 2.75rem;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .field-input {
    transition: none;
  }

  .password-toggle {
    transition: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .field-input {
    border-width: 2px;
  }

  .field-input:focus {
    border-width: 3px;
  }

  .form-field.error .field-input {
    border-width: 3px;
  }
}
</style>
