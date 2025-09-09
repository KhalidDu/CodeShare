<template>
  <div class="share-management">
    <!-- 页面标题 -->
    <div class="page-header">
      <h1 class="page-title">分享管理</h1>
      <p class="page-description">管理您创建的代码分享链接</p>
    </div>

    <!-- 搜索和筛选区域 -->
    <div class="search-filter-section">
      <div class="search-bar">
        <div class="search-input-wrapper">
          <i class="fas fa-search search-icon"></i>
          <input
            v-model="searchQuery"
            type="text"
            placeholder="搜索分享链接..."
            class="search-input"
            @input="handleSearchInput"
            @keyup.enter="handleSearch"
          />
          <button
            v-if="searchQuery"
            @click="clearSearch"
            class="clear-search-btn"
            title="清除搜索"
          >
            <i class="fas fa-times"></i>
          </button>
        </div>
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
          <i class="fas fa-times"></i>
          清除筛选
        </button>
      </div>
    </div>

    <!-- 统计信息 -->
    <div class="stats-section">
      <div class="stat-card">
        <div class="stat-icon active">
          <i class="fas fa-link"></i>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ shareStore.activeShareTokens.length }}</div>
          <div class="stat-label">活跃分享</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon expired">
          <i class="fas fa-clock"></i>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ shareStore.expiredShareTokens.length }}</div>
          <div class="stat-label">已过期</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon revoked">
          <i class="fas fa-ban"></i>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ shareStore.revokedShareTokens.length }}</div>
          <div class="stat-label">已撤销</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon total">
          <i class="fas fa-chart-bar"></i>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ totalAccessCount }}</div>
          <div class="stat-label">总访问次数</div>
        </div>
      </div>
    </div>

    <!-- 批量操作栏 -->
    <div v-if="selectedTokens.length > 0" class="bulk-actions">
      <div class="bulk-info">
        <span class="bulk-count">已选择 {{ selectedTokens.length }} 个分享链接</span>
      </div>
      <div class="bulk-buttons">
        <button
          @click="handleBulkRevoke"
          class="bulk-btn revoke"
          :disabled="shareStore.isUpdating"
        >
          <i class="fas fa-ban"></i>
          批量撤销
        </button>
        <button
          @click="handleBulkDelete"
          class="bulk-btn delete"
          :disabled="shareStore.isDeleting"
        >
          <i class="fas fa-trash"></i>
          批量删除
        </button>
        <button
          @click="clearSelection"
          class="bulk-btn clear"
        >
          <i class="fas fa-times"></i>
          取消选择
        </button>
      </div>
    </div>

    <!-- 分享链接列表 -->
    <div class="share-list">
      <!-- 表格头部 -->
      <div class="table-header">
        <div class="table-cell checkbox-cell">
          <input
            type="checkbox"
            :checked="isAllSelected"
            :indeterminate="isIndeterminate"
            @change="handleSelectAll"
            class="checkbox"
          />
        </div>
        <div class="table-cell snippet-cell">代码片段</div>
        <div class="table-cell token-cell">分享令牌</div>
        <div class="table-cell status-cell">状态</div>
        <div class="table-cell permission-cell">权限</div>
        <div class="table-cell access-cell">访问统计</div>
        <div class="table-cell date-cell">创建时间</div>
        <div class="table-cell actions-cell">操作</div>
      </div>

      <!-- 表格内容 -->
      <div class="table-body">
        <div
          v-for="token in shareStore.shareTokens"
          :key="token.id"
          class="table-row"
          :class="{
            'selected': selectedTokens.includes(token.id),
            'expired': token.status === 'EXPIRED',
            'revoked': token.status === 'REVOKED'
          }"
        >
          <!-- 选择框 -->
          <div class="table-cell checkbox-cell">
            <input
              type="checkbox"
              :checked="selectedTokens.includes(token.id)"
              @change="handleTokenSelect(token.id)"
              class="checkbox"
            />
          </div>

          <!-- 代码片段信息 -->
          <div class="table-cell snippet-cell">
            <div class="snippet-info">
              <div class="snippet-title">{{ token.snippetTitle || '无标题' }}</div>
              <div class="snippet-language">{{ token.snippetLanguage }}</div>
            </div>
          </div>

          <!-- 分享令牌 -->
          <div class="table-cell token-cell">
            <div class="token-display">
              <code class="token-code">{{ token.token }}</code>
              <button
                @click="copyToken(token.token)"
                class="copy-btn"
                title="复制令牌"
              >
                <i class="fas fa-copy"></i>
              </button>
            </div>
          </div>

          <!-- 状态 -->
          <div class="table-cell status-cell">
            <span
              class="status-badge"
              :class="token.status.toLowerCase()"
            >
              {{ getStatusLabel(token.status) }}
            </span>
          </div>

          <!-- 权限 -->
          <div class="table-cell permission-cell">
            <span
              class="permission-badge"
              :class="token.permission.toLowerCase()"
            >
              {{ getPermissionLabel(token.permission) }}
            </span>
          </div>

          <!-- 访问统计 -->
          <div class="table-cell access-cell">
            <div class="access-stats">
              <div class="access-count">
                <i class="fas fa-eye"></i>
                {{ token.currentAccessCount }}
                <span v-if="token.maxAccessCount">
                  / {{ token.maxAccessCount }}
                </span>
              </div>
              <div v-if="token.lastAccessedAt" class="last-access">
                {{ formatDate(token.lastAccessedAt) }}
              </div>
            </div>
          </div>

          <!-- 创建时间 -->
          <div class="table-cell date-cell">
            <div class="date-info">
              <div class="create-date">{{ formatDate(token.createdAt) }}</div>
              <div v-if="token.expiresAt" class="expire-date">
                过期: {{ formatDate(token.expiresAt) }}
              </div>
            </div>
          </div>

          <!-- 操作按钮 -->
          <div class="table-cell actions-cell">
            <div class="action-buttons">
              <button
                @click="copyShareLink(token.token)"
                class="action-btn copy-link"
                title="复制分享链接"
              >
                <i class="fas fa-share-alt"></i>
              </button>
              <button
                @click="viewStats(token.id)"
                class="action-btn view-stats"
                title="查看统计"
              >
                <i class="fas fa-chart-bar"></i>
              </button>
              <button
                v-if="token.status === 'ACTIVE'"
                @click="revokeToken(token.id)"
                class="action-btn revoke"
                title="撤销分享"
                :disabled="shareStore.isUpdating"
              >
                <i class="fas fa-ban"></i>
              </button>
              <button
                @click="deleteToken(token.id)"
                class="action-btn delete"
                title="删除分享"
                :disabled="shareStore.isDeleting"
              >
                <i class="fas fa-trash"></i>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- 空状态 -->
      <div v-if="shareStore.shareTokens.length === 0 && !shareStore.isLoading" class="empty-state">
        <div class="empty-icon">
          <i class="fas fa-share-alt"></i>
        </div>
        <div class="empty-title">暂无分享链接</div>
        <div class="empty-description">
          您还没有创建任何分享链接
        </div>
      </div>

      <!-- 加载状态 -->
      <div v-if="shareStore.isLoading" class="loading-state">
        <div class="loading-spinner"></div>
        <div class="loading-text">加载中...</div>
      </div>
    </div>

    <!-- 分页组件 -->
    <div class="pagination-section">
      <Pagination
        :current-page="shareStore.currentPage"
        :total-pages="totalPages"
        :total-items="shareStore.totalCount"
        :page-size="shareStore.pageSize"
        :show-jump-to="true"
        :show-size-changer="true"
        @page-change="handlePageChange"
        @size-change="handlePageSizeChange"
      />
    </div>

    <!-- 确认对话框 -->
    <ConfirmDialog
      v-if="showConfirmDialog"
      :title="confirmDialog.title"
      :message="confirmDialog.message"
      :confirm-text="confirmDialog.confirmText"
      :cancel-text="confirmDialog.cancelText"
      :type="confirmDialog.type"
      @confirm="handleConfirm"
      @cancel="hideConfirmDialog"
    />

    <!-- 统计对话框 -->
    <div v-if="showStatsDialog" class="stats-dialog-overlay" @click="closeStatsDialog">
      <div class="stats-dialog" @click.stop>
        <div class="stats-dialog-header">
          <h2 class="stats-dialog-title">分享统计</h2>
          <button @click="closeStatsDialog" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="stats-dialog-body">
          <ShareStats
            :token-id="selectedStatsTokenId"
            :visible="showStatsDialog"
            @view-access-records="viewAccessRecords"
            @revoke-share="handleRevokeFromStats"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useShareStore } from '@/stores/share'
import { useToastStore } from '@/stores/toast'
import { useAuthStore } from '@/stores/auth'
import { ShareTokenStatus, SharePermission } from '@/types/share'
import Pagination from '@/components/common/Pagination.vue'
import ConfirmDialog from '@/components/common/ConfirmDialog.vue'
import ShareStats from './ShareStats.vue'

// Store注入
const shareStore = useShareStore()
const toastStore = useToastStore()
const authStore = useAuthStore()

// 响应式状态
const searchQuery = ref('')
const selectedStatus = ref('')
const selectedPermission = ref('')
const sortBy = ref('createdAt_desc')
const selectedTokens = ref<string[]>([])

// 确认对话框状态
const showConfirmDialog = ref(false)
const confirmDialog = ref({
  title: '',
  message: '',
  confirmText: '',
  cancelText: '',
  type: 'warning' as 'warning' | 'danger',
  action: '' as string,
  data: null as any
})

// 统计对话框状态
const showStatsDialog = ref(false)
const selectedStatsTokenId = ref('')

// 搜索防抖
const searchTimeout = ref<number | null>(null)

// 计算属性
const totalPages = computed(() => {
  return Math.ceil(shareStore.totalCount / shareStore.pageSize)
})

const hasActiveFilters = computed(() => {
  return searchQuery.value || selectedStatus.value || selectedPermission.value
})

const isAllSelected = computed(() => {
  return shareStore.shareTokens.length > 0 && 
         selectedTokens.value.length === shareStore.shareTokens.length
})

const isIndeterminate = computed(() => {
  return selectedTokens.value.length > 0 && 
         selectedTokens.value.length < shareStore.shareTokens.length
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
    toastStore.error('加载分享链接失败')
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

function handlePageChange(page: number) {
  shareStore.currentPage = page
  loadShareTokens()
}

function handlePageSizeChange(size: number) {
  shareStore.pageSize = size
  shareStore.currentPage = 1
  loadShareTokens()
}

function handleSelectAll(event: Event) {
  const target = event.target as HTMLInputElement
  if (target.checked) {
    selectedTokens.value = shareStore.shareTokens.map(token => token.id)
  } else {
    selectedTokens.value = []
  }
}

function handleTokenSelect(tokenId: string) {
  const index = selectedTokens.value.indexOf(tokenId)
  if (index > -1) {
    selectedTokens.value.splice(index, 1)
  } else {
    selectedTokens.value.push(tokenId)
  }
}

function clearSelection() {
  selectedTokens.value = []
}

async function copyToken(token: string) {
  try {
    await navigator.clipboard.writeText(token)
    toastStore.success('令牌已复制到剪贴板')
  } catch (error) {
    toastStore.error('复制令牌失败')
  }
}

async function copyShareLink(token: string) {
  try {
    const link = shareStore.getShareLink(token)
    await navigator.clipboard.writeText(link)
    toastStore.success('分享链接已复制到剪贴板')
  } catch (error) {
    toastStore.error('复制分享链接失败')
  }
}

function viewStats(tokenId: string) {
  selectedStatsTokenId.value = tokenId
  showStatsDialog.value = true
}

function revokeToken(tokenId: string) {
  showConfirmDialog.value = true
  confirmDialog.value = {
    title: '撤销分享',
    message: '确定要撤销这个分享链接吗？撤销后将无法访问。',
    confirmText: '撤销',
    cancelText: '取消',
    type: 'warning',
    action: 'revoke',
    data: { tokenId }
  }
}

function deleteToken(tokenId: string) {
  showConfirmDialog.value = true
  confirmDialog.value = {
    title: '删除分享',
    message: '确定要删除这个分享链接吗？此操作不可撤销。',
    confirmText: '删除',
    cancelText: '取消',
    type: 'danger',
    action: 'delete',
    data: { tokenId }
  }
}

function handleBulkRevoke() {
  showConfirmDialog.value = true
  confirmDialog.value = {
    title: '批量撤销',
    message: `确定要撤销选中的 ${selectedTokens.value.length} 个分享链接吗？`,
    confirmText: '撤销',
    cancelText: '取消',
    type: 'warning',
    action: 'bulkRevoke',
    data: { tokenIds: selectedTokens.value }
  }
}

function handleBulkDelete() {
  showConfirmDialog.value = true
  confirmDialog.value = {
    title: '批量删除',
    message: `确定要删除选中的 ${selectedTokens.value.length} 个分享链接吗？此操作不可撤销。`,
    confirmText: '删除',
    cancelText: '取消',
    type: 'danger',
    action: 'bulkDelete',
    data: { tokenIds: selectedTokens.value }
  }
}

async function handleConfirm() {
  try {
    switch (confirmDialog.value.action) {
      case 'revoke':
        await shareStore.revokeShareToken({ tokenId: confirmDialog.value.data.tokenId })
        toastStore.success('分享链接已撤销')
        break
      case 'delete':
        await shareStore.deleteShareToken(confirmDialog.value.data.tokenId)
        toastStore.success('分享链接已删除')
        break
      case 'bulkRevoke':
        await shareStore.batchShareTokens({
          tokenIds: confirmDialog.value.data.tokenIds,
          action: 'revoke'
        })
        toastStore.success('批量撤销成功')
        clearSelection()
        break
      case 'bulkDelete':
        await shareStore.batchShareTokens({
          tokenIds: confirmDialog.value.data.tokenIds,
          action: 'revoke'
        })
        toastStore.success('批量删除成功')
        clearSelection()
        break
    }
    hideConfirmDialog()
  } catch (error) {
    toastStore.error('操作失败')
  }
}

function hideConfirmDialog() {
  showConfirmDialog.value = false
  confirmDialog.value = {
    title: '',
    message: '',
    confirmText: '',
    cancelText: '',
    type: 'warning',
    action: '',
    data: null
  }
}

function closeStatsDialog() {
  showStatsDialog.value = false
  selectedStatsTokenId.value = ''
}

function viewAccessRecords(tokenId: string) {
  // TODO: 实现访问记录查看功能
  toastStore.info('访问记录功能开发中')
}

function handleRevokeFromStats(tokenId: string) {
  closeStatsDialog()
  revokeToken(tokenId)
}

function getStatusLabel(status: ShareTokenStatus): string {
  const labels = {
    [ShareTokenStatus.ACTIVE]: '活跃',
    [ShareTokenStatus.EXPIRED]: '已过期',
    [ShareTokenStatus.REVOKED]: '已撤销'
  }
  return labels[status] || status
}

function getPermissionLabel(permission: SharePermission): string {
  const labels = {
    [SharePermission.VIEW]: '仅查看',
    [SharePermission.EDIT]: '可编辑'
  }
  return labels[permission] || permission
}

function formatDate(dateString: string): string {
  const date = new Date(dateString)
  const now = new Date()
  const diffInHours = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60))
  
  if (diffInHours < 1) {
    return '刚刚'
  } else if (diffInHours < 24) {
    return `${diffInHours}小时前`
  } else if (diffInHours < 48) {
    return '昨天'
  } else {
    return date.toLocaleDateString('zh-CN')
  }
}
</script>

<style scoped>
.share-management {
  max-width: 1400px;
  margin: 0 auto;
  padding: 2rem;
}

/* 页面标题 */
.page-header {
  margin-bottom: 2rem;
}

.page-title {
  font-size: 2rem;
  font-weight: 700;
  color: #1f2937;
  margin-bottom: 0.5rem;
}

.page-description {
  color: #6b7280;
  font-size: 1rem;
}

/* 搜索和筛选区域 */
.search-filter-section {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.search-bar {
  margin-bottom: 1rem;
}

.search-input-wrapper {
  position: relative;
}

.search-icon {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #6b7280;
}

.search-input {
  width: 100%;
  padding: 0.75rem 1rem 0.75rem 2.5rem;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 0.875rem;
  transition: border-color 0.2s;
}

.search-input:focus {
  outline: none;
  border-color: #3b82f6;
}

.clear-search-btn {
  position: absolute;
  right: 1rem;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #6b7280;
  cursor: pointer;
  padding: 0.25rem;
}

.clear-search-btn:hover {
  color: #374151;
}

.filter-controls {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  align-items: flex-end;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.filter-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
}

.filter-select {
  padding: 0.5rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 0.875rem;
  background: white;
  cursor: pointer;
}

.filter-select:focus {
  outline: none;
  border-color: #3b82f6;
}

.clear-filters-btn {
  padding: 0.5rem 1rem;
  background: #ef4444;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 0.875rem;
  cursor: pointer;
  transition: background-color 0.2s;
}

.clear-filters-btn:hover {
  background: #dc2626;
}

/* 统计信息 */
.stats-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.stat-card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  gap: 1rem;
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  color: white;
}

.stat-icon.active {
  background: #10b981;
}

.stat-icon.expired {
  background: #f59e0b;
}

.stat-icon.revoked {
  background: #ef4444;
}

.stat-icon.total {
  background: #3b82f6;
}

.stat-value {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1f2937;
}

.stat-label {
  font-size: 0.875rem;
  color: #6b7280;
}

/* 批量操作栏 */
.bulk-actions {
  background: #f3f4f6;
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.bulk-info {
  font-size: 0.875rem;
  color: #374151;
}

.bulk-buttons {
  display: flex;
  gap: 0.5rem;
}

.bulk-btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 6px;
  font-size: 0.875rem;
  cursor: pointer;
  transition: background-color 0.2s;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.bulk-btn.revoke {
  background: #f59e0b;
  color: white;
}

.bulk-btn.revoke:hover {
  background: #d97706;
}

.bulk-btn.delete {
  background: #ef4444;
  color: white;
}

.bulk-btn.delete:hover {
  background: #dc2626;
}

.bulk-btn.clear {
  background: #6b7280;
  color: white;
}

.bulk-btn.clear:hover {
  background: #4b5563;
}

.bulk-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 分享链接列表 */
.share-list {
  background: white;
  border-radius: 12px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  margin-bottom: 1.5rem;
}

.table-header {
  display: grid;
  grid-template-columns: 40px 2fr 2fr 1fr 1fr 1.5fr 1.5fr 1fr;
  gap: 1rem;
  padding: 1rem;
  background: #f9fafb;
  border-bottom: 1px solid #e5e7eb;
  font-weight: 600;
  color: #374151;
  font-size: 0.875rem;
}

.table-row {
  display: grid;
  grid-template-columns: 40px 2fr 2fr 1fr 1fr 1.5fr 1.5fr 1fr;
  gap: 1rem;
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
  transition: background-color 0.2s;
}

.table-row:hover {
  background: #f9fafb;
}

.table-row.selected {
  background: #eff6ff;
}

.table-row.expired {
  opacity: 0.7;
}

.table-row.revoked {
  opacity: 0.5;
}

.table-cell {
  display: flex;
  align-items: center;
  font-size: 0.875rem;
}

.checkbox-cell {
  justify-content: center;
}

.snippet-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.snippet-title {
  font-weight: 500;
  color: #1f2937;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.snippet-language {
  color: #6b7280;
  font-size: 0.75rem;
}

.token-display {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex: 1;
}

.token-code {
  font-family: monospace;
  font-size: 0.75rem;
  background: #f3f4f6;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  color: #374151;
  flex: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.copy-btn {
  background: none;
  border: none;
  color: #6b7280;
  cursor: pointer;
  padding: 0.25rem;
  transition: color 0.2s;
}

.copy-btn:hover {
  color: #374151;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: uppercase;
}

.status-badge.active {
  background: #d1fae5;
  color: #065f46;
}

.status-badge.expired {
  background: #fef3c7;
  color: #92400e;
}

.status-badge.revoked {
  background: #fee2e2;
  color: #991b1b;
}

.permission-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
}

.permission-badge.view {
  background: #dbeafe;
  color: #1e40af;
}

.permission-badge.edit {
  background: #e0e7ff;
  color: #3730a3;
}

.access-stats {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.access-count {
  color: #374151;
  font-weight: 500;
}

.last-access {
  color: #6b7280;
  font-size: 0.75rem;
}

.date-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.create-date {
  color: #374151;
  font-weight: 500;
}

.expire-date {
  color: #6b7280;
  font-size: 0.75rem;
}

.action-buttons {
  display: flex;
  gap: 0.5rem;
}

.action-btn {
  width: 32px;
  height: 32px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background-color 0.2s;
}

.action-btn.copy-link {
  background: #e0e7ff;
  color: #3730a3;
}

.action-btn.copy-link:hover {
  background: #c7d2fe;
}

.action-btn.view-stats {
  background: #dbeafe;
  color: #1e40af;
}

.action-btn.view-stats:hover {
  background: #bfdbfe;
}

.action-btn.revoke {
  background: #fef3c7;
  color: #92400e;
}

.action-btn.revoke:hover {
  background: #fde68a;
}

.action-btn.delete {
  background: #fee2e2;
  color: #991b1b;
}

.action-btn.delete:hover {
  background: #fecaca;
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 空状态 */
.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: #6b7280;
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
  opacity: 0.5;
}

.empty-title {
  font-size: 1.25rem;
  font-weight: 600;
  margin-bottom: 0.5rem;
}

.empty-description {
  color: #6b7280;
}

/* 加载状态 */
.loading-state {
  text-align: center;
  padding: 4rem 2rem;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f4f6;
  border-top: 4px solid #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

.loading-text {
  color: #6b7280;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 分页区域 */
.pagination-section {
  display: flex;
  justify-content: center;
}

/* 响应式设计 */
@media (max-width: 1024px) {
  .table-header,
  .table-row {
    grid-template-columns: 40px 1.5fr 1.5fr 1fr 1fr;
  }
  
  .access-cell,
  .date-cell,
  .actions-cell {
    display: none;
  }
}

@media (max-width: 768px) {
  .share-management {
    padding: 1rem;
  }
  
  .filter-controls {
    flex-direction: column;
    align-items: stretch;
  }
  
  .stats-section {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .bulk-actions {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }
  
  .bulk-buttons {
    justify-content: center;
  }
  
  .table-header,
  .table-row {
    grid-template-columns: 40px 1fr;
  }
  
  .token-cell,
  .status-cell,
  .permission-cell {
    display: none;
  }
}

@media (max-width: 480px) {
  .stats-section {
    grid-template-columns: 1fr;
  }
  
  .page-title {
    font-size: 1.5rem;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .table-row,
  .action-btn,
  .bulk-btn,
  .copy-btn {
    transition: none;
  }
  
  .loading-spinner {
    animation: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .share-management {
    border: 2px solid #000;
  }
  
  .table-header,
  .table-row {
    border-width: 2px;
  }
  
  .action-btn,
  .bulk-btn {
    border: 2px solid currentColor;
  }
}

/* 统计对话框样式 */
.stats-dialog-overlay {
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
  padding: 2rem;
}

.stats-dialog {
  background: white;
  border-radius: 12px;
  max-width: 90vw;
  max-height: 90vh;
  width: 1000px;
  height: 80vh;
  display: flex;
  flex-direction: column;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}

.stats-dialog-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
  flex-shrink: 0;
}

.stats-dialog-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.25rem;
  color: #6b7280;
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 6px;
  transition: all 0.2s;
}

.close-btn:hover {
  background: #f3f4f6;
  color: #374151;
}

.stats-dialog-body {
  flex: 1;
  overflow-y: auto;
  padding: 0;
}

/* 响应式设计 */
@media (max-width: 1024px) {
  .stats-dialog {
    max-width: 95vw;
    width: 100%;
    height: 85vh;
  }
}

@media (max-width: 768px) {
  .stats-dialog-overlay {
    padding: 1rem;
  }
  
  .stats-dialog {
    height: 90vh;
  }
  
  .stats-dialog-header {
    padding: 1rem;
  }
  
  .stats-dialog-title {
    font-size: 1.25rem;
  }
}

@media (max-width: 480px) {
  .stats-dialog-overlay {
    padding: 0.5rem;
  }
  
  .stats-dialog {
    height: 95vh;
    border-radius: 0;
  }
  
  .stats-dialog-header {
    padding: 0.75rem;
  }
  
  .stats-dialog-title {
    font-size: 1.125rem;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .stats-dialog-overlay {
    transition: none;
  }
  
  .close-btn {
    transition: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .stats-dialog {
    border: 3px solid #000;
  }
  
  .stats-dialog-header {
    border-width: 2px;
  }
  
  .close-btn {
    border: 2px solid currentColor;
  }
}
</style>