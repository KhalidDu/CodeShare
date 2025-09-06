<template>
  <div
    class="group relative glass rounded-2xl overflow-hidden transition-all duration-300 hover:shadow-soft-lg hover:border-slate-300/60 hover:-translate-y-1 animate-fade-in cursor-pointer"
    :class="{
      'opacity-70 pointer-events-none': isLoading,
      'ring-2 ring-blue-400 bg-blue-50/50': isSelected
    }"
    @click="handleCardClick"
  >
    <!-- 卡片头部 -->
    <div class="p-6 pb-4">
      <div class="flex items-start justify-between mb-3">
        <!-- 标题和语言 -->
        <div class="flex-1 min-w-0">
          <div class="flex items-center gap-3 mb-2">
            <!-- 语言图标 -->
            <div
              class="w-8 h-8 rounded-lg flex items-center justify-center text-white text-xs font-semibold"
              :style="{ backgroundColor: getLanguageColor(snippet.language) }"
            >
              {{ snippet.language.slice(0, 2).toUpperCase() }}
            </div>

            <!-- 标题 -->
            <h3 class="font-semibold text-slate-900 truncate group-hover:text-blue-600 transition-colors duration-200" :class="{
              'text-slate-500 italic': !snippet.title?.trim()
            }">
              {{ snippet.title?.trim() || '无标题' }}
            </h3>
          </div>

          <!-- 语言标签 -->
          <div
            class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium transition-all duration-200 hover:scale-105 cursor-pointer"
            :style="{
              backgroundColor: getLanguageColor(snippet.language) + '20',
              color: getLanguageColor(snippet.language),
              border: `1px solid ${getLanguageColor(snippet.language)}40`
            }"
            :title="snippet.language"
          >
            <div
              class="w-2 h-2 rounded-full mr-1.5"
              :style="{ backgroundColor: getLanguageColor(snippet.language) }"
            ></div>
            {{ getLanguageDisplayName(snippet.language) }}
          </div>
        </div>

        <!-- 操作按钮 -->
        <div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-all duration-200 ml-4" @click.stop>
          <button
            @click="handleCopy"
            class="w-8 h-8 flex items-center justify-center text-gray-500 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors duration-200"
            :disabled="isLoading"
            title="复制代码"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
            </svg>
          </button>

          <button
            v-if="canEdit"
            @click="handleEdit"
            class="w-8 h-8 flex items-center justify-center text-gray-500 hover:text-yellow-600 hover:bg-yellow-50 rounded-lg transition-colors duration-200"
            :disabled="isLoading"
            title="编辑代码片段"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
            </svg>
          </button>

          <button
            v-if="canDelete"
            @click="handleDelete"
            class="w-8 h-8 flex items-center justify-center text-gray-500 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors duration-200"
            :disabled="isLoading"
            title="删除代码片段"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
            </svg>
          </button>
        </div>
      </div>

      <!-- 描述 -->
      <div v-if="snippet.description" class="mb-4">
        <p class="text-sm text-slate-600 line-clamp-2 leading-relaxed">
          {{ snippet.description }}
        </p>
      </div>
    </div>

    <!-- 代码预览 -->
    <div class="px-6 pb-4">
      <div class="relative bg-gray-900 rounded-lg overflow-hidden border border-gray-700">
        <!-- 代码窗口装饰 -->
        <div class="flex items-center justify-between px-3 py-2 bg-gray-800 border-b border-gray-700">
          <div class="flex items-center gap-1.5">
            <div class="w-3 h-3 bg-red-500 rounded-full"></div>
            <div class="w-3 h-3 bg-yellow-500 rounded-full"></div>
            <div class="w-3 h-3 bg-green-500 rounded-full"></div>
          </div>
          <span class="text-xs text-gray-400">{{ snippet.language }}</span>
        </div>

        <!-- 代码内容 -->
        <pre class="p-3 text-xs text-gray-100 leading-relaxed overflow-hidden max-h-32 scrollbar-thin scrollbar-thumb-gray-600 scrollbar-track-gray-800"><code>{{ codePreview }}</code></pre>

        <!-- 展开/收起按钮 -->
        <div v-if="isCodeTruncated" class="absolute bottom-0 left-0 right-0 h-8 bg-gradient-to-t from-gray-900 to-transparent flex items-end justify-center pb-2" @click.stop>
          <button
            @click="toggleCodeExpansion"
            class="text-xs text-blue-400 hover:text-blue-300 transition-colors duration-200 bg-gray-800/80 px-3 py-1 rounded-md hover:bg-gray-700/80 border border-gray-600"
          >
            {{ isCodeExpanded ? '收起' : '展开更多' }}
          </button>
        </div>
      </div>
    </div>

    <!-- 卡片底部 -->
    <div class="px-6 pb-6">
      <!-- 标签 -->
      <div v-if="snippet.tags && snippet.tags.length > 0" class="flex flex-wrap gap-2 mb-4" @click.stop>
        <span
          v-for="tag in snippet.tags?.slice(0, 3)"
          :key="tag.id"
          class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800 hover:bg-gray-200 cursor-pointer transition-colors duration-200"
          :title="tag.name"
          @click="handleTagClick(tag)"
        >
          {{ tag.name }}
        </span>

        <span
          v-if="snippet.tags && snippet.tags.length > 3"
          class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium bg-gray-50 text-gray-500 border border-gray-200"
          :title="`还有 ${snippet.tags.length - 3} 个标签`"
        >
          +{{ snippet.tags.length - 3 }}
        </span>
      </div>

      <!-- 无内容提示 -->
      <div v-if="!snippet.code?.trim()" class="mb-4">
        <span class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
          <svg class="w-3 h-3 mr-1" fill="currentColor" viewBox="0 0 24 24">
            <path d="M12 2L13.09 8.26L22 9L13.09 9.74L12 16L10.91 9.74L2 9L10.91 8.26L12 2Z"/>
          </svg>
          无内容
        </span>
      </div>

      <!-- 统计信息 -->
      <div class="flex items-center justify-between text-xs text-gray-500">
        <div class="flex items-center gap-4">
          <!-- 查看次数 -->
          <div class="flex items-center gap-1 text-gray-500" title="查看次数">
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
            </svg>
            <span class="font-medium">{{ snippet.viewCount || 0 }}</span>
          </div>

          <!-- 复制次数 -->
          <div class="flex items-center gap-1 text-gray-500" title="复制次数">
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
            </svg>
            <span class="font-medium">{{ snippet.copyCount || 0 }}</span>
          </div>

          <!-- 可见性 -->
          <div class="flex items-center gap-1" :title="snippet.isPublic ? '公开' : '私有'">
            <svg v-if="snippet.isPublic" class="w-3.5 h-3.5 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
            <svg v-else class="w-3.5 h-3.5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
            </svg>
          </div>
        </div>

        <!-- 更新时间 -->
        <div class="flex items-center gap-1 text-gray-500" title="更新时间">
          <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          <span class="font-medium">{{ formatDate(snippet.updatedAt || snippet.createdAt) }}</span>
        </div>
      </div>
    </div>

    <!-- 复制成功提示 -->
    <div
      v-if="showCopySuccess"
      class="absolute top-2 right-2 bg-green-500 text-white px-3 py-1 rounded-lg text-xs flex items-center gap-2 animate-pulse z-10"
    >
      <i class="fas fa-check"></i>
      复制成功！
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useCopy } from '@/composables/useCopy'
import type { CodeSnippet, Tag } from '@/types'
import { UserRole } from '@/types'

interface Props {
  snippet: CodeSnippet
  viewMode?: 'grid' | 'list'
  isLoading?: boolean
  isSelected?: boolean
}

interface Emits {
  (e: 'copy', snippet: CodeSnippet): void
  (e: 'delete', snippet: CodeSnippet): void
  (e: 'tag-click', tag: Tag): void
  (e: 'select', snippet: CodeSnippet): void
}

const props = withDefaults(defineProps<Props>(), {
  viewMode: 'grid',
  isLoading: false,
  isSelected: false
})

const emit = defineEmits<Emits>()

const router = useRouter()
const authStore = useAuthStore()
const { copyCodeSnippet } = useCopy()

// 响应式状态
const isCodeExpanded = ref(false)
const showCopySuccess = ref(false)

// 计算属性
const canEdit = computed(() => {
  const user = authStore.user
  if (!user) return false

  return user.role === UserRole.Admin ||
         (user.role === UserRole.Editor && user.id === props.snippet.createdBy)
})

const canDelete = computed(() => {
  const user = authStore.user
  if (!user) return false

  return user.role === UserRole.Admin || user.id === props.snippet.createdBy
})

const codePreview = computed(() => {
  const maxLines = isCodeExpanded.value ? Infinity : 10
  const lines = props.snippet.code.split('\n')

  if (lines.length <= maxLines) {
    return props.snippet.code
  }

  return lines.slice(0, maxLines).join('\n')
})

const isCodeTruncated = computed(() => {
  return props.snippet.code.split('\n').length > 10
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
  const now = new Date()
  const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000)

  if (diffInSeconds < 60) {
    return '刚刚'
  } else if (diffInSeconds < 3600) {
    const minutes = Math.floor(diffInSeconds / 60)
    return `${minutes}分钟前`
  } else if (diffInSeconds < 86400) {
    const hours = Math.floor(diffInSeconds / 3600)
    return `${hours}小时前`
  } else if (diffInSeconds < 2592000) {
    const days = Math.floor(diffInSeconds / 86400)
    return `${days}天前`
  } else {
    return date.toLocaleDateString('zh-CN')
  }
}

/**
 * 切换代码展开状态
 */
function toggleCodeExpansion() {
  isCodeExpanded.value = !isCodeExpanded.value
}

/**
 * 处理复制操作
 */
async function handleCopy() {
  try {
    const success = await copyCodeSnippet(
      props.snippet.code,
      props.snippet.language,
      props.snippet.id,
      // 禁用内置的toast提示，使用卡片内的视觉提示
      { showSuccessToast: false }
    )

    if (success) {
      showCopySuccess.value = true

      // 触发复制事件
      emit('copy', props.snippet)

      // 3秒后隐藏成功提示
      setTimeout(() => {
        showCopySuccess.value = false
      }, 3000)
    }
  } catch (error) {
    console.error('复制失败:', error)
    // 这里可以显示错误提示
  }
}

/**
 * 处理编辑操作
 */
function handleEdit() {
  router.push(`/snippets/${props.snippet.id}/edit`)
}

/**
 * 处理删除操作
 */
function handleDelete() {
  if (confirm('确定要删除这个代码片段吗？此操作不可撤销。')) {
    emit('delete', props.snippet)
  }
}

/**
 * 处理卡片点击
 */
function handleCardClick() {
  router.push(`/snippets/${props.snippet.id}`)
}

/**
 * 处理标签点击
 */
function handleTagClick(tag: Tag) {
  emit('tag-click', tag)
}
</script>

<style scoped>
/* 组件特定样式 */
</style>
