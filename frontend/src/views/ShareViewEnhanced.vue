<template>
  <div class="share-view-enhanced">
    <!-- 分享视图主容器 -->
    <SnippetDetailEnhanced
      :snippet-id="snippetId"
      view-mode="view"
      :theme="theme"
      :language="language"
      :is-share-view="true"
      :share-token="shareToken"
      @share-expired="handleShareExpired"
      @share-invalid="handleShareInvalid"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import SnippetDetailEnhanced from '@/components/snippet/SnippetDetailEnhanced.vue'
import { useToast } from '@/composables/useToast'

// 路由参数
const route = useRoute()
const router = useRouter()

// 响应式状态
const shareToken = computed(() => route.params.token as string)
const theme = computed(() => (route.query.theme as string) || 'vs-dark')
const language = computed(() => route.query.language as string)
const viewMode = computed(() => (route.query.view as string) || 'view')

// 内部状态
const snippetId = ref<string>('')
const isLoading = ref(true)
const hasError = ref(false)
const errorMessage = ref('')

// 工具函数
const { showToast } = useToast()

// 页面标题
const pageTitle = computed(() => {
  return '分享的代码片段'
})

// 生命周期
onMounted(async () => {
  // 设置页面标题
  document.title = pageTitle.value
  
  // 验证分享链接并获取代码片段信息
  await validateShareLink()
})

/**
 * 验证分享链接
 */
async function validateShareLink() {
  try {
    isLoading.value = true
    hasError.value = false
    
    // 这里应该调用 API 验证分享令牌并获取代码片段信息
    // const response = await shareService.validateShareToken(shareToken.value)
    // snippetId.value = response.snippetId
    
    // 模拟 API 调用
    await new Promise(resolve => setTimeout(resolve, 1000))
    
    // 临时模拟数据，实际应该从 API 获取
    snippetId.value = 'shared-snippet'
    
    isLoading.value = false
  } catch (error) {
    console.error('Failed to validate share link:', error)
    hasError.value = true
    errorMessage.value = '分享链接无效或已过期'
    isLoading.value = false
    showToast('分享链接无效或已过期', 'error')
    
    // 重定向到错误页面或首页
    setTimeout(() => {
      router.push('/')
    }, 3000)
  }
}

/**
 * 处理分享过期
 */
function handleShareExpired() {
  hasError.value = true
  errorMessage.value = '分享链接已过期'
  showToast('分享链接已过期', 'warning')
  
  setTimeout(() => {
    router.push('/')
  }, 3000)
}

/**
 * 处理分享链接无效
 */
function handleShareInvalid() {
  hasError.value = true
  errorMessage.value = '分享链接无效'
  showToast('分享链接无效', 'error')
  
  setTimeout(() => {
    router.push('/')
  }, 3000)
}
</script>

<style scoped>
.share-view-enhanced {
  width: 100%;
  height: 100vh;
  background: var(--snippet-bg-primary);
  overflow: hidden;
  position: relative;
}

/* 加载状态 */
.share-view-enhanced::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: var(--snippet-bg-overlay);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: var(--snippet-z-index-modal);
  opacity: v-bind('isLoading ? 1 : 0');
  pointer-events: v-bind('isLoading ? "auto" : "none"');
  transition: opacity var(--snippet-transition-normal);
}

/* 错误状态 */
.error-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: var(--snippet-bg-overlay);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: var(--snippet-z-index-modal);
}

.error-content {
  text-align: center;
  padding: var(--snippet-spacing-xl);
  background: var(--snippet-bg-secondary);
  border-radius: var(--snippet-radius-lg);
  border: 1px solid var(--snippet-border-primary);
  box-shadow: var(--snippet-shadow-lg);
  max-width: 400px;
}

.error-icon {
  width: 64px;
  height: 64px;
  margin: 0 auto var(--snippet-spacing-lg);
  color: var(--snippet-color-error);
}

.error-title {
  font-size: var(--snippet-font-size-heading-md);
  font-weight: var(--snippet-font-weight-bold);
  color: var(--snippet-text-primary);
  margin-bottom: var(--snippet-spacing-md);
}

.error-message {
  font-size: var(--snippet-font-size-body);
  color: var(--snippet-text-secondary);
  margin-bottom: var(--snippet-spacing-lg);
}

.error-action {
  display: inline-flex;
  align-items: center;
  gap: var(--snippet-spacing-sm);
  padding: var(--snippet-spacing-sm) var(--snippet-spacing-lg);
  background: var(--snippet-bg-brand);
  color: white;
  border: none;
  border-radius: var(--snippet-radius-md);
  font-size: var(--snippet-font-size-body-sm);
  font-weight: var(--snippet-font-weight-medium);
  cursor: pointer;
  transition: all var(--snippet-transition-normal);
}

.error-action:hover {
  background: var(--snippet-color-brand-dark);
  transform: var(--snippet-transform-translate-y-hover);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .share-view-enhanced {
    height: 100vh;
  }
  
  .error-content {
    margin: var(--snippet-spacing-md);
    padding: var(--snippet-spacing-lg);
  }
  
  .error-icon {
    width: 48px;
    height: 48px;
  }
  
  .error-title {
    font-size: var(--snippet-font-size-heading-sm);
  }
  
  .error-message {
    font-size: var(--snippet-font-size-body-sm);
  }
}

/* 深色模式支持 */
@media (prefers-color-scheme: dark) {
  .share-view-enhanced {
    background: var(--snippet-bg-primary-dark);
  }
}
</style>