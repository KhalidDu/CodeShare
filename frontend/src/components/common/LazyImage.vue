<template>
  <div
    ref="containerRef"
    class="lazy-image-container"
    :class="{ 'is-loading': isLoading, 'has-error': hasError }"
  >
    <!-- 占位符 -->
    <div v-if="isLoading && !hasError" class="lazy-image-placeholder">
      <div class="lazy-image-skeleton"></div>
      <span v-if="showLoadingText" class="loading-text">加载中...</span>
    </div>

    <!-- 错误状态 -->
    <div v-else-if="hasError" class="lazy-image-error">
      <div class="error-icon">📷</div>
      <span class="error-text">图片加载失败</span>
    </div>

    <!-- 实际图片 -->
    <img
      v-else
      :src="currentSrc"
      :alt="alt"
      :class="imageClass"
      class="lazy-image"
      @load="handleLoad"
      @error="handleError"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'

/**
 * 懒加载图片组件
 * 支持 Intersection Observer API 实现图片懒加载
 */

interface Props {
  /** 图片源地址 */
  src: string
  /** 图片替代文本 */
  alt?: string
  /** 占位符图片 */
  placeholder?: string
  /** 图片类名 */
  imageClass?: string
  /** 是否显示加载文本 */
  showLoadingText?: boolean
  /** 根边距（用于提前加载） */
  rootMargin?: string
  /** 交叉阈值 */
  threshold?: number
}

const props = withDefaults(defineProps<Props>(), {
  alt: '',
  placeholder: '',
  imageClass: '',
  showLoadingText: false,
  rootMargin: '50px',
  threshold: 0.1
})

const emit = defineEmits<{
  load: [event: Event]
  error: [event: Event]
}>()

const containerRef = ref<HTMLElement>()
const isLoading = ref(true)
const hasError = ref(false)
const currentSrc = ref('')
const observer = ref<IntersectionObserver>()

/**
 * 处理图片加载成功
 */
const handleLoad = (event: Event) => {
  isLoading.value = false
  hasError.value = false
  emit('load', event)
}

/**
 * 处理图片加载失败
 */
const handleError = (event: Event) => {
  isLoading.value = false
  hasError.value = true
  emit('error', event)
}

/**
 * 开始加载图片
 */
const loadImage = () => {
  if (props.src && !currentSrc.value) {
    currentSrc.value = props.src
  }
}

/**
 * 创建 Intersection Observer
 */
const createObserver = () => {
  if (!containerRef.value) return

  observer.value = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          loadImage()
          // 加载后停止观察
          if (observer.value) {
            observer.value.unobserve(entry.target)
          }
        }
      })
    },
    {
      rootMargin: props.rootMargin,
      threshold: props.threshold
    }
  )

  observer.value.observe(containerRef.value)
}

/**
 * 销毁 Observer
 */
const destroyObserver = () => {
  if (observer.value) {
    observer.value.disconnect()
    observer.value = undefined
  }
}

/**
 * 重置状态
 */
const reset = () => {
  isLoading.value = true
  hasError.value = false
  currentSrc.value = ''
  destroyObserver()
  createObserver()
}

// 监听 src 变化
watch(() => props.src, () => {
  reset()
})

onMounted(() => {
  // 检查是否支持 Intersection Observer
  if ('IntersectionObserver' in window) {
    createObserver()
  } else {
    // 不支持则直接加载
    loadImage()
  }
})

onUnmounted(() => {
  destroyObserver()
})

// 暴露方法给父组件
defineExpose({
  reset,
  loadImage
})
</script>

<style scoped>
.lazy-image-container {
  position: relative;
  display: inline-block;
  overflow: hidden;
}

.lazy-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: opacity 0.3s ease;
}

.lazy-image-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  background-color: #f5f5f5;
  color: #999;
}

.lazy-image-skeleton {
  width: 60%;
  height: 60%;
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  animation: skeleton-loading 1.5s infinite;
  border-radius: 4px;
}

.loading-text {
  margin-top: 8px;
  font-size: 12px;
}

.lazy-image-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  background-color: #fafafa;
  color: #999;
  border: 1px dashed #ddd;
}

.error-icon {
  font-size: 24px;
  margin-bottom: 8px;
}

.error-text {
  font-size: 12px;
}

@keyframes skeleton-loading {
  0% {
    background-position: -200% 0;
  }
  100% {
    background-position: 200% 0;
  }
}

/* 响应式设计 */
@media (max-width: 768px) {
  .loading-text,
  .error-text {
    font-size: 11px;
  }

  .error-icon {
    font-size: 20px;
  }
}
</style>
