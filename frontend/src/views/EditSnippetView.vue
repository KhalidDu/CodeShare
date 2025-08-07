<template>
  <AppLayout>
    <div class="edit-snippet-view">
      <div class="view-header">
        <Breadcrumb :items="breadcrumbItems" />
        <PageTitle title="编辑代码片段" />
      </div>

      <div v-if="loading" class="loading-container">
        <div class="loading-spinner"></div>
        <p>加载中...</p>
      </div>

      <div v-else-if="error" class="error-container">
        <div class="error-icon">⚠️</div>
        <h2>加载失败</h2>
        <p>{{ error }}</p>
        <div class="error-actions">
          <button @click="loadSnippet" class="btn btn-primary">重试</button>
          <router-link to="/snippets" class="btn btn-secondary">返回列表</router-link>
        </div>
      </div>

      <div v-else-if="snippet" class="view-content">
        <SnippetForm
          :snippet="snippet"
          :is-editing="true"
          @submit="onSubmit"
          @cancel="onCancel"
        />
      </div>
    </div>
  </AppLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppLayout from '@/components/layout/AppLayout.vue'
import Breadcrumb from '@/components/common/Breadcrumb.vue'
import PageTitle from '@/components/common/PageTitle.vue'
import SnippetForm from '@/components/snippets/SnippetForm.vue'
import { codeSnippetService } from '@/services/codeSnippetService'
import { useAuthStore } from '@/stores/auth'
import type { CodeSnippet, UpdateSnippetRequest } from '@/types'

// 路由和认证
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

// 响应式数据
const snippet = ref<CodeSnippet | null>(null)
const loading = ref(false)
const error = ref('')

// 计算属性
const snippetId = computed(() => route.params.id as string)

const breadcrumbItems = computed(() => [
  { title: '首页', path: '/' },
  { title: '代码片段', path: '/snippets' },
  { title: snippet.value?.title || '详情', path: `/snippets/${snippetId.value}` },
  { title: '编辑', path: '' }
])

/**
 * 检查编辑权限
 */
const checkEditPermission = (): boolean => {
  if (!snippet.value || !authStore.user) return false

  // 管理员可以编辑所有片段
  if (authStore.user.role === 2) return true

  // 编辑者可以编辑自己的片段
  if (authStore.user.role === 1 && snippet.value.createdBy === authStore.user.id) return true

  return false
}

/**
 * 加载代码片段
 */
const loadSnippet = async () => {
  if (!snippetId.value) {
    error.value = '无效的代码片段ID'
    return
  }

  loading.value = true
  error.value = ''

  try {
    snippet.value = await codeSnippetService.getSnippet(snippetId.value)

    // 检查编辑权限
    if (!checkEditPermission()) {
      error.value = '您没有权限编辑此代码片段'
      return
    }

  } catch (err: any) {
    console.error('Failed to load snippet:', err)
    error.value = err.response?.data?.message || '加载代码片段失败'
  } finally {
    loading.value = false
  }
}

/**
 * 表单提交处理
 */
const onSubmit = async (data: UpdateSnippetRequest) => {
  if (!snippet.value) return

  try {
    const updatedSnippet = await codeSnippetService.updateSnippet(snippet.value.id, data)

    // 更新成功后跳转到详情页
    router.push(`/snippets/${updatedSnippet.id}`)

    console.log('代码片段更新成功')
  } catch (error: any) {
    console.error('Failed to update snippet:', error)

    // 这里可以添加错误提示逻辑
    const errorMessage = error.response?.data?.message || '更新代码片段失败'
    alert(errorMessage) // 临时使用 alert，后续可以替换为更好的提示组件
  }
}

/**
 * 取消处理
 */
const onCancel = () => {
  if (snippet.value) {
    router.push(`/snippets/${snippet.value.id}`)
  } else {
    router.push('/snippets')
  }
}

// 生命周期
onMounted(() => {
  loadSnippet()
})
</script>

<style scoped>
.edit-snippet-view {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.view-header {
  margin-bottom: 24px;
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

.btn-primary {
  background: #0969da;
  color: #fff;
  border-color: #0969da;
}

.btn-primary:hover {
  background: #0860ca;
  border-color: #0860ca;
}

.btn-secondary {
  background: #f6f8fa;
  color: #24292f;
  border-color: #d0d7de;
}

.btn-secondary:hover {
  background: #f3f4f6;
  border-color: #8c959f;
}

@media (max-width: 768px) {
  .edit-snippet-view {
    padding: 16px;
  }

  .view-header {
    margin-bottom: 20px;
  }
}
</style>
