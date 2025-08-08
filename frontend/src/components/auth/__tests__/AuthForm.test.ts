import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import AuthForm from '../AuthForm.vue'

// Mock the auth store
vi.mock('@/stores/auth', () => ({
  useAuthStore: vi.fn(() => ({
    login: vi.fn(),
    register: vi.fn(),
    loading: false,
    error: null
  }))
}))

describe('AuthForm', () => {
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
      wrapper = mount(AuthForm, {
        props: {
          mode: 'login',
          title: '登录',
          submitText: '登录',
          loadingText: '登录中...'
        }
      })

      expect(wrapper.exists()).toBe(true)
    })

    it('应该显示正确的标题', () => {
      wrapper = mount(AuthForm, {
        props: {
          mode: 'login',
          title: '用户登录',
          submitText: '登录',
          loadingText: '登录中...'
        }
      })

      expect(wrapper.text()).toContain('用户登录')
    })
  })

  describe('属性测试', () => {
    it('应该接受所有必需的props', () => {
      const props = {
        mode: 'register',
        title: '注册账户',
        submitText: '注册',
        loadingText: '注册中...',
        isLoading: false,
        error: undefined
      }

      wrapper = mount(AuthForm, { props })

      // 检查组件是否正确挂载
      expect(wrapper.exists()).toBe(true)
      expect(wrapper.text()).toContain('注册账户')
    })
  })
})
