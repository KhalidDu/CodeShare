<template>
  <AppLayout>
    <div class="version-compare-view">
      <div v-if="loading" class="loading-container">
        <div class="loading-spinner"></div>
        <p>加载版本比较中...</p>
      </div>

      <div v-else-if="error" class="error-container">
        <div class="error-icon">⚠️</div>
        <h2>加载失败</h2>
        <p>{{ error }}</p>
        <div class="error-actions">
          <button @click="loadComparison" class="btn btn-primary">重试</button>
          <router-link :to="`/snippets/${snippetId}`" class="btn btn-secondary">返回详情</router-link>
        </div>
      </div>

      <div v-else-if="comparison" class="compare-content">
        <!-- 头部导航 -->
        <div class="compare-header">
          <Breadcrumb :items="breadcrumbItems" />
          <div class="header-actions">
            <router-link :to="`/snippets/${snippetId}`" class="btn btn-secondary">
              返回详情
            </router-link>
          </div>
        </div>

        <!-- 版本比较组件 -->
        <div class="comparison-container">
          <VersionComparison :comparison="comparison" />
        </div>
      </div>
    </div>
  </AppLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppLayout from '@/components/layout/AppLayout.vue'
import Breadcrumb from '@/components/common/Breadcrumb.vue'
import VersionComparison from '@/components/snippets/VersionComparison.vue'
import { codeSnippetService } from '@/services/codeSnippetService'
import type { VersionComparison as VersionComparisonType } from '@/types'

// 路由
const route = useRoute()
const router = useRouter()

// 响应式数据
const comparison = ref<VersionComparisonType | null>(null)
const loading = ref(false)
const error = ref('')

// 计算属性
const snippetId = computed(() => route.params.snippetId as string)
const oldVersionId = computed(() => route.query.oldVersionId as string)
const newVersionId = computed(() => route.query.newVersionId as string)

const breadcrumbItems = computed(() => [
  { title: '首页', path: '/' },
  { title: '代码片段', path: '/snippets' },
  { title: '详情', path: `/snippets/${snippetId.value}` },
  { title: '版本比较', path: '' }
])

/**
 * 加载版本比较数据
 */
const loadComparison = async () => {
  if (!snippetId.value || !oldVersionId.value || !newVersionId.value) {
    error.value = '缺少必要的参数'
    return
  }

  loading.value = true
  error.value = ''

  try {
    comparison.value = await codeSnippetService.compareVersions(
      oldVersionId.value,
      newVersionId.value
    )
  } catch (err: any) {
    console.error('Failed to load version comparison:', err)
    error.value = err.response?.data?.message || '加载版本比较失败'
  } finally {
    loading.value = false
  }
}

// 生命周期
onMounted(() => {
  loadComparison()
})
</script>

<style scoped>
.version-compare-view {
  max-width: 1400px;
  margin: 0 auto;
  padding: 20px;
}

.loading-container,
.error-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  text-align: center;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #e1e5e9;
  border-top: 4px solid #0969da;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 16px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-container .error-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.error-container h2 {
  color: #d1242f;
  margin-bottom: 8px;
}

.error-actions {
  display: flex;
  gap: 12px;
  margin-top: 20px;
}

.compare-content {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.compare-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 20px;
  padding-bottom: 20px;
  border-bottom: 1px solid #e1e5e9;
}

.header-actions {
  display: flex;
  gap: 12px;
  flex-shrink: 0;
}

.comparison-container {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.btn {
  padding: 8px 16px;
  border: 1px solid;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background: #0969da;
  color: #fff;
  border-color: #0969da;
}

.btn-primary:hover:not(:disabled) {
  background: #0860ca;
  border-color: #0860ca;
}

.btn-secondary {
  background: #f6f8fa;
  color: #24292f;
  border-color: #d0d7de;
}

.btn-secondary:hover:not(:disabled) {
  background: #f3f4f6;
  border-color: #8c959f;
}

@media (max-width: 768px) {
  .version-compare-view {
    padding: 16px;
  }

  .compare-header {
    flex-direction: column;
    align-items: stretch;
  }

  .header-actions {
    justify-content: flex-end;
  }
}
</style>
