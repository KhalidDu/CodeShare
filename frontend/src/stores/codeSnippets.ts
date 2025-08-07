import { defineStore } from 'pinia'
import { ref } from 'vue'
import { codeSnippetService } from '@/services/codeSnippetService'
import type {
  CodeSnippet,
  CreateSnippetRequest,
  UpdateSnippetRequest,
  PaginatedResult,
  SnippetFilter
} from '@/types'

export const useCodeSnippetsStore = defineStore('codeSnippets', () => {
  // 状态
  const snippets = ref<CodeSnippet[]>([])
  const currentSnippet = ref<CodeSnippet | null>(null)
  const totalCount = ref(0)
  const currentPage = ref(1)
  const pageSize = ref(10)
  const isLoading = ref(false)
  const filter = ref<SnippetFilter>({
    page: 1,
    pageSize: 10
  })

  // 获取代码片段列表
  async function fetchSnippets(newFilter?: Partial<SnippetFilter>) {
    isLoading.value = true
    try {
      if (newFilter) {
        filter.value = { ...filter.value, ...newFilter }
      }

      const result: PaginatedResult<CodeSnippet> = await codeSnippetService.getSnippets(filter.value)
      snippets.value = result.items
      totalCount.value = result.totalCount
      currentPage.value = result.page
      pageSize.value = result.pageSize
    } finally {
      isLoading.value = false
    }
  }

  // 获取单个代码片段
  async function fetchSnippet(id: string) {
    isLoading.value = true
    try {
      currentSnippet.value = await codeSnippetService.getSnippet(id)
      return currentSnippet.value
    } finally {
      isLoading.value = false
    }
  }

  // 创建代码片段
  async function createSnippet(snippet: CreateSnippetRequest) {
    isLoading.value = true
    try {
      const newSnippet = await codeSnippetService.createSnippet(snippet)
      snippets.value.unshift(newSnippet)
      return newSnippet
    } finally {
      isLoading.value = false
    }
  }

  // 更新代码片段
  async function updateSnippet(id: string, snippet: UpdateSnippetRequest) {
    isLoading.value = true
    try {
      const updatedSnippet = await codeSnippetService.updateSnippet(id, snippet)
      const index = snippets.value.findIndex(s => s.id === id)
      if (index !== -1) {
        snippets.value[index] = updatedSnippet
      }
      if (currentSnippet.value?.id === id) {
        currentSnippet.value = updatedSnippet
      }
      return updatedSnippet
    } finally {
      isLoading.value = false
    }
  }

  // 删除代码片段
  async function deleteSnippet(id: string) {
    isLoading.value = true
    try {
      await codeSnippetService.deleteSnippet(id)
      snippets.value = snippets.value.filter(s => s.id !== id)
      if (currentSnippet.value?.id === id) {
        currentSnippet.value = null
      }
    } finally {
      isLoading.value = false
    }
  }

  // 复制代码片段
  async function copySnippet(id: string) {
    try {
      await codeSnippetService.copySnippet(id)
      // 更新复制计数
      const snippet = snippets.value.find(s => s.id === id)
      if (snippet) {
        snippet.copyCount++
      }
      if (currentSnippet.value?.id === id) {
        currentSnippet.value.copyCount++
      }
    } catch (error) {
      console.error('Failed to copy snippet:', error)
      throw error
    }
  }

  // 搜索代码片段
  async function searchSnippets(searchTerm: string) {
    await fetchSnippets({ search: searchTerm, page: 1 })
  }

  // 按语言筛选
  async function filterByLanguage(language: string) {
    await fetchSnippets({ language, page: 1 })
  }

  // 按标签筛选
  async function filterByTag(tag: string) {
    await fetchSnippets({ tag, page: 1 })
  }

  // 清除筛选
  async function clearFilters() {
    filter.value = { page: 1, pageSize: 10 }
    await fetchSnippets()
  }

  return {
    // 状态
    snippets,
    currentSnippet,
    totalCount,
    currentPage,
    pageSize,
    isLoading,
    filter,
    // 方法
    fetchSnippets,
    fetchSnippet,
    createSnippet,
    updateSnippet,
    deleteSnippet,
    copySnippet,
    searchSnippets,
    filterByLanguage,
    filterByTag,
    clearFilters
  }
})
