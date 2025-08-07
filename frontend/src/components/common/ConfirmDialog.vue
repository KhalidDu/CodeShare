<template>
  <div class="dialog-overlay" @click="$emit('cancel')">
    <div class="dialog" @click.stop>
      <div class="dialog-header">
        <div class="dialog-icon" :class="iconClass">
          <i :class="iconName"></i>
        </div>
        <h2>{{ title }}</h2>
      </div>

      <div class="dialog-content">
        <p>{{ message }}</p>
      </div>

      <div class="dialog-actions">
        <button
          type="button"
          class="btn btn-secondary"
          @click="$emit('cancel')"
        >
          {{ cancelText }}
        </button>
        <button
          type="button"
          class="btn"
          :class="confirmButtonClass"
          @click="$emit('confirm')"
        >
          {{ confirmText }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

// Props
interface Props {
  title: string
  message: string
  confirmText?: string
  cancelText?: string
  confirmType?: 'primary' | 'danger' | 'warning' | 'success'
}

const props = withDefaults(defineProps<Props>(), {
  confirmText: '确认',
  cancelText: '取消',
  confirmType: 'primary'
})

// Emits
defineEmits<{
  confirm: []
  cancel: []
}>()

// 计算属性
/**
 * 图标样式类
 */
const iconClass = computed(() => {
  switch (props.confirmType) {
    case 'danger':
      return 'danger'
    case 'warning':
      return 'warning'
    case 'success':
      return 'success'
    default:
      return 'primary'
  }
})

/**
 * 图标名称
 */
const iconName = computed(() => {
  switch (props.confirmType) {
    case 'danger':
      return 'icon-alert-triangle'
    case 'warning':
      return 'icon-alert-circle'
    case 'success':
      return 'icon-check-circle'
    default:
      return 'icon-help-circle'
  }
})

/**
 * 确认按钮样式类
 */
const confirmButtonClass = computed(() => {
  switch (props.confirmType) {
    case 'danger':
      return 'btn-danger'
    case 'warning':
      return 'btn-warning'
    case 'success':
      return 'btn-success'
    default:
      return 'btn-primary'
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

.dialog {
  background: white;
  border-radius: 12px;
  width: 100%;
  max-width: 400px;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.dialog-header {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 24px 24px 16px 24px;
  text-align: center;
}

.dialog-icon {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  margin-bottom: 16px;
}

.dialog-icon.primary {
  background: #dbeafe;
  color: #3b82f6;
}

.dialog-icon.danger {
  background: #fef2f2;
  color: #dc2626;
}

.dialog-icon.warning {
  background: #fef3c7;
  color: #f59e0b;
}

.dialog-icon.success {
  background: #dcfce7;
  color: #10b981;
}

.dialog-header h2 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #1a1a1a;
}

.dialog-content {
  padding: 0 24px 24px 24px;
  text-align: center;
}

.dialog-content p {
  margin: 0;
  color: #666;
  line-height: 1.5;
}

.dialog-actions {
  display: flex;
  gap: 12px;
  padding: 16px 24px 24px 24px;
  justify-content: center;
}

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  min-width: 80px;
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

.btn-danger {
  background: #dc2626;
  color: white;
}

.btn-danger:hover {
  background: #b91c1c;
}

.btn-warning {
  background: #f59e0b;
  color: white;
}

.btn-warning:hover {
  background: #d97706;
}

.btn-success {
  background: #10b981;
  color: white;
}

.btn-success:hover {
  background: #059669;
}

/* 响应式设计 */
@media (max-width: 480px) {
  .dialog {
    margin: 10px;
    max-width: none;
  }

  .dialog-actions {
    flex-direction: column;
  }

  .btn {
    width: 100%;
  }
}
</style>