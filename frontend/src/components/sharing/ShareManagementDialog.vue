<template>
  <div class="dialog-overlay" @click="$emit('close')">
    <div class="dialog share-management-dialog" @click.stop>
      <div class="dialog-header">
        <div class="dialog-icon">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m9.632 4.316C18.114 15.062 18 14.518 18 14c0-.482.114-.938.316-1.342m0 2.684a3 3 0 110-2.684M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
        </div>
        <h2>分享管理</h2>
        <button class="dialog-close" @click="$emit('close')">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <div class="dialog-content">
        <!-- 加载状态 -->
        <div v-if="loading" class="loading-state">
          <div class="loading-spinner"></div>
          <p>加载中...</p>
        </div>

        <!-- 分享令牌列表 -->
        <div v-else-if="shareTokens.length > 0" class="share-tokens-list">
          <div class="list-header">
            <h3>分享链接列表</h3>
            <button
              @click="showCreateDialog = true"
              class="btn btn-primary btn-sm"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              新建分享
            </button>
          </div>

          <div class="tokens-grid">
            <div
              v-for="token in shareTokens"
              :key="token.id"
              class="token-card"
              :class="{
                'expired': token.status === 'EXPIRED',
                'revoked': token.status === 'REVOKED'
              }"
            >
              <div class="token-header">
                <div class="token-status">
                  <span
                    class="status-badge"
                    :class="token.status.toLowerCase()"
                  >
                    {{ getStatusLabel(token.status) }}
                  </span>
                  <span
                    class="permission-badge"
                    :class="token.permission.toLowerCase()"
                  >
                    {{ getPermissionLabel(token.permission) }}
                  </span>
                </div>
                <div class="token-actions">
                  <button
                    @click="copyShareLink(token.token)"
                    class="action-btn copy"
                    title="复制分享链接"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                    </svg>
                  </button>
                  <button
                    @click="viewStats(token)"
                    class="action-btn stats"
                    title="查看统计"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                    </svg>
                  </button>
                  <button
                    v-if="token.status === 'ACTIVE'"
                    @click="revokeToken(token)"
                    class="action-btn revoke"
                    title="撤销分享"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636" />
                    </svg>
                  </button>
                  <button
                    @click="deleteToken(token)"
                    class="action-btn delete"
                    title="删除分享"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                  </button>
                </div>
              </div>

              <div class="token-info">
                <div class="token-display">
                  <code class="token-code">{{ token.token }}</code>
                </div>
                <div class="token-meta">
                  <div class="meta-item">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                    </svg>
                    <span>{{ token.currentAccessCount }} 次访问</span>
                  </div>
                  <div class="meta-item">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <span>创建于 {{ formatDate(token.createdAt) }}</span>
                  </div>
                  <div v-if="token.expiresAt" class="meta-item">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <span>过期于 {{ formatDate(token.expiresAt) }}</span>
                  </div>
                </div>
              </div>

              <div v-if="token.isPasswordProtected" class="token-protected">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2h-8a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                </svg>
                <span>密码保护</span>
              </div>
            </div>
          </div>
        </div>

        <!-- 空状态 -->
        <div v-else class="empty-state">
          <div class="empty-icon">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m9.632 4.316C18.114 15.062 18 14.518 18 14c0-.482.114-.938.316-1.342m0 2.684a3 3 0 110-2.684M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
          </div>
          <div class="empty-content">
            <h3>暂无分享链接</h3>
            <p>您还没有创建任何分享链接</p>
            <button
              @click="showCreateDialog = true"
              class="btn btn-primary"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              创建第一个分享链接
            </button>
          </div>
        </div>
      </div>

      <div class="dialog-actions">
        <button
          @click="$emit('close')"
          class="btn btn-secondary"
        >
          关闭
        </button>
      </div>
    </div>
  </div>

  <!-- 创建分享对话框 -->
  <ShareDialog
    v-if="showCreateDialog"
    :snippet-id="snippetId"
    :snippet-title="snippetTitle"
    :visible="showCreateDialog"
    @cancel="showCreateDialog = false"
    @share-created="handleShareCreated"
  />

  <!-- 统计对话框 -->
  <div v-if="showStatsDialog" class="stats-dialog-overlay" @click="closeStatsDialog">
    <div class="stats-dialog" @click.stop>
      <div class="stats-dialog-header">
        <h2 class="stats-dialog-title">分享统计</h2>
        <button @click="closeStatsDialog" class="close-btn">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
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
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useShareStore } from '@/stores/share'
import { useToastStore } from '@/stores/toast'
import { useAuthStore } from '@/stores/auth'
import { shareService } from '@/services/shareService'
import { ShareTokenStatus, SharePermission } from '@/types/share'
import ShareDialog from './ShareDialog.vue'
import ShareStats from './ShareStats.vue'
import type { ShareToken } from '@/types/share'

interface Props {
  snippetId: string
  snippetTitle?: string
  visible?: boolean
}

interface Emits {
  (e: 'close'): void
  (e: 'share-updated'): void
}

const props = withDefaults(defineProps<Props>(), {
  visible: false
})

const emit = defineEmits<Emits>()

// Store注入
const shareStore = useShareStore()
const toastStore = useToastStore()
const authStore = useAuthStore()

// 响应式状态
const loading = ref(false)
const shareTokens = ref<ShareToken[]>([])
const showCreateDialog = ref(false)
const showStatsDialog = ref(false)
const selectedStatsTokenId = ref('')

// 生命周期
onMounted(async () => {
  if (props.visible) {
    await loadShareTokens()
  }
})

// 监听visible变化
watch(() => props.visible, async (newVisible) => {
  if (newVisible) {
    await loadShareTokens()
  }
})

// 方法
async function loadShareTokens() {
  if (!props.snippetId) return

  loading.value = true
  try {
    const result = await shareService.getSnippetShareTokens(props.snippetId)
    shareTokens.value = result
  } catch (error) {
    toastStore.error('加载分享链接失败')
    console.error('Failed to load share tokens:', error)
  } finally {
    loading.value = false
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

function viewStats(token: ShareToken) {
  selectedStatsTokenId.value = token.id
  showStatsDialog.value = true
}

function revokeToken(token: ShareToken) {
  // 这里可以添加确认对话框
  handleRevokeToken(token.id)
}

async function handleRevokeToken(tokenId: string) {
  try {
    await shareStore.revokeShareToken({ tokenId })
    toastStore.success('分享链接已撤销')
    await loadShareTokens()
    emit('share-updated')
  } catch (error) {
    toastStore.error('撤销分享链接失败')
  }
}

async function deleteToken(token: ShareToken) {
  // 这里可以添加确认对话框
  handleDeleteToken(token.id)
}

async function handleDeleteToken(tokenId: string) {
  try {
    await shareStore.deleteShareToken(tokenId)
    toastStore.success('分享链接已删除')
    await loadShareTokens()
    emit('share-updated')
  } catch (error) {
    toastStore.error('删除分享链接失败')
  }
}

function handleShareCreated(shareToken: ShareToken) {
  showCreateDialog.value = false
  toastStore.success('分享链接创建成功！')
  loadShareTokens()
  emit('share-updated')
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
  handleRevokeToken(tokenId)
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

.share-management-dialog {
  background: white;
  border-radius: 12px;
  width: 100%;
  max-width: 800px;
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

/* 加载状态 */
.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 2rem;
  color: #6b7280;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f4f6;
  border-top: 4px solid #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 列表头部 */
.list-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.list-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #1f2937;
}

/* 令牌网格 */
.tokens-grid {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

/* 令牌卡片 */
.token-card {
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 1rem;
  transition: all 0.2s;
}

.token-card:hover {
  border-color: #d1d5db;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

.token-card.expired {
  opacity: 0.7;
  background: #fef3c7;
  border-color: #f59e0b;
}

.token-card.revoked {
  opacity: 0.5;
  background: #fee2e2;
  border-color: #ef4444;
}

.token-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.75rem;
}

.token-status {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.status-badge {
  padding: 0.25rem 0.5rem;
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
  padding: 0.25rem 0.5rem;
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

.token-actions {
  display: flex;
  gap: 0.25rem;
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

.action-btn.copy {
  background: #e0e7ff;
  color: #3730a3;
}

.action-btn.copy:hover {
  background: #c7d2fe;
}

.action-btn.stats {
  background: #dbeafe;
  color: #1e40af;
}

.action-btn.stats:hover {
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

.token-info {
  margin-bottom: 0.75rem;
}

.token-display {
  margin-bottom: 0.5rem;
}

.token-code {
  font-family: monospace;
  font-size: 0.875rem;
  background: white;
  padding: 0.5rem;
  border-radius: 4px;
  color: #374151;
  display: block;
  word-break: break-all;
}

.token-meta {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: #6b7280;
}

.token-protected {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem;
  background: #fef3c7;
  border: 1px solid #f59e0b;
  border-radius: 6px;
  font-size: 0.875rem;
  color: #92400e;
}

/* 空状态 */
.empty-state {
  text-align: center;
  padding: 3rem 2rem;
  color: #6b7280;
}

.empty-icon {
  width: 64px;
  height: 64px;
  margin: 0 auto 1rem;
  opacity: 0.5;
}

.empty-content h3 {
  margin: 0 0 0.5rem 0;
  font-size: 1.125rem;
  font-weight: 600;
  color: #374151;
}

.empty-content p {
  margin: 0 0 1.5rem 0;
  color: #6b7280;
}

/* 按钮样式 */
.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
}

.btn-sm {
  padding: 0.375rem 0.75rem;
  font-size: 0.75rem;
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
  z-index: 1001;
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
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
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
  font-size: 1.25rem;
  font-weight: 600;
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
@media (max-width: 768px) {
  .share-management-dialog {
    margin: 10px;
    max-width: none;
  }

  .list-header {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }

  .token-header {
    flex-direction: column;
    gap: 0.5rem;
  }

  .token-actions {
    justify-content: center;
  }

  .stats-dialog-overlay {
    padding: 1rem;
  }

  .stats-dialog {
    width: 100%;
    height: 90vh;
    border-radius: 8px;
  }
}

@media (max-width: 480px) {
  .dialog-overlay {
    padding: 10px;
  }

  .share-management-dialog {
    border-radius: 8px;
  }

  .dialog-header {
    padding: 16px 16px 12px 16px;
  }

  .dialog-content {
    padding: 16px;
  }

  .stats-dialog-overlay {
    padding: 0.5rem;
  }

  .stats-dialog {
    height: 95vh;
    border-radius: 0;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .token-card,
  .action-btn,
  .btn,
  .dialog-close,
  .close-btn {
    transition: none;
  }

  .loading-spinner {
    animation: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .share-management-dialog,
  .stats-dialog {
    border: 2px solid #000;
  }

  .token-card {
    border-width: 2px;
  }

  .action-btn,
  .btn {
    border: 1px solid currentColor;
  }
}
</style>