<template>
  <div class="version-history">
    <div class="version-history-header">
      <h3>版本历史</h3>
      <div v-if="selectedVersions.length === 2" class="compare-actions">
        <button 
          @click="compareVersions"
          class="btn btn-primary btn-sm"
        >
          快速比较
        </button>
        <button 
          @click="compareVersionsFullscreen"
          class="btn btn-secondary btn-sm"
        >
          全屏比较
        </button>
      </div>
    </div>

    <div v-if="loading" class="loading">
      <div class="spinner"></div>
      <span>加载版本历史中...</span>
    </div>

    <div v-else-if="error" class="error">
      <p>{{ error }}</p>
      <button @click="loadVersionHistory" class="btn btn-secondary btn-sm">重试</button>
    </div>

    <div v-else-if="versions.length === 0" class="empty-state">
      <p>暂无版本历史</p>
    </div>

    <div v-else class="version-list">
      <div 
        v-for="version in versions" 
        :key="version.id"
        class="version-item"
        :class="{ 
          'selected': selectedVersions.includes(version.id),
          'current': version.versionNumber === currentVersionNumber
        }"
      >
        <div class="version-header">
          <div class="version-info">
            <input 
              v-if="version.versionNumber !== currentVersionNumber"
              type="checkbox" 
              :value="version.id"
              v-model="selectedVersions"
              :disabled="selectedVersions.length >= 2 && !selectedVersions.includes(version.id)"
              class="version-checkbox"
            />
            <div class="version-details">
              <div class="version-number">
                版本 {{ version.versionNumber }}
                <span v-if="version.versionNumber === currentVersionNumber" class="current-badge">当前版本</span>
              </div>
              <div class="version-meta">
                <span class="creator">{{ version.creatorName }}</span>
                <span class="separator">•</span>
                <span class="date">{{ formatDate(version.createdAt) }}</span>
              </div>
            </div>
          </div>
          <div class="version-actions">
            <button 
              @click="viewVersion(version)"
              class="btn btn-outline btn-sm"
              title="查看此版本"
            >
              查看
            </button>
            <button 
              v-if="version.versionNumber !== currentVersionNumber && canRestore"
              @click="restoreVersion(version)"
              class="btn btn-secondary btn-sm"
              :disabled="restoring"
              title="恢复到此版本"
            >
              {{ restoring ? '恢复中...' : '恢复' }}
            </button>
          </div>
        </div>

        <div v-if="version.changeDescription" class="version-description">
          <p>{{ version.changeDescription }}</p>
        </div>

        <!-- 展开的版本详情 -->
        <div v-if="expandedVersion === version.id" class="version-content">
          <div class="version-code">
            <div class="code-header">
              <span class="language-badge">{{ version.language }}</span>
              <button @click="copyVersionCode(version)" class="btn btn-outline btn-sm">
                复制代码
              </button>
            </div>
            <pre><code :class="`language-${version.language}`">{{ version.code }}</code></pre>
          </div>
        </div>
      </div>
    </div>

    <!-- 版本比较模态框 -->
    <div v-if="showCompareModal" class="modal-overlay" @click="closeCompareModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h4>版本比较</h4>
          <button @click="closeCompareModal" class="btn-close">&times;</button>
        </div>
        <div class="modal-body">
          <VersionComparison 
            v-if="comparisonData"
            :comparison="comparisonData"
            @close="closeCompareModal"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { codeSnippetService } from '@/services/codeSnippetService'
import { usePermissions } from '@/composables/usePermissions'
import type { SnippetVersion, VersionComparison as VersionComparisonType } from '@/types'
import VersionComparison from './VersionComparison.vue'

interface Props {
  snippetId: string
  currentVersionNumber?: number
}

interface Emits {
  (e: 'version-restored', version: SnippetVersion): void
  (e: 'version-selected', version: SnippetVersion): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const router = useRouter()
const { canEditSnippet } = usePermissions()

// 响应式数据
const versions = ref<SnippetVersion[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const restoring = ref(false)
const expandedVersion = ref<string | null>(null)
const selectedVersions = ref<string[]>([])
const showCompareModal = ref(false)
const comparisonData = ref<VersionComparisonType | null>(null)

// 计算属性
const canRestore = computed(() => canEditSnippet.value)

/**
 * 加载版本历史
 */
const loadVersionHistory = async () => {
  if (!props.snippetId) return

  loading.value = true
  error.value = null

  try {
    versions.value = await codeSnippetService.getVersionHistory(props.snippetId)
  } catch (err) {
    error.value = '加载版本历史失败'
    console.error('Failed to load version history:', err)
  } finally {
    loading.value = false
  }
}

/**
 * 查看版本详情
 */
const viewVersion = (version: SnippetVersion) => {
  if (expandedVersion.value === version.id) {
    expandedVersion.value = null
  } else {
    expandedVersion.value = version.id
    emit('version-selected', version)
  }
}

/**
 * 恢复版本
 */
const restoreVersion = async (version: SnippetVersion) => {
  if (!canRestore.value) return

  const confirmed = confirm(`确定要恢复到版本 ${version.versionNumber} 吗？这将创建一个新版本。`)
  if (!confirmed) return

  restoring.value = true

  try {
    await codeSnippetService.restoreVersion(props.snippetId, version.id)
    emit('version-restored', version)
    await loadVersionHistory() // 重新加载版本历史
  } catch (err) {
    error.value = '恢复版本失败'
    console.error('Failed to restore version:', err)
  } finally {
    restoring.value = false
  }
}

/**
 * 比较版本
 */
const compareVersions = async () => {
  if (selectedVersions.value.length !== 2) return

  try {
    const [oldVersionId, newVersionId] = selectedVersions.value.sort((a, b) => {
      const versionA = versions.value.find(v => v.id === a)
      const versionB = versions.value.find(v => v.id === b)
      return (versionA?.versionNumber || 0) - (versionB?.versionNumber || 0)
    })

    comparisonData.value = await codeSnippetService.compareVersions(
      props.snippetId, 
      oldVersionId, 
      newVersionId
    )
    showCompareModal.value = true
  } catch (err) {
    error.value = '版本比较失败'
    console.error('Failed to compare versions:', err)
  }
}

/**
 * 关闭比较模态框
 */
const closeCompareModal = () => {
  showCompareModal.value = false
  comparisonData.value = null
  selectedVersions.value = []
}

/**
 * 全屏比较版本
 */
const compareVersionsFullscreen = () => {
  if (selectedVersions.value.length !== 2) return

  const [oldVersionId, newVersionId] = selectedVersions.value.sort((a, b) => {
    const versionA = versions.value.find(v => v.id === a)
    const versionB = versions.value.find(v => v.id === b)
    return (versionA?.versionNumber || 0) - (versionB?.versionNumber || 0)
  })

  router.push({
    name: 'version-compare',
    params: { snippetId: props.snippetId },
    query: { oldVersionId, newVersionId }
  })
}

/**
 * 复制版本代码
 */
const copyVersionCode = async (version: SnippetVersion) => {
  try {
    await navigator.clipboard.writeText(version.code)
    // 这里可以添加成功提示
  } catch (err) {
    console.error('Failed to copy code:', err)
    // 降级处理：选择文本
    const textArea = document.createElement('textarea')
    textArea.value = version.code
    document.body.appendChild(textArea)
    textArea.select()
    document.execCommand('copy')
    document.body.removeChild(textArea)
  }
}

/**
 * 格式化日期
 */
const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  const now = new Date()
  const diffInHours = (now.getTime() - date.getTime()) / (1000 * 60 * 60)

  if (diffInHours < 1) {
    return '刚刚'
  } else if (diffInHours < 24) {
    return `${Math.floor(diffInHours)} 小时前`
  } else if (diffInHours < 24 * 7) {
    return `${Math.floor(diffInHours / 24)} 天前`
  } else {
    return date.toLocaleDateString('zh-CN', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  }
}

// 监听 snippetId 变化
watch(() => props.snippetId, loadVersionHistory, { immediate: true })

// 组件挂载时加载数据
onMounted(() => {
  loadVersionHistory()
})
</script>

<style scoped>
.version-history {
  background: white;
  border-radius: 8px;
  border: 1px solid #e1e5e9;
}

.version-history-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  border-bottom: 1px solid #e1e5e9;
}

.version-history-header h3 {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
  color: #24292f;
}

.compare-actions {
  display: flex;
  gap: 0.5rem;
}

.loading, .error, .empty-state {
  padding: 2rem;
  text-align: center;
  color: #656d76;
}

.loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.spinner {
  width: 24px;
  height: 24px;
  border: 2px solid #e1e5e9;
  border-top: 2px solid #0969da;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error {
  color: #d1242f;
}

.version-list {
  max-height: 500px;
  overflow-y: auto;
}

.version-item {
  border-bottom: 1px solid #e1e5e9;
  transition: background-color 0.2s;
}

.version-item:hover {
  background-color: #f6f8fa;
}

.version-item.selected {
  background-color: #dbeafe;
}

.version-item.current {
  background-color: #f0f9ff;
  border-left: 3px solid #0969da;
}

.version-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
}

.version-info {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.version-checkbox {
  margin: 0;
}

.version-details {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.version-number {
  font-weight: 600;
  color: #24292f;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.current-badge {
  background: #0969da;
  color: white;
  font-size: 0.75rem;
  padding: 0.125rem 0.5rem;
  border-radius: 12px;
  font-weight: 500;
}

.version-meta {
  font-size: 0.875rem;
  color: #656d76;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.separator {
  color: #d0d7de;
}

.version-actions {
  display: flex;
  gap: 0.5rem;
}

.version-description {
  padding: 0 1rem 1rem 2.75rem;
  color: #656d76;
  font-size: 0.875rem;
}

.version-content {
  padding: 0 1rem 1rem 2.75rem;
}

.version-code {
  background: #f6f8fa;
  border-radius: 6px;
  overflow: hidden;
}

.code-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.5rem 1rem;
  background: #f1f3f4;
  border-bottom: 1px solid #d0d7de;
}

.language-badge {
  background: #0969da;
  color: white;
  font-size: 0.75rem;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-weight: 500;
}

.version-code pre {
  margin: 0;
  padding: 1rem;
  overflow-x: auto;
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
  font-size: 0.875rem;
  line-height: 1.45;
}

.version-code code {
  background: none;
  padding: 0;
}

/* 按钮样式 */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.375rem 0.75rem;
  font-size: 0.875rem;
  font-weight: 500;
  line-height: 1.5;
  border-radius: 6px;
  border: 1px solid transparent;
  text-decoration: none;
  cursor: pointer;
  transition: all 0.2s;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  color: white;
  background-color: #0969da;
  border-color: #0969da;
}

.btn-primary:hover:not(:disabled) {
  background-color: #0860ca;
  border-color: #0860ca;
}

.btn-secondary {
  color: #24292f;
  background-color: #f6f8fa;
  border-color: #d0d7de;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #f3f4f6;
  border-color: #d0d7de;
}

.btn-outline {
  color: #0969da;
  background-color: transparent;
  border-color: #d0d7de;
}

.btn-outline:hover:not(:disabled) {
  color: white;
  background-color: #0969da;
  border-color: #0969da;
}

.btn-sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}

/* 模态框样式 */
.modal-overlay {
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
}

.modal-content {
  background: white;
  border-radius: 8px;
  max-width: 90vw;
  max-height: 90vh;
  overflow: hidden;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  border-bottom: 1px solid #e1e5e9;
}

.modal-header h4 {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
}

.btn-close {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #656d76;
  padding: 0;
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-close:hover {
  color: #24292f;
}

.modal-body {
  padding: 0;
  overflow: auto;
  max-height: calc(90vh - 4rem);
}
</style>