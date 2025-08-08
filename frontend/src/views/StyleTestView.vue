<template>
  <div class="p-8 bg-gray-100 min-h-screen">
    <h1 class="text-3xl font-bold text-gray-800 mb-8">样式测试页面</h1>

    <!-- Tailwind CSS 测试 -->
    <div class="bg-white rounded-lg shadow-md p-6 mb-6">
      <h2 class="text-xl font-semibold mb-4">Tailwind CSS 测试</h2>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div class="bg-blue-500 text-white p-4 rounded">蓝色卡片</div>
        <div class="bg-green-500 text-white p-4 rounded">绿色卡片</div>
        <div class="bg-red-500 text-white p-4 rounded">红色卡片</div>
      </div>
    </div>

    <!-- 设计系统测试 -->
    <div class="card p-6 mb-6">
      <h2 class="text-xl font-semibold mb-4">设计系统测试</h2>
      <div class="flex gap-4 mb-4">
        <button class="btn btn-primary">主要按钮</button>
        <button class="btn btn-secondary">次要按钮</button>
      </div>
      <input class="input mb-4" placeholder="输入框测试" />
      <div class="flex gap-2">
        <span class="tag" style="background-color: #007bff;">标签1</span>
        <span class="tag" style="background-color: #28a745;">标签2</span>
        <span class="tag" style="background-color: #dc3545;">标签3</span>
      </div>
    </div>

    <!-- 组件测试 -->
    <div class="bg-white rounded-lg shadow-md p-6">
      <h2 class="text-xl font-semibold mb-4">组件测试</h2>
      <SkeletonLoader type="card" class="mb-4" />
      <SkeletonLoader type="text" :lines="3" />
    </div>
  </div>
</template>

<script setup lang="ts">
import SkeletonLoader from '@/components/common/SkeletonLoader.vue'
</script>
