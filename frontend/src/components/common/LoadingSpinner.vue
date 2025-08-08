<!--
  加载旋转器组件
  可配置大小、颜色和样式的加载指示器
-->
<template>
  <div
    :class="[
      'loading-spinner',
      `size-${size}`,
      `variant-${variant}`
    ]"
    :style="customStyle"
    role="status"
    :aria-label="ariaLabel"
  >
    <!-- 旋转器 -->
    <div class="spinner">
      <div class="spinner-circle"></div>
    </div>

    <!-- 加载文本 -->
    <div v-if="showText && text" class="loading-text">
      {{ text }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

// 组件属性
interface Props {
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl'
  variant?: 'primary' | 'secondary' | 'light' | 'dark'
  color?: string
  text?: string
  showText?: boolean
  inline?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  variant: 'primary',
  showText: true,
  inline: false
})

// 计算属性
const ariaLabel = computed(() => {
  return props.text || '加载中...'
})

const customStyle = computed(() => {
  const style: Record<string, string> = {}

  if (props.color) {
    style['--spinner-color'] = props.color
  }

  if (props.inline) {
    style.display = 'inline-flex'
  }

  return style
})
</script>

<style scoped>
.loading-spinner {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.spinner {
  position: relative;
  display: inline-block;
}

.spinner-circle {
  border: 2px solid transparent;
  border-top: 2px solid var(--spinner-color, currentColor);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

/* 尺寸变体 */
.size-xs .spinner-circle {
  width: 12px;
  height: 12px;
  border-width: 1px;
}

.size-sm .spinner-circle {
  width: 16px;
  height: 16px;
  border-width: 2px;
}

.size-md .spinner-circle {
  width: 24px;
  height: 24px;
  border-width: 2px;
}

.size-lg .spinner-circle {
  width: 32px;
  height: 32px;
  border-width: 3px;
}

.size-xl .spinner-circle {
  width: 48px;
  height: 48px;
  border-width: 4px;
}

/* 颜色变体 */
.variant-primary {
  --spinner-color: #3b82f6;
  color: #3b82f6;
}

.variant-secondary {
  --spinner-color: #6b7280;
  color: #6b7280;
}

.variant-light {
  --spinner-color: #f3f4f6;
  color: #f3f4f6;
}

.variant-dark {
  --spinner-color: #1f2937;
  color: #1f2937;
}

.loading-text {
  font-size: 14px;
  color: currentColor;
  text-align: center;
}

.size-xs .loading-text {
  font-size: 11px;
}

.size-sm .loading-text {
  font-size: 12px;
}

.size-lg .loading-text {
  font-size: 16px;
}

.size-xl .loading-text {
  font-size: 18px;
}

/* 旋转动画 */
@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 减少动画效果（无障碍性） */
@media (prefers-reduced-motion: reduce) {
  .spinner-circle {
    animation: none;
  }

  .spinner-circle::after {
    content: '';
    position: absolute;
    top: 50%;
    left: 50%;
    width: 4px;
    height: 4px;
    background: var(--spinner-color, currentColor);
    border-radius: 50%;
    transform: translate(-50%, -50%);
    animation: pulse 1.5s ease-in-out infinite;
  }
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}
</style>
