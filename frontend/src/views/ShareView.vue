<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-50 via-white to-slate-100">
    <div class="max-w-7xl mx-auto page-container py-xl">
      <!-- 加载状态 -->
      <div v-if="loading" class="flex items-center justify-center py-12">
        <div class="text-center">
          <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <p class="mt-4 text-gray-600">正在加载分享内容...</p>
        </div>
      </div>

      <!-- 密码验证状态 -->
      <div v-else-if="requiresPassword" class="max-w-md mx-auto">
        <div class="bg-white rounded-xl shadow-lg border border-gray-200 p-6">
          <div class="text-center mb-6">
            <div class="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg class="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2h-8a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
            </div>
            <h2 class="text-2xl font-bold text-gray-900 mb-2">需要密码访问</h2>
            <p class="text-gray-600">此分享内容需要密码才能访问</p>
          </div>

          <form @submit.prevent="handlePasswordSubmit" class="space-y-4">
            <div>
              <label for="password" class="block text-sm font-medium text-gray-700 mb-1">
                访问密码
              </label>
              <input
                id="password"
                v-model="password"
                type="password"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="请输入访问密码"
                :disabled="passwordSubmitting"
              />
            </div>

            <div v-if="passwordError" class="text-red-600 text-sm">
              {{ passwordError }}
            </div>

            <button
              type="submit"
              :disabled="passwordSubmitting"
              class="w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 transition-colors duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <span v-if="passwordSubmitting" class="flex items-center justify-center">
                <svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                验证中...
              </span>
              <span v-else>访问分享</span>
            </button>
          </form>
        </div>
      </div>

      <!-- 错误状态 -->
      <div v-else-if="error" class="max-w-md mx-auto">
        <div class="bg-white rounded-xl shadow-lg border border-gray-200 p-6 text-center">
          <div class="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg class="w-8 h-8 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          
          <h2 class="text-2xl font-bold text-gray-900 mb-2">{{ errorTitle }}</h2>
          <p class="text-gray-600 mb-6">{{ error }}</p>
          
          <div class="space-y-3">
            <button 
              v-if="canRetry"
              @click="retryAccess" 
              class="w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 transition-colors duration-200"
            >
              重试访问
            </button>
            <button 
              @click="goToHome" 
              class="w-full bg-gray-200 text-gray-700 py-2 px-4 rounded-lg hover:bg-gray-300 transition-colors duration-200"
            >
              返回首页
            </button>
          </div>
        </div>
      </div>

      <!-- 分享内容 -->
      <div v-else-if="shareToken" class="max-w-4xl mx-auto">
        <!-- 单一卡片容器 -->
        <div class="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
          <!-- 顶部标题区域 -->
          <div class="p-6 border-b border-gray-200">
            <div class="flex items-start justify-between mb-4">
              <!-- 标题和语言信息 -->
              <div class="flex-1">
                <h1 class="text-2xl font-bold text-gray-900 mb-3">
                  {{ shareToken.snippetTitle || '无标题' }}
                </h1>
                
                <!-- 元信息行 -->
                <div class="flex items-center gap-4 text-sm text-gray-600">
                  <!-- 语言类型 -->
                  <div
                    class="inline-flex items-center px-2.5 py-1 rounded-md text-xs font-medium"
                    :style="{
                      backgroundColor: getLanguageColor(shareToken.snippetLanguage) + '20',
                      color: getLanguageColor(shareToken.snippetLanguage),
                      border: `1px solid ${getLanguageColor(shareToken.snippetLanguage)}40`
                    }"
                  >
                    <div
                      class="w-1.5 h-1.5 rounded-full mr-1.5"
                      :style="{ backgroundColor: getLanguageColor(shareToken.snippetLanguage) }"
                    ></div>
                    {{ getLanguageDisplayName(shareToken.snippetLanguage) }}
                  </div>
                  
                  <!-- 分享信息 -->
                  <div class="flex items-center gap-4">
                    <div class="flex items-center gap-1">
                      <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m9.632 4.316C18.114 15.062 18 14.518 18 14c0-.482.114-.938.316-1.342m0 2.684a3 3 0 110-2.684M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                      </svg>
                      <span>已访问 {{ shareToken.currentAccessCount }} 次</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 过期提醒 -->
          <div v-if="isExpiringSoon" class="p-4 bg-yellow-50 border-b border-yellow-200">
            <div class="flex items-center gap-2 text-yellow-800">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span class="text-sm font-medium">此分享将于 {{ formatExpiryTime }} 过期</span>
            </div>
          </div>

          <!-- 访问限制提醒 -->
          <div v-if="hasAccessLimit" class="p-4 bg-blue-50 border-b border-blue-200">
            <div class="flex items-center gap-2 text-blue-800">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2h-8a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
              <span class="text-sm font-medium">
                剩余访问次数：{{ remainingAccessCount }} 次
              </span>
            </div>
          </div>

          <!-- 代码块 -->
          <div class="bg-gray-900">
            <div class="px-4 py-3 border-b border-gray-700 flex items-center justify-between">
              <span class="text-sm font-medium text-gray-300">代码</span>
              <div class="flex items-center gap-2">
                <button
                  v-if="canCopy"
                  @click="handleCopy"
                  class="text-gray-400 hover:text-gray-300 transition-colors duration-200"
                  title="复制代码"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                  </svg>
                </button>
                <button
                  v-if="canDownload"
                  @click="handleDownload"
                  class="text-gray-400 hover:text-gray-300 transition-colors duration-200"
                  title="下载代码"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                </button>
              </div>
            </div>
            <div class="p-6">
              <pre class="text-sm text-gray-100 leading-relaxed whitespace-pre-wrap"><code>{{ shareToken.snippetCode }}</code></pre>
            </div>
          </div>
          
          <!-- 底部信息 -->
          <div class="p-4 bg-gray-50 border-t border-gray-200">
            <div class="flex items-center justify-between text-xs text-gray-500">
              <div class="flex items-center gap-4">
                <span>分享者: {{ shareToken.creatorName || '未知' }}</span>
                <span>创建时间: {{ formatDate(shareToken.createdAt) }}</span>
                <span v-if="shareToken.lastAccessedAt">最后访问: {{ formatDate(shareToken.lastAccessedAt) }}</span>
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
import { shareService } from '@/services/shareService'
import { useCopy } from '@/composables/useCopy'
import { useToast } from '@/composables/useToast'
import type { ShareToken } from '@/types/share'
import { SharePermission, ShareTokenStatus } from '@/types/share'

const route = useRoute()
const router = useRouter()
const { copyCodeSnippet } = useCopy()
const toast = useToast()

// 响应式状态
const shareToken = ref<ShareToken | null>(null)
const loading = ref(false)
const error = ref('')
const requiresPassword = ref(false)
const password = ref('')
const passwordSubmitting = ref(false)
const passwordError = ref('')
const showCopySuccess = ref(false)

// 计算属性
const token = computed(() => route.params.token as string)

const errorTitle = computed(() => {
  if (error.value.includes('不存在') || error.value.includes('无效')) {
    return '分享链接无效'
  } else if (error.value.includes('过期')) {
    return '分享已过期'
  } else if (error.value.includes('次数')) {
    return '访问次数已达上限'
  } else if (error.value.includes('密码')) {
    return '密码错误'
  } else {
    return '访问失败'
  }
})

const canRetry = computed(() => {
  return !error.value.includes('不存在') && !error.value.includes('过期')
})

const isExpiringSoon = computed(() => {
  if (!shareToken.value?.expiresAt) return false
  const expiryTime = new Date(shareToken.value.expiresAt).getTime()
  const now = Date.now()
  const oneDay = 24 * 60 * 60 * 1000
  return expiryTime - now < oneDay
})

const formatExpiryTime = computed(() => {
  if (!shareToken.value?.expiresAt) return ''
  const date = new Date(shareToken.value.expiresAt)
  return date.toLocaleString('zh-CN', {
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
})

const hasAccessLimit = computed(() => {
  return shareToken.value?.maxAccessCount !== undefined && shareToken.value.maxAccessCount > 0
})

const remainingAccessCount = computed(() => {
  if (!shareToken.value?.maxAccessCount) return 0
  return Math.max(0, shareToken.value.maxAccessCount - shareToken.value.currentAccessCount)
})

const canCopy = computed(() => {
  return shareToken.value?.permission === SharePermission.VIEW || 
         shareToken.value?.permission === SharePermission.EDIT
})

const canDownload = computed(() => {
  return shareToken.value?.allowDownload === true
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
 * 加载分享内容
 */
async function loadShareContent() {
  if (!token.value) {
    error.value = '无效的分享令牌'
    return
  }

  loading.value = true
  error.value = ''

  try {
    // 先验证分享令牌
    const validateRequest = {
      token: token.value,
      password: password.value || undefined
    }

    const validateResponse = await shareService.validateShareToken(validateRequest)

    if (!validateResponse.isValid) {
      error.value = validateResponse.errorMessage || '分享令牌验证失败'
      return
    }

    // 如果需要密码但未提供，显示密码输入界面
    if (validateResponse.shareToken?.isPasswordProtected && !password.value) {
      requiresPassword.value = true
      return
    }

    // 访问分享内容
    const accessRequest = {
      token: token.value,
      password: password.value || undefined,
      userAgent: navigator.userAgent
    }

    const accessResponse = await shareService.accessShare(accessRequest)

    if (accessResponse.success && accessResponse.shareToken) {
      shareToken.value = accessResponse.shareToken
    } else {
      error.value = accessResponse.errorMessage || '访问分享内容失败'
    }
  } catch (err: unknown) {
    console.error('Failed to load share content:', err)
    const axiosError = err as { response?: { data?: { message?: string } } }
    error.value = axiosError.response?.data?.message || '加载分享内容失败'
  } finally {
    loading.value = false
  }
}

/**
 * 处理密码提交
 */
async function handlePasswordSubmit() {
  if (!password.value) {
    passwordError.value = '请输入访问密码'
    return
  }

  passwordSubmitting.value = true
  passwordError.value = ''

  try {
    await loadShareContent()
    
    if (!error.value) {
      requiresPassword.value = false
    }
  } catch (err) {
    passwordError.value = '密码验证失败，请重试'
  } finally {
    passwordSubmitting.value = false
  }
}

/**
 * 重试访问
 */
function retryAccess() {
  password.value = ''
  requiresPassword.value = false
  error.value = ''
  loadShareContent()
}

/**
 * 返回首页
 */
function goToHome() {
  router.push('/')
}

/**
 * 处理复制操作
 */
async function handleCopy() {
  if (!shareToken.value) return

  try {
    const success = await copyCodeSnippet(
      shareToken.value.snippetCode,
      shareToken.value.snippetLanguage
    )

    if (success) {
      showCopySuccess.value = true
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
 * 处理下载操作
 */
function handleDownload() {
  if (!shareToken.value) return

  try {
    const content = shareToken.value.snippetCode
    const language = shareToken.value.snippetLanguage
    const title = shareToken.value.snippetTitle || 'code'
    
    // 确定文件扩展名
    const extensions: Record<string, string> = {
      javascript: 'js',
      typescript: 'ts',
      python: 'py',
      java: 'java',
      csharp: 'cs',
      cpp: 'cpp',
      html: 'html',
      css: 'css',
      vue: 'vue',
      php: 'php',
      ruby: 'rb',
      go: 'go',
      rust: 'rs',
      swift: 'swift',
      kotlin: 'kt',
      dart: 'dart',
      sql: 'sql',
      shell: 'sh',
      powershell: 'ps1',
      json: 'json',
      xml: 'xml',
      yaml: 'yaml',
      markdown: 'md'
    }

    const extension = extensions[language.toLowerCase()] || 'txt'
    const filename = `${title}.${extension}`

    // 创建并下载文件
    const blob = new Blob([content], { type: 'text/plain' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = filename
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)

    toast.success('代码文件已下载')
  } catch (error) {
    console.error('下载失败:', error)
    toast.error('下载失败，请手动复制代码')
  }
}

// 生命周期
onMounted(() => {
  loadShareContent()
})
</script>

<style scoped>
/* 组件特定样式 */
</style>