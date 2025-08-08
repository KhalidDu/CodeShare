<template>
  <div
    class="p-4 bg-white dark:bg-gray-700 backdrop-blur-sm border border-slate-200 dark:border-gray-600 rounded-xl cursor-pointer transition-all duration-200 hover:bg-gray-50 dark:hover:bg-gray-600 hover:border-slate-300 dark:hover:border-gray-500 hover:shadow-sm group relative"
    :class="{
      'opacity-70 pointer-events-none': isLoading,
      'ring-2 ring-blue-400 dark:ring-blue-500 bg-blue-50 dark:bg-blue-900 border-blue-300 dark:border-blue-600': isSelected
    }"
    @click="$emit('select', snippet)"
  >
    <!-- 标题行 -->
    <div class="flex items-center justify-between mb-3">
      <!-- 标题 -->
      <h3 class="font-medium text-slate-800 dark:text-slate-200 truncate flex-1">
        <router-link
          :to="`/snippets/${snippet.id}`"
          class="hover:text-blue-600 dark:hover:text-blue-400 transition-colors duration-200"
          :class="{
            'text-slate-500 dark:text-slate-400 italic': !snippet.title?.trim()
          }"
        >
          {{ snippet.title?.trim() || '无标题' }}
        </router-link>
      </h3>

      <!-- 操作按钮 -->
      <div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
        <button
          @click.stop="handleCopy"
          class="w-8 h-8 bg-blue-100 hover:bg-blue-200 dark:bg-blue-900/50 dark:hover:bg-blue-800/50 text-blue-600 dark:text-blue-400 rounded-lg transition-all duration-200 flex items-center justify-center"
          :disabled="isLoading"
          title="复制代码"
        >
          <i class="fas fa-copy text-xs"></i>
        </button>

        <button
          v-if="canEdit"
          @click.stop="handleEdit"
          class="w-8 h-8 bg-yellow-100 hover:bg-yellow-200 dark:bg-yellow-900/50 dark:hover:bg-yellow-800/50 text-yellow-600 dark:text-yellow-400 rounded-lg transition-all duration-200 flex items-center justify-center"
          :disabled="isLoading"
          title="编辑代码片段"
        >
          <i class="fas fa-edit text-xs"></i>
        </button>

        <button
          v-if="canDelete"
          @click.stop="handleDelete"
          class="w-8 h-8 bg-red-100 hover:bg-red-200 dark:bg-red-900/50 dark:hover:bg-red-800/50 text-red-600 dark:text-red-400 rounded-lg transition-all duration-200 flex items-center justify-center"
          :disabled="isLoading"
          title="删除代码片段"
        >
          <i class="fas fa-trash text-xs"></i>
        </button>
      </div>

      <!-- 语言标签 -->
      <span
        class="text-xs px-2.5 py-1 bg-slate-200/80 dark:bg-gray-600/80 rounded-full text-slate-600 dark:text-slate-300 flex items-center gap-1 flex-shrink-0 font-medium ml-2"
        :style="{ backgroundColor: getLanguageColor(snippet.language) + '20', color: getLanguageColor(snippet.language) }"
      >
        {{ snippet.language }}
      </span>
    </div>

    <!-- 描述 -->
    <div v-if="snippet.description" class="mb-3">
      <p class="text-sm text-slate-600 dark:text-slate-400 line-clamp-2">
        {{ snippet.description }}
      </p>
    </div>

    <!-- 代码预览 -->
    <div class="mb-3">
      <div class="bg-slate-900 dark:bg-gray-800 rounded-lg p-3 relative overflow-hidden">
        <pre class="text-xs text-slate-300 dark:text-slate-400 font-mono leading-relaxed overflow-hidden"><code>{{ codePreview }}</code></pre>
        <div v-if="isCodeTruncated" class="absolute bottom-0 left-0 right-0 h-6 bg-gradient-to-t from-slate-900 dark:from-gray-800 to-transparent flex items-end justify-center">
          <button
            @click.stop="toggleCodeExpansion"
            class="text-xs text-blue-400 hover:text-blue-300 transition-colors duration-200"
          >
            {{ isCodeExpanded ? '收起' : '展开更多' }}
          </button>
        </div>
      </div>
    </div>

    <!-- 标签和时间行 -->
    <div class="flex items-center justify-between text-xs">
      <!-- 标签区域 -->
      <div class="flex flex-wrap gap-1 flex-1 min-w-0 mr-3">
        <!-- 无内容标记 -->
        <span
          v-if="!snippet.code?.trim()"
          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 dark:bg-yellow-900/50 text-yellow-800 dark:text-yellow-300 flex-shrink-0"
          title="无内容"
        >
          <i class="fas fa-exclamation-triangle text-xs mr-1"></i>
          无内容
        </span>

        <span
          v-for="tag in snippet.tags?.slice(0, 3)"
          :key="tag.id"
          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 dark:bg-blue-900/50 text-blue-800 dark:text-blue-300 max-w-20 truncate cursor-pointer hover:bg-blue-200 dark:hover:bg-blue-800/50 transition-colors duration-200"
          :title="tag.name"
          @click.stop="handleTagClick(tag)"
        >
          {{ tag.name }}
        </span>

        <span
          v-if="snippet.tags && snippet.tags.length > 3"
          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 dark:bg-gray-600/50 text-gray-500 dark:text-gray-400 flex-shrink-0"
          :title="`还有 ${snippet.tags.length - 3} 个标签`"
        >
          +{{ snippet.tags.length - 3 }}
        </span>
      </div>

      <!-- 统计和时间信息 -->
      <div class="flex items-center gap-3 text-slate-500 dark:text-slate-400 flex-shrink-0">
        <!-- 查看次数 -->
        <div class="flex items-center gap-1" title="查看次数">
          <i class="fas fa-eye text-xs"></i>
          <span>{{ snippet.viewCount || 0 }}</span>
        </div>

        <!-- 复制次数 -->
        <div class="flex items-center gap-1" title="复制次数">
          <i class="fas fa-copy text-xs"></i>
          <span>{{ snippet.copyCount || 0 }}</span>
        </div>

        <!-- 可见性 -->
        <div class="flex items-center gap-1" :title="snippet.isPublic ? '公开' : '私有'">
          <i :class="snippet.isPublic ? 'fas fa-eye' : 'fas fa-eye-slash'" class="text-xs"></i>
        </div>

        <!-- 更新时间 -->
        <div class="flex items-center gap-1" title="更新时间">
          <i class="fas fa-clock text-xs"></i>
          <span>{{ formatDate(snippet.updatedAt || snippet.createdAt) }}</span>
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
      props.snippet.id
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
 * 处理标签点击
 */
function handleTagClick(tag: Tag) {
  emit('tag-click', tag)
}
</script>

<style scoped>
/* 文本截断样式 */
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  * {
    transition: none !important;
    animation: none !important;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .bg-white\/70 {
    background: white !important;
  }

  .dark .bg-gray-700\/70 {
    background: black !important;
  }

  .border-slate-200\/60 {
    border-color: black !important;
    border-width: 2px !important;
  }

  .dark .border-gray-600\/60 {
    border-color: white !important;
    border-width: 2px !important;
  }
}
</style>
