<template>
  <div :class="filterClasses">
    <!-- 过滤器头部 -->
    <div class="filter-header">
      <h3 class="filter-title">
        <i class="fas fa-filter mr-2"></i>
        {{ title }}
      </h3>
      <div class="header-actions">
        <button
          v-if="hasActiveFilters"
          @click="resetAllFilters"
          class="reset-btn"
          title="重置所有过滤器"
        >
          <i class="fas fa-times"></i>
          重置
        </button>
        <button
          @click="togglePresetManager"
          class="preset-btn"
          title="管理预设过滤器"
        >
          <i class="fas fa-save"></i>
          预设
        </button>
        <button
          @click="toggleExpanded"
          class="expand-btn"
          :title="isExpanded ? '收起过滤器' : '展开过滤器'"
        >
          <i :class="isExpanded ? 'fas fa-chevron-up' : 'fas fa-chevron-down'"></i>
        </button>
      </div>
    </div>

    <!-- 快速过滤按钮 -->
    <div v-if="showQuickFilters" class="quick-filters">
      <button
        v-for="quickFilter in quickFilters"
        :key="quickFilter.id"
        @click="applyQuickFilter(quickFilter)"
        :class="getQuickFilterClasses(quickFilter)"
      >
        <i :class="quickFilter.icon"></i>
        {{ quickFilter.label }}
      </button>
    </div>

    <!-- 主要过滤区域 -->
    <div v-show="isExpanded" class="filter-content">
      <!-- 搜索框 -->
      <div class="filter-section">
        <label class="filter-label">
          <i class="fas fa-search mr-2"></i>
          搜索
        </label>
        <div class="search-input-wrapper">
          <input
            v-model="filters.search"
            type="text"
            :placeholder="searchPlaceholder"
            class="search-input"
            @input="debounceUpdate"
          />
          <button
            v-if="filters.search"
            @click="clearSearch"
            class="clear-search"
          >
            <i class="fas fa-times"></i>
          </button>
        </div>
      </div>

      <!-- 消息类型过滤 -->
      <div class="filter-section">
        <label class="filter-label">
          <i class="fas fa-envelope mr-2"></i>
          消息类型
        </label>
        <div class="filter-options">
          <label
            v-for="messageType in messageTypes"
            :key="messageType.value"
            class="filter-option"
          >
            <input
              type="checkbox"
              :value="messageType.value"
              v-model="filters.messageTypes"
              @change="immediateUpdate"
            />
            <span class="option-label">
              <i :class="messageType.icon"></i>
              {{ messageType.label }}
            </span>
          </label>
        </div>
      </div>

      <!-- 消息状态过滤 -->
      <div class="filter-section">
        <label class="filter-label">
          <i class="fas fa-info-circle mr-2"></i>
          消息状态
        </label>
        <div class="filter-options">
          <label
            v-for="status in messageStatuses"
            :key="status.value"
            class="filter-option"
          >
            <input
              type="checkbox"
              :value="status.value"
              v-model="filters.statuses"
              @change="immediateUpdate"
            />
            <span class="option-label">
              <span :class="['status-dot', status.color]"></span>
              {{ status.label }}
            </span>
          </label>
        </div>
      </div>

      <!-- 优先级过滤 -->
      <div class="filter-section">
        <label class="filter-label">
          <i class="fas fa-flag mr-2"></i>
          优先级
        </label>
        <div class="filter-options">
          <label
            v-for="priority in priorities"
            :key="priority.value"
            class="filter-option"
          >
            <input
              type="checkbox"
              :value="priority.value"
              v-model="filters.priorities"
              @change="immediateUpdate"
            />
            <span class="option-label">
              <span :class="['priority-badge', priority.color]">
                {{ priority.icon }}
              </span>
              {{ priority.label }}
            </span>
          </label>
        </div>
      </div>

      <!-- 发送者/接收者过滤 -->
      <div class="filter-section">
        <label class="filter-label">
          <i class="fas fa-user mr-2"></i>
          用户过滤
        </label>
        <div class="user-filters">
          <div class="user-filter">
            <select v-model="filters.senderFilter" @change="immediateUpdate">
              <option value="">发送者</option>
              <option
                v-for="user in users"
                :key="user.id"
                :value="user.id"
              >
                {{ user.name }}
              </option>
            </select>
          </div>
          <div class="user-filter">
            <select v-model="filters.receiverFilter" @change="immediateUpdate">
              <option value="">接收者</option>
              <option
                v-for="user in users"
                :key="user.id"
                :value="user.id"
              >
                {{ user.name }}
              </option>
            </select>
          </div>
        </div>
      </div>

      <!-- 日期范围过滤 -->
      <div class="filter-section">
        <label class="filter-label">
          <i class="fas fa-calendar mr-2"></i>
          日期范围
        </label>
        <div class="date-filters">
          <div class="date-filter">
            <label>开始日期</label>
            <input
              v-model="filters.startDate"
              type="date"
              class="date-input"
              @change="immediateUpdate"
            />
          </div>
          <div class="date-filter">
            <label>结束日期</label>
            <input
              v-model="filters.endDate"
              type="date"
              class="date-input"
              @change="immediateUpdate"
            />
          </div>
        </div>
        <div class="quick-date-filters">
          <button
            v-for="dateRange in dateRanges"
            :key="dateRange.id"
            @click="applyDateRange(dateRange)"
            class="quick-date-btn"
            :class="{ active: isDateRangeActive(dateRange) }"
          >
            {{ dateRange.label }}
          </button>
        </div>
      </div>

      <!-- 附件过滤 -->
      <div class="filter-section">
        <label class="filter-label">
          <i class="fas fa-paperclip mr-2"></i>
          附件过滤
        </label>
        <div class="filter-options">
          <label class="filter-option">
            <input
              type="checkbox"
              v-model="filters.hasAttachments"
              @change="immediateUpdate"
            />
            <span class="option-label">有附件</span>
          </label>
          <label class="filter-option">
            <input
              type="checkbox"
              v-model="filters.hasImages"
              @change="immediateUpdate"
            />
            <span class="option-label">有图片</span>
          </label>
          <label class="filter-option">
            <input
              type="checkbox"
              v-model="filters.hasDocuments"
              @change="immediateUpdate"
            />
            <span class="option-label">有文档</span>
          </label>
        </div>
      </div>

      <!-- 标签过滤 -->
      <div v-if="showTagFilter && availableTags.length > 0" class="filter-section">
        <label class="filter-label">
          <i class="fas fa-tags mr-2"></i>
          标签
        </label>
        <div class="tag-filters">
          <span
            v-for="tag in availableTags"
            :key="tag.id"
            @click="toggleTag(tag)"
            :class="getTagClasses(tag)"
            class="tag-filter"
          >
            {{ tag.name }}
          </span>
        </div>
      </div>
    </div>

    <!-- 活跃过滤器显示 -->
    <div v-if="hasActiveFilters" class="active-filters">
      <div class="active-filters-header">
        <span>活跃过滤器:</span>
        <button @click="resetAllFilters" class="clear-all-btn">
          清除全部
        </button>
      </div>
      <div class="active-filter-pills">
        <span
          v-for="activeFilter in activeFiltersList"
          :key="activeFilter.id"
          class="active-filter-pill"
        >
          {{ activeFilter.label }}
          <button
            @click="removeFilter(activeFilter)"
            class="remove-filter-btn"
          >
            <i class="fas fa-times"></i>
          </button>
        </span>
      </div>
    </div>

    <!-- 预设管理模态框 -->
    <div v-if="showPresetManager" class="preset-manager-modal">
      <div class="modal-backdrop" @click="togglePresetManager"></div>
      <div class="modal-content">
        <div class="modal-header">
          <h3>管理预设过滤器</h3>
          <button @click="togglePresetManager" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <div class="preset-list">
            <div
              v-for="preset in savedPresets"
              :key="preset.id"
              class="preset-item"
            >
              <div class="preset-info">
                <span class="preset-name">{{ preset.name }}</span>
                <span class="preset-description">{{ preset.description }}</span>
              </div>
              <div class="preset-actions">
                <button
                  @click="applyPreset(preset)"
                  class="apply-preset-btn"
                  title="应用预设"
                >
                  <i class="fas fa-check"></i>
                </button>
                <button
                  @click="deletePreset(preset)"
                  class="delete-preset-btn"
                  title="删除预设"
                >
                  <i class="fas fa-trash"></i>
                </button>
              </div>
            </div>
          </div>
          <div class="save-preset-form">
            <input
              v-model="newPresetName"
              type="text"
              placeholder="预设名称"
              class="preset-name-input"
            />
            <input
              v-model="newPresetDescription"
              type="text"
              placeholder="预设描述"
              class="preset-description-input"
            />
            <button
              @click="saveCurrentAsPreset"
              :disabled="!newPresetName || !hasActiveFilters"
              class="save-preset-btn"
            >
              <i class="fas fa-save"></i>
              保存当前过滤器
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useMessageStore } from '@/stores/message'
import { messageService } from '@/services/messageService'
import type { Message, MessageStatus, MessagePriority, MessageType, User, Tag } from '@/types/message'

// 定义Props
interface Props {
  title?: string
  searchPlaceholder?: string
  showQuickFilters?: boolean
  showTagFilter?: boolean
  compactMode?: boolean
  initiallyExpanded?: boolean
  debounceDelay?: number
  availableUsers?: User[]
  availableTags?: Tag[]
  enablePresets?: boolean
  enableDateRanges?: boolean
  maxActiveFilters?: number
}

const props = withDefaults(defineProps<Props>(), {
  title: '消息过滤器',
  searchPlaceholder: '搜索消息内容、发送者...',
  showQuickFilters: true,
  showTagFilter: true,
  compactMode: false,
  initiallyExpanded: false,
  debounceDelay: 500,
  availableUsers: () => [],
  availableTags: () => [],
  enablePresets: true,
  enableDateRanges: true,
  maxActiveFilters: 10
})

// 定义Emits
interface Emits {
  (e: 'filter-change', filters: MessageFilters): void
  (e: 'filter-reset'): void
  (e: 'preset-save', preset: FilterPreset): void
  (e: 'preset-delete', presetId: string): void
  (e: 'preset-apply', preset: FilterPreset): void
}

const emit = defineEmits<Emits>()

// Store
const messageStore = useMessageStore()

// 响应式状态
const isExpanded = ref(props.initiallyExpanded)
const showPresetManager = ref(false)
const debounceTimer = ref<NodeJS.Timeout>()
const newPresetName = ref('')
const newPresetDescription = ref('')

// 过滤器状态
const filters = ref<MessageFilters>({
  search: '',
  messageTypes: [],
  statuses: [],
  priorities: [],
  senderFilter: '',
  receiverFilter: '',
  startDate: '',
  endDate: '',
  hasAttachments: false,
  hasImages: false,
  hasDocuments: false,
  tags: []
})

// 预设过滤器
const savedPresets = ref<FilterPreset[]>([
  {
    id: 'unread',
    name: '未读消息',
    description: '显示所有未读消息',
    filters: {
      statuses: [MessageStatus.UNREAD],
      messageTypes: [],
      priorities: [],
      senderFilter: '',
      receiverFilter: '',
      startDate: '',
      endDate: '',
      hasAttachments: false,
      hasImages: false,
      hasDocuments: false,
      tags: []
    }
  },
  {
    id: 'important',
    name: '重要消息',
    description: '显示高优先级消息',
    filters: {
      statuses: [],
      messageTypes: [],
      priorities: [MessagePriority.HIGH, MessagePriority.URGENT],
      senderFilter: '',
      receiverFilter: '',
      startDate: '',
      endDate: '',
      hasAttachments: false,
      hasImages: false,
      hasDocuments: false,
      tags: []
    }
  },
  {
    id: 'recent',
    name: '最近消息',
    description: '显示最近7天的消息',
    filters: {
      statuses: [],
      messageTypes: [],
      priorities: [],
      senderFilter: '',
      receiverFilter: '',
      startDate: getDateRange(7),
      endDate: new Date().toISOString().split('T')[0],
      hasAttachments: false,
      hasImages: false,
      hasDocuments: false,
      tags: []
    }
  }
])

// 消息类型选项
const messageTypes = [
  { value: MessageType.SENT, label: '已发送', icon: 'fas fa-paper-plane' },
  { value: MessageType.RECEIVED, label: '已接收', icon: 'fas fa-inbox' },
  { value: MessageType.DRAFT, label: '草稿', icon: 'fas fa-file-alt' },
  { value: MessageType.SYSTEM, label: '系统', icon: 'fas fa-cog' }
]

// 消息状态选项
const messageStatuses = [
  { value: MessageStatus.UNREAD, label: '未读', color: 'bg-blue-500' },
  { value: MessageStatus.READ, label: '已读', color: 'bg-green-500' },
  { value: MessageStatus.REPLIED, label: '已回复', color: 'bg-purple-500' },
  { value: MessageStatus.FORWARDED, label: '已转发', color: 'bg-orange-500' },
  { value: MessageStatus.DELETED, label: '已删除', color: 'bg-red-500' },
  { value: MessageStatus.ARCHIVED, label: '已归档', color: 'bg-gray-500' }
]

// 优先级选项
const priorities = [
  { value: MessagePriority.LOW, label: '低', icon: '↓', color: 'bg-green-500' },
  { value: MessagePriority.NORMAL, label: '普通', icon: '→', color: 'bg-gray-500' },
  { value: MessagePriority.HIGH, label: '高', icon: '↑', color: 'bg-yellow-500' },
  { value: MessagePriority.URGENT, label: '紧急', icon: '!', color: 'bg-red-500' }
]

// 快速过滤器
const quickFilters = [
  { id: 'all', label: '全部', icon: 'fas fa-inbox', filters: {} },
  { id: 'unread', label: '未读', icon: 'fas fa-envelope', filters: { statuses: [MessageStatus.UNREAD] } },
  { id: 'important', label: '重要', icon: 'fas fa-star', filters: { priorities: [MessagePriority.HIGH, MessagePriority.URGENT] } },
  { id: 'attachments', label: '有附件', icon: 'fas fa-paperclip', filters: { hasAttachments: true } },
  { id: 'recent', label: '最近', icon: 'fas fa-clock', filters: { startDate: getDateRange(7) } }
]

// 日期范围选项
const dateRanges = [
  { id: 'today', label: '今天', days: 0 },
  { id: 'yesterday', label: '昨天', days: 1 },
  { id: 'week', label: '本周', days: 7 },
  { id: 'month', label: '本月', days: 30 },
  { id: 'quarter', label: '本季度', days: 90 }
]

// 用户列表
const users = computed(() => {
  return props.availableUsers.length > 0 ? props.availableUsers : messageStore.users
})

// 计算属性
const filterClasses = computed(() => {
  const baseClasses = [
    'message-filter',
    'bg-white',
    'dark:bg-gray-800',
    'border',
    'border-gray-200',
    'dark:border-gray-700',
    'rounded-lg',
    'shadow-sm'
  ]

  if (props.compactMode) {
    baseClasses.push('compact-mode', 'p-3')
  } else {
    baseClasses.push('p-4')
  }

  return baseClasses.join(' ')
})

const hasActiveFilters = computed(() => {
  return activeFiltersList.value.length > 0
})

const activeFiltersList = computed(() => {
  const activeFilters: ActiveFilter[] = []

  // 搜索过滤器
  if (filters.value.search) {
    activeFilters.push({
      id: 'search',
      label: `搜索: ${filters.value.search}`,
      type: 'search',
      value: filters.value.search
    })
  }

  // 消息类型过滤器
  filters.value.messageTypes.forEach(type => {
    const messageType = messageTypes.find(mt => mt.value === type)
    if (messageType) {
      activeFilters.push({
        id: `type-${type}`,
        label: `类型: ${messageType.label}`,
        type: 'messageType',
        value: type
      })
    }
  })

  // 状态过滤器
  filters.value.statuses.forEach(status => {
    const messageStatus = messageStatuses.find(ms => ms.value === status)
    if (messageStatus) {
      activeFilters.push({
        id: `status-${status}`,
        label: `状态: ${messageStatus.label}`,
        type: 'status',
        value: status
      })
    }
  })

  // 优先级过滤器
  filters.value.priorities.forEach(priority => {
    const priorityInfo = priorities.find(p => p.value === priority)
    if (priorityInfo) {
      activeFilters.push({
        id: `priority-${priority}`,
        label: `优先级: ${priorityInfo.label}`,
        type: 'priority',
        value: priority
      })
    }
  })

  // 发送者过滤器
  if (filters.value.senderFilter) {
    const sender = users.value.find(u => u.id === filters.value.senderFilter)
    if (sender) {
      activeFilters.push({
        id: 'sender',
        label: `发送者: ${sender.name}`,
        type: 'sender',
        value: filters.value.senderFilter
      })
    }
  }

  // 接收者过滤器
  if (filters.value.receiverFilter) {
    const receiver = users.value.find(u => u.id === filters.value.receiverFilter)
    if (receiver) {
      activeFilters.push({
        id: 'receiver',
        label: `接收者: ${receiver.name}`,
        type: 'receiver',
        value: filters.value.receiverFilter
      })
    }
  }

  // 日期范围过滤器
  if (filters.value.startDate) {
    activeFilters.push({
      id: 'startDate',
      label: `开始日期: ${filters.value.startDate}`,
      type: 'startDate',
      value: filters.value.startDate
    })
  }

  if (filters.value.endDate) {
    activeFilters.push({
      id: 'endDate',
      label: `结束日期: ${filters.value.endDate}`,
      type: 'endDate',
      value: filters.value.endDate
    })
  }

  // 附件过滤器
  if (filters.value.hasAttachments) {
    activeFilters.push({
      id: 'attachments',
      label: '有附件',
      type: 'attachments',
      value: true
    })
  }

  if (filters.value.hasImages) {
    activeFilters.push({
      id: 'images',
      label: '有图片',
      type: 'images',
      value: true
    })
  }

  if (filters.value.hasDocuments) {
    activeFilters.push({
      id: 'documents',
      label: '有文档',
      type: 'documents',
      value: true
    })
  }

  // 标签过滤器
  filters.value.tags.forEach(tagId => {
    const tag = props.availableTags.find(t => t.id === tagId)
    if (tag) {
      activeFilters.push({
        id: `tag-${tagId}`,
        label: `标签: ${tag.name}`,
        type: 'tag',
        value: tagId
      })
    }
  })

  return activeFilters.slice(0, props.maxActiveFilters)
})

// 方法定义
const toggleExpanded = () => {
  isExpanded.value = !isExpanded.value
}

const togglePresetManager = () => {
  showPresetManager.value = !showPresetManager.value
}

const debounceUpdate = () => {
  clearTimeout(debounceTimer.value)
  debounceTimer.value = setTimeout(() => {
    emit('filter-change', { ...filters.value })
  }, props.debounceDelay)
}

const immediateUpdate = () => {
  clearTimeout(debounceTimer.value)
  emit('filter-change', { ...filters.value })
}

const clearSearch = () => {
  filters.value.search = ''
  immediateUpdate()
}

const resetAllFilters = () => {
  filters.value = {
    search: '',
    messageTypes: [],
    statuses: [],
    priorities: [],
    senderFilter: '',
    receiverFilter: '',
    startDate: '',
    endDate: '',
    hasAttachments: false,
    hasImages: false,
    hasDocuments: false,
    tags: []
  }
  emit('filter-reset')
  emit('filter-change', { ...filters.value })
}

const applyQuickFilter = (quickFilter: any) => {
  // 重置过滤器
  resetAllFilters()
  
  // 应用快速过滤器
  Object.assign(filters.value, quickFilter.filters)
  
  immediateUpdate()
}

const applyDateRange = (dateRange: any) => {
  const today = new Date()
  const startDate = new Date(today)
  startDate.setDate(startDate.getDate() - dateRange.days)
  
  filters.value.startDate = startDate.toISOString().split('T')[0]
  filters.value.endDate = today.toISOString().split('T')[0]
  
  immediateUpdate()
}

const isDateRangeActive = (dateRange: any) => {
  if (!filters.value.startDate || !filters.value.endDate) {
    return false
  }
  
  const today = new Date()
  const startDate = new Date(today)
  startDate.setDate(startDate.getDate() - dateRange.days)
  const expectedStartDate = startDate.toISOString().split('T')[0]
  const expectedEndDate = today.toISOString().split('T')[0]
  
  return filters.value.startDate === expectedStartDate && 
         filters.value.endDate === expectedEndDate
}

const toggleTag = (tag: Tag) => {
  const index = filters.value.tags.indexOf(tag.id)
  if (index > -1) {
    filters.value.tags.splice(index, 1)
  } else {
    filters.value.tags.push(tag.id)
  }
  immediateUpdate()
}

const getTagClasses = (tag: Tag) => {
  const isActive = filters.value.tags.includes(tag.id)
  return [
    'tag-filter',
    {
      'active': isActive,
      'bg-blue-100': isActive,
      'text-blue-700': isActive,
      'dark:bg-blue-900': isActive,
      'dark:text-blue-300': isActive,
      'bg-gray-100': !isActive,
      'text-gray-700': !isActive,
      'dark:bg-gray-700': !isActive,
      'dark:text-gray-300': !isActive
    }
  ]
}

const getQuickFilterClasses = (quickFilter: any) => {
  // 简化的活动检测，实际应该根据当前过滤器状态
  return [
    'quick-filter-btn',
    {
      'active': false, // 这里应该有更复杂的逻辑
      'bg-blue-100': false,
      'text-blue-700': false,
      'dark:bg-blue-900': false,
      'dark:text-blue-300': false,
      'bg-gray-100': true,
      'text-gray-700': true,
      'dark:bg-gray-700': true,
      'dark:text-gray-300': true
    }
  ]
}

const removeFilter = (filter: ActiveFilter) => {
  switch (filter.type) {
    case 'search':
      filters.value.search = ''
      break
    case 'messageType':
      filters.value.messageTypes = filters.value.messageTypes.filter(t => t !== filter.value)
      break
    case 'status':
      filters.value.statuses = filters.value.statuses.filter(s => s !== filter.value)
      break
    case 'priority':
      filters.value.priorities = filters.value.priorities.filter(p => p !== filter.value)
      break
    case 'sender':
      filters.value.senderFilter = ''
      break
    case 'receiver':
      filters.value.receiverFilter = ''
      break
    case 'startDate':
      filters.value.startDate = ''
      break
    case 'endDate':
      filters.value.endDate = ''
      break
    case 'attachments':
      filters.value.hasAttachments = false
      break
    case 'images':
      filters.value.hasImages = false
      break
    case 'documents':
      filters.value.hasDocuments = false
      break
    case 'tag':
      filters.value.tags = filters.value.tags.filter(t => t !== filter.value)
      break
  }
  immediateUpdate()
}

const saveCurrentAsPreset = () => {
  if (!newPresetName.value || !hasActiveFilters.value) {
    return
  }

  const newPreset: FilterPreset = {
    id: `custom-${Date.now()}`,
    name: newPresetName.value,
    description: newPresetDescription.value,
    filters: { ...filters.value }
  }

  savedPresets.value.push(newPreset)
  emit('preset-save', newPreset)

  // 重置表单
  newPresetName.value = ''
  newPresetDescription.value = ''
}

const deletePreset = (preset: FilterPreset) => {
  const index = savedPresets.value.findIndex(p => p.id === preset.id)
  if (index > -1) {
    savedPresets.value.splice(index, 1)
    emit('preset-delete', preset.id)
  }
}

const applyPreset = (preset: FilterPreset) => {
  filters.value = { ...preset.filters }
  emit('preset-apply', preset)
  immediateUpdate()
  togglePresetManager()
}

// 辅助函数
const getDateRange = (daysAgo: number): string => {
  const date = new Date()
  date.setDate(date.getDate() - daysAgo)
  return date.toISOString().split('T')[0]
}

// 生命周期钩子
onMounted(() => {
  // 加载保存的预设
  // 这里可以从localStorage或API加载
})

onUnmounted(() => {
  clearTimeout(debounceTimer.value)
})

// 暴露方法
defineExpose({
  filters,
  hasActiveFilters,
  activeFiltersList,
  resetAllFilters,
  applyPreset,
  saveCurrentAsPreset,
  deletePreset,
  isExpanded,
  toggleExpanded
})
</script>

<style scoped>
.message-filter {
  @apply relative;
}

.message-filter.compact-mode {
  @apply space-y-3;
}

/* 头部样式 */
.filter-header {
  @apply flex items-center justify-between mb-4;
}

.filter-title {
  @apply text-lg font-semibold text-gray-900 dark:text-white flex items-center;
}

.header-actions {
  @apply flex items-center space-x-2;
}

.reset-btn,
.preset-btn,
.expand-btn {
  @apply px-3 py-1 text-sm rounded-md border border-gray-300 dark:border-gray-600
         bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300
         hover:bg-gray-50 dark:hover:bg-gray-600 transition-colors;
}

/* 快速过滤器 */
.quick-filters {
  @apply flex flex-wrap gap-2 mb-4;
}

.quick-filter-btn {
  @apply px-3 py-1.5 text-sm rounded-full border border-gray-300 dark:border-gray-600
         bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300
         hover:bg-gray-50 dark:hover:bg-gray-600 transition-colors flex items-center space-x-1;
}

.quick-filter-btn.active {
  @apply bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300
         border-blue-300 dark:border-blue-600;
}

/* 过滤器内容 */
.filter-content {
  @apply space-y-4;
}

.filter-section {
  @apply space-y-2;
}

.filter-label {
  @apply text-sm font-medium text-gray-700 dark:text-gray-300 flex items-center;
}

/* 搜索输入框 */
.search-input-wrapper {
  @apply relative;
}

.search-input {
  @apply w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md
         bg-white dark:bg-gray-700 text-gray-900 dark:text-white
         focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent;
}

.clear-search {
  @apply absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600;
}

/* 过滤器选项 */
.filter-options {
  @apply grid grid-cols-2 md:grid-cols-3 gap-2;
}

.filter-option {
  @apply flex items-center space-x-2 cursor-pointer;
}

.filter-option input[type="checkbox"] {
  @apply rounded border-gray-300 dark:border-gray-600 text-blue-600 
         focus:ring-blue-500 focus:ring-2;
}

.option-label {
  @apply text-sm text-gray-700 dark:text-gray-300 flex items-center space-x-1;
}

/* 用户过滤器 */
.user-filters {
  @apply grid grid-cols-1 md:grid-cols-2 gap-2;
}

.user-filter select {
  @apply w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md
         bg-white dark:bg-gray-700 text-gray-900 dark:text-white
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

/* 日期过滤器 */
.date-filters {
  @apply grid grid-cols-1 md:grid-cols-2 gap-2;
}

.date-filter {
  @apply space-y-1;
}

.date-filter label {
  @apply text-xs text-gray-600 dark:text-gray-400;
}

.date-input {
  @apply w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md
         bg-white dark:bg-gray-700 text-gray-900 dark:text-white
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.quick-date-filters {
  @apply flex flex-wrap gap-2 mt-2;
}

.quick-date-btn {
  @apply px-2 py-1 text-xs rounded-md border border-gray-300 dark:border-gray-600
         bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300
         hover:bg-gray-50 dark:hover:bg-gray-600 transition-colors;
}

.quick-date-btn.active {
  @apply bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300
         border-blue-300 dark:border-blue-600;
}

/* 标签过滤器 */
.tag-filters {
  @apply flex flex-wrap gap-2;
}

.tag-filter {
  @apply px-2 py-1 text-xs rounded-full cursor-pointer transition-colors;
}

/* 活跃过滤器 */
.active-filters {
  @apply mt-4 p-3 bg-gray-50 dark:bg-gray-700 rounded-lg;
}

.active-filters-header {
  @apply flex items-center justify-between mb-2;
}

.active-filters-header span {
  @apply text-sm font-medium text-gray-700 dark:text-gray-300;
}

.clear-all-btn {
  @apply text-xs text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300;
}

.active-filter-pills {
  @apply flex flex-wrap gap-2;
}

.active-filter-pill {
  @apply flex items-center space-x-1 px-2 py-1 bg-blue-100 dark:bg-blue-900 
         text-blue-700 dark:text-blue-300 rounded-full text-xs;
}

.remove-filter-btn {
  @apply hover:text-blue-900 dark:hover:text-blue-100;
}

/* 预设管理模态框 */
.preset-manager-modal {
  @apply fixed inset-0 z-50 flex items-center justify-center p-4;
}

.modal-backdrop {
  @apply absolute inset-0 bg-black bg-opacity-50;
}

.modal-content {
  @apply relative bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full max-h-[80vh] overflow-y-auto;
}

.modal-header {
  @apply flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700;
}

.modal-header h3 {
  @apply text-lg font-semibold text-gray-900 dark:text-white;
}

.close-btn {
  @apply text-gray-400 hover:text-gray-600 dark:hover:text-gray-300;
}

.modal-body {
  @apply p-4 space-y-4;
}

.preset-list {
  @apply space-y-2 max-h-60 overflow-y-auto;
}

.preset-item {
  @apply flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 rounded-lg;
}

.preset-info {
  @apply flex-1;
}

.preset-name {
  @apply block text-sm font-medium text-gray-900 dark:text-white;
}

.preset-description {
  @apply block text-xs text-gray-500 dark:text-gray-400;
}

.preset-actions {
  @apply flex items-center space-x-2;
}

.apply-preset-btn {
  @apply p-1 text-green-600 hover:text-green-800 dark:hover:text-green-400;
}

.delete-preset-btn {
  @apply p-1 text-red-600 hover:text-red-800 dark:hover:text-red-400;
}

.save-preset-form {
  @apply space-y-2 pt-4 border-t border-gray-200 dark:border-gray-700;
}

.preset-name-input,
.preset-description-input {
  @apply w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md
         bg-white dark:bg-gray-700 text-gray-900 dark:text-white
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.save-preset-btn {
  @apply w-full px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700
         focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2
         disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center space-x-2;
}

/* 状态和优先级指示器 */
.status-dot {
  @apply inline-block w-2 h-2 rounded-full;
}

.priority-badge {
  @apply inline-flex items-center justify-center w-4 h-4 rounded text-xs;
}

/* 深色模式优化 */
.dark .filter-header {
  @apply border-gray-700;
}

.dark .filter-title {
  @apply text-white;
}

.dark .filter-label {
  @apply text-gray-300;
}

.dark .filter-option input[type="checkbox"] {
  @apply border-gray-600;
}

.dark .option-label {
  @apply text-gray-300;
}

.dark .search-input {
  @apply bg-gray-700 text-white border-gray-600;
}

.dark .user-filter select {
  @apply bg-gray-700 text-white border-gray-600;
}

.dark .date-input {
  @apply bg-gray-700 text-white border-gray-600;
}

.dark .active-filters {
  @apply bg-gray-700;
}

.dark .preset-item {
  @apply bg-gray-700;
}

.dark .preset-name {
  @apply text-white;
}

.dark .preset-description {
  @apply text-gray-400;
}

.dark .preset-name-input,
.dark .preset-description-input {
  @apply bg-gray-700 text-white border-gray-600;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .filter-options {
    @apply grid-cols-1;
  }
  
  .user-filters {
    @apply grid-cols-1;
  }
  
  .date-filters {
    @apply grid-cols-1;
  }
  
  .quick-filters {
    @apply flex-col;
  }
  
  .active-filter-pills {
    @apply flex-col;
  }
  
  .active-filter-pill {
    @apply justify-between;
  }
}

/* 动画效果 */
.message-filter {
  @apply transition-all duration-200 ease-in-out;
}

.filter-content {
  @apply transition-all duration-200 ease-in-out;
}

/* 可访问性 */
.filter-option input[type="checkbox"]:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

.search-input:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

.user-filter select:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

.date-input:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

.preset-name-input:focus,
.preset-description-input:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .message-filter {
    @apply border-2;
  }
  
  .filter-option input[type="checkbox"] {
    @apply border-2;
  }
}

/* 减少动画 */
@media (prefers-reduced-motion: reduce) {
  .message-filter {
    @apply transition-none;
  }
  
  .filter-content {
    @apply transition-none;
  }
}
</style>