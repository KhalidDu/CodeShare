<template>
  <div class="test-container">
    <h1>分享统计组件测试</h1>
    
    <div class="test-controls">
      <button @click="showStats" class="btn-test">
        显示统计对话框
      </button>
    </div>
    
    <!-- 分享管理组件 -->
    <ShareManagement />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import ShareManagement from '@/components/sharing/ShareManagement.vue'

const showStats = () => {
  // 这里可以通过点击分享管理页面中的统计按钮来测试
  console.log('点击分享管理页面中的统计按钮来测试')
}
</script>

<style scoped>
.test-container {
  padding: 2rem;
}

.test-controls {
  margin-bottom: 2rem;
}

.btn-test {
  padding: 0.75rem 1.5rem;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  cursor: pointer;
}

.btn-test:hover {
  background: #2563eb;
}
</style>