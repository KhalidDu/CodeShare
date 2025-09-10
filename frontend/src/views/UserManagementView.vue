<template>
  <div class="user-management">
    <!-- 页面标题和操作栏 -->
    <div class="header">
      <div class="title-section">
        <h1>用户管理</h1>
        <p class="subtitle">管理系统用户和权限</p>
      </div>
      <div class="actions">
        <button 
          class="btn btn-primary"
          @click="showCreateDialog = true"
        >
          <i class="icon-plus"></i>
          添加用户
        </button>
        <button 
          class="btn btn-secondary"
          @click="refreshUsers"
          :disabled="loading"
        >
          <i class="icon-refresh" :class="{ spinning: loading }"></i>
          刷新
        </button>
      </div>
    </div>

    <!-- 用户统计卡片 -->
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-icon total">
          <i class="icon-users"></i>
        </div>
        <div class="stat-content">
          <div class="stat-number">{{ userStats.total }}</div>
          <div class="stat-label">总用户数</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon active">
          <i class="icon-user-check"></i>
        </div>
        <div class="stat-content">
          <div class="stat-number">{{ userStats.active }}</div>
          <div class="stat-label">活跃用户</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon admin">
          <i class="icon-shield"></i>
        </div>
        <div class="stat-content">
          <div class="stat-number">{{ userStats.admins }}</div>
          <div class="stat-label">管理员</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon editor">
          <i class="icon-edit"></i>
        </div>
        <div class="stat-content">
          <div class="stat-number">{{ userStats.editors }}</div>
          <div class="stat-label">编辑者</div>
        </div>
      </div>
    </div>

    <!-- 筛选和搜索 -->
    <div class="filters">
      <div class="search-box">
        <i class="icon-search"></i>
        <input
          v-model="searchQuery"
          type="text"
          placeholder="搜索用户名或邮箱..."
          class="search-input"
        />
      </div>
      <div class="filter-tabs">
        <button
          v-for="tab in filterTabs"
          :key="tab.key"
          class="filter-tab"
          :class="{ active: activeFilter === tab.key }"
          @click="activeFilter = tab.key"
        >
          {{ tab.label }}
          <span class="count">{{ tab.count }}</span>
        </button>
      </div>
    </div>

    <!-- 用户列表 -->
    <div class="user-list">
      <div v-if="loading && users.length === 0" class="loading-state">
        <div class="spinner"></div>
        <p>加载用户列表中...</p>
      </div>

      <div v-else-if="error" class="error-state">
        <i class="icon-alert-circle"></i>
        <p>{{ error }}</p>
        <button class="btn btn-primary" @click="refreshUsers">重试</button>
      </div>

      <div v-else-if="filteredUsers.length === 0" class="empty-state">
        <i class="icon-users"></i>
        <p>{{ searchQuery ? '未找到匹配的用户' : '暂无用户' }}</p>
      </div>

      <div v-else class="user-grid">
        <UserCard
          v-for="user in filteredUsers"
          :key="user.id"
          :user="user"
          @edit="editUser"
          @delete="confirmDeleteUser"
          @toggle-status="toggleUserStatus"
          @reset-password="showResetPasswordDialog"
        />
      </div>
    </div>

    <!-- 创建用户对话框 -->
    <CreateUserDialog
      v-if="showCreateDialog"
      @close="showCreateDialog = false"
      @created="handleUserCreated"
    />

    <!-- 编辑用户对话框 -->
    <EditUserDialog
      v-if="showEditDialog && selectedUser"
      :user="selectedUser"
      @close="showEditDialog = false"
      @updated="handleUserUpdated"
    />

    <!-- 重置密码对话框 -->
    <ResetPasswordDialog
      v-if="showResetDialog && selectedUser"
      :user="selectedUser"
      @close="showResetDialog = false"
      @reset="handlePasswordReset"
    />

    <!-- 删除确认对话框 -->
    <ConfirmDialog
      v-if="showDeleteDialog && selectedUser"
      :title="`删除用户 ${selectedUser.username}`"
      :message="`确定要删除用户 ${selectedUser.username} 吗？此操作不可撤销。`"
      confirm-text="删除"
      confirm-type="danger"
      @confirm="handleDeleteUser"
      @cancel="showDeleteDialog = false"
    />

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
import { ref, computed, onMounted } from 'vue'
import { useUsersStore } from '@/stores/users'
import { useAuthStore } from '@/stores/auth'
import { UserRole, type User } from '@/types'
import UserCard from '@/components/users/UserCard.vue'
import CreateUserDialog from '@/components/users/CreateUserDialog.vue'
import EditUserDialog from '@/components/users/EditUserDialog.vue'
import ResetPasswordDialog from '@/components/users/ResetPasswordDialog.vue'
import ConfirmDialog from '@/components/common/ConfirmDialog.vue'
import Toast from '@/components/common/Toast.vue'

// 状态管理
const usersStore = useUsersStore()
const authStore = useAuthStore()

// 响应式数据
const searchQuery = ref('')
const activeFilter = ref('all')
const showCreateDialog = ref(false)
const showEditDialog = ref(false)
const showResetDialog = ref(false)
const showDeleteDialog = ref(false)
const selectedUser = ref<User | null>(null)

// Toast 通知
const toast = ref({
  show: false,
  type: 'success' as 'success' | 'error' | 'warning' | 'info',
  message: ''
})

// 计算属性
const { users, loading, error, userStats } = usersStore

/**
 * 筛选标签配置
 */
const filterTabs = computed(() => [
  { key: 'all', label: '全部', count: users.length },
  { key: 'active', label: '活跃', count: userStats.active },
  { key: 'inactive', label: '禁用', count: userStats.inactive },
  { key: 'admin', label: '管理员', count: userStats.admins },
  { key: 'editor', label: '编辑者', count: userStats.editors },
  { key: 'viewer', label: '查看者', count: userStats.viewers }
])

/**
 * 过滤后的用户列表
 */
const filteredUsers = computed(() => {
  let filtered = users

  // 按状态筛选
  switch (activeFilter.value) {
    case 'active':
      filtered = filtered.filter(user => user.isActive)
      break
    case 'inactive':
      filtered = filtered.filter(user => !user.isActive)
      break
    case 'admin':
      filtered = filtered.filter(user => user.role === UserRole.Admin)
      break
    case 'editor':
      filtered = filtered.filter(user => user.role === UserRole.Editor)
      break
    case 'viewer':
      filtered = filtered.filter(user => user.role === UserRole.Viewer)
      break
  }

  // 按搜索关键词筛选
  if (searchQuery.value.trim()) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(user =>
      user.username.toLowerCase().includes(query) ||
      user.email.toLowerCase().includes(query)
    )
  }

  return filtered
})

// 方法
/**
 * 刷新用户列表
 */
async function refreshUsers() {
  await usersStore.fetchUsers()
}

/**
 * 编辑用户
 */
function editUser(user: User) {
  selectedUser.value = user
  showEditDialog.value = true
}

/**
 * 确认删除用户
 */
function confirmDeleteUser(user: User) {
  selectedUser.value = user
  showDeleteDialog.value = true
}

/**
 * 切换用户状态
 */
async function toggleUserStatus(user: User) {
  const success = await usersStore.toggleUserStatus(user.id, !user.isActive)
  if (success) {
    showToast('success', `用户 ${user.username} 状态已${user.isActive ? '禁用' : '启用'}`)
  } else {
    showToast('error', usersStore.error || '操作失败')
  }
}

/**
 * 显示重置密码对话框
 */
function showResetPasswordDialog(user: User) {
  selectedUser.value = user
  showResetDialog.value = true
}

/**
 * 处理用户创建成功
 */
function handleUserCreated(user: User) {
  showCreateDialog.value = false
  showToast('success', `用户 ${user.username} 创建成功`)
}

/**
 * 处理用户更新成功
 */
function handleUserUpdated(user: User) {
  showEditDialog.value = false
  selectedUser.value = null
  showToast('success', `用户 ${user.username} 更新成功`)
}

/**
 * 处理密码重置成功
 */
function handlePasswordReset() {
  showResetDialog.value = false
  selectedUser.value = null
  showToast('success', '密码重置成功')
}

/**
 * 处理用户删除
 */
async function handleDeleteUser() {
  if (!selectedUser.value) return

  const success = await usersStore.deleteUser(selectedUser.value.id)
  showDeleteDialog.value = false
  
  if (success) {
    showToast('success', `用户 ${selectedUser.value.username} 已删除`)
  } else {
    showToast('error', usersStore.error || '删除失败')
  }
  
  selectedUser.value = null
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
  refreshUsers()
})
</script>

<style scoped>
.user-management {
  max-width: 1200px;
  margin: 0 auto;
}

/* 页面标题 */
.header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: var(--spacing-2xl);
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

.actions {
  display: flex;
  gap: 12px;
}

/* 统计卡片 */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: var(--spacing-lg);
  margin-bottom: var(--spacing-2xl);
}

.stat-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border: 1px solid #e5e7eb;
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  color: white;
}

.stat-icon.total { background: #3b82f6; }
.stat-icon.active { background: #10b981; }
.stat-icon.admin { background: #8b5cf6; }
.stat-icon.editor { background: #f59e0b; }

.stat-number {
  font-size: 24px;
  font-weight: 700;
  color: #1a1a1a;
  line-height: 1;
}

.stat-label {
  font-size: 14px;
  color: #666;
  margin-top: 4px;
}

/* 筛选和搜索 */
.filters {
  background: white;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 24px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border: 1px solid #e5e7eb;
}

.search-box {
  position: relative;
  margin-bottom: 20px;
}

.search-box i {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: #666;
}

.search-input {
  width: 100%;
  padding: 12px 12px 12px 40px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.2s;
}

.search-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.filter-tabs {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.filter-tab {
  padding: 8px 16px;
  border: 1px solid #d1d5db;
  border-radius: 20px;
  background: white;
  color: #666;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 6px;
}

.filter-tab:hover {
  border-color: #3b82f6;
  color: #3b82f6;
}

.filter-tab.active {
  background: #3b82f6;
  border-color: #3b82f6;
  color: white;
}

.count {
  background: rgba(255, 255, 255, 0.2);
  padding: 2px 6px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 500;
}

.filter-tab.active .count {
  background: rgba(255, 255, 255, 0.3);
}

/* 用户列表 */
.user-list {
  min-height: 400px;
}

.user-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 20px;
}

/* 状态样式 */
.loading-state,
.error-state,
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  text-align: center;
  color: #666;
}

.loading-state .spinner {
  width: 32px;
  height: 32px;
  border: 3px solid #e5e7eb;
  border-top: 3px solid #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 16px;
}

.error-state i,
.empty-state i {
  font-size: 48px;
  color: #d1d5db;
  margin-bottom: 16px;
}

/* 按钮样式 */
.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  text-decoration: none;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background: #3b82f6;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #2563eb;
}

.btn-secondary {
  background: #f3f4f6;
  color: #374151;
  border: 1px solid #d1d5db;
}

.btn-secondary:hover:not(:disabled) {
  background: #e5e7eb;
}

/* 动画 */
@keyframes spin {
  to { transform: rotate(360deg); }
}

.spinning {
  animation: spin 1s linear infinite;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .user-management {
    padding: 16px;
  }

  .header {
    flex-direction: column;
    gap: 16px;
    align-items: stretch;
  }

  .actions {
    justify-content: flex-start;
  }

  .stats-grid {
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  }

  .user-grid {
    grid-template-columns: 1fr;
  }

  .filter-tabs {
    justify-content: center;
  }
}
</style>