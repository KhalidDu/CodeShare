import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { CodeSnippet, CreateSnippetDto, UpdateSnippetDto, PaginatedResult } from '@/types'
import { codeSnippetService } from '@/services/codeSnippetService'
import { useErrorHandler } from '@/composables/useErrorHandler'
import { useLoading } from '@/composables/useLoading'

/**
 * 代码片段状态管理
 * 管理代码片段的CRUD操作、搜索筛选和分页
 */
export const useSnippetStore = defineStore('snippets', () => {
  const { handleApiError } = useErrorHandler()
  const { startGlobalLoading, stopGlobalLoading } = useLoading()

  // 状态
  const snippets = ref<CodeSnippet[]>([])
  const currentSnippet = ref<CodeSnippet | null>(null)
  const totalCount = ref(0)
  const currentPage = ref(1)
  const pageSize = ref(20)
  const isLoading = ref(false)

  // 筛选条件
  const filters = ref({
    search: '',
    language: '',
    tag: '',
    createdBy: ''
  })

  // 计算属性
  const hasSnippets = computed(() => snippets.value.length > 0)
  const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))
  const hasNextPage = computed(() => currentPage.value < totalPages.value)
  const hasPrevPage = computed(() => currentPage.value > 1)

  /**
   * 获取代码片段列表
   */
  async function fetchSnippets(params?: {
    page?: number
    pageSize?: number
    search?: string
    language?: string
    tag?: string
    createdBy?: string
  }) {
    try {
      isLoading.value = true
      startGlobalLoading('获取代码片段列表...')

      const queryParams = {
        page: params?.page || currentPage.value,
        pageSize: params?.pageSize || pageSize.value,
        search: params?.search || filters.value.search,
        language: params?.language || filters.value.language,
        tag: params?.tag || filters.value.tag,
        createdBy: params?.createdBy || filters.value.createdBy
      }

      const result: PaginatedResult<CodeSnippet> = await codeSnippetService.getSnippets(queryParams)

      snippets.value = result.items
      totalCount.value = result.totalCount
      currentPage.value = result.page
      pageSize.value = result.pageSize

    } catch (error: unknown) {
      handleApiError(error as Error & { apiError?: boolean; response?: { data?: unknown; status?: number } }, { operation: '获取代码片段列表' })
    } finally {
      isLoading.value = false
      stopGlobalLoading()
    }
  }

  /**
   * 根据ID获取代码片段详情
   */
  async function fetchSnippetById(id: string) {
    try {
      startGlobalLoading('获取代码片段详情...')
      const snippet = await codeSnippetService.getSnippet(id)
      currentSnippet.value = snippet
      return snippet
    } catch (error: unknown) {
      handleApiError(error as Error & { apiError?: boolean; response?: { data?: unknown; status?: number } }, { operation: '获取代码片段详情' })
      throw error
    } finally {
      stopGlobalLoading()
    }
  }

  /**
   * 创建新的代码片段
   */
  async function createSnippet(data: CreateSnippetDto) {
    try {
      startGlobalLoading('创建代码片段...')
      const newSnippet = await codeSnippetService.createSnippet(data)

      // 如果当前在第一页，将新片段添加到列表开头
      if (currentPage.value === 1) {
        snippets.value.unshift(newSnippet)
        totalCount.value++
      }

      return newSnippet
    } catch (error: unknown) {
      handleApiError(error as Error & { apiError?: boolean; response?: { data?: unknown; status?: number } }, { operation: '创建代码片段' })
      throw error
    } finally {
      stopGlobalLoading()
    }
  }

  /**
   * 更新代码片段
   */
  async function updateSnippet(id: string, data: UpdateSnippetDto) {
    try {
      startGlobalLoading('更新代码片段...')
      const updatedSnippet = await codeSnippetService.updateSnippet(id, data)

      // 更新列表中的片段
      const index = snippets.value.findIndex(s => s.id === id)
      if (index !== -1) {
        snippets.value[index] = updatedSnippet
      }

      // 更新当前片段
      if (currentSnippet.value?.id === id) {
        currentSnippet.value = updatedSnippet
      }

      return updatedSnippet
    } catch (error: unknown) {
      handleApiError(error as Error & { apiError?: boolean; response?: { data?: unknown; status?: number } }, { operation: '更新代码片段' })
      throw error
    } finally {
      stopGlobalLoading()
    }
  }

  /**
   * 删除代码片段
   */
  async function deleteSnippet(id: string) {
    try {
      startGlobalLoading('删除代码片段...')
      await codeSnippetService.deleteSnippet(id)

      // 从列表中移除
      snippets.value = snippets.value.filter(s => s.id !== id)
      totalCount.value--

      // 清除当前片段
      if (currentSnippet.value?.id === id) {
        currentSnippet.value = null
      }

    } catch (error: unknown) {
      handleApiError(error as Error & { apiError?: boolean; response?: { data?: unknown; status?: number } }, { operation: '删除代码片段' })
      throw error
    } finally {
      stopGlobalLoading()
    }
  }

  /**
   * 设置筛选条件
   */
  function setFilters(newFilters: Partial<typeof filters.value>) {
    filters.value = { ...filters.value, ...newFilters }
    currentPage.value = 1 // 重置到第一页
  }

  /**
   * 清除筛选条件
   */
  function clearFilters() {
    filters.value = {
      search: '',
      language: '',
      tag: '',
      createdBy: ''
    }
    currentPage.value = 1
  }

  /**
   * 设置当前页
   */
  function setCurrentPage(page: number) {
    currentPage.value = page
  }

  /**
   * 设置页面大小
   */
  function setPageSize(size: number) {
    pageSize.value = size
    currentPage.value = 1 // 重置到第一页
  }

  /**
   * 清除当前片段
   */
  function clearCurrentSnippet() {
    currentSnippet.value = null
  }

  /**
   * 重置状态
   */
  function resetState() {
    snippets.value = []
    currentSnippet.value = null
    totalCount.value = 0
    currentPage.value = 1
    pageSize.value = 20
    isLoading.value = false
    clearFilters()
  }

  return {
    // 状态
    snippets,
    currentSnippet,
    totalCount,
    currentPage,
    pageSize,
    isLoading,
    filters,

    // 计算属性
    hasSnippets,
    totalPages,
    hasNextPage,
    hasPrevPage,

    // 方法
    fetchSnippets,
    fetchSnippetById,
    createSnippet,
    updateSnippet,
    deleteSnippet,
    setFilters,
    clearFilters,
    setCurrentPage,
    setPageSize,
    clearCurrentSnippet,
    resetState
  }
})
