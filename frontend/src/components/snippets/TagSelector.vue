<template>
  <div class="tag-selector">
    <label class="selector-label">标签</label>

    <div class="tag-input-container">
      <div class="selected-tags" v-if="selectedTags.length > 0">
        <span
          v-for="tag in selectedTags"
          :key="tag.id"
          class="tag-chip"
          :style="{ backgroundColor: tag.color }"
        >
          {{ tag.name }}
          <button
            @click="removeTag(tag)"
            class="tag-remove"
            type="button"
            :aria-label="`移除标签 ${tag.name}`"
          >
            ×
          </button>
        </span>
      </div>

      <div class="tag-input-wrapper">
        <input
          v-model="tagInput"
          @input="onTagInput"
          @keydown="onKeyDown"
          @focus="showSuggestions = true"
          @blur="onInputBlur"
          type="text"
          class="tag-input"
          placeholder="输入标签名称..."
          autocomplete="off"
        />

        <div v-if="showSuggestions && filteredSuggestions.length > 0" class="tag-suggestions">
          <div
            v-for="(suggestion, index) in filteredSuggestions"
            :key="suggestion.id"
            @mousedown="selectSuggestion(suggestion)"
            class="suggestion-item"
            :class="{ active: index === activeSuggestionIndex }"
          >
            <span
              class="suggestion-color"
              :style="{ backgroundColor: suggestion.color }"
            ></span>
            <span class="suggestion-name">{{ suggestion.name }}</span>
          </div>

          <div
            v-if="tagInput && !filteredSuggestions.some(s => s.name.toLowerCase() === tagInput.toLowerCase())"
            @mousedown="createNewTag"
            class="suggestion-item create-new"
            :class="{ active: activeSuggestionIndex === filteredSuggestions.length }"
          >
            <span class="suggestion-icon">+</span>
            <span class="suggestion-name">创建新标签 "{{ tagInput }}"</span>
          </div>
        </div>
      </div>
    </div>

    <div v-if="availableTags.length > 0" class="popular-tags">
      <span class="popular-label">常用标签:</span>
      <button
        v-for="tag in popularTags"
        :key="tag.id"
        @click="addTag(tag)"
        type="button"
        class="popular-tag"
        :style="{ backgroundColor: tag.color }"
        :disabled="isTagSelected(tag)"
      >
        {{ tag.name }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { tagService } from '@/services/tagService'
import type { Tag, CreateTagRequest } from '@/types'

// Props 定义
interface Props {
  modelValue: Tag[]
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  disabled: false
})

// Emits 定义
interface Emits {
  'update:modelValue': [tags: Tag[]]
}

const emit = defineEmits<Emits>()

// 响应式数据
const tagInput = ref('')
const showSuggestions = ref(false)
const activeSuggestionIndex = ref(-1)
const availableTags = ref<Tag[]>([])
const loading = ref(false)

// 计算属性
const selectedTags = computed(() => props.modelValue)

const filteredSuggestions = computed(() => {
  if (!tagInput.value) return []

  const input = tagInput.value.toLowerCase()
  return availableTags.value
    .filter(tag =>
      tag.name.toLowerCase().includes(input) &&
      !isTagSelected(tag)
    )
    .slice(0, 5) // 限制显示数量
})

const popularTags = computed(() => {
  return availableTags.value
    .filter(tag => !isTagSelected(tag))
    .slice(0, 8) // 显示前8个常用标签
})

/**
 * 检查标签是否已选中
 */
const isTagSelected = (tag: Tag): boolean => {
  return selectedTags.value.some(selected => selected.id === tag.id)
}

/**
 * 添加标签
 */
const addTag = (tag: Tag) => {
  if (!isTagSelected(tag)) {
    const newTags = [...selectedTags.value, tag]
    emit('update:modelValue', newTags)
  }
}

/**
 * 移除标签
 */
const removeTag = (tag: Tag) => {
  const newTags = selectedTags.value.filter(selected => selected.id !== tag.id)
  emit('update:modelValue', newTags)
}

/**
 * 标签输入处理
 */
const onTagInput = () => {
  activeSuggestionIndex.value = -1
  showSuggestions.value = true
}

/**
 * 键盘事件处理
 */
const onKeyDown = (event: KeyboardEvent) => {
  const suggestions = filteredSuggestions.value
  const canCreateNew = tagInput.value && !suggestions.some(s => s.name.toLowerCase() === tagInput.value.toLowerCase())
  const totalItems = suggestions.length + (canCreateNew ? 1 : 0)

  switch (event.key) {
    case 'ArrowDown':
      event.preventDefault()
      activeSuggestionIndex.value = Math.min(activeSuggestionIndex.value + 1, totalItems - 1)
      break

    case 'ArrowUp':
      event.preventDefault()
      activeSuggestionIndex.value = Math.max(activeSuggestionIndex.value - 1, -1)
      break

    case 'Enter':
      event.preventDefault()
      if (activeSuggestionIndex.value >= 0) {
        if (activeSuggestionIndex.value < suggestions.length) {
          selectSuggestion(suggestions[activeSuggestionIndex.value])
        } else if (canCreateNew) {
          createNewTag()
        }
      } else if (tagInput.value) {
        createNewTag()
      }
      break

    case 'Escape':
      showSuggestions.value = false
      activeSuggestionIndex.value = -1
      break

    case 'Backspace':
      if (!tagInput.value && selectedTags.value.length > 0) {
        removeTag(selectedTags.value[selectedTags.value.length - 1])
      }
      break
  }
}

/**
 * 输入框失焦处理
 */
const onInputBlur = () => {
  // 延迟隐藏建议，以便点击建议项能正常工作
  setTimeout(() => {
    showSuggestions.value = false
    activeSuggestionIndex.value = -1
  }, 200)
}

/**
 * 选择建议项
 */
const selectSuggestion = (tag: Tag) => {
  addTag(tag)
  tagInput.value = ''
  showSuggestions.value = false
  activeSuggestionIndex.value = -1
}

/**
 * 创建新标签
 */
const createNewTag = async () => {
  if (!tagInput.value.trim()) return

  try {
    loading.value = true

    // 生成随机颜色
    const colors = ['#0969da', '#1f883d', '#d1242f', '#8250df', '#fb8500', '#0969da']
    const randomColor = colors[Math.floor(Math.random() * colors.length)]

    const newTagRequest: CreateTagRequest = {
      name: tagInput.value.trim(),
      color: randomColor
    }

    const newTag = await tagService.createTag(newTagRequest)

    // 添加到可用标签列表
    availableTags.value.unshift(newTag)

    // 选中新创建的标签
    addTag(newTag)

    tagInput.value = ''
    showSuggestions.value = false
    activeSuggestionIndex.value = -1

  } catch (error) {
    console.error('Failed to create tag:', error)
  } finally {
    loading.value = false
  }
}

/**
 * 加载可用标签
 */
const loadTags = async () => {
  try {
    loading.value = true
    availableTags.value = await tagService.getTags()
  } catch (error) {
    console.error('Failed to load tags:', error)
  } finally {
    loading.value = false
  }
}

// 生命周期
onMounted(() => {
  loadTags()
})

// 监听外部标签变化
watch(() => props.modelValue, (newTags) => {
  // 可以在这里添加额外的处理逻辑
}, { deep: true })
</script>

<style scoped>
.tag-selector {
  margin-bottom: 16px;
}

.selector-label {
  display: block;
  font-size: 14px;
  font-weight: 500;
  color: #24292f;
  margin-bottom: 8px;
}

.tag-input-container {
  position: relative;
  border: 1px solid #d0d7de;
  border-radius: 6px;
  background: #fff;
  min-height: 40px;
  padding: 4px 8px;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 4px;
}

.tag-input-container:focus-within {
  border-color: #0969da;
  box-shadow: 0 0 0 2px rgba(9, 105, 218, 0.1);
}

.selected-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.tag-chip {
  display: inline-flex;
  align-items: center;
  padding: 2px 8px;
  background: #0969da;
  color: #fff;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.tag-remove {
  background: none;
  border: none;
  color: #fff;
  font-size: 16px;
  line-height: 1;
  margin-left: 4px;
  cursor: pointer;
  padding: 0;
  width: 16px;
  height: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: background-color 0.2s;
}

.tag-remove:hover {
  background: rgba(255, 255, 255, 0.2);
}

.tag-input-wrapper {
  position: relative;
  flex: 1;
  min-width: 120px;
}

.tag-input {
  width: 100%;
  border: none;
  outline: none;
  padding: 4px 0;
  font-size: 14px;
  background: transparent;
}

.tag-suggestions {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: #fff;
  border: 1px solid #d0d7de;
  border-radius: 6px;
  box-shadow: 0 8px 24px rgba(140, 149, 159, 0.2);
  z-index: 1000;
  max-height: 200px;
  overflow-y: auto;
}

.suggestion-item {
  display: flex;
  align-items: center;
  padding: 8px 12px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.2s;
}

.suggestion-item:hover,
.suggestion-item.active {
  background: #f6f8fa;
}

.suggestion-item.create-new {
  border-top: 1px solid #e1e5e9;
  color: #0969da;
  font-weight: 500;
}

.suggestion-color {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  margin-right: 8px;
  flex-shrink: 0;
}

.suggestion-icon {
  width: 12px;
  height: 12px;
  margin-right: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  flex-shrink: 0;
}

.suggestion-name {
  flex: 1;
}

.popular-tags {
  margin-top: 8px;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 6px;
}

.popular-label {
  font-size: 12px;
  color: #656d76;
  margin-right: 4px;
}

.popular-tag {
  padding: 2px 8px;
  background: #0969da;
  color: #fff;
  border: none;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.popular-tag:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.popular-tag:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 深色主题样式 */
.tag-selector.dark .selector-label {
  color: #f0f6fc;
}

.tag-selector.dark .tag-input-container {
  background: #21262d;
  border-color: #30363d;
}

.tag-selector.dark .tag-input-container:focus-within {
  border-color: #58a6ff;
}

.tag-selector.dark .tag-input {
  color: #f0f6fc;
}

.tag-selector.dark .tag-suggestions {
  background: #21262d;
  border-color: #30363d;
}

.tag-selector.dark .suggestion-item:hover,
.tag-selector.dark .suggestion-item.active {
  background: #30363d;
}

.tag-selector.dark .suggestion-item.create-new {
  border-top-color: #30363d;
  color: #58a6ff;
}

.tag-selector.dark .popular-label {
  color: #8b949e;
}
</style>
