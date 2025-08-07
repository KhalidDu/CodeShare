<template>
  <div class="page-title">
    <div class="title-content">
      <!-- 标题图标 -->
      <div class="title-icon" v-if="icon">
        <svg class="icon" viewBox="0 0 24 24" fill="currentColor">
          <path :d="icon"/>
        </svg>
      </div>

      <!-- 标题文本 -->
      <div class="title-text">
        <h1 class="title">{{ title }}</h1>
        <p class="subtitle" v-if="subtitle">{{ subtitle }}</p>
      </div>
    </div>

    <!-- 操作按钮区域 -->
    <div class="title-actions" v-if="$slots.actions">
      <slot name="actions"></slot>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  title: string
  subtitle?: string
  icon?: string
}

defineProps<Props>()
defineSlots<{
  actions(): any
}>()
</script>

<style scoped>
.page-title {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  padding: 1.5rem 0 1rem 0;
  border-bottom: 1px solid #dee2e6;
  margin-bottom: 1.5rem;
}

.title-content {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  flex: 1;
  min-width: 0;
}

.title-icon {
  flex-shrink: 0;
  width: 48px;
  height: 48px;
  background-color: #007bff;
  color: white;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 4px rgba(0, 123, 255, 0.2);
}

.icon {
  width: 24px;
  height: 24px;
}

.title-text {
  flex: 1;
  min-width: 0;
}

.title {
  font-size: 1.875rem;
  font-weight: 700;
  color: #212529;
  margin: 0;
  line-height: 1.2;
  word-wrap: break-word;
}

.subtitle {
  font-size: 1rem;
  color: #6c757d;
  margin: 0.5rem 0 0 0;
  line-height: 1.4;
  word-wrap: break-word;
}

.title-actions {
  flex-shrink: 0;
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  flex-wrap: wrap;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .page-title {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
    padding: 1rem 0 0.75rem 0;
  }

  .title-content {
    gap: 0.75rem;
  }

  .title-icon {
    width: 40px;
    height: 40px;
    border-radius: 10px;
  }

  .icon {
    width: 20px;
    height: 20px;
  }

  .title {
    font-size: 1.5rem;
  }

  .subtitle {
    font-size: 0.875rem;
    margin-top: 0.375rem;
  }

  .title-actions {
    justify-content: flex-start;
    gap: 0.5rem;
  }
}

@media (max-width: 480px) {
  .page-title {
    padding: 0.75rem 0 0.5rem 0;
    margin-bottom: 1rem;
  }

  .title-content {
    gap: 0.5rem;
  }

  .title-icon {
    width: 36px;
    height: 36px;
    border-radius: 8px;
  }

  .icon {
    width: 18px;
    height: 18px;
  }

  .title {
    font-size: 1.375rem;
  }

  .subtitle {
    font-size: 0.8125rem;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .title-icon {
    box-shadow: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .page-title {
    border-bottom-width: 2px;
  }

  .title-icon {
    border: 2px solid currentColor;
  }
}

/* 打印样式 */
@media print {
  .page-title {
    border-bottom: 2px solid #000;
    margin-bottom: 1rem;
  }

  .title-icon {
    display: none;
  }

  .title-actions {
    display: none;
  }

  .title {
    color: #000;
  }

  .subtitle {
    color: #666;
  }
}
</style>
