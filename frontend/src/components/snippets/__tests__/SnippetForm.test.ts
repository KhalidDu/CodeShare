import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import SnippetForm from '../SnippetForm.vue'

// Mock the stores
vi.mock('@/stores/codeSnippets', () => ({
  useCodeSnippetsStore: vi.fn(() => ({
    createSnippet: vi.fn(),
    updateSnippet: vi.fn(),
    loading: false,
    error: null
  }))
}))

vi.mock('@/stores/tags', () => ({
  useTagStore: vi.fn(() => ({
    tags: [
      { id: '1', name: 'javascript', color: '#f7df1e', createdBy: 'user1', createdAt: '2024-01-01T00:00:00Z' },
      { id: '2', name: 'vue', color: '#4fc08d', createdBy: 'user1', createdAt: '2024-01-01T00:00:00Z' }
    ],
    fetchTags: vi.fn()
  }))
}))

describe('SnippetForm', () => {
  let wrapper: any

  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    if (wrapper) {
      wrapper.unmount()
    }
  })

  describe('基本渲染测试', () => {
    it('应该能够挂载组件', () => {
      wrapper = mount(SnippetForm, {
        props: {
          mode: 'create'
        }
      })

      expect(wrapper.exists()).toBe(true)
    })
  })

  describe('编辑模式测试', () => {
    const existingSnippet = {
      id: '1',
      title: 'Existing Snippet',
      description: 'Existing Description',
      language: 'javascript',
      code: 'console.log("existing");',
      tags: [{ id: '1', name: 'javascript', color: '#f7df1e', createdBy: 'user1', createdAt: '2024-01-01T00:00:00Z' }],
      isPublic: true,
      createdBy: 'user1',
      creatorName: 'Test User',
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
      viewCount: 0,
      copyCount: 0,
      shareCount: 0
    }

    it('应该能够在编辑模式下挂载', () => {
      wrapper = mount(SnippetForm, {
        props: {
          mode: 'edit',
          snippet: existingSnippet
        }
      })

      expect(wrapper.exists()).toBe(true)
    })
  })
})
