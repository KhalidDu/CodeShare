<template>
  <div id="app">
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
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterView } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import AppLayout from '@/components/layout/AppLayout.vue'

const authStore = useAuthStore()

const isAuthenticated = computed(() => authStore.isAuthenticated)
</script>

<style>
/* 全局样式重置 */
* {
  box-sizing: border-box;
}

html {
  font-size: 16px;
  line-height: 1.5;
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
