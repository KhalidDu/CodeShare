import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import AuthForm from '../AuthForm.vue'
import { useAuthStore } from '@/stores/auth'

// Mock the auth store
vi.mock('@/stores/auth', () => ({
  useAuthStore: vi.fn()
}))

describe('AuthForm', () => {
  let wrapper: any
  let mockAuthStore: any

  beforeEach(() => {
    setActivePinia(createPinia())

    // Setup mock auth store
    mockAuthStore = {
      login: vi.fn(),
      register: vi.fn(),
      loading: false,
      error: null
    }

    vi.mocked(useAuthStore).mockReturnValue(mockAuthStore)
  })

  afterEach(() => {
    if (wrapper) {
      wrapper.unmount()
    }
  })

  describe('Login Form', () => {
    beforeEach(() => {
      wrapper = mount(AuthForm, {
        props: {
          mode: 'login'
        }
      })
    })

    it('renders login form correctly', () => {
      expect(wrapper.find('h2').text()).toBe('登录')
      expect(wrapper.find('input[type="text"]').exists()).toBe(true)
      expect(wrapper.find('input[type="password"]').exists()).toBe(true)
      expect(wrapper.find('button[type="submit"]').text()).toBe('登录')
    })

    it('validates required fields', async () => {
      const form = wrapper.find('form')
      await form.trigger('submit.prevent')

      expect(wrapper.text()).toContain('用户名不能为空')
      expect(wrapper.text()).toContain('密码不能为空')
    })

    it('validates username format', async () => {
      const usernameInput = wrapper.find('input[type="text"]')
      await usernameInput.setValue('ab') // Too short
      await usernameInput.trigger('blur')

      expect(wrapper.text()).toContain('用户名长度必须在3-30个字符之间')
    })

    it('validates username characters', async () => {
      const usernameInput = wrapper.find('input[type="text"]')
      await usernameInput.setValue('user@name') // Invalid characters
      await usernameInput.trigger('blur')

      expect(wrapper.text()).toContain('用户名只能包含字母、数字、下划线和连字符')
    })

    it('submits valid login form', async () => {
      const usernameInput = wrapper.find('input[type="text"]')
      const passwordInput = wrapper.find('input[type="password"]')
      const form = wrapper.find('form')

      await usernameInput.setValue('testuser')
      await passwordInput.setValue('password123')
      await form.trigger('submit.prevent')

      expect(mockAuthStore.login).toHaveBeenCalledWith({
        username: 'testuser',
        password: 'password123'
      })
    })

    it('sanitizes input values', async () => {
      const usernameInput = wrapper.find('input[type="text"]')
      await usernameInput.setValue('  testuser  ')
      await usernameInput.trigger('blur')

      expect(usernameInput.element.value).toBe('testuser')
    })

    it('prevents XSS in input fields', async () => {
      const usernameInput = wrapper.find('input[type="text"]')
      await usernameInput.setValue('<script>alert("xss")</script>')
      await usernameInput.trigger('blur')

      expect(usernameInput.element.value).not.toContain('<script>')
    })
  })

  describe('Register Form', () => {
    beforeEach(() => {
      wrapper = mount(AuthForm, {
        props: {
          mode: 'register'
        }
      })
    })

    it('renders register form correctly', () => {
      expect(wrapper.find('h2').text()).toBe('注册')
      expect(wrapper.findAll('input[type="text"]')).toHaveLength(2) // username and email
      expect(wrapper.find('input[type="email"]').exists()).toBe(true)
      expect(wrapper.findAll('input[type="password"]')).toHaveLength(2) // password and confirm
      expect(wrapper.find('button[type="submit"]').text()).toBe('注册')
    })

    it('validates email format', async () => {
      const emailInput = wrapper.find('input[type="email"]')
      await emailInput.setValue('invalid-email')
      await emailInput.trigger('blur')

      expect(wrapper.text()).toContain('请输入有效的邮箱地址')
    })

    it('validates password strength', async () => {
      const passwordInput = wrapper.find('input[type="password"]')
      await passwordInput.setValue('weak')
      await passwordInput.trigger('blur')

      expect(wrapper.text()).toContain('密码必须至少8个字符')
    })

    it('validates password complexity', async () => {
      const passwordInput = wrapper.find('input[type="password"]')
      await passwordInput.setValue('password123')
      await passwordInput.trigger('blur')

      expect(wrapper.text()).toContain('密码必须包含大写字母、小写字母、数字和特殊字符')
    })

    it('validates password confirmation', async () => {
      const passwordInputs = wrapper.findAll('input[type="password"]')
      await passwordInputs[0].setValue('Password123!')
      await passwordInputs[1].setValue('Password456!')
      await passwordInputs[1].trigger('blur')

      expect(wrapper.text()).toContain('两次输入的密码不一致')
    })

    it('submits valid register form', async () => {
      const usernameInput = wrapper.find('input[type="text"]')
      const emailInput = wrapper.find('input[type="email"]')
      const passwordInputs = wrapper.findAll('input[type="password"]')
      const form = wrapper.find('form')

      await usernameInput.setValue('testuser')
      await emailInput.setValue('test@example.com')
      await passwordInputs[0].setValue('Password123!')
      await passwordInputs[1].setValue('Password123!')
      await form.trigger('submit.prevent')

      expect(mockAuthStore.register).toHaveBeenCalledWith({
        username: 'testuser',
        email: 'test@example.com',
        password: 'Password123!'
      })
    })
  })

  describe('Security Features', () => {
    beforeEach(() => {
      wrapper = mount(AuthForm, {
        props: {
          mode: 'login'
        }
      })
    })

    it('prevents form submission with malicious input', async () => {
      const usernameInput = wrapper.find('input[type="text"]')
      const passwordInput = wrapper.find('input[type="password"]')
      const form = wrapper.find('form')

      await usernameInput.setValue('<script>alert("xss")</script>')
      await passwordInput.setValue('javascript:alert("xss")')
      await form.trigger('submit.prevent')

      expect(mockAuthStore.login).not.toHaveBeenCalled()
      expect(wrapper.text()).toContain('输入包含不安全的内容')
    })

    it('limits input length to prevent buffer overflow', async () => {
      const usernameInput = wrapper.find('input[type="text"]')
      const longInput = 'a'.repeat(1000)

      await usernameInput.setValue(longInput)
      await usernameInput.trigger('blur')

      expect(wrapper.text()).toContain('用户名长度不能超过30个字符')
    })

    it('shows loading state during form submission', async () => {
      mockAuthStore.loading = true
      await wrapper.vm.$nextTick()

      const submitButton = wrapper.find('button[type="submit"]')
      expect(submitButton.attributes('disabled')).toBeDefined()
      expect(wrapper.text()).toContain('处理中...')
    })

    it('displays error messages from store', async () => {
      mockAuthStore.error = '用户名或密码错误'
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).toContain('用户名或密码错误')
    })

    it('clears sensitive data on unmount', async () => {
      const passwordInput = wrapper.find('input[type="password"]')
      await passwordInput.setValue('sensitive-password')

      wrapper.unmount()

      // Verify that password field is cleared
      expect(passwordInput.element.value).toBe('')
    })
  })

  describe('Accessibility', () => {
    beforeEach(() => {
      wrapper = mount(AuthForm, {
        props: {
          mode: 'login'
        }
      })
    })

    it('has proper ARIA labels', () => {
      const usernameInput = wrapper.find('input[type="text"]')
      const passwordInput = wrapper.find('input[type="password"]')

      expect(usernameInput.attributes('aria-label')).toBe('用户名')
      expect(passwordInput.attributes('aria-label')).toBe('密码')
    })

    it('associates labels with inputs', () => {
      const labels = wrapper.findAll('label')
      const inputs = wrapper.findAll('input')

      labels.forEach((label, index) => {
        const forAttr = label.attributes('for')
        const inputId = inputs[index].attributes('id')
        expect(forAttr).toBe(inputId)
      })
    })

    it('shows validation errors with proper ARIA attributes', async () => {
      const form = wrapper.find('form')
      await form.trigger('submit.prevent')

      const errorMessages = wrapper.findAll('[role="alert"]')
      expect(errorMessages.length).toBeGreaterThan(0)
    })

    it('supports keyboard navigation', async () => {
      const inputs = wrapper.findAll('input')

      // Test tab navigation
      for (let i = 0; i < inputs.length - 1; i++) {
        await inputs[i].trigger('keydown.tab')
        expect(document.activeElement).toBe(inputs[i + 1].element)
      }
    })
  })
})
