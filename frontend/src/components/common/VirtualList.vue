<template>
  <div
    ref="containerRef"
    class="virtual-list-container"
    :style="{ height: containerHeight + 'px' }"
    @scroll="handleScroll"
  >
    <!-- 占位空间 - 上方 -->
    <div :style="{ height: offsetY + 'px' }"></div>

    <!-- 可见项目 -->
    <div
      v-for="(item, index) in visibleItems"
      :key="getItemKey(item, startIndex + index)"
      :style="{ height: itemHeight + 'px' }"
      class="virtual-list-item"
    >
      <slot :item="item" :index="startIndex + index"></slot>
    </div>

    <!-- 占位空间 - 下方 -->
    <div :style="{ height: (totalHeight - offsetY - visibleHeight) + 'px' }"></div>
  </div>
</template>

<script setup lang="ts" generic="T">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'

/**
 * 虚拟滚动列表组件
 * 用于优化大量数据的列表渲染性能
 */

interface Props {
  /** 列表数据 */
  items: T[]
  /** 每项高度 */
  itemHeight: number
  /** 容器高度 */
  containerHeight: number
  /** 缓冲区大小（额外渲染的项目数） */
  buffer?: number
  /** 获取项目唯一键的函数 */
  keyField?: string | ((item: T, index: number) => string | number)
}

const props = withDefaults(defineProps<Props>(), {
  buffer: 5,
  keyField: 'id'
})

const containerRef = ref<HTMLElement>()
const scrollTop = ref(0)

// 计算属性
const totalHeight = computed(() => props.items.length * props.itemHeight)
const visibleCount = computed(() => Math.ceil(props.containerHeight / props.itemHeight))
const startIndex = computed(() => Math.max(0, Math.floor(scrollTop.value / props.itemHeight) - props.buffer))
const endIndex = computed(() => Math.min(props.items.length - 1, startIndex.value + visibleCount.value + props.buffer * 2))
const visibleItems = computed(() => props.items.slice(startIndex.value, endIndex.value + 1))
const offsetY = computed(() => startIndex.value * props.itemHeight)
const visibleHeight = computed(() => (endIndex.value - startIndex.value + 1) * props.itemHeight)

/**
 * 获取项目的唯一键
 */
const getItemKey = (item: T, index: number): string | number => {
  if (typeof props.keyField === 'function') {
    return props.keyField(item, index)
  }
  return (item as any)[props.keyField] || index
}

/**
 * 处理滚动事件
 */
const handleScroll = (event: Event) => {
  const target = event.target as HTMLElement
  scrollTop.value = target.scrollTop
}

/**
 * 滚动到指定索引
 */
const scrollToIndex = (index: number) => {
  if (containerRef.value) {
    const targetScrollTop = index * props.itemHeight
    containerRef.value.scrollTop = targetScrollTop
  }
}

/**
 * 滚动到顶部
 */
const scrollToTop = () => {
  scrollToIndex(0)
}

/**
 * 滚动到底部
 */
const scrollToBottom = () => {
  scrollToIndex(props.items.length - 1)
}

// 监听数据变化，重置滚动位置
watch(() => props.items.length, () => {
  scrollTop.value = 0
})

// 暴露方法给父组件
defineExpose({
  scrollToIndex,
  scrollToTop,
  scrollToBottom
})
</script>

<style scoped>
.virtual-list-container {
  overflow-y: auto;
  position: relative;
}

.virtual-list-item {
  display: flex;
  align-items: center;
}

/* 滚动条样式优化 */
.virtual-list-container::-webkit-scrollbar {
  width: 8px;
}

.virtual-list-container::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 4px;
}

.virtual-list-container::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 4px;
}

.virtual-list-container::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}
</style>
