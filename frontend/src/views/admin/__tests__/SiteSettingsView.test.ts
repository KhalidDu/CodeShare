import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import SiteSettingsView from '@/views/admin/SiteSettingsView.vue'

// Mock the settings store
vi.mock('@/stores/settings', () => ({
  useSettingsStore: vi.fn(() => ({
    settings: {
      siteSettings: {
        siteName: '测试站点',
        siteDescription: '这是一个测试站点',
        siteKeywords: '测试,代码,片段',
        logoUrl: '',
        faviconUrl: '',
        footerText: '© 2024 测试站点',
        defaultLanguage: 'zh-CN',
        theme: 'light',
        maxUploadSize: 10,
        maxSnippetLength: 50000,
        enableComments: true,
        enableRatings: true,
        enableSharing: true,
        enableClipboard: true,
        enableSearch: true,
        customCss: '',
        customJs: '',
        seoTitle: '',
        seoDescription: '',
        seoKeywords: ''
      }
    },
    loading: false,
    error: null,
    updateSiteSettings: vi.fn()
  }))
})

// Mock the auth store
vi.mock('@/stores/auth', () => ({
  useAuthStore: vi.fn(() => ({
    user: { role: 'admin' },
    isAuthenticated: true
  }))
}))

describe('SiteSettingsView', () => {
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
      wrapper = mount(SiteSettingsView, {
        global: {
          stubs: {
            'el-form': true,
            'el-form-item': true,
            'el-input': true,
            'el-input-number': true,
            'el-switch': true,
            'el-select': true,
            'el-option': true,
            'el-button': true,
            'el-upload': true,
            'el-card': true,
            'el-row': true,
            'el-col': true,
            'el-tabs': true,
            'el-tab-pane': true,
            'el-textarea': true
          }
        }
      })

      expect(wrapper.exists()).toBe(true)
    })

    it('应该显示站点设置标题', () => {
      wrapper = mount(SiteSettingsView, {
        global: {
          stubs: {
            'el-form': true,
            'el-form-item': true,
            'el-input': true,
            'el-input-number': true,
            'el-switch': true,
            'el-select': true,
            'el-option': true,
            'el-button': true,
            'el-upload': true,
            'el-card': true,
            'el-row': true,
            'el-col': true,
            'el-tabs': true,
            'el-tab-pane': true,
            'el-textarea': true
          }
        }
      })

      expect(wrapper.find('h1').text()).toContain('站点设置')
    })

    it('应该显示基本信息表单', () => {
      wrapper = mount(SiteSettingsView, {
        global: {
          stubs: {
            'el-form': true,
            'el-form-item': true,
            'el-input': true,
            'el-input-number': true,
            'el-switch': true,
            'el-select': true,
            'el-option': true,
            'el-button': true,
            'el-upload': true,
            'el-card': true,
            'el-row': true,
            'el-col': true,
            'el-tabs': true,
            'el-tab-pane': true,
            'el-textarea': true
          }
        }
      })

      expect(wrapper.find('[data-test="basic-info-form"]').exists()).toBe(true)
    })
  })

  describe('表单数据绑定测试', () => {
    it('应该正确显示站点名称', () => {
      wrapper = mount(SiteSettingsView, {
        global: {
          stubs: {
            'el-form': true,
            'el-form-item': true,
            'el-input': {
              template: '<input :value="modelValue" @input="$emit(\'update:modelValue\', $event.target.value)" />',
              props: ['modelValue']
            },
            'el-input-number': true,
            'el-switch': true,
            'el-select': true,
            'el-option': true,
            'el-button': true,
            'el-upload': true,
            'el-card': true,
            'el-row': true,
            'el-col': true,
            'el-tabs': true,
            'el-tab-pane': true,
            'el-textarea': true
          }
        }
      })

      const siteNameInput = wrapper.find('[data-test="site-name"] input')
      expect(siteNameInput.exists()).toBe(true)
      expect(siteNameInput.element.value).toBe('测试站点')
    })

    it('应该正确显示站点描述', () => {
      wrapper = mount(SiteSettingsView, {
        global: {
          stubs: {
            'el-form': true,
            'el-form-item': true,
            'el-input': true,
            'el-input-number': true,
            'el-switch': true,
            'el-select': true,
            'el-option': true,
            'el-button': true,
            'el-upload': true,
            'el-card': true,
            'el-row': true,
            'el-col': true,
            'el-tabs': true,
            'el-tab-pane': true,
            'el-textarea': {
              template: '<textarea :value="modelValue" @input="$emit(\'update:modelValue\', $event.target.value)" />',
              props: ['modelValue']
            }
          }
        }
      })

      const descriptionInput = wrapper.find('[data-test="site-description"] textarea')
      expect(descriptionInput.exists()).toBe(true)
      expect(descriptionInput.element.value).toBe('这是一个测试站点')
    })
  })

  describe('表单提交测试', () => {
    it('应该能够提交表单', async () => {
      const { useSettingsStore } = await import('@/stores/settings')
      const mockStore = useSettingsStore()
      
      wrapper = mount(SiteSettingsView, {
        global: {
          stubs: {
            'el-form': {
              template: '<form @submit.prevent="$emit(\'submit\')"><slot /></form>',
              emits: ['submit']
            },
            'el-form-item': true,
            'el-input': true,
            'el-input-number': true,
            'el-switch': true,
            'el-select': true,
            'el-option': true,
            'el-button': {
              template: '<button @click="$emit(\'click\')"><slot /></button>',
              emits: ['click']
            },
            'el-upload': true,
            'el-card': true,
            'el-row': true,
            'el-col': true,
            'el-tabs': true,
            'el-tab-pane': true,
            'el-textarea': true
          }
        }
      })

      await wrapper.find('[data-test="submit-btn"]').trigger('click')
      
      expect(mockStore.updateSiteSettings).toHaveBeenCalled()
    })

    it('应该验证必填字段', async () => {
      wrapper = mount(SiteSettingsView, {
        global: {
          stubs: {
            'el-form': {
              template: '<form @submit.prevent="$emit(\'submit\')"><slot /></form>',
              emits: ['submit'],
              methods: {
                validate: vi.fn().mockResolvedValue(false)
              }
            },
            'el-form-item': true,
            'el-input': true,
            'el-input-number': true,
            'el-switch': true,
            'el-select': true,
            'el-option': true,
            'el-button': {
              template: '<button @click="$emit(\'click\')"><slot /></button>',
              emits: ['click']
            },
            'el-upload': true,
            'el-card': true,
            'el-row': true,
            'el-col': true,
            'el-tabs': true,
            'el-tab-pane': true,
            'el-textarea': true
          }
        }
      })

      const form = wrapper.findComponent({ name: 'el-form' })
      await form.vm.$emit('submit')
      
      expect(form.vm.validate).toHaveBeenCalled()
    })
  })

  describe('文件上传测试', () => {
    it('应该处理logo上传', async () => {
      wrapper = mount(SiteSettingsView, {
        global: {
          stubs: {
            'el-form': true,
            'el-form-item': true,
            'el-input': true,
            'el-input-number': true,
            'el-switch': true,
            'el-select': true,
            'el-option': true,
            'el-button': true,
            'el-upload': {
              template: '<div class="el-upload"><slot /></div>',
              emits: ['success']
            },
            'el-card': true,
            'el-row': true,
            'el-col': true,
            'el-tabs': true,
            'el-tab-pane': true,
            'el-textarea': true
          }
        }
      })

      const upload = wrapper.findComponent({ name: 'el-upload' })
      await upload.vm.$emit('success', { response: { url: '/uploads/logo.png' } })
      
      expect(wrapper.vm.form.logoUrl).toBe('/uploads/logo.png')
    })

    it('应该处理favicon上传', async () => {
      wrapper = mount(SiteSettingsView, {
        global: {
          stubs: {
            'el-form': true,
            'el-form-item': true,
            'el-input': true,
            'el-input-number': true,
            'el-switch': true,
            'el-select': true,
            'el-option': true,
            'el-button': true,
            'el-upload': {
              template: '<div class="el-upload"><slot /></div>',
              emits: ['success']
            },
            'el-card': true,
            'el-row': true,
            'el-col': true,
            'el-tabs': true,
            'el-tab-pane': true,
            'el-textarea': true
          }
        }
      })

      const upload = wrapper.findAllComponents({ name: 'el-upload' })[1]
      await upload.vm.$emit('success', { response: { url: '/uploads/favicon.ico' } })
      
      expect(wrapper.vm.form.faviconUrl).toBe('/uploads/favicon.ico')
    })
  })

  describe('主题切换测试', () => {
    it('应该能够切换主题', async () => {
      wrapper = mount(SiteSettingsView, {
        global: {
          stubs: {
            'el-form': true,
            'el-form-item': true,
            'el-input': true,
            'el-input-number': true,
            'el-switch': true,
            'el-select': {
              template: '<select :value="modelValue" @change="$emit(\'update:modelValue\', $event.target.value)"><slot /></select>',
              props: ['modelValue']
            },
            'el-option': true,
            'el-button': true,
            'el-upload': true,
            'el-card': true,
            'el-row': true,
            'el-col': true,
            'el-tabs': true,
            'el-tab-pane': true,
            'el-textarea': true
          }
        }
      })

      const themeSelect = wrapper.find('[data-test="theme"] select')
      await themeSelect.setValue('dark')
      
      expect(wrapper.vm.form.theme).toBe('dark')
    })
  })
})