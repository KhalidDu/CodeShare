<template>
  <div class="settings-history-view">
    <div class="page-header">
      <div class="header-content">
        <h1 class="page-title">设置历史</h1>
        <p class="page-description">查看和管理系统设置的变更历史记录</p>
      </div>
      <div class="header-actions">
        <button 
          @click="refreshHistory" 
          class="btn btn-outline-primary"
          :disabled="loading"
        >
          <i class="fas fa-sync-alt" :class="{ 'fa-spin': loading }"></i>
          刷新
        </button>
        <button 
          @click="exportHistory" 
          class="btn btn-outline-success"
          :disabled="loading || history.length === 0"
        >
          <i class="fas fa-download"></i>
          导出
        </button>
      </div>
    </div>

    <!-- 统计信息 -->
    <div v-if="statistics" class="statistics-section">
      <div class="row">
        <div class="col-md-3">
          <div class="stat-card">
            <div class="stat-icon">
              <i class="fas fa-edit"></i>
            </div>
            <div class="stat-content">
              <div class="stat-number">{{ statistics.totalChanges }}</div>
              <div class="stat-label">总变更次数</div>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="stat-card">
            <div class="stat-icon">
              <i class="fas fa-user"></i>
            </div>
            <div class="stat-content">
              <div class="stat-number">{{ statistics.activeUsers }}</div>
              <div class="stat-label">活跃用户</div>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="stat-card">
            <div class="stat-icon">
              <i class="fas fa-calendar-day"></i>
            </div>
            <div class="stat-content">
              <div class="stat-number">{{ statistics.changesToday }}</div>
              <div class="stat-label">今日变更</div>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="stat-card">
            <div class="stat-icon">
              <i class="fas fa-clock"></i>
            </div>
            <div class="stat-content">
              <div class="stat-number">{{ formatTime(statistics.lastChangeTime) }}</div>
              <div class="stat-label">最后变更</div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 筛选器 -->
    <div class="filters-section">
      <div class="card">
        <div class="card-header">
          <h3><i class="fas fa-filter"></i> 筛选器</h3>
        </div>
        <div class="card-body">
          <div class="row">
            <div class="col-md-3">
              <div class="form-group">
                <label for="settingType" class="form-label">设置类型</label>
                <select
                  id="settingType"
                  v-model="filters.settingType"
                  class="form-select"
                  @change="applyFilters"
                >
                  <option value="">全部类型</option>
                  <option value="site">站点设置</option>
                  <option value="security">安全设置</option>
                  <option value="feature">功能设置</option>
                  <option value="email">邮件设置</option>
                </select>
              </div>
            </div>
            <div class="col-md-3">
              <div class="form-group">
                <label for="changeType" class="form-label">变更类型</label>
                <select
                  id="changeType"
                  v-model="filters.changeType"
                  class="form-select"
                  @change="applyFilters"
                >
                  <option value="">全部类型</option>
                  <option value="create">创建</option>
                  <option value="update">更新</option>
                  <option value="delete">删除</option>
                </select>
              </div>
            </div>
            <div class="col-md-3">
              <div class="form-group">
                <label for="status" class="form-label">状态</label>
                <select
                  id="status"
                  v-model="filters.status"
                  class="form-select"
                  @change="applyFilters"
                >
                  <option value="">全部状态</option>
                  <option value="success">成功</option>
                  <option value="failed">失败</option>
                  <option value="pending">待处理</option>
                </select>
              </div>
            </div>
            <div class="col-md-3">
              <div class="form-group">
                <label for="dateRange" class="form-label">时间范围</label>
                <select
                  id="dateRange"
                  v-model="filters.dateRange"
                  class="form-select"
                  @change="applyFilters"
                >
                  <option value="today">今天</option>
                  <option value="week">本周</option>
                  <option value="month">本月</option>
                  <option value="quarter">本季度</option>
                  <option value="year">本年</option>
                  <option value="all">全部</option>
                </select>
              </div>
            </div>
          </div>
          <div class="row">
            <div class="col-md-6">
              <div class="form-group">
                <label for="search" class="form-label">搜索</label>
                <div class="input-group">
                  <input
                    id="search"
                    v-model="filters.search"
                    type="text"
                    class="form-control"
                    placeholder="搜索设置名称、用户名..."
                    @keyup.enter="applyFilters"
                  />
                  <button 
                    @click="applyFilters"
                    class="btn btn-outline-secondary"
                  >
                    <i class="fas fa-search"></i>
                  </button>
                </div>
              </div>
            </div>
            <div class="col-md-3">
              <div class="form-group">
                <label for="startDate" class="form-label">开始日期</label>
                <input
                  id="startDate"
                  v-model="filters.startDate"
                  type="date"
                  class="form-control"
                  @change="applyFilters"
                />
              </div>
            </div>
            <div class="col-md-3">
              <div class="form-group">
                <label for="endDate" class="form-label">结束日期</label>
                <input
                  id="endDate"
                  v-model="filters.endDate"
                  type="date"
                  class="form-control"
                  @change="applyFilters"
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 历史记录列表 -->
    <div class="history-section">
      <div class="card">
        <div class="card-header d-flex justify-content-between align-items-center">
          <h3><i class="fas fa-history"></i> 历史记录</h3>
          <span class="badge bg-primary">{{ history.length }} 条记录</span>
        </div>
        <div class="card-body">
          <!-- 加载状态 -->
          <div v-if="loading" class="loading-overlay">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">加载中...</span>
            </div>
            <p class="loading-text">正在加载历史记录...</p>
          </div>

          <!-- 空状态 -->
          <div v-else-if="history.length === 0" class="empty-state">
            <i class="fas fa-history"></i>
            <h4>暂无历史记录</h4>
            <p>当前筛选条件下没有找到设置变更记录</p>
          </div>

          <!-- 历史记录列表 -->
          <div v-else class="history-list">
            <div 
              v-for="record in history" 
              :key="record.id"
              class="history-item"
              @click="showRecordDetail(record)"
            >
              <div class="history-header">
                <div class="history-info">
                  <div class="history-title">
                    <span class="setting-type-badge" :class="getSettingTypeClass(record.settingType)">
                      {{ getSettingTypeText(record.settingType) }}
                    </span>
                    <span class="change-type-badge" :class="getChangeTypeClass(record.changeType)">
                      {{ getChangeTypeText(record.changeType) }}
                    </span>
                    <span class="record-title">{{ record.settingName }}</span>
                  </div>
                  <div class="history-meta">
                    <span class="user-info">
                      <i class="fas fa-user"></i>
                      {{ record.changedBy }}
                    </span>
                    <span class="time-info">
                      <i class="fas fa-clock"></i>
                      {{ formatDateTime(record.changedAt) }}
                    </span>
                  </div>
                </div>
                <div class="history-status">
                  <span class="status-badge" :class="getStatusClass(record.status)">
                    {{ getStatusText(record.status) }}
                  </span>
                </div>
              </div>
              
              <div v-if="record.description" class="history-description">
                {{ record.description }}
              </div>
              
              <div class="history-details">
                <div class="detail-item">
                  <span class="detail-label">变更前：</span>
                  <span class="detail-value">{{ formatValue(record.oldValue) }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">变更后：</span>
                  <span class="detail-value">{{ formatValue(record.newValue) }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- 分页 -->
          <div v-if="hasMore" class="pagination-section">
            <button 
              @click="loadMore"
              class="btn btn-outline-primary w-100"
              :disabled="loadingMore"
            >
              <i class="fas fa-plus" :class="{ 'fa-spin': loadingMore }"></i>
              {{ loadingMore ? '加载中...' : '加载更多' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 详情模态框 -->
    <div v-if="selectedRecord" class="modal-backdrop">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">历史记录详情</h5>
            <button type="button" class="btn-close" @click="closeDetail"></button>
          </div>
          <div class="modal-body">
            <div class="detail-section">
              <h6>基本信息</h6>
              <div class="detail-grid">
                <div class="detail-item">
                  <span class="detail-label">设置类型：</span>
                  <span class="detail-value">{{ getSettingTypeText(selectedRecord.settingType) }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">设置名称：</span>
                  <span class="detail-value">{{ selectedRecord.settingName }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">变更类型：</span>
                  <span class="detail-value">{{ getChangeTypeText(selectedRecord.changeType) }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">状态：</span>
                  <span class="detail-value">
                    <span class="status-badge" :class="getStatusClass(selectedRecord.status)">
                      {{ getStatusText(selectedRecord.status) }}
                    </span>
                  </span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">操作用户：</span>
                  <span class="detail-value">{{ selectedRecord.changedBy }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">操作时间：</span>
                  <span class="detail-value">{{ formatDateTime(selectedRecord.changedAt) }}</span>
                </div>
              </div>
            </div>

            <div v-if="selectedRecord.description" class="detail-section">
              <h6>描述</h6>
              <p>{{ selectedRecord.description }}</p>
            </div>

            <div class="detail-section">
              <h6>变更详情</h6>
              <div class="changes-comparison">
                <div class="change-column">
                  <h7>变更前</h7>
                  <pre class="change-content">{{ formatValue(selectedRecord.oldValue, true) }}</pre>
                </div>
                <div class="change-column">
                  <h7>变更后</h7>
                  <pre class="change-content">{{ formatValue(selectedRecord.newValue, true) }}</pre>
                </div>
              </div>
            </div>

            <div v-if="selectedRecord.error" class="detail-section">
              <h6>错误信息</h6>
              <div class="alert alert-danger">
                <pre>{{ selectedRecord.error }}</pre>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="closeDetail">
              关闭
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useSettingsStore } from '@/stores/settings'
import { useToast } from '@/composables/useToast'
import type { 
  SettingsHistoryDto, 
  SettingsHistoryQueryRequest,
  SettingsHistoryStatisticsDto,
  SettingType,
  ChangeType,
  ChangeStatus
} from '@/types/settings'

const settingsStore = useSettingsStore()
const toast = useToast()

// 响应式状态
const history = ref<SettingsHistoryDto[]>([])
const statistics = ref<SettingsHistoryStatisticsDto | null>(null)
const loading = ref(false)
const loadingMore = ref(false)
const hasMore = ref(false)
const selectedRecord = ref<SettingsHistoryDto | null>(null)
const currentPage = ref(1)
const pageSize = ref(20)

// 筛选器
const filters = ref({
  settingType: '',
  changeType: '',
  status: '',
  dateRange: 'week',
  search: '',
  startDate: '',
  endDate: ''
})

// 计算属性
const hasActiveFilters = computed(() => {
  return filters.value.settingType || 
         filters.value.changeType || 
         filters.value.status || 
         filters.value.search || 
         filters.value.startDate || 
         filters.value.endDate ||
         filters.value.dateRange !== 'week'
})

// 生命周期钩子
onMounted(async () => {
  await loadHistory()
  await loadStatistics()
})

// 方法
async function loadHistory(append = false) {
  try {
    if (append) {
      loadingMore.value = true
    } else {
      loading.value = true
      currentPage.value = 1
      history.value = []
    }

    const request: SettingsHistoryQueryRequest = {
      page: currentPage.value,
      pageSize: pageSize.value,
      settingType: filters.value.settingType as SettingType || undefined,
      changeType: filters.value.changeType as ChangeType || undefined,
      status: filters.value.status as ChangeStatus || undefined,
      search: filters.value.search || undefined,
      startDate: filters.value.startDate || undefined,
      endDate: filters.value.endDate || undefined
    }

    // 根据时间范围设置日期
    if (filters.value.dateRange !== 'all' && !filters.value.startDate) {
      const now = new Date()
      let startDate = new Date()
      
      switch (filters.value.dateRange) {
        case 'today':
          startDate.setHours(0, 0, 0, 0)
          break
        case 'week':
          startDate.setDate(now.getDate() - 7)
          break
        case 'month':
          startDate.setMonth(now.getMonth() - 1)
          break
        case 'quarter':
          startDate.setMonth(now.getMonth() - 3)
          break
        case 'year':
          startDate.setFullYear(now.getFullYear() - 1)
          break
      }
      
      request.startDate = startDate.toISOString().split('T')[0]
    }

    const response = await settingsStore.getSettingsHistory(request)
    
    if (append) {
      history.value.push(...response.items)
    } else {
      history.value = response.items
    }
    
    hasMore.value = response.hasMore
    currentPage.value++
  } catch (error) {
    toast.error('加载历史记录失败')
  } finally {
    loading.value = false
    loadingMore.value = false
  }
}

async function loadStatistics() {
  try {
    statistics.value = await settingsStore.getSettingsHistoryStatistics()
  } catch (error) {
    toast.error('加载统计信息失败')
  }
}

async function refreshHistory() {
  await loadHistory()
  await loadStatistics()
  toast.success('历史记录已刷新')
}

async function applyFilters() {
  await loadHistory()
}

async function loadMore() {
  await loadHistory(true)
}

async function exportHistory() {
  try {
    const request: SettingsHistoryQueryRequest = {
      settingType: filters.value.settingType as SettingType || undefined,
      changeType: filters.value.changeType as ChangeType || undefined,
      status: filters.value.status as ChangeStatus || undefined,
      search: filters.value.search || undefined,
      startDate: filters.value.startDate || undefined,
      endDate: filters.value.endDate || undefined
    }
    
    await settingsStore.exportSettingsHistory(request)
    toast.success('历史记录已导出')
  } catch (error) {
    toast.error('导出历史记录失败')
  }
}

function showRecordDetail(record: SettingsHistoryDto) {
  selectedRecord.value = record
}

function closeDetail() {
  selectedRecord.value = null
}

// 格式化方法
function formatDateTime(dateString: string): string {
  return new Intl.DateTimeFormat('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  }).format(new Date(dateString))
}

function formatTime(dateString: string): string {
  return new Intl.DateTimeFormat('zh-CN', {
    hour: '2-digit',
    minute: '2-digit'
  }).format(new Date(dateString))
}

function formatValue(value: any, formatted = false): string {
  if (value === null || value === undefined) {
    return '无'
  }
  
  if (typeof value === 'object') {
    try {
      return formatted ? JSON.stringify(value, null, 2) : JSON.stringify(value)
    } catch {
      return String(value)
    }
  }
  
  return String(value)
}

function getSettingTypeText(type: SettingType): string {
  const types: Record<SettingType, string> = {
    site: '站点设置',
    security: '安全设置',
    feature: '功能设置',
    email: '邮件设置'
  }
  return types[type] || type
}

function getSettingTypeClass(type: SettingType): string {
  const classes: Record<SettingType, string> = {
    site: 'bg-primary',
    security: 'bg-danger',
    feature: 'bg-success',
    email: 'bg-info'
  }
  return classes[type] || 'bg-secondary'
}

function getChangeTypeText(type: ChangeType): string {
  const types: Record<ChangeType, string> = {
    create: '创建',
    update: '更新',
    delete: '删除'
  }
  return types[type] || type
}

function getChangeTypeClass(type: ChangeType): string {
  const classes: Record<ChangeType, string> = {
    create: 'bg-success',
    update: 'bg-warning',
    delete: 'bg-danger'
  }
  return classes[type] || 'bg-secondary'
}

function getStatusText(status: ChangeStatus): string {
  const statuses: Record<ChangeStatus, string> = {
    success: '成功',
    failed: '失败',
    pending: '待处理'
  }
  return statuses[status] || status
}

function getStatusClass(status: ChangeStatus): string {
  const classes: Record<ChangeStatus, string> = {
    success: 'bg-success',
    failed: 'bg-danger',
    pending: 'bg-warning'
  }
  return classes[status] || 'bg-secondary'
}
</script>

<style scoped>
.settings-history-view {
  padding: 1.5rem;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #e9ecef;
}

.header-content h1 {
  font-size: 2rem;
  font-weight: 600;
  margin: 0 0 0.5rem 0;
  color: #2c3e50;
}

.page-description {
  color: #6c757d;
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 0.5rem;
}

.statistics-section {
  margin-bottom: 2rem;
}

.stat-card {
  display: flex;
  align-items: center;
  padding: 1.5rem;
  background: white;
  border-radius: 0.5rem;
  box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
  border: 1px solid #e9ecef;
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 0.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 1rem;
  font-size: 1.5rem;
  color: white;
}

.stat-card:nth-child(1) .stat-icon { background: #007bff; }
.stat-card:nth-child(2) .stat-icon { background: #28a745; }
.stat-card:nth-child(3) .stat-icon { background: #ffc107; color: #212529; }
.stat-card:nth-child(4) .stat-icon { background: #6f42c1; }

.stat-content {
  flex: 1;
}

.stat-number {
  font-size: 1.5rem;
  font-weight: 600;
  color: #2c3e50;
  margin-bottom: 0.25rem;
}

.stat-label {
  color: #6c757d;
  font-size: 0.875rem;
}

.filters-section {
  margin-bottom: 2rem;
}

.card {
  border: none;
  box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
}

.card-header {
  background: #f8f9fa;
  border-bottom: 1px solid #dee2e6;
  padding: 1rem;
}

.card-header h3 {
  font-size: 1.1rem;
  font-weight: 600;
  margin: 0;
  color: #495057;
}

.card-header i {
  margin-right: 0.5rem;
  color: #6c757d;
}

.history-section .card-body {
  padding: 0;
}

.loading-overlay {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  color: #6c757d;
}

.loading-text {
  margin-top: 1rem;
  font-size: 1rem;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  color: #6c757d;
  text-align: center;
}

.empty-state i {
  font-size: 4rem;
  margin-bottom: 1rem;
  opacity: 0.5;
}

.empty-state h4 {
  margin-bottom: 0.5rem;
  color: #495057;
}

.history-list {
  max-height: 600px;
  overflow-y: auto;
}

.history-item {
  padding: 1rem;
  border-bottom: 1px solid #e9ecef;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.history-item:hover {
  background-color: #f8f9fa;
}

.history-item:last-child {
  border-bottom: none;
}

.history-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.5rem;
}

.history-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.setting-type-badge,
.change-type-badge,
.status-badge {
  padding: 0.25rem 0.5rem;
  border-radius: 0.25rem;
  font-size: 0.75rem;
  font-weight: 500;
  color: white;
}

.record-title {
  font-weight: 500;
  color: #2c3e50;
}

.history-meta {
  display: flex;
  gap: 1rem;
  margin-top: 0.25rem;
  font-size: 0.875rem;
  color: #6c757d;
}

.history-meta i {
  margin-right: 0.25rem;
}

.history-description {
  color: #6c757d;
  margin-bottom: 0.5rem;
  font-size: 0.875rem;
}

.history-details {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.5rem;
  font-size: 0.875rem;
}

.detail-item {
  display: flex;
  align-items: flex-start;
}

.detail-label {
  font-weight: 500;
  color: #6c757d;
  margin-right: 0.5rem;
  min-width: 80px;
}

.detail-value {
  color: #2c3e50;
  flex: 1;
  word-break: break-word;
}

.pagination-section {
  padding: 1rem;
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
  max-width: 800px;
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

.detail-section {
  margin-bottom: 1.5rem;
}

.detail-section h6 {
  font-weight: 600;
  margin-bottom: 0.75rem;
  color: #495057;
}

.detail-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 0.75rem;
}

.changes-comparison {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-top: 0.75rem;
}

.change-column h7 {
  font-weight: 600;
  margin-bottom: 0.5rem;
  color: #495057;
}

.change-content {
  background: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 0.25rem;
  padding: 0.75rem;
  font-size: 0.875rem;
  white-space: pre-wrap;
  word-break: break-word;
  max-height: 300px;
  overflow-y: auto;
}

@media (max-width: 768px) {
  .settings-history-view {
    padding: 1rem;
  }
  
  .page-header {
    flex-direction: column;
    gap: 1rem;
    align-items: flex-start;
  }
  
  .history-details {
    grid-template-columns: 1fr;
  }
  
  .detail-grid {
    grid-template-columns: 1fr;
  }
  
  .changes-comparison {
    grid-template-columns: 1fr;
  }
  
  .history-title {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.25rem;
  }
  
  .history-meta {
    flex-direction: column;
    gap: 0.25rem;
  }
}
</style>