<!--
  骨架屏加载组件
  在内容加载时显示占位符
-->
<template>
  <div
    :class="[
      'skeleton-loader',
      {
        'skeleton-animated': animated,
        'skeleton-rounded': rounded
      }
    ]"
    :style="skeletonStyle"
    role="status"
    aria-label="内容加载中"
  >
    <!-- 自定义骨架内容 -->
    <slot v-if="$slots.default" />

    <!-- 预设骨架类型 -->
    <template v-else>
      <!-- 文本骨架 -->
      <div v-if="type === 'text'" class="skeleton-text">
        <div
          v-for="line in lines"
          :key="line"
          class="skeleton-line"
          :style="getLineStyle(line)"
        ></div>
      </div>

      <!-- 头像骨架 -->
      <div v-else-if="type === 'avatar'" class="skeleton-avatar"></div>

      <!-- 图片骨架 -->
      <div v-else-if="type === 'image'" class="skeleton-image"></div>

      <!-- 按钮骨架 -->
      <div v-else-if="type === 'button'" class="skeleton-button"></div>

      <!-- 卡片骨架 -->
      <div v-else-if="type === 'card'" class="skeleton-card">
        <div class="skeleton-card-header">
          <div class="skeleton-avatar"></div>
          <div class="skeleton-text">
            <div class="skeleton-line" style="width: 60%"></div>
            <div class="skeleton-line" style="width: 40%"></div>
          </div>
        </div>
        <div class="skeleton-card-content">
          <div class="skeleton-line"></div>
          <div class="skeleton-line"></div>
          <div class="skeleton-line" style="width: 80%"></div>
        </div>
        <div class="skeleton-card-footer">
          <div class="skeleton-button"></div>
          <div class="skeleton-button"></div>
        </div>
      </div>

      <!-- 列表项骨架 -->
      <div v-else-if="type === 'list-item'" class="skeleton-list-item">
        <div class="skeleton-avatar"></div>
        <div class="skeleton-content">
          <div class="skeleton-line" style="width: 70%"></div>
          <div class="skeleton-line" style="width: 50%"></div>
        </div>
      </div>

      <!-- 表格行骨架 -->
      <div v-else-if="type === 'table-row'" class="skeleton-table-row">
        <div
          v-for="col in columns"
          :key="col"
          class="skeleton-table-cell"
          :style="{ width: getColumnWidth(col) }"
        ></div>
      </div>

      <!-- 默认矩形骨架 -->
      <div v-else class="skeleton-rect"></div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

// 组件属性
interface Props {
  type?: 'text' | 'avatar' | 'image' | 'button' | 'card' | 'list-item' | 'table-row' | 'rect'
  width?: string | number
  height?: string | number
  lines?: number
  columns?: number
  animated?: boolean
  rounded?: boolean
  variant?: 'light' | 'dark'
}

const props = withDefaults(defineProps<Props>(), {
  type: 'rect',
  lines: 3,
  columns: 4,
  animated: true,
  rounded: false,
  variant: 'light'
})

// 计算属性
const skeletonStyle = computed(() => {
  const style: Record<string, string> = {}

  if (props.width) {
    style.width = typeof props.width === 'number' ? `${props.width}px` : props.width
  }

  if (props.height) {
    style.height = typeof props.height === 'number' ? `${props.height}px` : props.height
  }

  return style
})

/**
 * 获取文本行样式
 */
function getLineStyle(lineIndex: number) {
  const style: Record<string, string> = {}

  // 最后一行通常较短
  if (lineIndex === props.lines) {
    style.width = '60%'
  }

  return style
}

/**
 * 获取表格列宽度
 */
function getColumnWidth(columnIndex: number) {
  const widths = ['20%', '30%', '25%', '25%']
  return widths[columnIndex - 1] || '25%'
}
</script>

<style scoped>
.skeleton-loader {
  --skeleton-color: #e5e7eb;
  --skeleton-highlight: #f3f4f6;
}

.skeleton-loader[data-variant="dark"] {
  --skeleton-color: #374151;
  --skeleton-highlight: #4b5563;
}

.skeleton-loader * {
  background: var(--skeleton-color);
  border-radius: 4px;
}

.skeleton-rounded * {
  border-radius: 8px;
}

/* 动画效果 */
.skeleton-animated * {
  background: linear-gradient(
    90deg,
    var(--skeleton-color) 25%,
    var(--skeleton-highlight) 50%,
    var(--skeleton-color) 75%
  );
  background-size: 200% 100%;
  animation: skeleton-loading 1.5s infinite;
}

@keyframes skeleton-loading {
  0% {
    background-position: 200% 0;
  }
  100% {
    background-position: -200% 0;
  }
}

/* 文本骨架 */
.skeleton-text {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.skeleton-line {
  height: 16px;
  width: 100%;
}

/* 头像骨架 */
.skeleton-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50% !important;
  flex-shrink: 0;
}

/* 图片骨架 */
.skeleton-image {
  width: 100%;
  height: 200px;
  border-radius: 8px;
}

/* 按钮骨架 */
.skeleton-button {
  width: 80px;
  height: 32px;
  border-radius: 6px;
}

/* 矩形骨架 */
.skeleton-rect {
  width: 100%;
  height: 20px;
}

/* 卡片骨架 */
.skeleton-card {
  padding: 16px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.skeleton-card-header {
  display: flex;
  align-items: center;
  gap: 12px;
}

.skeleton-card-header .skeleton-text {
  flex: 1;
  gap: 6px;
}

.skeleton-card-header .skeleton-line {
  height: 14px;
}

.skeleton-card-content {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.skeleton-card-footer {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}

/* 列表项骨架 */
.skeleton-list-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 0;
}

.skeleton-list-item .skeleton-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.skeleton-list-item .skeleton-line {
  height: 14px;
}

/* 表格行骨架 */
.skeleton-table-row {
  display: flex;
  gap: 16px;
  padding: 12px 0;
}

.skeleton-table-cell {
  height: 16px;
  flex-shrink: 0;
}

/* 响应式设计 */
@media (max-width: 640px) {
  .skeleton-card {
    padding: 12px;
    gap: 12px;
  }

  .skeleton-card-header {
    gap: 8px;
  }

  .skeleton-list-item {
    gap: 8px;
  }

  .skeleton-table-row {
    gap: 8px;
  }
}

/* 减少动画效果（无障碍性） */
@media (prefers-reduced-motion: reduce) {
  .skeleton-animated * {
    animation: none;
    background: var(--skeleton-color);
  }
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .skeleton-loader {
    --skeleton-color: #666;
    --skeleton-highlight: #999;
  }
}
</style>
