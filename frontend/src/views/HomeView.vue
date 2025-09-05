<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-50 via-white to-slate-100">
    <!-- Hero Section -->
    <div class="relative overflow-hidden">
      <!-- 背景装饰 -->
      <div class="absolute inset-0">
        <div class="absolute top-20 left-10 w-72 h-72 bg-blue-100 rounded-full mix-blend-multiply filter blur-xl opacity-70 animate-blob"></div>
        <div class="absolute top-40 right-10 w-72 h-72 bg-purple-100 rounded-full mix-blend-multiply filter blur-xl opacity-70 animate-blob animation-delay-2000"></div>
        <div class="absolute -bottom-8 left-20 w-72 h-72 bg-yellow-100 rounded-full mix-blend-multiply filter blur-xl opacity-70 animate-blob animation-delay-4000"></div>
      </div>

      <!-- 主要内容 -->
      <div class="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-24">
        <div class="text-center">
          <!-- Logo 和标题 -->
          <div class="flex justify-center mb-8">
            <div class="relative">
              <div class="w-20 h-20 bg-gradient-to-br from-blue-500 to-blue-600 rounded-2xl flex items-center justify-center shadow-lg">
                <svg class="w-12 h-12 text-white" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z" />
                </svg>
              </div>
              <div class="absolute -top-2 -right-2 w-6 h-6 bg-green-500 rounded-full flex items-center justify-center">
                <svg class="w-4 h-4 text-white" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M9,20.42L2.79,14.21L5.62,11.38L9,14.77L18.88,4.88L21.71,7.71L9,20.42Z" />
                </svg>
              </div>
            </div>
          </div>

          <h1 class="text-5xl md:text-6xl font-bold bg-gradient-to-r from-slate-900 via-blue-900 to-slate-900 bg-clip-text text-transparent mb-6">
            代码片段管理工具
          </h1>
          <p class="text-xl md:text-2xl text-slate-600 mb-12 max-w-3xl mx-auto leading-relaxed">
            欢迎使用代码片段管理工具！一个支持团队协作的代码片段存储、管理和分享平台。
          </p>

          <!-- 未登录用户显示的内容 -->
          <div v-if="!user" class="space-y-8">
            <!-- 功能特性 -->
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 max-w-5xl mx-auto mb-12">
              <div class="glass rounded-2xl p-6 text-center group hover:shadow-lg transition-all duration-300">
                <div class="w-12 h-12 bg-blue-100 rounded-xl flex items-center justify-center mx-auto mb-4 group-hover:scale-110 transition-transform duration-300">
                  <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                </div>
                <h3 class="text-lg font-semibold text-slate-900 mb-2">存储和管理</h3>
                <p class="text-slate-600 text-sm">安全存储和组织您的代码片段</p>
              </div>

              <div class="glass rounded-2xl p-6 text-center group hover:shadow-lg transition-all duration-300">
                <div class="w-12 h-12 bg-green-100 rounded-xl flex items-center justify-center mx-auto mb-4 group-hover:scale-110 transition-transform duration-300">
                  <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                </div>
                <h3 class="text-lg font-semibold text-slate-900 mb-2">智能搜索</h3>
                <p class="text-slate-600 text-sm">强大的搜索和筛选功能</p>
              </div>

              <div class="glass rounded-2xl p-6 text-center group hover:shadow-lg transition-all duration-300">
                <div class="w-12 h-12 bg-purple-100 rounded-xl flex items-center justify-center mx-auto mb-4 group-hover:scale-110 transition-transform duration-300">
                  <svg class="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
                  </svg>
                </div>
                <h3 class="text-lg font-semibold text-slate-900 mb-2">标签分类</h3>
                <p class="text-slate-600 text-sm">灵活的标签分类系统</p>
              </div>

              <div class="glass rounded-2xl p-6 text-center group hover:shadow-lg transition-all duration-300">
                <div class="w-12 h-12 bg-yellow-100 rounded-xl flex items-center justify-center mx-auto mb-4 group-hover:scale-110 transition-transform duration-300">
                  <svg class="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                  </svg>
                </div>
                <h3 class="text-lg font-semibold text-slate-900 mb-2">团队协作</h3>
                <p class="text-slate-600 text-sm">支持团队协作和分享</p>
              </div>

              <div class="glass rounded-2xl p-6 text-center group hover:shadow-lg transition-all duration-300">
                <div class="w-12 h-12 bg-red-100 rounded-xl flex items-center justify-center mx-auto mb-4 group-hover:scale-110 transition-transform duration-300">
                  <svg class="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                  </svg>
                </div>
                <h3 class="text-lg font-semibold text-slate-900 mb-2">一键复制</h3>
                <p class="text-slate-600 text-sm">快速复制到剪贴板</p>
              </div>

              <div class="glass rounded-2xl p-6 text-center group hover:shadow-lg transition-all duration-300">
                <div class="w-12 h-12 bg-indigo-100 rounded-xl flex items-center justify-center mx-auto mb-4 group-hover:scale-110 transition-transform duration-300">
                  <svg class="w-6 h-6 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <h3 class="text-lg font-semibold text-slate-900 mb-2">版本控制</h3>
                <p class="text-slate-600 text-sm">完整的版本历史记录</p>
              </div>
            </div>

            <!-- 操作按钮 -->
            <div class="flex flex-col sm:flex-row gap-4 justify-center items-center">
              <router-link to="/login" class="btn btn-primary px-8 py-4 text-lg font-semibold shadow-lg hover:shadow-xl transform hover:scale-105 transition-all duration-200">
                <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1" />
                </svg>
                立即登录
              </router-link>
              <router-link to="/register" class="btn btn-secondary px-8 py-4 text-lg font-semibold shadow-lg hover:shadow-xl transform hover:scale-105 transition-all duration-200">
                <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
                </svg>
                免费注册
              </router-link>
            </div>

            <!-- 浏览公开片段 -->
            <div class="mt-8">
              <router-link to="/snippets" class="inline-flex items-center text-blue-600 hover:text-blue-700 font-medium transition-colors duration-200">
                <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                </svg>
                浏览公开代码片段
                <svg class="w-4 h-4 ml-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                </svg>
              </router-link>
            </div>
          </div>

          <!-- 已登录用户显示的内容 -->
          <div v-else class="space-y-8">
            <!-- 欢迎卡片 -->
            <div class="glass rounded-3xl p-8 max-w-2xl mx-auto text-center">
              <div class="flex justify-center mb-6">
                <div class="w-16 h-16 bg-gradient-to-br from-green-400 to-blue-500 rounded-full flex items-center justify-center">
                  <svg class="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14.828 14.828a4 4 0 01-5.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
              </div>
              <h2 class="text-3xl font-bold text-slate-900 mb-4">
                欢迎回来，{{ user.username }}！
              </h2>
              <p class="text-lg text-slate-600 mb-6">
                角色：<span class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800">
                  {{ getRoleName(user.role) }}
                </span>
              </p>
              
              <!-- 快速操作 -->
              <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <router-link to="/snippets" class="glass rounded-xl p-4 hover:shadow-lg transition-all duration-300 group">
                  <div class="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center mx-auto mb-3 group-hover:scale-110 transition-transform duration-300">
                    <svg class="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                    </svg>
                  </div>
                  <h3 class="font-semibold text-slate-900 mb-1">浏览片段</h3>
                  <p class="text-sm text-slate-600">查看所有代码片段</p>
                </router-link>

                <router-link
                  v-if="canCreateSnippet"
                  to="/snippets/create"
                  class="glass rounded-xl p-4 hover:shadow-lg transition-all duration-300 group"
                >
                  <div class="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center mx-auto mb-3 group-hover:scale-110 transition-transform duration-300">
                    <svg class="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
                    </svg>
                  </div>
                  <h3 class="font-semibold text-slate-900 mb-1">创建片段</h3>
                  <p class="text-sm text-slate-600">添加新的代码片段</p>
                </router-link>

                <router-link to="/clipboard/history" class="glass rounded-xl p-4 hover:shadow-lg transition-all duration-300 group">
                  <div class="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center mx-auto mb-3 group-hover:scale-110 transition-transform duration-300">
                    <svg class="w-5 h-5 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                    </svg>
                  </div>
                  <h3 class="font-semibold text-slate-900 mb-1">剪贴板历史</h3>
                  <p class="text-sm text-slate-600">查看复制历史</p>
                </router-link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 底部装饰 -->
    <div class="relative">
      <div class="absolute inset-0 flex items-end">
        <div class="w-full h-32 bg-gradient-to-t from-slate-100 to-transparent"></div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { UserRole } from '@/types'

const authStore = useAuthStore()
const user = computed(() => authStore.user)

/**
 * 获取用户角色的中文名称
 * @param role 用户角色枚举
 * @returns 角色的中文名称
 */
function getRoleName(role: UserRole): string {
  switch (role) {
    case UserRole.Admin:
      return '管理员'
    case UserRole.Editor:
      return '编辑者'
    case UserRole.Viewer:
      return '查看者'
    default:
      return '未知'
  }
}

/**
 * 检查当前用户是否可以创建代码片段
 */
const canCreateSnippet = computed(() => {
  if (!user.value) return false
  return user.value.role === UserRole.Admin || user.value.role === UserRole.Editor
})
</script>

<style scoped>
/* 动画效果 */
@keyframes blob {
  0% {
    transform: translate(0px, 0px) scale(1);
  }
  33% {
    transform: translate(30px, -50px) scale(1.1);
  }
  66% {
    transform: translate(-20px, 20px) scale(0.9);
  }
  100% {
    transform: translate(0px, 0px) scale(1);
  }
}

.animate-blob {
  animation: blob 7s infinite;
}

.animation-delay-2000 {
  animation-delay: 2s;
}

.animation-delay-4000 {
  animation-delay: 4s;
}

/* 响应式调整 */
@media (max-width: 768px) {
  .text-5xl {
    font-size: 2.5rem;
  }
  
  .text-6xl {
    font-size: 3rem;
  }
  
  .text-xl {
    font-size: 1.1rem;
  }
  
  .text-2xl {
    font-size: 1.25rem;
  }
  
  .grid-cols-1 {
    grid-template-columns: 1fr;
  }
  
  .sm\:grid-cols-2 {
    grid-template-columns: 1fr;
  }
  
  .lg\:grid-cols-3 {
    grid-template-columns: 1fr;
  }
  
  .sm\:grid-cols-3 {
    grid-template-columns: 1fr;
  }
  
  .flex-col {
    flex-direction: column;
  }
  
  .sm\:flex-row {
    flex-direction: column;
  }
}

/* 移动端优化 */
@media (max-width: 640px) {
  .py-24 {
    padding-top: 2rem;
    padding-bottom: 2rem;
  }
  
  .px-4 {
    padding-left: 1rem;
    padding-right: 1rem;
  }
  
  .mb-8 {
    margin-bottom: 1.5rem;
  }
  
  .mb-12 {
    margin-bottom: 2rem;
  }
  
  .space-y-8 > * + * {
    margin-top: 2rem;
  }
  
  .gap-6 {
    gap: 1rem;
  }
  
  .gap-4 {
    gap: 0.75rem;
  }
  
  .p-6 {
    padding: 1rem;
  }
  
  .p-8 {
    padding: 1.5rem;
  }
  
  .p-4 {
    padding: 0.75rem;
  }
  
  .w-20 {
    width: 4rem;
    height: 4rem;
  }
  
  .w-12 {
    width: 3rem;
    height: 3rem;
  }
  
  .w-10 {
    width: 2.5rem;
    height: 2.5rem;
  }
  
  .w-16 {
    width: 3.5rem;
    height: 3.5rem;
  }
}

/* 深色模式支持 */
@media (prefers-color-scheme: dark) {
  .from-slate-50 {
    --tw-gradient-from: #1e293b;
    --tw-gradient-stops: var(--tw-gradient-from), var(--tw-gradient-to, rgba(30, 41, 59, 0));
  }
  
  .via-white {
    --tw-gradient-via: #334155;
    --tw-gradient-stops: var(--tw-gradient-from), var(--tw-gradient-via), var(--tw-gradient-to, rgba(51, 65, 85, 0));
  }
  
  .to-slate-100 {
    --tw-gradient-to: #475569;
  }
  
  .text-slate-900 {
    color: #f1f5f9;
  }
  
  .text-slate-600 {
    color: #94a3b8;
  }
  
  .bg-blue-100 {
    background-color: #1e3a8a;
  }
  
  .bg-purple-100 {
    background-color: #581c87;
  }
  
  .bg-yellow-100 {
    background-color: #713f12;
  }
  
  .glass {
    background: rgba(30, 41, 59, 0.7);
    backdrop-filter: blur(12px);
    border-color: rgba(148, 163, 184, 0.1);
  }
}

/* 减少动画偏好 */
@media (prefers-reduced-motion: reduce) {
  .animate-blob {
    animation: none;
  }
  
  .transition-all {
    transition-property: none;
  }
  
  .hover\:scale-105:hover {
    transform: none;
  }
  
  .hover\:scale-110:hover {
    transform: none;
  }
  
  .group-hover\:scale-110:hover {
    transform: none;
  }
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .glass {
    background: rgba(255, 255, 255, 0.95);
    border: 2px solid #000;
  }
  
  .btn-primary {
    border: 2px solid #000;
  }
  
  .btn-secondary {
    border: 2px solid #000;
  }
  
  .rounded-2xl {
    border-radius: 0.5rem;
  }
  
  .rounded-3xl {
    border-radius: 0.75rem;
  }
}
</style>
