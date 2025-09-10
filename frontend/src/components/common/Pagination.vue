<template>
  <nav class="pagination" v-if="totalPages > 1" aria-label="分页导航">
    <div class="pagination-info">
      <span class="page-info">
        第 {{ currentPage }} 页，共 {{ totalPages }} 页
      </span>
      <span class="item-info">
        显示第 {{ startItem }}-{{ endItem }} 项，共 {{ totalItems }} 项
      </span>
    </div>

    <div class="pagination-controls">
      <!-- 首页按钮 -->
      <button
        @click="goToPage(1)"
        :disabled="currentPage === 1"
        class="pagination-btn first-btn"
        title="首页"
        aria-label="跳转到首页"
      >
        <svg class="btn-icon" viewBox="0 0 24 24" fill="currentColor">
          <path d="M18.41,16.59L13.82,12L18.41,7.41L17,6L11,12L17,18L18.41,16.59M6,6H8V18H6V6Z"/>
        </svg>
        <span class="btn-text">首页</span>
      </button>

      <!-- 上一页按钮 -->
      <button
        @click="goToPage(currentPage - 1)"
        :disabled="currentPage === 1"
        class="pagination-btn prev-btn"
        title="上一页"
        aria-label="跳转到上一页"
      >
        <svg class="btn-icon" viewBox="0 0 24 24" fill="currentColor">
          <path d="M15.41,16.59L10.83,12L15.41,7.41L14,6L8,12L14,18L15.41,16.59Z"/>
        </svg>
        <span class="btn-text">上一页</span>
      </button>

      <!-- 页码按钮 -->
      <div class="page-numbers">
        <button
          v-for="page in visiblePages"
          :key="page"
          @click="typeof page === 'number' ? goToPage(page) : undefined"
          :class="[
            'page-btn',
            {
              active: page === currentPage,
              ellipsis: page === '...'
            }
          ]"
          :disabled="page === '...'"
          :aria-label="typeof page === 'number' ? `跳转到第${page}页` : undefined"
          :aria-current="page === currentPage ? 'page' : undefined"
        >
          {{ page }}
        </button>
      </div>

      <!-- 下一页按钮 -->
      <button
        @click="goToPage(currentPage + 1)"
        :disabled="currentPage === totalPages"
        class="pagination-btn next-btn"
        title="下一页"
        aria-label="跳转到下一页"
      >
        <span class="btn-text">下一页</span>
        <svg class="btn-icon" viewBox="0 0 24 24" fill="currentColor">
          <path d="M8.59,16.59L13.17,12L8.59,7.41L10,6L16,12L10,18L8.59,16.59Z"/>
        </svg>
      </button>

      <!-- 末页按钮 -->
      <button
        @click="goToPage(totalPages)"
        :disabled="currentPage === totalPages"
        class="pagination-btn last-btn"
        title="末页"
        aria-label="跳转到末页"
      >
        <span class="btn-text">末页</span>
        <svg class="btn-icon" viewBox="0 0 24 24" fill="currentColor">
          <path d="M5.59,7.41L10.18,12L5.59,16.59L7,18L13,12L7,6L5.59,7.41M16,6H18V18H16V6Z"/>
        </svg>
      </button>
    </div>

    <!-- 页面跳转 -->
    <div class="pagination-jump" v-if="showJumpTo">
      <label class="jump-label">跳转到</label>
      <input
        v-model.number="jumpToPage"
        @keyup.enter="handleJumpTo"
        @blur="handleJumpTo"
        type="number"
        :min="1"
        :max="totalPages"
        class="jump-input"
        aria-label="输入页码"
      />
      <button @click="handleJumpTo" class="jump-btn">确定</button>
    </div>

    <!-- 每页显示数量选择 -->
    <div class="pagination-size" v-if="showSizeChanger">
      <label class="size-label">每页显示</label>
      <select
        :value="pageSize"
        @change="handleSizeChange"
        class="size-select"
        aria-label="选择每页显示数量"
      >
        <option
          v-for="size in pageSizeOptions"
          :key="size"
          :value="size"
        >
          {{ size }} 条
        </option>
      </select>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'

interface Props {
  currentPage: number
  totalPages: number
  totalItems: number
  pageSize: number
  showJumpTo?: boolean
  showSizeChanger?: boolean
  pageSizeOptions?: number[]
  maxVisiblePages?: number
}

interface Emits {
  (e: 'page-change', page: number): void
  (e: 'size-change', size: number): void
}

const props = withDefaults(defineProps<Props>(), {
  showJumpTo: true,
  showSizeChanger: true,
  pageSizeOptions: () => [10, 20, 50, 100],
  maxVisiblePages: 7
})

const emit = defineEmits<Emits>()

// 响应式状态
const jumpToPage = ref(props.currentPage)

// 计算属性
const startItem = computed(() => {
  return (props.currentPage - 1) * props.pageSize + 1
})

const endItem = computed(() => {
  return Math.min(props.currentPage * props.pageSize, props.totalItems)
})

/**
 * 计算可见的页码
 */
const visiblePages = computed(() => {
  const { currentPage, totalPages, maxVisiblePages } = props
  const pages: (number | string)[] = []

  if (totalPages <= maxVisiblePages) {
    // 如果总页数不超过最大可见页数，显示所有页码
    for (let i = 1; i <= totalPages; i++) {
      pages.push(i)
    }
  } else {
    // 复杂的页码显示逻辑
    const halfVisible = Math.floor(maxVisiblePages / 2)
    let startPage = Math.max(1, currentPage - halfVisible)
    let endPage = Math.min(totalPages, currentPage + halfVisible)

    // 调整起始和结束页码，确保显示足够的页码
    if (endPage - startPage + 1 < maxVisiblePages) {
      if (startPage === 1) {
        endPage = Math.min(totalPages, startPage + maxVisiblePages - 1)
      } else {
        startPage = Math.max(1, endPage - maxVisiblePages + 1)
      }
    }

    // 添加首页
    if (startPage > 1) {
      pages.push(1)
      if (startPage > 2) {
        pages.push('...')
      }
    }

    // 添加中间页码
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i)
    }

    // 添加末页
    if (endPage < totalPages) {
      if (endPage < totalPages - 1) {
        pages.push('...')
      }
      pages.push(totalPages)
    }
  }

  return pages
})

/**
 * 跳转到指定页面
 */
function goToPage(page: number) {
  if (page >= 1 && page <= props.totalPages && page !== props.currentPage) {
    emit('page-change', page)
  }
}

/**
 * 处理页面跳转
 */
function handleJumpTo() {
  const page = jumpToPage.value
  if (page && page >= 1 && page <= props.totalPages) {
    goToPage(page)
  } else {
    // 重置为当前页
    jumpToPage.value = props.currentPage
  }
}

/**
 * 处理每页大小变化
 */
function handleSizeChange(event: Event) {
  const target = event.target as HTMLSelectElement
  const newSize = parseInt(target.value)
  emit('size-change', newSize)
}
</script>

<style scoped>
.pagination {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  padding: var(--spacing-xl);
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

/* 分页信息 */
.pagination-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.875rem;
  color: #6c757d;
}

.page-info {
  font-weight: 500;
}

.item-info {
  color: #adb5bd;
}

/* 分页控制 */
.pagination-controls {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: var(--spacing-sm);
}

.pagination-btn {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-sm) var(--spacing-md);
  border: 1px solid #dee2e6;
  background: white;
  color: #6c757d;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 0.875rem;
  min-height: 36px;
}

.pagination-btn:hover:not(:disabled) {
  border-color: #007bff;
  color: #007bff;
  background: #f8f9fa;
}

.pagination-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  background: #f8f9fa;
}

.btn-icon {
  width: 16px;
  height: 16px;
  flex-shrink: 0;
}

.btn-text {
  white-space: nowrap;
}

/* 页码按钮 */
.page-numbers {
  display: flex;
  gap: var(--spacing-xs);
}

.page-btn {
  width: 36px;
  height: 36px;
  border: 1px solid #dee2e6;
  background: white;
  color: #6c757d;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 0.875rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.page-btn:hover:not(:disabled):not(.active) {
  border-color: #007bff;
  color: #007bff;
  background: #f8f9fa;
}

.page-btn.active {
  border-color: #007bff;
  background: #007bff;
  color: white;
  font-weight: 500;
}

.page-btn.ellipsis {
  border: none;
  background: none;
  cursor: default;
  color: #adb5bd;
}

.page-btn:disabled {
  cursor: not-allowed;
}

/* 页面跳转 */
.pagination-jump {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  font-size: 0.875rem;
}

.jump-label {
  color: #6c757d;
  white-space: nowrap;
}

.jump-input {
  width: 60px;
  padding: var(--spacing-xs) var(--spacing-sm);
  border: 1px solid #dee2e6;
  border-radius: 4px;
  text-align: center;
  font-size: 0.875rem;
}

.jump-input:focus {
  outline: none;
  border-color: #007bff;
}

.jump-btn {
  padding: var(--spacing-xs) var(--spacing-md);
  border: 1px solid #007bff;
  background: #007bff;
  color: white;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.875rem;
  transition: all 0.3s ease;
}

.jump-btn:hover {
  background: #0056b3;
  border-color: #0056b3;
}

/* 每页大小选择 */
.pagination-size {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  font-size: 0.875rem;
}

.size-label {
  color: #6c757d;
  white-space: nowrap;
}

.size-select {
  padding: var(--spacing-xs) var(--spacing-sm);
  border: 1px solid #dee2e6;
  border-radius: 4px;
  background: white;
  font-size: 0.875rem;
  cursor: pointer;
}

.size-select:focus {
  outline: none;
  border-color: #007bff;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .pagination {
    padding: var(--spacing-lg);
    gap: var(--spacing-sm);
  }

  .pagination-info {
    flex-direction: column;
    gap: var(--spacing-xs);
    text-align: center;
  }

  .pagination-controls {
    flex-wrap: wrap;
    gap: var(--spacing-xs);
  }

  .btn-text {
    display: none;
  }

  .pagination-btn {
    padding: var(--spacing-sm);
    min-width: 36px;
  }

  .page-numbers {
    order: -1;
    width: 100%;
    justify-content: center;
    flex-wrap: wrap;
  }

  .pagination-jump,
  .pagination-size {
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .pagination-controls {
    justify-content: space-between;
  }

  .page-numbers {
    gap: calc(var(--spacing-xs) / 2);
  }

  .page-btn {
    width: 32px;
    height: 32px;
    font-size: 0.8125rem;
  }

  .pagination-btn {
    min-width: 32px;
    height: 32px;
  }

  .btn-icon {
    width: 14px;
    height: 14px;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .pagination-btn,
  .page-btn,
  .jump-btn {
    transition: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .pagination {
    border: 2px solid #000;
  }

  .pagination-btn,
  .page-btn,
  .jump-input,
  .size-select {
    border-width: 2px;
  }

  .page-btn.active {
    border-width: 2px;
  }
}

/* 打印样式 */
@media print {
  .pagination {
    display: none;
  }
}
</style>
