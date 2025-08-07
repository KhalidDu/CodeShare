<template>
  <div class="user-card" :class="{ inactive: !user.isActive }">
    <!-- 用户头像和基本信息 -->
    <div class="user-header">
      <div class="avatar">
        <i class="icon-user"></i>
      </div>
      <div class="user-info">
        <h3 class="username">{{ user.username }}</h3>
        <p class="email">{{ user.email }}</p>
      </div>
      <div class="status-badge" :class="statusClass">
        {{ statusText }}
      </div>
    </div>

    <!-- 用户详情 -->
    <div class="user-details">
      <div class="detail-item">
        <span class="label">角色:</span>
        <span class="value role" :class="roleClass">
          <i :class="roleIcon"></i>
          {{ roleText }}
        </span>
      </div>
      <div class="detail-item">
        <span class="label">创建时间:</span>
        <span class="value">{{ formatDate(user.createdAt) }}</span>
      </div>
      <div class="detail-item">
        <span class="label">用户ID:</span>
        <span class="value user-id">{{ user.id }}</span>
      </div>
    </div>

    <!-- 操作按钮 -->
    <div class="user-actions">
      <button
        class="action-btn edit"
        @click="$emit('edit', user)"
        title="编辑用户"
      >
        <i class="icon-edit"></i>
      </button>
      
      <button
        class="action-btn toggle"
        :class="{ active: user.isActive }"
        @click="$emit('toggle-status', user)"
        :title="user.isActive ? '禁用用户' : '启用用户'"
      >
        <i :class="user.isActive ? 'icon-user-x' : 'icon-user-check'"></i>
      </button>
      
      <button
        class="action-btn reset"
        @click="$emit('reset-password', user)"
        title="重置密码"
      >
        <i class="icon-key"></i>
      </button>
      
      <button
        class="action-btn delete"
        @click="$emit('delete', user)"
        :disabled="isCurrentUser"
        :title="isCurrentUser ? '不能删除自己' : '删除用户'"
      >
        <i class="icon-trash"></i>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { UserRole, type User } from '@/types'

// Props
interface Props {
  user: User
}

const props = defineProps<Props>()

// Emits
defineEmits<{
  edit: [user: User]
  delete: [user: User]
  'toggle-status': [user: User]
  'reset-password': [user: User]
}>()

// 状态管理
const authStore = useAuthStore()

// 计算属性
/**
 * 是否为当前用户
 */
const isCurrentUser = computed(() => {
  return authStore.user?.id === props.user.id
})

/**
 * 用户状态样式类
 */
const statusClass = computed(() => {
  return props.user.isActive ? 'active' : 'inactive'
})

/**
 * 用户状态文本
 */
const statusText = computed(() => {
  return props.user.isActive ? '活跃' : '禁用'
})

/**
 * 角色样式类
 */
const roleClass = computed(() => {
  switch (props.user.role) {
    case UserRole.Admin:
      return 'admin'
    case UserRole.Editor:
      return 'editor'
    case UserRole.Viewer:
      return 'viewer'
    default:
      return 'viewer'
  }
})

/**
 * 角色文本
 */
const roleText = computed(() => {
  switch (props.user.role) {
    case UserRole.Admin:
      return '管理员'
    case UserRole.Editor:
      return '编辑者'
    case UserRole.Viewer:
      return '查看者'
    default:
      return '查看者'
  }
})

/**
 * 角色图标
 */
const roleIcon = computed(() => {
  switch (props.user.role) {
    case UserRole.Admin:
      return 'icon-shield'
    case UserRole.Editor:
      return 'icon-edit'
    case UserRole.Viewer:
      return 'icon-eye'
    default:
      return 'icon-eye'
  }
})

// 方法
/**
 * 格式化日期
 */
function formatDate(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>

<style scoped>
.user-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border: 1px solid #e5e7eb;
  transition: all 0.2s;
  position: relative;
}

.user-card:hover {
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
  transform: translateY(-2px);
}

.user-card.inactive {
  opacity: 0.7;
  background: #f9fafb;
}

/* 用户头部 */
.user-header {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  margin-bottom: 16px;
}

.avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: linear-gradient(135deg, #3b82f6, #1d4ed8);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 20px;
  flex-shrink: 0;
}

.user-info {
  flex: 1;
  min-width: 0;
}

.username {
  margin: 0 0 4px 0;
  font-size: 16px;
  font-weight: 600;
  color: #1a1a1a;
  word-break: break-word;
}

.email {
  margin: 0;
  font-size: 14px;
  color: #666;
  word-break: break-word;
}

.status-badge {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.status-badge.active {
  background: #dcfce7;
  color: #166534;
}

.status-badge.inactive {
  background: #fef2f2;
  color: #dc2626;
}

/* 用户详情 */
.user-details {
  margin-bottom: 16px;
}

.detail-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
  font-size: 14px;
}

.detail-item:last-child {
  margin-bottom: 0;
}

.label {
  color: #666;
  font-weight: 500;
}

.value {
  color: #1a1a1a;
  font-weight: 400;
}

.value.role {
  display: flex;
  align-items: center;
  gap: 4px;
  font-weight: 500;
}

.role.admin {
  color: #8b5cf6;
}

.role.editor {
  color: #f59e0b;
}

.role.viewer {
  color: #6b7280;
}

.user-id {
  font-family: 'Courier New', monospace;
  font-size: 12px;
  color: #666;
  word-break: break-all;
}

/* 操作按钮 */
.user-actions {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
  padding-top: 16px;
  border-top: 1px solid #e5e7eb;
}

.action-btn {
  width: 36px;
  height: 36px;
  border: none;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 14px;
}

.action-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.action-btn.edit {
  background: #f3f4f6;
  color: #374151;
}

.action-btn.edit:hover:not(:disabled) {
  background: #e5e7eb;
  color: #1f2937;
}

.action-btn.toggle {
  background: #fef2f2;
  color: #dc2626;
}

.action-btn.toggle.active {
  background: #dcfce7;
  color: #166534;
}

.action-btn.toggle:hover:not(:disabled) {
  background: #fee2e2;
}

.action-btn.toggle.active:hover:not(:disabled) {
  background: #bbf7d0;
}

.action-btn.reset {
  background: #fef3c7;
  color: #d97706;
}

.action-btn.reset:hover:not(:disabled) {
  background: #fde68a;
  color: #b45309;
}

.action-btn.delete {
  background: #fef2f2;
  color: #dc2626;
}

.action-btn.delete:hover:not(:disabled) {
  background: #fee2e2;
  color: #b91c1c;
}

/* 响应式设计 */
@media (max-width: 480px) {
  .user-card {
    padding: 16px;
  }

  .user-header {
    flex-direction: column;
    align-items: center;
    text-align: center;
  }

  .detail-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 4px;
  }

  .user-actions {
    justify-content: center;
  }
}
</style>