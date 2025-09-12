import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createWebHistory, Router } from 'vue-router'
import SystemSettingsView from '@/views/admin/SystemSettingsView.vue'

// Mock the settings store
vi.mock('@/stores/settings', () => ({
  useSettingsStore: vi.fn(() => ({
    settings: null,
    loading: false,
    error: null,
    activeTab: 'site',
    fetchSettings: vi.fn(),
    updateSiteSettings: vi.fn(),
    updateSecuritySettings: vi.fn(),
    updateFeatureSettings: vi.fn(),
    updateEmailSettings: vi.fn(),
    setActiveTab: vi.fn()
  }))
}))

// Mock the auth store
vi.mock('@/stores/auth', () => ({
  useAuthStore: vi.fn(() => ({
    user: { role: 'admin' },
    isAuthenticated: true
  }))
}))

describe('SystemSettingsView', () => {
  let wrapper: any
  let router: Router

  beforeEach(() => {
    setActivePinia(createPinia())
    
    router = createRouter({
      history: createWebHistory(),
      routes: [
        { path: '/admin/settings', component: SystemSettingsView }
      ]
    })
  })

  afterEach(() => {
    if (wrapper) {
      wrapper.unmount()
    }
  })

  describe('基本渲染测试', () => {
    it('应该能够挂载组件', () => {
      wrapper = mount(SystemSettingsView, {
        global: {
          plugins: [router],
          stubs: {
            'el-tabs': true,
            'el-tab-pane': true,
            'site-settings': true,
            'security-settings': true,
            'feature-settings': true,
            'email-settings': true,
            'settings-history': true,
            'import-export-settings': true
          }
        }
      })

      expect(wrapper.exists()).toBe(true)
    })

    it('应该显示系统设置标题', () => {
      wrapper = mount(SystemSettingsView, {
        global: {
          plugins: [router],
          stubs: {
            'el-tabs': true,
            'el-tab-pane': true,
            'site-settings': true,
            'security-settings': true,
            'feature-settings': true,
            'email-settings': true,
            'settings-history': true,
            'import-export-settings': true
          }
        }
      })

      expect(wrapper.find('h1').text()).toContain('系统设置')
    })

    it('应该显示所有设置选项卡', () => {
      wrapper = mount(SystemSettingsView, {
        global: {
          plugins: [router],
          stubs: {
            'el-tabs': true,
            'el-tab-pane': true,
            'site-settings': true,
            'security-settings': true,
            'feature-settings': true,
            'email-settings': true,
            'settings-history': true,
            'import-export-settings': true
          }
        }
      })

      const tabs = wrapper.findAllComponents({ name: 'el-tab-pane' })
      expect(tabs.length).toBe(6)
    })
  })

  describe('数据加载测试', () => {
    it('组件挂载时应该调用获取设置数据', async () => {
      const { useSettingsStore } = await import('@/stores/settings')
      const mockStore = useSettingsStore()
      
      wrapper = mount(SystemSettingsView, {
        global: {
          plugins: [router],
          stubs: {
            'el-tabs': true,
            'el-tab-pane': true,
            'site-settings': true,
            'security-settings': true,
            'feature-settings': true,
            'email-settings': true,
            'settings-history': true,
            'import-export-settings': true
          }
        }
      })

      expect(mockStore.fetchSettings).toHaveBeenCalled()
    })

    it('应该显示加载状态', async () => {
      const { useSettingsStore } = await import('@/stores/settings')
      const mockStore = useSettingsStore()
      mockStore.loading = true

      wrapper = mount(SystemSettingsView, {
        global: {
          plugins: [router],
          stubs: {
            'el-tabs': true,
            'el-tab-pane': true,
            'site-settings': true,
            'security-settings': true,
            'feature-settings': true,
            'email-settings': true,
            'settings-history': true,
            'import-export-settings': true,
            'el-loading': true
          }
        }
      })

      expect(wrapper.find('[data-test="loading"]').exists()).toBe(true)
    })

    it('应该显示错误状态', async () => {
      const { useSettingsStore } = await import('@/stores/settings')
      const mockStore = useSettingsStore()
      mockStore.error = '加载设置失败'

      wrapper = mount(SystemSettingsView, {
        global: {
          plugins: [router],
          stubs: {
            'el-tabs': true,
            'el-tab-pane': true,
            'site-settings': true,
            'security-settings': true,
            'feature-settings': true,
            'email-settings': true,
            'settings-history': true,
            'import-export-settings': true,
            'el-alert': true
          }
        }
      })

      expect(wrapper.find('[data-test="error"]').exists()).toBe(true)
      expect(wrapper.find('[data-test="error"]').text()).toContain('加载设置失败')
    })
  })

  describe('选项卡切换测试', () => {
    it('应该能够切换到不同的设置选项卡', async () => {
      const { useSettingsStore } = await import('@/stores/settings')
      const mockStore = useSettingsStore()
      
      wrapper = mount(SystemSettingsView, {
        global: {
          plugins: [router],
          stubs: {
            'el-tabs': {
              template: '<div class="el-tabs"><slot /></div>',
              emits: ['tab-change']
            },
            'el-tab-pane': {
              template: '<div class="el-tab-pane"><slot /></div>'
            },
            'site-settings': true,
            'security-settings': true,
            'feature-settings': true,
            'email-settings': true,
            'settings-history': true,
            'import-export-settings': true
          }
        }
      })

      await wrapper.vm.$nextTick()
      
      // 模拟选项卡切换
      await wrapper.findComponent({ name: 'el-tabs' }).vm.$emit('tab-change', 'security')
      
      expect(mockStore.setActiveTab).toHaveBeenCalledWith('security')
    })
  })

  describe('权限控制测试', () => {
    it('非管理员用户应该无法访问', async () => {
      const { useAuthStore } = await import('@/stores/auth')
      const mockAuthStore = useAuthStore()
      mockAuthStore.user = { role: 'user' }

      wrapper = mount(SystemSettingsView, {
        global: {
          plugins: [router],
          stubs: {
            'el-tabs': true,
            'el-tab-pane': true,
            'site-settings': true,
            'security-settings': true,
            'feature-settings': true,
            'email-settings': true,
            'settings-history': true,
            'import-export-settings': true
          }
        }
      })

      expect(wrapper.find('[data-test="no-permission"]').exists()).toBe(true)
      expect(wrapper.find('[data-test="no-permission"]').text()).toContain('没有权限')
    })
  })
})