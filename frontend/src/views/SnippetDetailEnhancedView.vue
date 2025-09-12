<template>
  <div class="snippet-detail-enhanced-view">
    <!-- 主容器组件 -->
    <SnippetDetailEnhanced
      :snippet-id="snippetId"
      :view-mode="viewMode"
      :theme="theme"
      :language="language"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import SnippetDetailEnhanced from '@/components/snippet/SnippetDetailEnhanced.vue'
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'

// 路由参数
const route = useRoute()
const router = useRouter()

// 响应式状态
const snippetId = computed(() => route.params.id as string)
const viewMode = computed(() => (route.query.view as string) || 'view')
const theme = computed(() => (route.query.theme as string) || 'vs-dark')
const language = computed(() => route.query.language as string)

// 认证状态
const authStore = useAuthStore()
const { showToast } = useToast()

// 页面标题
const pageTitle = computed(() => {
  return `代码片段详情 - ${snippetId.value}`
})

// 监听路由参数变化
onMounted(() => {
  // 设置页面标题
  document.title = pageTitle.value
  
  // 初始化页面
  initializePage()
})

/**
 * 初始化页面
 */
function initializePage() {
  // 检查用户认证状态（如果需要）
  if (viewMode.value === 'edit' && !authStore.isAuthenticated) {
    showToast('请先登录后再编辑代码片段', 'warning')
    router.push(`/snippets/${snippetId.value}/enhanced?view=view`)
  }
}

// 监听路由变化
watch(
  () => route.query,
  (newQuery) => {
    // 更新页面标题
    document.title = pageTitle.value
    
    // 处理视图模式变化
    if (newQuery.view === 'edit' && !authStore.isAuthenticated) {
      showToast('请先登录后再编辑代码片段', 'warning')
      router.push(`/snippets/${snippetId.value}/enhanced?view=view`)
    }
  },
  { deep: true }
)
</script>

<style scoped>
.snippet-detail-enhanced-view {
  width: 100%;
  height: 100vh;
  background: var(--snippet-bg-primary);
  overflow: hidden;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .snippet-detail-enhanced-view {
    height: 100vh;
  }
}

/* 深色模式支持 */
@media (prefers-color-scheme: dark) {
  .snippet-detail-enhanced-view {
    background: var(--snippet-bg-primary-dark);
  }
}
</style>