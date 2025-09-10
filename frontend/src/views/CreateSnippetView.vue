<template>
  <AppLayout>
    <div class="create-snippet-view">
      <div class="view-header">
        <Breadcrumb :items="breadcrumbItems" />
        <PageTitle title="创建代码片段" />
      </div>

      <div class="view-content">
        <SnippetForm
          :is-editing="false"
          @submit="onSubmit"
          @cancel="onCancel"
        />
      </div>
    </div>
  </AppLayout>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import AppLayout from '@/components/layout/AppLayout.vue'
import Breadcrumb from '@/components/common/Breadcrumb.vue'
import PageTitle from '@/components/common/PageTitle.vue'
import SnippetForm from '@/components/snippets/SnippetForm.vue'
import { codeSnippetService } from '@/services/codeSnippetService'
import type { CreateSnippetRequest } from '@/types'

// 路由
const router = useRouter()

// 计算属性
const breadcrumbItems = computed(() => [
  { title: '首页', path: '/' },
  { title: '代码片段', path: '/snippets' },
  { title: '创建', path: '' }
])

/**
 * 表单提交处理
 */
const onSubmit = async (data: CreateSnippetRequest) => {
  try {
    const createdSnippet = await codeSnippetService.createSnippet(data)

    // 创建成功后跳转到详情页
    router.push(`/snippets/${createdSnippet.id}`)

    console.log('代码片段创建成功')
  } catch (error: any) {
    console.error('Failed to create snippet:', error)

    // 这里可以添加错误提示逻辑
    const errorMessage = error.response?.data?.message || '创建代码片段失败'
    alert(errorMessage) // 临时使用 alert，后续可以替换为更好的提示组件
  }
}

/**
 * 取消处理
 */
const onCancel = () => {
  router.push('/snippets')
}
</script>

<style scoped>
.create-snippet-view {
  max-width: 1200px;
  margin: 0 auto;
}

.view-header {
  margin-bottom: var(--spacing-xl);
}

.view-content {
  /* 内容样式由 SnippetForm 组件处理 */
}

@media (max-width: 768px) {
  .create-snippet-view {
    padding: 16px;
  }

  .view-header {
    margin-bottom: 20px;
  }
}
</style>
