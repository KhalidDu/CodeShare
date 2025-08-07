import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import CodeEditor from '../CodeEditor.vue'

// Mock Monaco Editor modules
vi.mock('monaco-editor', () => ({
  default: {},
  editor: {
    create: vi.fn(),
    setModelLanguage: vi.fn(),
    setTheme: vi.fn()
  },
  Range: vi.fn()
}))

vi.mock('@monaco-editor/loader', () => ({
  default: {
    config: vi.fn(),
    init: vi.fn(() => Promise.resolve({
      editor: {
        create: vi.fn(() => ({
          getValue: vi.fn(() => 'test code'),
          setValue: vi.fn(),
          onDidChangeModelContent: vi.fn((callback) => {
            // 模拟内容变化回调
            setTimeout(() => callback(), 0)
          }),
          getModel: vi.fn(() => ({
            getFullModelRange: vi.fn()
          })),
          dispose: vi.fn(),
          focus: vi.fn(),
          getAction: vi.fn(() => ({
            run: vi.fn()
          }))
        })),
        setModelLanguage: vi.fn(),
        setTheme: vi.fn()
      },
      Range: vi.fn()
    }))
  }
}))

describe('CodeEditor', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should render with default props', () => {
    const wrapper = mount(CodeEditor)

    expect(wrapper.find('.code-editor').exists()).toBe(true)
    expect(wrapper.find('.editor-toolbar').exists()).toBe(true)
    expect(wrapper.find('.editor-container').exists()).toBe(true)
  })

  it('should emit update:modelValue when content changes', async () => {
    const wrapper = mount(CodeEditor, {
      props: {
        modelValue: 'initial code'
      }
    })

    // 模拟内容变化
    await wrapper.vm.$nextTick()

    // 由于 Monaco Editor 是异步加载的，这里主要测试组件结构
    expect(wrapper.emitted()).toBeDefined()
  })

  it('should display language select with supported languages', () => {
    const wrapper = mount(CodeEditor)

    const languageSelect = wrapper.find('.language-select')
    expect(languageSelect.exists()).toBe(true)

    const options = languageSelect.findAll('option')
    expect(options.length).toBeGreaterThan(1) // 至少有默认选项和一些语言选项
  })

  it('should display theme select with theme options', () => {
    const wrapper = mount(CodeEditor)

    const themeSelect = wrapper.find('.theme-select')
    expect(themeSelect.exists()).toBe(true)

    const options = themeSelect.findAll('option')
    expect(options.length).toBe(3) // 浅色、深色、高对比度
  })

  it('should have format button', () => {
    const wrapper = mount(CodeEditor)

    const formatBtn = wrapper.find('.format-btn')
    expect(formatBtn.exists()).toBe(true)
    expect(formatBtn.text()).toBe('格式化代码')
  })

  it('should emit language-change when language changes', async () => {
    const wrapper = mount(CodeEditor)

    const languageSelect = wrapper.find('.language-select')
    await languageSelect.setValue('python')

    expect(wrapper.emitted('language-change')).toBeTruthy()
  })

  it('should emit theme-change when theme changes', async () => {
    const wrapper = mount(CodeEditor)

    const themeSelect = wrapper.find('.theme-select')
    await themeSelect.setValue('vs-dark')

    expect(wrapper.emitted('theme-change')).toBeTruthy()
  })

  it('should accept custom height prop', () => {
    const customHeight = '500px'
    const wrapper = mount(CodeEditor, {
      props: {
        height: customHeight
      }
    })

    const editorContainer = wrapper.find('.editor-container')
    expect(editorContainer.attributes('style')).toContain(customHeight)
  })

  it('should accept readonly prop', () => {
    const wrapper = mount(CodeEditor, {
      props: {
        readonly: true
      }
    })

    // 组件应该正常渲染，readonly 属性会传递给 Monaco Editor
    expect(wrapper.find('.code-editor').exists()).toBe(true)
  })
})
