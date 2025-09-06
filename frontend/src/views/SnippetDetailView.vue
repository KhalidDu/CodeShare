<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-50 via-white to-slate-100">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 返回按钮 -->
      <div class="mb-6">
        <router-link 
          to="/snippets" 
          class="inline-flex items-center text-blue-600 hover:text-blue-700 transition-colors duration-200"
        >
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
          </svg>
          返回代码片段列表
        </router-link>
      </div>

      <!-- 加载状态 -->
      <div v-if="loading" class="flex items-center justify-center py-12">
        <div class="text-center">
          <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <p class="mt-4 text-gray-600">加载中...</p>
        </div>
      </div>

      <!-- 错误状态 -->
      <div v-else-if="error" class="text-center py-12">
        <div class="text-red-500 mb-4">
          <svg class="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <h2 class="text-xl font-semibold text-gray-900 mb-2">加载失败</h2>
        <p class="text-gray-600 mb-6">{{ error }}</p>
        <div class="space-x-4">
          <button 
            @click="loadSnippet" 
            class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors duration-200"
          >
            重试
          </button>
          <router-link 
            to="/snippets" 
            class="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition-colors duration-200"
          >
            返回列表
          </router-link>
        </div>
      </div>

      <!-- 详情内容 -->
      <div v-else-if="snippet" class="max-w-4xl mx-auto">
        <!-- 单一卡片容器 -->
        <div class="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
          <!-- 顶部标题区域 -->
          <div class="p-6 border-b border-gray-200">
            <div class="flex items-start justify-between mb-4">
              <!-- 标题和语言信息 -->
              <div class="flex-1">
                <h1 class="text-2xl font-bold text-gray-900 mb-3">
                  {{ snippet.title || '无标题' }}
                </h1>
                
                <!-- 元信息行 -->
                <div class="flex items-center gap-4 text-sm text-gray-600">
                  <!-- 语言类型 -->
                  <div
                    class="inline-flex items-center px-2.5 py-1 rounded-md text-xs font-medium"
                    :style="{
                      backgroundColor: getLanguageColor(snippet.language) + '20',
                      color: getLanguageColor(snippet.language),
                      border: `1px solid ${getLanguageColor(snippet.language)}40`
                    }"
                  >
                    <div
                      class="w-1.5 h-1.5 rounded-full mr-1.5"
                      :style="{ backgroundColor: getLanguageColor(snippet.language) }"
                    ></div>
                    {{ getLanguageDisplayName(snippet.language) }}
                  </div>
                  
                  <!-- 统计信息 -->
                  <div class="flex items-center gap-4">
                    <div class="flex items-center gap-1">
                      <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                      </svg>
                      <span>{{ snippet.viewCount || 0 }}</span>
                    </div>
                    <div class="flex items-center gap-1">
                      <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                      </svg>
                      <span>{{ snippet.copyCount || 0 }}</span>
                    </div>
                    <div class="flex items-center gap-1">
                      <svg v-if="snippet.isPublic" class="w-3.5 h-3.5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 002-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      <svg v-else class="w-3.5 h-3.5 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2h-8a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                      </svg>
                      <span>{{ snippet.isPublic ? '公开' : '私有' }}</span>
                    </div>
                  </div>
                </div>
              </div>
              
              <!-- 操作按钮 -->
              <div class="flex items-center gap-2 ml-4">
                <button
                  v-if="canEdit"
                  @click="editSnippet"
                  class="px-3 py-1.5 text-sm font-medium text-yellow-700 bg-yellow-100 hover:bg-yellow-200 rounded-md transition-colors duration-200"
                >
                  编辑
                </button>
              </div>
            </div>
          </div>
          
          <!-- 描述信息 -->
          <div v-if="snippet.description" class="p-6 border-b border-gray-200 bg-gray-50">
            <p class="text-gray-700 leading-relaxed">{{ snippet.description }}</p>
          </div>
          
          <!-- 标签 -->
          <div v-if="snippet.tags && snippet.tags.length > 0" class="p-6 border-b border-gray-200">
            <div class="flex flex-wrap gap-2">
              <span
                v-for="tag in snippet.tags"
                :key="tag.id"
                class="inline-flex items-center px-2.5 py-1 rounded-md text-xs font-medium"
                :style="{ backgroundColor: tag.color + '20', color: tag.color, border: `1px solid ${tag.color}40` }"
              >
                {{ tag.name }}
              </span>
            </div>
          </div>
          
          <!-- 代码块 -->
          <div class="bg-gray-900">
            <div class="px-4 py-3 border-b border-gray-700 flex items-center justify-between">
              <span class="text-sm font-medium text-gray-300">代码</span>
              <button
                @click="handleCopy"
                class="text-gray-400 hover:text-gray-300 transition-colors duration-200"
                title="复制代码"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                </svg>
              </button>
            </div>
            <div class="p-6">
              <pre class="text-sm text-gray-100 leading-relaxed whitespace-pre-wrap"><code>{{ snippet.code }}</code></pre>
            </div>
          </div>
          
          <!-- 底部信息 -->
          <div class="p-4 bg-gray-50 border-t border-gray-200">
            <div class="flex items-center justify-between text-xs text-gray-500">
              <div class="flex items-center gap-4">
                <span>创建者: {{ snippet.creatorName || '未知' }}</span>
                <span>创建时间: {{ formatDate(snippet.createdAt) }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 复制成功提示 -->
    <div
      v-if="showCopySuccess"
      class="fixed top-4 right-4 bg-green-500 text-white px-6 py-3 rounded-lg shadow-lg z-50 animate-pulse"
    >
      <div class="flex items-center gap-2">
        <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
          <path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41L9 16.17z" />
        </svg>
        复制成功！
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useCopy } from '@/composables/useCopy'
import { codeSnippetService } from '@/services/codeSnippetService'
import type { CodeSnippet } from '@/types'
import { UserRole } from '@/types'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { copyCodeSnippet } = useCopy()

// 响应式状态
const snippet = ref<CodeSnippet | null>(null)
const loading = ref(false)
const error = ref('')
const showCopySuccess = ref(false)

// 计算属性
const snippetId = computed(() => route.params.id as string)

const canEdit = computed(() => {
  if (!snippet.value || !authStore.user) return false
  return authStore.user.role === UserRole.Admin || 
         (authStore.user.role === UserRole.Editor && authStore.user.id === snippet.value.createdBy)
})

/**
 * 获取编程语言对应的颜色
 */
function getLanguageColor(language: string): string {
  const colors: Record<string, string> = {
    javascript: '#f7df1e',
    typescript: '#3178c6',
    python: '#3776ab',
    java: '#ed8b00',
    csharp: '#239120',
    cpp: '#00599c',
    html: '#e34f26',
    css: '#1572b6',
    vue: '#4fc08d',
    react: '#61dafb',
    angular: '#dd0031',
    php: '#777bb4',
    ruby: '#cc342d',
    go: '#00add8',
    rust: '#000000',
    swift: '#fa7343',
    kotlin: '#7f52ff',
    dart: '#0175c2',
    sql: '#336791',
    shell: '#89e051',
    powershell: '#012456',
    json: '#000000',
    xml: '#0060ac',
    yaml: '#cb171e',
    markdown: '#083fa1'
  }

  return colors[language.toLowerCase()] || '#6c757d'
}

/**
 * 获取编程语言的显示名称
 */
function getLanguageDisplayName(language: string): string {
  const displayNames: Record<string, string> = {
    javascript: 'JavaScript',
    typescript: 'TypeScript',
    python: 'Python',
    java: 'Java',
    csharp: 'C#',
    cpp: 'C++',
    html: 'HTML',
    css: 'CSS',
    vue: 'Vue',
    react: 'React',
    angular: 'Angular',
    php: 'PHP',
    ruby: 'Ruby',
    go: 'Go',
    rust: 'Rust',
    swift: 'Swift',
    kotlin: 'Kotlin',
    dart: 'Dart',
    sql: 'SQL',
    shell: 'Shell',
    powershell: 'PowerShell',
    json: 'JSON',
    xml: 'XML',
    yaml: 'YAML',
    markdown: 'Markdown'
  }

  return displayNames[language.toLowerCase()] || language
}

/**
 * 格式化日期
 */
function formatDate(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

/**
 * 加载代码片段
 */
async function loadSnippet() {
  if (!snippetId.value) {
    error.value = '无效的代码片段ID'
    return
  }

  loading.value = true
  error.value = ''

  try {
    // 这里使用模拟数据，实际应该调用API
    snippet.value = await codeSnippetService.getSnippet(snippetId.value)
  } catch (err: unknown) {
    console.error('Failed to load snippet:', err)
    const axiosError = err as { response?: { data?: { message?: string } } }
    error.value = axiosError.response?.data?.message || '加载代码片段失败'
  } finally {
    loading.value = false
  }
}

/**
 * 处理复制操作
 */
async function handleCopy() {
  if (!snippet.value) return

  try {
    const success = await copyCodeSnippet(
      snippet.value.code,
      snippet.value.language,
      snippet.value.id
    )

    if (success) {
      showCopySuccess.value = true
      // 更新复制次数
      if (snippet.value) {
        snippet.value.copyCount = (snippet.value.copyCount || 0) + 1
      }
      // 3秒后隐藏提示
      setTimeout(() => {
        showCopySuccess.value = false
      }, 3000)
    }
  } catch (error) {
    console.error('复制失败:', error)
  }
}

/**
 * 编辑代码片段
 */
function editSnippet() {
  router.push(`/snippets/${snippetId.value}/edit`)
}

// 生命周期
onMounted(() => {
  loadSnippet()
})
</script>

<style scoped>
/* 组件特定样式 */
</style>