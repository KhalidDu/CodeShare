import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import ImportExportSettings from '@/components/admin/settings/ImportExportSettings.vue'

// Mock the settings store
vi.mock('@/stores/settings', () => ({
  useSettingsStore: vi.fn(() => ({
    settings: {
      siteSettings: { siteName: '测试站点' },
      securitySettings: { minPasswordLength: 8 },
      featureSettings: { enableCodeSnippets: true },
      emailSettings: { smtpHost: 'smtp.test.com' }
    },
    loading: false,
    error: null,
    exportSettings: vi.fn(),
    importSettings: vi.fn(),
    validateImportData: vi.fn()
  }))
})

// Mock the auth store
vi.mock('@/stores/auth', () => ({
  useAuthStore: vi.fn(() => ({
    user: { role: 'admin' },
    isAuthenticated: true
  }))
})

describe('ImportExportSettings', () => {
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
      wrapper = mount(ImportExportSettings, {
        global: {
          stubs: {
            'el-card': true,
            'el-button': true,
            'el-upload': true,
            'el-select': true,
            'el-option': true,
            'el-radio-group': true,
            'el-radio': true,
            'el-checkbox': true,
            'el-form': true,
            'el-form-item': true,
            'el-alert': true,
            'el-progress': true,
            'el-icon': true
          }
        }
      })

      expect(wrapper.exists()).toBe(true)
    })

    it('应该显示导入导出标题', () => {
      wrapper = mount(ImportExportSettings, {
        global: {
          stubs: {
            'el-card': true,
            'el-button': true,
            'el-upload': true,
            'el-select': true,
            'el-option': true,
            'el-radio-group': true,
            'el-radio': true,
            'el-checkbox': true,
            'el-form': true,
            'el-form-item': true,
            'el-alert': true,
            'el-progress': true,
            'el-icon': true
          }
        }
      })

      expect(wrapper.find('h3').text()).toContain('导入导出设置')
    })

    it('应该显示导出和导入两个部分', () => {
      wrapper = mount(ImportExportSettings, {
        global: {
          stubs: {
            'el-card': true,
            'el-button': true,
            'el-upload': true,
            'el-select': true,
            'el-option': true,
            'el-radio-group': true,
            'el-radio': true,
            'el-checkbox': true,
            'el-form': true,
            'el-form-item': true,
            'el-alert': true,
            'el-progress': true,
            'el-icon': true
          }
        }
      })

      expect(wrapper.find('[data-test="export-section"]').exists()).toBe(true)
      expect(wrapper.find('[data-test="import-section"]').exists()).toBe(true)
    })
  })

  describe('导出功能测试', () => {
    it('应该显示导出格式选项', () => {
      wrapper = mount(ImportExportSettings, {
        global: {
          stubs: {
            'el-card': true,
            'el-button': true,
            'el-upload': true,
            'el-select': {
              template: '<select :value="modelValue" @change="$emit(\'update:modelValue\', $event.target.value)"><slot /></select>',
              props: ['modelValue']
            },
            'el-option': true,
            'el-radio-group': true,
            'el-radio': true,
            'el-checkbox': true,
            'el-form': true,
            'el-form-item': true,
            'el-alert': true,
            'el-progress': true,
            'el-icon': true
          }
        }
      })

      const formatSelect = wrapper.find('[data-test="export-format"] select')
      expect(formatSelect.exists()).toBe(true)
      expect(formatSelect.element.value).toBe('json')
    })

    it('应该能够切换导出格式', async () => {
      wrapper = mount(ImportExportSettings, {
        global: {
          stubs: {
            'el-card': true,
            'el-button': true,
            'el-upload': true,
            'el-select': {
              template: '<select :value="modelValue" @change="$emit(\'update:modelValue\', $event.target.value)"><slot /></select>',
              props: ['modelValue']
            },
            'el-option': true,
            'el-radio-group': true,
            'el-radio': true,
            'el-checkbox': true,
            'el-form': true,
            'el-form-item': true,
            'el-alert': true,
            'el-progress': true,
            'el-icon': true
          }
        }
      })

      const formatSelect = wrapper.find('[data-test="export-format"] select')
      await formatSelect.setValue('csv')
      
      expect(wrapper.vm.exportForm.format).toBe('csv')
    })

    it('应该能够选择导出的设置类型', async () => {
      wrapper = mount(ImportExportSettings, {
        global: {
          stubs: {
            'el-card': true,
            'el-button': true,
            'el-upload': true,
            'el-select': true,
            'el-option': true,
            'el-radio-group': true,
            'el-radio': true,
            'el-checkbox': {
              template: '<input type="checkbox" :checked="modelValue" @change="$emit(\'update:modelValue\', $event.target.checked)" />',
              props: ['modelValue']
            },
            'el-form': true,
            'el-form-item': true,
            'el-alert': true,
            'el-progress': true,
            'el-icon': true
          }
        }
      })

      const siteCheckbox = wrapper.find('[data-test="export-site"] input')
      await siteCheckbox.setChecked(false)
      
      expect(wrapper.vm.exportForm.includeSiteSettings).toBe(false)
    })

    it('应该能够执行导出操作', async () => {
      const { useSettingsStore } = await import('@/stores/settings')
      const mockStore = useSettingsStore()
      mockStore.exportSettings.mockResolvedValue({
        success: true,
        data: new Blob(['{"siteName":"测试站点"}'], { type: 'application/json' }),
        fileName: 'settings-export.json'
      })
      
      wrapper = mount(ImportExportSettings, {
        global: {
          stubs: {
            'el-card': true,
            'el-button': {
              template: '<button @click="$emit(\'click\')"><slot /></button>',
              emits: ['click']
            },
            'el-upload': true,
            'el-select': true,
            'el-option': true,
            'el-radio-group': true,
            'el-radio': true,
            'el-checkbox': true,
            'el-form': true,
            'el-form-item': true,
            'el-alert': true,
            'el-progress': true,
            'el-icon': true
          }
        }
      })

      // Mock URL.createObjectURL and URL.revokeObjectURL
      global.URL.createObjectURL = vi.fn(() => 'blob:test-url')
      global.URL.revokeObjectURL = vi.fn()
      
      // Mock document.createElement
      const mockLink = {
        href: '',
        download: '',
        click: vi.fn()
      }
      document.createElement = vi.fn(() => mockLink)
      
      await wrapper.find('[data-test="export-btn"]').trigger('click')
      
      expect(mockStore.exportSettings).toHaveBeenCalled()
      expect(mockLink.click).toHaveBeenCalled()
    })
  })

  describe('导入功能测试', () => {
    it('应该显示文件上传组件', () => {
      wrapper = mount(ImportExportSettings, {
        global: {
          stubs: {
            'el-card': true,
            'el-button': true,
            'el-upload': {
              template: '<div class="el-upload"><slot /></div>',
              emits: ['change']
            },
            'el-select': true,
            'el-option': true,
            'el-radio-group': true,
            'el-radio': true,
            'el-checkbox': true,
            'el-form': true,
            'el-form-item': true,
            'el-alert': true,
            'el-progress': true,
            'el-icon': true
          }
        }
      })

      expect(wrapper.find('[data-test="file-upload"]').exists()).toBe(true)
    })

    it('应该处理文件选择', async () => {
      wrapper = mount(ImportExportSettings, {
        global: {
          stubs: {
            'el-card': true,
            'el-button': true,
            'el-upload': {
              template: '<div class="el-upload"><slot /></div>',
              emits: ['change']
            },
            'el-select': true,
            'el-option': true,
            'el-radio-group': true,
            'el-radio': true,
            'el-checkbox': true,
            'el-form': true,
            'el-form-item': true,
            'el-alert': true,
            'el-progress': true,
            'el-icon': true
          }
        }
      })

      const mockFile = new File(['{"siteName":"导入站点"}'], 'settings.json', { type: 'application/json' })
      const upload = wrapper.findComponent({ name: 'el-upload' })
      await upload.vm.$emit('change', { raw: mockFile })
      
      expect(wrapper.vm.importForm.file).toBe(mockFile)
    })

    it('应该能够选择导入模式', async () => {
      wrapper = mount(ImportExportSettings, {
        global: {
          stubs: {
            'el-card': true,
            'el-button': true,
            'el-upload': true,
            'el-select': true,
            'el-option': true,
            'el-radio-group': {
              template: '<div @click="$emit(\'update:modelValue\', $event.target.value)"><slot /></div>',
              emits: ['update:modelValue']
            },
            'el-radio': {
              template: '<label><input type="radio" :value="value" @change="$emit(\'change\', $event.target.value)" /> <slot /></label>',
              props: ['value', 'modelValue'],
              emits: ['change']
            },
            'el-checkbox': true,
            'el-form': true,
            'el-form-item': true,
            'el-alert': true,
            'el-progress': true,
            'el-icon': true
          }
        }
      })

      const mergeRadio = wrapper.find('[data-test="import-merge"] input')
      await mergeRadio.setValue('merge')
      
      expect(wrapper.vm.importForm.mode).toBe('merge')
    })

    it('应该验证导入数据', async () => {
      const { useSettingsStore } = await import('@/stores/settings')
      const mockStore = useSettingsStore()
      mockStore.validateImportData.mockResolvedValue({
        valid: true,
        errors: [],
        warnings: []
      })
      
      wrapper = mount(ImportExportSettings, {
        global: {
          stubs: {
            'el-card': true,
            'el-button': {
              template: '<button @click="$emit(\'click\')"><slot /></button>',
              emits: ['click']
            },
            'el-upload': true,
            'el-select': true,
            'el-option': true,
            'el-radio-group': true,
            'el-radio': true,
            'el-checkbox': true,
            'el-form': true,
            'el-form-item': true,
            'el-alert': true,
            'el-progress': true,
            'el-icon': true
          }
        }
      })

      // 设置文件
      const mockFile = new File(['{"siteName":"导入站点"}'], 'settings.json', { type: 'application/json' })
      wrapper.vm.importForm.file = mockFile
      
      await wrapper.find('[data-test="validate-btn"]').trigger('click')
      
      expect(mockStore.validateImportData).toHaveBeenCalledWith({
        jsonData: '{"siteName":"导入站点"}',
        format: 'json'
      })
    })

    it('应该能够执行导入操作', async () => {
      const { useSettingsStore } = await import('@/stores/settings')
      const mockStore = useSettingsStore()
      mockStore.importSettings.mockResolvedValue({
        success: true,
        message: '导入成功'
      })
      
      wrapper = mount(ImportExportSettings, {
        global: {
          stubs: {
            'el-card': true,
            'el-button': {
              template: '<button @click="$emit(\'click\')"><slot /></button>',
              emits: ['click']
            },
            'el-upload': true,
            'el-select': true,
            'el-option': true,
            'el-radio-group': true,
            'el-radio': true,
            'el-checkbox': true,
            'el-form': true,
            'el-form-item': true,
            'el-alert': true,
            'el-progress': true,
            'el-icon': true
          }
        }
      })

      // 设置文件和验证状态
      const mockFile = new File(['{"siteName":"导入站点"}'], 'settings.json', { type: 'application/json' })
      wrapper.vm.importForm.file = mockFile
      wrapper.vm.validationResult = { valid: true, errors: [], warnings: [] }
      
      await wrapper.find('[data-test="import-btn"]').trigger('click')
      
      expect(mockStore.importSettings).toHaveBeenCalledWith({
        jsonData: '{"siteName":"导入站点"}',
        format: 'json',
        mode: 'merge',
        overwriteExisting: false
      })
    })
  })

  describe('错误处理测试', () => {
    it('应该显示导出错误', async () => {
      const { useSettingsStore } = await import('@/stores/settings')
      const mockStore = useSettingsStore()
      mockStore.exportSettings.mockRejectedValue(new Error('导出失败'))
      
      wrapper = mount(ImportExportSettings, {
        global: {
          stubs: {
            'el-card': true,
            'el-button': {
              template: '<button @click="$emit(\'click\')"><slot /></button>',
              emits: ['click']
            },
            'el-upload': true,
            'el-select': true,
            'el-option': true,
            'el-radio-group': true,
            'el-radio': true,
            'el-checkbox': true,
            'el-form': true,
            'el-form-item': true,
            'el-alert': {
              template: '<div class="el-alert" :title="title" :type="type"><slot /></div>',
              props: ['title', 'type']
            },
            'el-progress': true,
            'el-icon': true
          }
        }
      })

      await wrapper.find('[data-test="export-btn"]').trigger('click')
      await wrapper.vm.$nextTick()
      
      expect(wrapper.vm.exportError).toBe('导出失败')
      expect(wrapper.find('[data-test="export-error"]').exists()).toBe(true)
    })

    it('应该显示导入验证错误', async () => {
      const { useSettingsStore } = await import('@/stores/settings')
      const mockStore = useSettingsStore()
      mockStore.validateImportData.mockResolvedValue({
        valid: false,
        errors: ['站点名称不能为空'],
        warnings: ['某些设置将被忽略']
      })
      
      wrapper = mount(ImportExportSettings, {
        global: {
          stubs: {
            'el-card': true,
            'el-button': {
              template: '<button @click="$emit(\'click\')"><slot /></button>',
              emits: ['click']
            },
            'el-upload': true,
            'el-select': true,
            'el-option': true,
            'el-radio-group': true,
            'el-radio': true,
            'el-checkbox': true,
            'el-form': true,
            'el-form-item': true,
            'el-alert': {
              template: '<div class="el-alert" :title="title" :type="type"><slot /></div>',
              props: ['title', 'type']
            },
            'el-progress': true,
            'el-icon': true
          }
        }
      })

      // 设置文件
      const mockFile = new File(['{"siteName":""}'], 'settings.json', { type: 'application/json' })
      wrapper.vm.importForm.file = mockFile
      
      await wrapper.find('[data-test="validate-btn"]').trigger('click')
      
      expect(wrapper.vm.validationResult.valid).toBe(false)
      expect(wrapper.vm.validationResult.errors).toContain('站点名称不能为空')
    })
  })
})