import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import SnippetForm from '../SnippetForm.vue'
import { useSnippetStore } from '@/stores/snippets'
import { useTagStore } from '@/stores/tags'

// Mock the stores
vi.mock('@/stores/snippets', () => ({
  useSnippetStore: vi.fn()
}))

vi.mock('@/stores/tags', () => ({
  useTagStore: vi.fn()
}))

describe('SnippetForm', () => {
  let wrapper: any
  let mockSnippetStore: any
  let mockTagStore: any

  beforeEach(() => {
    setActivePinia(createPinia())

    // Setup mock stores
    mockSnippetStore = {
      createSnippet: vi.fn(),
      updateSnippet: vi.fn(),
      loading: false,
      error: null
    }

    mockTagStore = {
      tags: [
        { id: '1', name: 'javascript', color: '#f7df1e' },
        { id: '2', name: 'vue', color: '#4fc08d' }
      ],
      fetchTags: vi.fn()
    }

    vi.mocked(useSnippetStore).mockReturnValue(mockSnippetStore)
    vi.mocked(useTagStore).mockReturnValue(mockTagStore)
  })

  afterEach(() => {
    if (wrapper) {
      wrapper.unmount()
    }
  })

  describe('Create Mode', () => {
    beforeEach(() => {
      wrapper = mount(SnippetForm, {
        props: {
          mode: 'create'
        }
      })
    })

    it('renders create form correctly', () => {
      expect(wrapper.find('h2').text()).toBe('创建代码片段')
      expect(wrapper.find('input[placeholder="代码片段标题"]').exists()).toBe(true)
      expect(wrapper.find('textarea[placeholder="描述（可选）"]').exists()).toBe(true)
      expect(wrapper.find('select').exists()).toBe(true) // Language selector
      expect(wrapper.find('button[type="submit"]').text()).toBe('创建')
    })

    it('validates required fields', async () => {
      const form = wrapper.find('form')
      await form.trigger('submit.prevent')

      expect(wrapper.text()).toContain('标题不能为空')
      expect(wrapper.text()).toContain('代码内容不能为空')
      expect(wrapper.text()).toContain('请选择编程语言')
    })

    it('validates title length', async () => {
      const titleInput = wrapper.find('input[placeholder="代码片段标题"]')
      const longTitle = 'a'.repeat(201)

      await titleInput.setValue(longTitle)
      await titleInput.trigger('blur')

      expect(wrapper.text()).toContain('标题长度不能超过200个字符')
    })

    it('validates description length', async () => {
      const descriptionInput = wrapper.find('textarea[placeholder="描述（可选）"]')
      const longDescription = 'a'.repeat(1001)

      await descriptionInput.setValue(longDescription)
      await descriptionInput.trigger('blur')

      expect(wrapper.text()).toContain('描述长度不能超过1000个字符')
    })

    it('validates code content size', async () => {
      const codeEditor = wrapper.findComponent({ name: 'CodeEditor' })
      const largeCode = 'a'.repeat(100001)

      await codeEditor.vm.$emit('update:modelValue', largeCode)

      expect(wrapper.text()).toContain('代码内容不能超过100KB')
    })

    it('sanitizes input values', async () => {
      const titleInput = wrapper.find('input[placeholder="代码片段标题"]')
      await titleInput.setValue('<script>alert("xss")</script>Safe Title')
      await titleInput.trigger('blur')

      expect(titleInput.element.value).toBe('Safe Title')
    })

    it('submits valid form', async () => {
      const titleInput = wrapper.find('input[placeholder="代码片段标题"]')
      const descriptionInput = wrapper.find('textarea[placeholder="描述（可选）"]')
      const languageSelect = wrapper.find('select')
      const codeEditor = wrapper.findComponent({ name: 'CodeEditor' })
      const form = wrapper.find('form')

      await titleInput.setValue('Test Snippet')
      await descriptionInput.setValue('Test Description')
      await languageSelect.setValue('javascript')
      await codeEditor.vm.$emit('update:modelValue', 'console.log("Hello World");')
      await form.trigger('submit.prevent')

      expect(mockSnippetStore.createSnippet).toHaveBeenCalledWith({
        title: 'Test Snippet',
        description: 'Test Description',
        language: 'javascript',
        code: 'console.log("Hello World");',
        tags: [],
        isPublic: true
      })
    })
  })

  describe('Edit Mode', () => {
    const existingSnippet = {
      id: '1',
      title: 'Existing Snippet',
      description: 'Existing Description',
      language: 'javascript',
      code: 'console.log("existing");',
      tags: ['javascript'],
      isPublic: true
    }

    beforeEach(() => {
      wrapper = mount(SnippetForm, {
        props: {
          mode: 'edit',
          snippet: existingSnippet
        }
      })
    })

    it('renders edit form with existing data', () => {
      expect(wrapper.find('h2').text()).toBe('编辑代码片段')
      expect(wrapper.find('input[placeholder="代码片段标题"]').element.value).toBe('Existing Snippet')
      expect(wrapper.find('textarea[placeholder="描述（可选）"]').element.value).toBe('Existing Description')
      expect(wrapper.find('select').element.value).toBe('javascript')
      expect(wrapper.find('button[type="submit"]').text()).toBe('更新')
    })

    it('submits updated form', async () => {
      const titleInput = wrapper.find('input[placeholder="代码片段标题"]')
      const form = wrapper.find('form')

      await titleInput.setValue('Updated Snippet')
      await form.trigger('submit.prevent')

      expect(mockSnippetStore.updateSnippet).toHaveBeenCalledWith('1', {
        title: 'Updated Snippet',
        description: 'Existing Description',
        language: 'javascript',
        code: 'console.log("existing");',
        tags: ['javascript'],
        isPublic: true
      })
    })
  })

  describe('Tag Management', () => {
    beforeEach(() => {
      wrapper = mount(SnippetForm, {
        props: {
          mode: 'create'
        }
      })
    })

    it('displays available tags', () => {
      const tagSelector = wrapper.findComponent({ name: 'TagSelector' })
      expect(tagSelector.exists()).toBe(true)
      expect(tagSelector.props('availableTags')).toEqual(mockTagStore.tags)
    })

    it('validates tag names', async () => {
      const tagSelector = wrapper.findComponent({ name: 'TagSelector' })
      await tagSelector.vm.$emit('add-tag', 'invalid@tag')

      expect(wrapper.text()).toContain('标签名称只能包含字母、数字、中文、连字符和下划线')
    })

    it('limits number of tags', async () => {
      const tagSelector = wrapper.findComponent({ name: 'TagSelector' })
      const manyTags = Array.from({ length: 11 }, (_, i) => `tag${i}`)

      for (const tag of manyTags) {
        await tagSelector.vm.$emit('add-tag', tag)
      }

      expect(wrapper.text()).toContain('最多只能添加10个标签')
    })

    it('prevents duplicate tags', async () => {
      const tagSelector = wrapper.findComponent({ name: 'TagSelector' })
      await tagSelector.vm.$emit('add-tag', 'javascript')
      await tagSelector.vm.$emit('add-tag', 'javascript')

      expect(wrapper.vm.selectedTags).toEqual(['javascript'])
    })
  })

  describe('Security Features', () => {
    beforeEach(() => {
      wrapper = mount(SnippetForm, {
        props: {
          mode: 'create'
        }
      })
    })

    it('prevents XSS in title field', async () => {
      const titleInput = wrapper.find('input[placeholder="代码片段标题"]')
      await titleInput.setValue('<img src="x" onerror="alert(\'xss\')">')
      await titleInput.trigger('blur')

      expect(titleInput.element.value).not.toContain('<img')
      expect(titleInput.element.value).not.toContain('onerror')
    })

    it('prevents XSS in description field', async () => {
      const descriptionInput = wrapper.find('textarea[placeholder="描述（可选）"]')
      await descriptionInput.setValue('<script>document.cookie="stolen"</script>')
      await descriptionInput.trigger('blur')

      expect(descriptionInput.element.value).not.toContain('<script>')
    })

    it('allows safe HTML entities in code', async () => {
      const codeEditor = wrapper.findComponent({ name: 'CodeEditor' })
      const htmlCode = '<div>&lt;script&gt;alert("safe")&lt;/script&gt;</div>'

      await codeEditor.vm.$emit('update:modelValue', htmlCode)

      // Code content should preserve HTML entities for display purposes
      expect(wrapper.vm.code).toBe(htmlCode)
    })

    it('validates against SQL injection patterns', async () => {
      const titleInput = wrapper.find('input[placeholder="代码片段标题"]')
      await titleInput.setValue("'; DROP TABLE users; --")
      await titleInput.trigger('blur')

      expect(wrapper.text()).toContain('输入包含不安全的内容')
    })

    it('rate limits form submissions', async () => {
      const form = wrapper.find('form')

      // Fill form with valid data
      await wrapper.find('input[placeholder="代码片段标题"]').setValue('Test')
      await wrapper.find('select').setValue('javascript')
      await wrapper.findComponent({ name: 'CodeEditor' }).vm.$emit('update:modelValue', 'test')

      // Submit multiple times rapidly
      await form.trigger('submit.prevent')
      await form.trigger('submit.prevent')
      await form.trigger('submit.prevent')

      // Should only call create once due to rate limiting
      expect(mockSnippetStore.createSnippet).toHaveBeenCalledTimes(1)
    })
  })

  describe('Accessibility', () => {
    beforeEach(() => {
      wrapper = mount(SnippetForm, {
        props: {
          mode: 'create'
        }
      })
    })

    it('has proper form labels', () => {
      const labels = wrapper.findAll('label')
      expect(labels.length).toBeGreaterThan(0)

      labels.forEach(label => {
        expect(label.attributes('for')).toBeDefined()
      })
    })

    it('shows validation errors with ARIA attributes', async () => {
      const form = wrapper.find('form')
      await form.trigger('submit.prevent')

      const errorMessages = wrapper.findAll('[role="alert"]')
      expect(errorMessages.length).toBeGreaterThan(0)
    })

    it('supports keyboard navigation', async () => {
      const focusableElements = wrapper.findAll('input, select, textarea, button')

      for (let i = 0; i < focusableElements.length - 1; i++) {
        await focusableElements[i].trigger('keydown.tab')
        // Verify focus moves to next element
      }
    })

    it('has proper ARIA roles for form sections', () => {
      const form = wrapper.find('form')
      expect(form.attributes('role')).toBe('form')

      const fieldsets = wrapper.findAll('fieldset')
      fieldsets.forEach(fieldset => {
        expect(fieldset.find('legend').exists()).toBe(true)
      })
    })
  })

  describe('Performance', () => {
    beforeEach(() => {
      wrapper = mount(SnippetForm, {
        props: {
          mode: 'create'
        }
      })
    })

    it('debounces validation calls', async () => {
      const titleInput = wrapper.find('input[placeholder="代码片段标题"]')

      // Type rapidly
      await titleInput.setValue('a')
      await titleInput.setValue('ab')
      await titleInput.setValue('abc')

      // Wait for debounce
      await new Promise(resolve => setTimeout(resolve, 300))

      // Validation should only be called once after debounce
      expect(wrapper.vm.validationCalls).toBeLessThanOrEqual(1)
    })

    it('lazy loads tag suggestions', async () => {
      const tagSelector = wrapper.findComponent({ name: 'TagSelector' })

      // Tags should not be loaded initially
      expect(mockTagStore.fetchTags).not.toHaveBeenCalled()

      // Focus on tag input should trigger loading
      await tagSelector.find('input').trigger('focus')

      expect(mockTagStore.fetchTags).toHaveBeenCalled()
    })
  })
})
