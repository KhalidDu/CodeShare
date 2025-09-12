<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-50 via-white to-slate-100">
    <div class="max-w-7xl mx-auto page-container py-xl">
      <!-- 页面标题 -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900 mb-2">消息中心</h1>
        <p class="text-gray-600">管理和查看您的消息</p>
      </div>

      <!-- 消息界面布局 -->
      <div class="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
        <div class="flex h-[calc(100vh-200px)]">
          <!-- 左侧消息列表 -->
          <div class="w-1/3 border-r border-gray-200 flex flex-col">
            <!-- 搜索栏 -->
            <div class="p-4 border-b border-gray-200">
              <div class="relative">
                <input
                  v-model="searchKeyword"
                  type="text"
                  placeholder="搜索消息..."
                  class="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  @input="handleSearch"
                />
                <svg class="absolute left-3 top-2.5 w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </div>
            </div>

            <!-- 过滤器 -->
            <div class="p-4 border-b border-gray-200">
              <div class="flex items-center gap-2">
                <select 
                  v-model="selectedFilter"
                  class="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  @change="handleFilterChange"
                >
                  <option value="all">全部消息</option>
                  <option value="unread">未读消息</option>
                  <option value="important">重要消息</option>
                  <option value="system">系统消息</option>
                  <option value="user">用户消息</option>
                </select>
                <button
                  @click="refreshMessages"
                  class="p-2 text-gray-600 hover:text-gray-800 hover:bg-gray-100 rounded-lg transition-colors"
                  :disabled="loading"
                >
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                  </svg>
                </button>
              </div>
            </div>

            <!-- 消息列表 -->
            <div class="flex-1 overflow-y-auto">
              <!-- 加载状态 -->
              <div v-if="loading && messages.length === 0" class="flex items-center justify-center h-64">
                <div class="text-center">
                  <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                  <p class="mt-4 text-gray-600">加载中...</p>
                </div>
              </div>

              <!-- 空状态 -->
              <div v-else-if="messages.length === 0" class="flex items-center justify-center h-64">
                <div class="text-center">
                  <svg class="w-16 h-16 mx-auto text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
                  </svg>
                  <h3 class="text-lg font-medium text-gray-900 mb-2">暂无消息</h3>
                  <p class="text-gray-600">您还没有任何消息</p>
                </div>
              </div>

              <!-- 消息列表项 -->
              <div v-else class="divide-y divide-gray-200">
                <div
                  v-for="message in messages"
                  :key="message.id"
                  class="p-4 hover:bg-gray-50 cursor-pointer transition-colors"
                  :class="{ 'bg-blue-50': selectedMessage?.id === message.id }"
                  @click="selectMessage(message)"
                >
                  <div class="flex items-start gap-3">
                    <!-- 头像 -->
                    <div class="flex-shrink-0">
                      <div v-if="message.senderAvatar" class="w-10 h-10 rounded-full overflow-hidden">
                        <img :src="message.senderAvatar" :alt="message.senderName" class="w-full h-full object-cover" />
                      </div>
                      <div v-else class="w-10 h-10 rounded-full bg-gray-300 flex items-center justify-center">
                        <svg class="w-6 h-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                        </svg>
                      </div>
                    </div>

                    <!-- 消息内容 -->
                    <div class="flex-1 min-w-0">
                      <div class="flex items-center justify-between mb-1">
                        <h4 class="text-sm font-medium text-gray-900 truncate">
                          {{ message.senderName }}
                        </h4>
                        <span class="text-xs text-gray-500">{{ formatTime(message.createdAt) }}</span>
                      </div>
                      <p class="text-sm text-gray-600 truncate mb-1">{{ message.subject }}</p>
                      <p class="text-xs text-gray-500 line-clamp-2">{{ message.content }}</p>
                      
                      <!-- 标签 -->
                      <div class="flex items-center gap-2 mt-2">
                        <span
                          v-if="message.isUnread"
                          class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
                        >
                          未读
                        </span>
                        <span
                          v-if="message.isImportant"
                          class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800"
                        >
                          重要
                        </span>
                        <span
                          v-if="message.type === 'system'"
                          class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
                        >
                          系统
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- 加载更多 -->
            <div v-if="hasMore && !loading" class="p-4 border-t border-gray-200">
              <button
                @click="loadMore"
                class="w-full py-2 text-blue-600 hover:text-blue-700 font-medium transition-colors"
                :disabled="loadingMore"
              >
                {{ loadingMore ? '加载中...' : '加载更多' }}
              </button>
            </div>
          </div>

          <!-- 右侧消息详情 -->
          <div class="flex-1 flex flex-col">
            <div v-if="selectedMessage" class="flex-1 flex flex-col">
              <!-- 消息头部 -->
              <div class="p-6 border-b border-gray-200">
                <div class="flex items-start justify-between mb-4">
                  <div>
                    <h2 class="text-xl font-semibold text-gray-900 mb-2">{{ selectedMessage.subject }}</h2>
                    <div class="flex items-center gap-4 text-sm text-gray-600">
                      <span>来自: {{ selectedMessage.senderName }}</span>
                      <span>{{ formatDateTime(selectedMessage.createdAt) }}</span>
                    </div>
                  </div>
                  <div class="flex items-center gap-2">
                    <button
                      v-if="selectedMessage.isUnread"
                      @click="markAsRead(selectedMessage.id)"
                      class="p-2 text-gray-600 hover:text-gray-800 hover:bg-gray-100 rounded-lg transition-colors"
                      title="标记为已读"
                    >
                      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                      </svg>
                    </button>
                    <button
                      @click="deleteMessage(selectedMessage.id)"
                      class="p-2 text-red-600 hover:text-red-800 hover:bg-red-100 rounded-lg transition-colors"
                      title="删除消息"
                    >
                      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                    </button>
                  </div>
                </div>
                
                <!-- 标签 -->
                <div class="flex items-center gap-2">
                  <span
                    v-if="selectedMessage.isImportant"
                    class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800"
                  >
                    重要消息
                  </span>
                  <span
                    v-if="selectedMessage.type === 'system'"
                    class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
                  >
                    系统通知
                  </span>
                  <span
                    v-if="selectedMessage.type === 'user'"
                    class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800"
                  >
                    用户消息
                  </span>
                </div>
              </div>

              <!-- 消息内容 -->
              <div class="flex-1 p-6 overflow-y-auto">
                <div class="prose prose-sm max-w-none">
                  <p class="text-gray-700 leading-relaxed whitespace-pre-wrap">{{ selectedMessage.content }}</p>
                </div>

                <!-- 附件 -->
                <div v-if="selectedMessage.attachments && selectedMessage.attachments.length > 0" class="mt-6">
                  <h3 class="text-sm font-medium text-gray-900 mb-3">附件</h3>
                  <div class="space-y-2">
                    <div
                      v-for="attachment in selectedMessage.attachments"
                      :key="attachment.id"
                      class="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                    >
                      <div class="flex items-center gap-3">
                        <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                        </svg>
                        <div>
                          <p class="text-sm font-medium text-gray-900">{{ attachment.name }}</p>
                          <p class="text-xs text-gray-500">{{ formatFileSize(attachment.size) }}</p>
                        </div>
                      </div>
                      <button
                        @click="downloadAttachment(attachment)"
                        class="p-2 text-blue-600 hover:text-blue-800 hover:bg-blue-100 rounded-lg transition-colors"
                      >
                        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                        </svg>
                      </button>
                    </div>
                  </div>
                </div>
              </div>

              <!-- 快速回复 -->
              <div v-if="selectedMessage.type === 'user'" class="p-6 border-t border-gray-200">
                <div class="flex gap-3">
                  <input
                    v-model="replyContent"
                    type="text"
                    placeholder="快速回复..."
                    class="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    @keyup.enter="sendReply"
                  />
                  <button
                    @click="sendReply"
                    class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
                    :disabled="!replyContent.trim() || sendingReply"
                  >
                    {{ sendingReply ? '发送中...' : '发送' }}
                  </button>
                </div>
              </div>
            </div>

            <!-- 空状态 -->
            <div v-else class="flex-1 flex items-center justify-center">
              <div class="text-center">
                <svg class="w-16 h-16 mx-auto text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
                </svg>
                <h3 class="text-lg font-medium text-gray-900 mb-2">选择消息</h3>
                <p class="text-gray-600">从左侧列表中选择一条消息查看详情</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useMessageStore } from '@/stores/message'
import { useToastStore } from '@/stores/toast'
import type { Message } from '@/types/message'

const authStore = useAuthStore()
const messageStore = useMessageStore()
const toastStore = useToastStore()

// 响应式状态
const messages = ref<Message[]>([])
const selectedMessage = ref<Message | null>(null)
const loading = ref(false)
const loadingMore = ref(false)
const hasMore = ref(false)
const searchKeyword = ref('')
const selectedFilter = ref('all')
const replyContent = ref('')
const sendingReply = ref(false)

// 计算属性
const unreadCount = computed(() => messages.value.filter(m => m.isUnread).length)

// 方法
async function loadMessages(reset = true) {
  loading.value = true
  
  try {
    const filter = {
      page: reset ? 1 : Math.floor(messages.value.length / 20) + 1,
      pageSize: 20,
      sortBy: 'CREATED_AT_DESC' as any
    }
    
    const response = await messageStore.fetchMessages(filter)
    
    if (reset) {
      messages.value = response.items || []
    } else {
      messages.value = [...messages.value, ...(response.items || [])]
    }
    
    hasMore.value = (response.items?.length || 0) >= 20
  } catch (error) {
    console.error('Failed to load messages:', error)
    toastStore.error('加载消息失败')
  } finally {
    loading.value = false
  }
}

async function loadMore() {
  if (loadingMore.value) return
  
  loadingMore.value = true
  try {
    await loadMessages(false)
  } finally {
    loadingMore.value = false
  }
}

function selectMessage(message: Message) {
  selectedMessage.value = message
  
  // 如果消息未读，自动标记为已读
  if (message.isUnread) {
    markAsRead(message.id)
  }
}

async function markAsRead(messageId: string) {
  try {
    await messageStore.markAsRead(messageId)
    
    // 更新本地状态
    const message = messages.value.find(m => m.id === messageId)
    if (message) {
      message.isUnread = false
    }
    
    if (selectedMessage.value?.id === messageId) {
      selectedMessage.value.isUnread = false
    }
    
    toastStore.success('已标记为已读')
  } catch (error) {
    console.error('Failed to mark message as read:', error)
    toastStore.error('操作失败')
  }
}

async function deleteMessage(messageId: string) {
  if (!confirm('确定要删除这条消息吗？')) return
  
  try {
    await messageStore.deleteMessage(messageId)
    
    // 从列表中移除
    messages.value = messages.value.filter(m => m.id !== messageId)
    
    // 如果删除的是当前选中的消息，清空选择
    if (selectedMessage.value?.id === messageId) {
      selectedMessage.value = null
    }
    
    toastStore.success('消息已删除')
  } catch (error) {
    console.error('Failed to delete message:', error)
    toastStore.error('删除失败')
  }
}

async function sendReply() {
  if (!selectedMessage.value || !replyContent.value.trim()) return
  
  sendingReply.value = true
  try {
    await messageStore.createMessage({
      conversationId: selectedMessage.value.conversationId,
      content: replyContent.value,
      recipientId: selectedMessage.value.senderId
    })
    
    replyContent.value = ''
    toastStore.success('回复已发送')
  } catch (error) {
    console.error('Failed to send reply:', error)
    toastStore.error('发送失败')
  } finally {
    sendingReply.value = false
  }
}

async function downloadAttachment(attachment: any) {
  try {
    await messageStore.downloadAttachment(attachment.id)
    toastStore.success('附件下载已开始')
  } catch (error) {
    console.error('Failed to download attachment:', error)
    toastStore.error('下载失败')
  }
}

function handleSearch() {
  loadMessages()
}

function handleFilterChange() {
  loadMessages()
}

function refreshMessages() {
  loadMessages()
}

function formatTime(dateString: string): string {
  const date = new Date(dateString)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  
  const minutes = Math.floor(diff / 60000)
  const hours = Math.floor(diff / 3600000)
  const days = Math.floor(diff / 86400000)
  
  if (minutes < 1) return '刚刚'
  if (minutes < 60) return `${minutes}分钟前`
  if (hours < 24) return `${hours}小时前`
  if (days < 7) return `${days}天前`
  
  return date.toLocaleDateString()
}

function formatDateTime(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function formatFileSize(bytes: number): string {
  const sizes = ['B', 'KB', 'MB', 'GB']
  if (bytes === 0) return '0 B'
  
  const i = Math.floor(Math.log(bytes) / Math.log(1024))
  return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i]
}

// 生命周期
onMounted(() => {
  loadMessages()
})
</script>

<style scoped>
.prose {
  line-height: 1.6;
}

.prose p {
  margin: 0;
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>