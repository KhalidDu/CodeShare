<template>
  <div class="share-management-view">
    <!-- 页面标题和操作栏 -->
    <div class="header">
      <div class="title-section">
        <h1>分享管理</h1>
        <p class="subtitle">管理您创建的代码分享链接</p>
      </div>
      <div class="actions">
        <button 
          class="btn btn-primary"
          @click="showCreateShareDialog = true"
        >
          <i class="icon-plus"></i>
          创建分享
        </button>
        <button 
          class="btn btn-secondary"
          @click="refreshShares"
          :disabled="shareStore.isLoading"
        >
          <i class="icon-refresh" :class="{ spinning: shareStore.isLoading }"></i>
          刷新
        </button>
      </div>
    </div>

    <!-- 分享统计卡片 -->
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-icon active">
          <i class="icon-link"></i>
        </div>
        <div class="stat-content">
          <div class="stat-number">{{ shareStore.activeShareTokens.length }}</div>
          <div class="stat-label">活跃分享</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon expired">
          <i class="icon-clock"></i>
        </div>
        <div class="stat-content">
          <div class="stat-number">{{ shareStore.expiredShareTokens.length }}</div>
          <div class="stat-label">已过期</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon revoked">
          <i class="icon-ban"></i>
        </div>
        <div class="stat-content">
          <div class="stat-number">{{ shareStore.revokedShareTokens.length }}</div>
          <div class="stat-label">已撤销</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon total">
          <i class="icon-chart-bar"></i>
        </div>
        <div class="stat-content">
          <div class="stat-number">{{ totalAccessCount }}</div>
          <div class="stat-label">总访问次数</div>
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
          placeholder="搜索分享链接..."
          class="search-input"
          @input="handleSearchInput"
        />
        <button
          v-if="searchQuery"
          @click="clearSearch"
          class="clear-search-btn"
        >
          <i class="icon-times"></i>
        </button>
      </div>
      <div class="filter-controls">
        <!-- 状态筛选 -->
        <div class="filter-group">
          <label class="filter-label">状态</label>
          <select
            v-model="selectedStatus"
            @change="handleStatusChange"
            class="filter-select"
          >
            <option value="">全部状态</option>
            <option value="ACTIVE">活跃</option>
            <option value="EXPIRED">已过期</option>
            <option value="REVOKED">已撤销</option>
          </select>
        </div>

        <!-- 权限筛选 -->
        <div class="filter-group">
          <label class="filter-label">权限</label>
          <select
            v-model="selectedPermission"
            @change="handlePermissionChange"
            class="filter-select"
          >
            <option value="">全部权限</option>
            <option value="VIEW">仅查看</option>
            <option value="EDIT">可编辑</option>
          </select>
        </div>

        <!-- 排序选择 -->
        <div class="filter-group">
          <label class="filter-label">排序</label>
          <select
            v-model="sortBy"
            @change="handleSortChange"
            class="filter-select"
          >
            <option value="createdAt_desc">最新创建</option>
            <option value="createdAt_asc">最早创建</option>
            <option value="lastAccessedAt_desc">最近访问</option>
            <option value="accessCount_desc">访问次数</option>
          </select>
        </div>

        <!-- 清除筛选按钮 -->
        <button
          v-if="hasActiveFilters"
          @click="clearAllFilters"
          class="clear-filters-btn"
        >
          <i class="icon-times"></i>
          清除筛选
        </button>
      </div>
    </div>

    <!-- 分享链接列表 -->
    <div class="share-list">
      <div v-if="shareStore.isLoading && shareStore.shareTokens.length === 0" class="loading-state">
        <div class="spinner"></div>
        <p>加载分享链接中...</p>
      </div>

      <div v-else-if="shareStore.error" class="error-state">
        <i class="icon-alert-circle"></i>
        <p>{{ shareStore.error }}</p>
        <button class="btn btn-primary" @click="refreshShares">重试</button>
      </div>

      <div v-else-if="shareStore.shareTokens.length === 0" class="empty-state">
        <i class="icon-share-alt"></i>
        <p>{{ searchQuery ? '未找到匹配的分享链接' : '暂无分享链接' }}</p>
        <button 
          v-if="!searchQuery"
          class="btn btn-primary"
          @click="showCreateShareDialog = true"
        >
          创建第一个分享链接
        </button>
      </div>

      <div v-else>
        <ShareManagement />
      </div>
    </div>

    <!-- 创建分享对话框 -->
    <CreateShareDialog
      v-if="showCreateShareDialog"
      @close="showCreateShareDialog = false"
      @created="handleShareCreated"
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
import { ref, computed, onMounted, watch } from 'vue'
import { useShareStore } from '@/stores/share'
import { useToastStore } from '@/stores/toast'
import { useAuthStore } from '@/stores/auth'
import { ShareTokenStatus, SharePermission } from '@/types/share'
import ShareManagement from '@/components/sharing/ShareManagement.vue'
import CreateShareDialog from '@/components/sharing/CreateShareDialog.vue'
import Toast from '@/components/common/Toast.vue'

// Store注入
const shareStore = useShareStore()
const toastStore = useToastStore()
const authStore = useAuthStore()

// 响应式数据
const searchQuery = ref('')
const selectedStatus = ref('')
const selectedPermission = ref('')
const sortBy = ref('createdAt_desc')
const showCreateShareDialog = ref(false)

// Toast 通知
const toast = ref({
  show: false,
  type: 'success' as 'success' | 'error' | 'warning' | 'info',
  message: ''
})

// 搜索防抖
const searchTimeout = ref<number | null>(null)

// 计算属性
const hasActiveFilters = computed(() => {
  return searchQuery.value || selectedStatus.value || selectedPermission.value
})

const totalAccessCount = computed(() => {
  return shareStore.shareTokens.reduce((total, token) => total + token.currentAccessCount, 0)
})

// 生命周期
onMounted(async () => {
  await loadShareTokens()
})

// 监听筛选器变化
watch([selectedStatus, selectedPermission, sortBy], async () => {
  await loadShareTokens()
})

// 方法
async function loadShareTokens() {
  try {
    await shareStore.fetchShareTokens({
      search: searchQuery.value || undefined,
      status: selectedStatus.value as ShareTokenStatus || undefined,
      permission: selectedPermission.value as SharePermission || undefined,
      sortBy: sortBy.value.split('_')[0],
      sortOrder: sortBy.value.split('_')[1] as 'asc' | 'desc',
      page: shareStore.currentPage,
      pageSize: shareStore.pageSize
    })
  } catch (error) {
    showToast('error', '加载分享链接失败')
  }
}

function handleSearchInput() {
  if (searchTimeout.value) {
    clearTimeout(searchTimeout.value)
  }
  
  searchTimeout.value = window.setTimeout(() => {
    handleSearch()
  }, 300)
}

function handleSearch() {
  shareStore.currentPage = 1
  loadShareTokens()
}

function clearSearch() {
  searchQuery.value = ''
  handleSearch()
}

function handleStatusChange() {
  shareStore.currentPage = 1
}

function handlePermissionChange() {
  shareStore.currentPage = 1
}

function handleSortChange() {
  shareStore.currentPage = 1
}

function clearAllFilters() {
  searchQuery.value = ''
  selectedStatus.value = ''
  selectedPermission.value = ''
  sortBy.value = 'createdAt_desc'
  shareStore.currentPage = 1
  loadShareTokens()
}

async function refreshShares() {
  await loadShareTokens()
}

function handleShareCreated() {
  showCreateShareDialog.value = false
  showToast('success', '分享链接创建成功')
  refreshShares()
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
</script>

<style scoped>
.share-management-view {
  padding: 24px;
  max-width: 1400px;
  margin: 0 auto;
}

/* 页面标题 */
.header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
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

.actions {
  display: flex;
  gap: 12px;
}

/* 统计卡片 */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-bottom: 32px;
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

.stat-icon.active { background: #10b981; }
.stat-icon.expired { background: #f59e0b; }
.stat-icon.revoked { background: #ef4444; }
.stat-icon.total { background: #3b82f6; }

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

.clear-search-btn {
  position: absolute;
  right: 12px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #666;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
}

.clear-search-btn:hover {
  color: #374151;
  background: #f3f4f6;
}

.filter-controls {
  display: flex;
  gap: 16px;
  flex-wrap: wrap;
  align-items: flex-end;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 150px;
}

.filter-label {
  font-size: 14px;
  font-weight: 500;
  color: #374151;
}

.filter-select {
  padding: 8px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  background: white;
  cursor: pointer;
  transition: border-color 0.2s;
}

.filter-select:focus {
  outline: none;
  border-color: #3b82f6;
}

.clear-filters-btn {
  padding: 8px 16px;
  background: #ef4444;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: background-color 0.2s;
  display: flex;
  align-items: center;
  gap: 6px;
  align-self: flex-end;
}

.clear-filters-btn:hover {
  background: #dc2626;
}

/* 分享链接列表 */
.share-list {
  min-height: 400px;
}

.share-grid {
  display: grid;
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

/* 图标样式 */
.icon-plus::before { content: "+"; }
.icon-refresh::before { content: "↻"; }
.icon-link::before { content: "🔗"; }
.icon-clock::before { content: "⏰"; }
.icon-ban::before { content: "🚫"; }
.icon-chart-bar::before { content: "📊"; }
.icon-search::before { content: "🔍"; }
.icon-times::before { content: "✕"; }
.icon-alert-circle::before { content: "⚠️"; }
.icon-share-alt::before { content: "📤"; }

/* 动画 */
@keyframes spin {
  to { transform: rotate(360deg); }
}

.spinning {
  animation: spin 1s linear infinite;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .share-management-view {
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

  .filter-controls {
    flex-direction: column;
    align-items: stretch;
  }

  .filter-group {
    min-width: auto;
  }

  .clear-filters-btn {
    align-self: stretch;
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }
}
</style>