import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Tag, CreateTagDto, UpdateTagDto } from '@/types'
import { tagService } from '@/services/tagService'
import { useErrorHandler } from '@/composables/useErrorHandler'
import { useLoading } from '@/composables/useLoading'

/**
 * 标签状态管理
 * 管理标签的CRUD操作、搜索和缓存
 */
export const useTagStore = defineStore('tags', () => {
  const { handleApiError } = useErrorHandler()
  const { startGlobalLoading, stopGlobalLoading } = useLoading()

  // 状态
  const tags = ref<Tag[]>([])
  const popularTags = ref<Tag[]>([])
  const isLoading = ref(false)
  const lastFetchTime = ref<number>(0)

  // 缓存时间（5分钟）
  const CACHE_DURATION = 5 * 60 * 1000

  // 计算属性
  const hasTags = computed(() => tags.value.length > 0)
  const tagsByName = computed(() => {
    const map = new Map<string, Tag>()
    tags.value.forEach(tag => {
      map.set(tag.name.toLowerCase(), tag)
    })
    return map
  })

  /**
   * 检查缓存是否有效
   */
  const isCacheValid = computed(() => {
    return Date.now() - lastFetchTime.value < CACHE_DURATION
  })

  /**
   * 获取所有标签
   */
  async function fetchTags(forceRefresh = false) {
    // 如果缓存有效且不强制刷新，直接返回
    if (!forceRefresh && isCacheValid.value && hasTags.value) {
      return tags.value
    }

    try {
      isLoading.value = true
      startGlobalLoading('获取标签列表...')

      const result = await tagService.getTags()
      tags.value = result
      lastFetchTime.value = Date.now()

      return result
    } catch (error: unknown) {
      handleApiError(error as Error & { apiError?: boolean; response?: { data?: unknown; status?: number } }, { operation: '获取标签列表' })
      throw error
    } finally {
      isLoading.value = false
      stopGlobalLoading()
    }
  }

  /**
   * 获取热门标签
   */
  async function fetchPopularTags(limit = 10) {
    try {
      startGlobalLoading('获取热门标签...')
      // 暂时使用所有标签，后续可以添加专门的热门标签API
      const allTags = await tagService.getTags()
      // 简单按名称排序作为热门标签的临时实现
      popularTags.value = allTags.slice(0, limit)
      return popularTags.value
    } catch (error: unknown) {
      handleApiError(error as Error & { apiError?: boolean; response?: { data?: unknown; status?: number } }, { operation: '获取热门标签' })
      throw error
    } finally {
      stopGlobalLoading()
    }
  }

  /**
   * 搜索标签
   */
  async function searchTags(prefix: string, limit = 10) {
    try {
      if (!prefix.trim()) {
        return []
      }

      // 暂时使用本地搜索，后续可以添加服务端搜索API
      const allTags = await fetchTags()
      const filtered = allTags.filter(tag =>
        tag.name.toLowerCase().includes(prefix.toLowerCase())
      ).slice(0, limit)

      return filtered
    } catch (error: unknown) {
      handleApiError(error as Error & { apiError?: boolean; response?: { data?: unknown; status?: number } }, { operation: '搜索标签' })
      return []
    }
  }

  /**
   * 根据ID获取标签
   */
  async function fetchTagById(id: string) {
    try {
      const result = await tagService.getTag(id)

      // 更新缓存中的标签
      const index = tags.value.findIndex(t => t.id === id)
      if (index !== -1) {
        tags.value[index] = result
      } else {
        tags.value.push(result)
      }

      return result
    } catch (error: unknown) {
      handleApiError(error as Error & { apiError?: boolean; response?: { data?: unknown; status?: number } }, { operation: '获取标签详情' })
      throw error
    }
  }

  /**
   * 创建新标签
   */
  async function createTag(data: CreateTagDto) {
    try {
      startGlobalLoading('创建标签...')
      const newTag = await tagService.createTag(data)

      // 添加到缓存
      tags.value.push(newTag)

      return newTag
    } catch (error: unknown) {
      handleApiError(error as Error & { apiError?: boolean; response?: { data?: unknown; status?: number } }, { operation: '创建标签' })
      throw error
    } finally {
      stopGlobalLoading()
    }
  }

  /**
   * 更新标签
   */
  async function updateTag(id: string, data: UpdateTagDto) {
    try {
      startGlobalLoading('更新标签...')
      const updatedTag = await tagService.updateTag(id, data)

      // 更新缓存
      const index = tags.value.findIndex(t => t.id === id)
      if (index !== -1) {
        tags.value[index] = updatedTag
      }

      return updatedTag
    } catch (error: unknown) {
      handleApiError(error as Error & { apiError?: boolean; response?: { data?: unknown; status?: number } }, { operation: '更新标签' })
      throw error
    } finally {
      stopGlobalLoading()
    }
  }

  /**
   * 删除标签
   */
  async function deleteTag(id: string) {
    try {
      startGlobalLoading('删除标签...')
      await tagService.deleteTag(id)

      // 从缓存中移除
      tags.value = tags.value.filter(t => t.id !== id)
      popularTags.value = popularTags.value.filter(t => t.id !== id)

    } catch (error: unknown) {
      handleApiError(error as Error & { apiError?: boolean; response?: { data?: unknown; status?: number } }, { operation: '删除标签' })
      throw error
    } finally {
      stopGlobalLoading()
    }
  }

  /**
   * 根据名称查找标签
   */
  function findTagByName(name: string): Tag | undefined {
    return tagsByName.value.get(name.toLowerCase())
  }

  /**
   * 根据名称列表获取标签
   */
  function getTagsByNames(names: string[]): Tag[] {
    return names
      .map(name => findTagByName(name))
      .filter((tag): tag is Tag => tag !== undefined)
  }

  /**
   * 检查标签是否存在
   */
  function hasTag(name: string): boolean {
    return tagsByName.value.has(name.toLowerCase())
  }

  /**
   * 获取标签的使用统计
   */
  async function getTagStats(id: string) {
    try {
      // 暂时返回模拟数据，后续可以添加专门的统计API
      const tag = tags.value.find(t => t.id === id)
      if (!tag) {
        throw new Error('标签不存在')
      }

      return {
        tagId: id,
        tagName: tag.name,
        usageCount: 0,
        snippetCount: 0
      }
    } catch (error: unknown) {
      handleApiError(error as Error & { apiError?: boolean; response?: { data?: unknown; status?: number } }, { operation: '获取标签统计' })
      throw error
    }
  }

  /**
   * 清除缓存
   */
  function clearCache() {
    tags.value = []
    popularTags.value = []
    lastFetchTime.value = 0
  }

  /**
   * 重置状态
   */
  function resetState() {
    clearCache()
    isLoading.value = false
  }

  return {
    // 状态
    tags,
    popularTags,
    isLoading,

    // 计算属性
    hasTags,
    tagsByName,
    isCacheValid,

    // 方法
    fetchTags,
    fetchPopularTags,
    searchTags,
    fetchTagById,
    createTag,
    updateTag,
    deleteTag,
    findTagByName,
    getTagsByNames,
    hasTag,
    getTagStats,
    clearCache,
    resetState
  }
})
