<template>
  <div id="app">
    <!-- 错误边界包装整个应用 -->
    <ErrorBoundary>
      <!-- 未认证用户显示简单布局 -->
      <template v-if="!isAuthenticated">
        <RouterView />
      </template>

      <!-- 已认证用户显示完整布局 -->
      <template v-else>
        <AppLayout>
          <RouterView />
        </AppLayout>
      </template>
    </ErrorBoundary>

    <!-- 全局错误通知容器 -->
    <ErrorContainer @retry="handleErrorRetry" />

    <!-- Toast 通知容器 -->
    <ToastContainer />

    <!-- 全局加载遮罩 -->
    <LoadingOverlay
      v-if="loadingStore.isLoading && loadingStore.primaryLoading"
      :visible="true"
      :message="loadingStore.primaryLoading.message"
      :progress="loadingStore.primaryLoading.progress"
      :show-progress="loadingStore.primaryLoading.progress !== undefined"
      :cancellable="loadingStore.primaryLoading.cancellable"
      @cancel="handleLoadingCancel"
    />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterView } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useLoadingStore } from '@/stores/loading'
import { useErrorHandler } from '@/composables/useErrorHandler'
import AppLayout from '@/components/layout/AppLayout.vue'
import ErrorBoundary from '@/components/common/ErrorBoundary.vue'
import ErrorContainer from '@/components/common/ErrorContainer.vue'
import ToastContainer from '@/components/common/ToastContainer.vue'
import LoadingOverlay from '@/components/common/LoadingOverlay.vue'

const authStore = useAuthStore()
const loadingStore = useLoadingStore()
const { clearError } = useErrorHandler()

const isAuthenticated = computed(() => authStore.isAuthenticated)

/**
 * 处理错误重试
 */
function handleErrorRetry(errorId: string, error: any) {
  console.log('Retrying error:', errorId, error)

  // 根据错误类型执行不同的重试逻辑
  switch (error.type) {
    case 'NETWORK':
      // 网络错误重试 - 可以重新发送请求
      handleNetworkRetry(error)
      break
    case 'AUTHENTICATION':
      // 认证错误重试 - 尝试刷新 token
      handleAuthRetry(error)
      break
    default:
      // 其他错误 - 简单清除错误
      clearError(errorId)
  }
}

/**
 * 处理网络错误重试
 */
function handleNetworkRetry(error: any) {
  // 这里可以实现具体的网络重试逻辑
  // 例如重新发送失败的请求
  console.log('Retrying network operation:', error)
}

/**
 * 处理认证错误重试
 */
function handleAuthRetry(error: any) {
  // 尝试刷新认证状态
  authStore.initialize()
}

/**
 * 处理加载取消
 */
function handleLoadingCancel() {
  // 清除所有加载状态
  loadingStore.clearAllLoading()
}
</script>

<style>
/* 全局样式重置 */
* {
  box-sizing: border-box;
}

html {
  font-size: 16px;
  line-height: 1.5;
  height: 100%;
}

body {
  margin: 0;
  padding: 0;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
    'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue',
    sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  background-color: #f8f9fa;
  color: #212529;
  min-height: 100%;
}

#app {
  min-height: 100vh;
}

/* 链接样式 */
a {
  color: #007bff;
  text-decoration: none;
}

a:hover {
  color: #0056b3;
  text-decoration: underline;
}

/* 按钮基础样式 */
button {
  font-family: inherit;
  font-size: inherit;
  line-height: inherit;
}

/* 表单元素样式 */
input,
textarea,
select {
  font-family: inherit;
  font-size: inherit;
  line-height: inherit;
}

/* 滚动条样式 */
::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 4px;
}

::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  body {
    background-color: white;
    color: black;
  }
}

/* 打印样式 */
@media print {
  body {
    background-color: white;
    color: black;
  }

  * {
    box-shadow: none !important;
    text-shadow: none !important;
  }
}
</style>
