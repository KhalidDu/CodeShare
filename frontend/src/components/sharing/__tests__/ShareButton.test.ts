import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { nextTick } from 'vue'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import ShareButton from '../ShareButton.vue'

// Mock the share service
vi.mock('@/services/shareService', () => ({
  shareService: {
    createShareToken: vi.fn(),
    validateShareRequest: vi.fn(),
    canCreateShareToken: vi.fn(),
    getShareLink: vi.fn()
  }
}))

// Mock window.isSecureContext
Object.assign(window, {
  isSecureContext: true
})

// Mock the clipboard API
Object.assign(navigator, {
  clipboard: {
    writeText: vi.fn()
  }
})

// Mock document.execCommand for clipboard functionality
Object.assign(document, {
  execCommand: vi.fn().mockReturnValue(true)
})

describe('ShareButton', () => {
  let wrapper: any

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  afterEach(() => {
    if (wrapper) {
      wrapper.unmount()
    }
  })

  describe('基本渲染测试', () => {
    it('应该能够挂载组件', () => {
      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id'
        }
      })

      expect(wrapper.exists()).toBe(true)
      expect(wrapper.find('.share-button').exists()).toBe(true)
    })

    it('应该显示默认文本', () => {
      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id'
        }
      })

      expect(wrapper.find('.share-button__text').text()).toBe('分享')
    })

    it('应该显示自定义文本', () => {
      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id',
          customText: '分享代码'
        }
      })

      expect(wrapper.find('.share-button__text').text()).toBe('分享代码')
    })

    it('应该隐藏文本当showText为false', () => {
      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id',
          showText: false
        }
      })

      expect(wrapper.find('.share-button__text').exists()).toBe(false)
    })
  })

  describe('状态管理测试', () => {
    it('应该显示加载状态', async () => {
      const { shareService } = await import('@/services/shareService')
      
      // Mock 验证和配额检查
      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })
      
      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })
      
      // Mock 一个长时间运行的异步操作
      shareService.createShareToken.mockImplementation(() => {
        return new Promise(resolve => {
          setTimeout(() => {
            resolve({
              id: 'test-token-id',
              token: 'test-token',
              snippetId: 'test-snippet-id',
              permission: 'VIEW',
              status: 'ACTIVE',
              createdAt: new Date().toISOString(),
              createdBy: 'user1',
              currentAccessCount: 0,
              maxAccessCount: null,
              expiresAt: null,
              password: null,
              allowDownload: true
            })
          }, 1000)
        })
      })

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id'
        }
      })

      // 模拟异步操作开始
      const sharePromise = wrapper.vm.handleShare()
      
      // 等待下一个 tick 以确保状态更新
      await nextTick()
      
      expect(wrapper.vm.sharing).toBe(true)
      expect(wrapper.find('.share-button--sharing').exists()).toBe(true)
      expect(wrapper.find('.share-button__text').text()).toBe('分享中...')
      
      // 取消操作
      sharePromise.catch(() => {})
    })

    it('应该显示成功状态', async () => {
      const { shareService } = await import('@/services/shareService')
      
      // Mock API调用
      shareService.createShareToken.mockResolvedValue({
        id: 'test-token-id',
        token: 'test-token',
        snippetId: 'test-snippet-id',
        permission: 'VIEW',
        status: 'ACTIVE',
        createdAt: new Date().toISOString(),
        createdBy: 'user1',
        currentAccessCount: 0,
        maxAccessCount: null,
        expiresAt: null,
        password: null,
        allowDownload: true
      })

      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id'
        }
      })

      await wrapper.vm.handleShare()
      
      expect(wrapper.find('.share-button--success').exists()).toBe(true)
      expect(wrapper.find('.share-button__text').text()).toBe('已分享')
    })

    it('应该显示错误状态', async () => {
      const { shareService } = await import('@/services/shareService')
      
      // Mock API调用失败
      shareService.createShareToken.mockRejectedValue(new Error('分享失败'))
      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id'
        }
      })

      await wrapper.vm.handleShare()
      
      expect(wrapper.find('.share-button--error').exists()).toBe(true)
      expect(wrapper.find('.share-button__text').text()).toBe('分享失败')
    })
  })

  describe('交互测试', () => {
    it('应该响应点击事件', async () => {
      const { shareService } = await import('@/services/shareService')
      
      shareService.createShareToken.mockResolvedValue({
        id: 'test-token-id',
        token: 'test-token',
        snippetId: 'test-snippet-id',
        permission: 'VIEW',
        status: 'ACTIVE',
        createdAt: new Date().toISOString(),
        createdBy: 'user1',
        currentAccessCount: 0,
        maxAccessCount: null,
        expiresAt: null,
        password: null,
        allowDownload: true
      })

      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id'
        }
      })

      await wrapper.find('.share-button').trigger('click')
      
      expect(shareService.createShareToken).toHaveBeenCalled()
    })

    it('应该禁用按钮当disabled为true', () => {
      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id',
          disabled: true
        }
      })

      expect(wrapper.find('.share-button').attributes('disabled')).toBeDefined()
    })

    it('应该禁用按钮当正在分享', async () => {
      const { shareService } = await import('@/services/shareService')
      
      // Mock 验证和配额检查
      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })
      
      // Mock一个长时间运行的异步操作
      shareService.createShareToken.mockImplementation(() => {
        return new Promise(resolve => {
          setTimeout(() => {
            resolve({
              id: 'test-token-id',
              token: 'test-token',
              snippetId: 'test-snippet-id',
              permission: 'VIEW',
              status: 'ACTIVE',
              createdAt: new Date().toISOString(),
              createdBy: 'user1',
              currentAccessCount: 0,
              maxAccessCount: null,
              expiresAt: null,
              password: null,
              allowDownload: true
            })
          }, 1000)
        })
      })

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id'
        }
      })

      // 开始分享操作
      const sharePromise = wrapper.vm.handleShare()
      
      // 等待下一个 tick 以确保状态更新
      await nextTick()
      
      // 按钮应该被禁用
      expect(wrapper.find('.share-button').attributes('disabled')).toBe('')
      
      // 等待操作完成
      await sharePromise
    })
  })

  describe('验证测试', () => {
    it('应该验证分享请求参数', async () => {
      const { shareService } = await import('@/services/shareService')
      
      shareService.validateShareRequest.mockReturnValue({
        isValid: false,
        errors: ['代码片段ID不能为空']
      })

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id'
        }
      })

      await wrapper.vm.handleShare()
      
      expect(shareService.createShareToken).not.toHaveBeenCalled()
    })

    it('应该检查用户是否可以创建分享令牌', async () => {
      const { shareService } = await import('@/services/shareService')
      
      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: false,
        reason: '分享配额已用完'
      })

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id'
        }
      })

      await wrapper.vm.handleShare()
      
      expect(shareService.createShareToken).not.toHaveBeenCalled()
    })
  })

  describe('事件测试', () => {
    it('应该触发share-success事件', async () => {
      const { shareService } = await import('@/services/shareService')
      
      const mockShareToken = {
        id: 'test-token-id',
        token: 'test-token',
        snippetId: 'test-snippet-id',
        permission: 'VIEW',
        status: 'ACTIVE',
        createdAt: new Date().toISOString(),
        createdBy: 'user1',
        currentAccessCount: 0,
        maxAccessCount: null,
        expiresAt: null,
        password: null,
        allowDownload: true
      }

      shareService.createShareToken.mockResolvedValue(mockShareToken)
      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id'
        }
      })

      await wrapper.find('.share-button').trigger('click')
      
      expect(wrapper.emitted('share-success')).toBeTruthy()
      expect(wrapper.emitted('share-success')[0][0]).toEqual(mockShareToken)
    })

    it('应该触发share-error事件', async () => {
      const { shareService } = await import('@/services/shareService')
      
      const mockError = new Error('分享失败')
      shareService.createShareToken.mockRejectedValue(mockError)
      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id'
        }
      })

      await wrapper.find('.share-button').trigger('click')
      
      expect(wrapper.emitted('share-error')).toBeTruthy()
      expect(wrapper.emitted('share-error')[0][0]).toBe(mockError)
    })

    it('应该触发link-copied事件', async () => {
      const { shareService } = await import('@/services/shareService')
      
      const mockShareToken = {
        id: 'test-token-id',
        token: 'test-token',
        snippetId: 'test-snippet-id',
        permission: 'VIEW',
        status: 'ACTIVE',
        createdAt: new Date().toISOString(),
        createdBy: 'user1',
        currentAccessCount: 0,
        maxAccessCount: null,
        expiresAt: null,
        password: null,
        allowDownload: true
      }

      shareService.createShareToken.mockResolvedValue(mockShareToken)
      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })

      shareService.getShareLink.mockReturnValue('https://example.com/share/test-token')

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id',
          autoCopyLink: true
        }
      })

      await wrapper.find('.share-button').trigger('click')
      
      expect(wrapper.emitted('link-copied')).toBeTruthy()
      expect(wrapper.emitted('link-copied')[0][0]).toEqual(mockShareToken)
    })
  })

  describe('剪贴板测试', () => {
    it('应该复制分享链接到剪贴板', async () => {
      const { shareService } = await import('@/services/shareService')
      
      const mockShareToken = {
        id: 'test-token-id',
        token: 'test-token',
        snippetId: 'test-snippet-id',
        permission: 'VIEW',
        status: 'ACTIVE',
        createdAt: new Date().toISOString(),
        createdBy: 'user1',
        currentAccessCount: 0,
        maxAccessCount: null,
        expiresAt: null,
        password: null,
        allowDownload: true
      }

      shareService.createShareToken.mockResolvedValue(mockShareToken)
      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })

      shareService.getShareLink.mockReturnValue('https://example.com/share/test-token')

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id',
          autoCopyLink: true
        }
      })

      await wrapper.find('.share-button').trigger('click')
      
      expect(navigator.clipboard.writeText).toHaveBeenCalledWith('https://example.com/share/test-token')
    })

    it('应该处理剪贴板复制失败', async () => {
      const { shareService } = await import('@/services/shareService')
      
      const mockShareToken = {
        id: 'test-token-id',
        token: 'test-token',
        snippetId: 'test-snippet-id',
        permission: 'VIEW',
        status: 'ACTIVE',
        createdAt: new Date().toISOString(),
        createdBy: 'user1',
        currentAccessCount: 0,
        maxAccessCount: null,
        expiresAt: null,
        password: null,
        allowDownload: true
      }

      shareService.createShareToken.mockResolvedValue(mockShareToken)
      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })

      shareService.getShareLink.mockReturnValue('https://example.com/share/test-token')

      // Mock clipboard API 失败
      navigator.clipboard.writeText.mockRejectedValue(new Error('复制失败'))

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id',
          autoCopyLink: true
        }
      })

      await wrapper.find('.share-button').trigger('click')
      
      // 即使复制失败，分享操作应该仍然成功
      expect(wrapper.emitted('share-success')).toBeTruthy()
      expect(wrapper.emitted('link-copied')).toBeFalsy()
    })
  })

  describe('边界情况测试', () => {
    it('应该处理空的snippetId', async () => {
      wrapper = mount(ShareButton, {
        props: {
          snippetId: ''
        }
      })

      await wrapper.vm.handleShare()
      
      // 不应该调用API
      const { shareService } = await import('@/services/shareService')
      expect(shareService.createShareToken).not.toHaveBeenCalled()
    })

    it('应该处理snippetId变化', async () => {
      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id'
        }
      })

      // 设置成功状态
      wrapper.vm.showSuccess = true
      
      // 改变snippetId
      await wrapper.setProps({ snippetId: 'new-snippet-id' })
      
      // 状态应该被重置
      expect(wrapper.vm.showSuccess).toBe(false)
      expect(wrapper.vm.showError).toBe(false)
    })

    it('应该处理自动隐藏状态', async () => {
      const { shareService } = await import('@/services/shareService')
      
      shareService.createShareToken.mockResolvedValue({
        id: 'test-token-id',
        token: 'test-token',
        snippetId: 'test-snippet-id',
        permission: 'VIEW',
        status: 'ACTIVE',
        createdAt: new Date().toISOString(),
        createdBy: 'user1',
        currentAccessCount: 0,
        maxAccessCount: null,
        expiresAt: null,
        password: null,
        allowDownload: true
      })

      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id',
          successDuration: 100 // 短时间用于测试
        }
      })

      await wrapper.find('.share-button').trigger('click')
      
      // 立即显示成功状态
      expect(wrapper.vm.showSuccess).toBe(true)
      
      // 等待状态自动隐藏
      await new Promise(resolve => setTimeout(resolve, 150))
      
      expect(wrapper.vm.showSuccess).toBe(false)
    })
  })

  describe('权限控制测试', () => {
    it('应该使用默认权限', async () => {
      const { shareService } = await import('@/services/shareService')
      
      shareService.createShareToken.mockImplementation((request) => {
        expect(request.permission).toBe('VIEW')
        return Promise.resolve({
          id: 'test-token-id',
          token: 'test-token',
          snippetId: 'test-snippet-id',
          permission: 'VIEW',
          status: 'ACTIVE',
          createdAt: new Date().toISOString(),
          createdBy: 'user1',
          currentAccessCount: 0,
          maxAccessCount: null,
          expiresAt: null,
          password: null,
          allowDownload: true
        })
      })

      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id'
        }
      })

      await wrapper.find('.share-button').trigger('click')
    })

    it('应该使用指定的权限', async () => {
      const { shareService } = await import('@/services/shareService')
      
      shareService.createShareToken.mockImplementation((request) => {
        expect(request.permission).toBe('EDIT')
        return Promise.resolve({
          id: 'test-token-id',
          token: 'test-token',
          snippetId: 'test-snippet-id',
          permission: 'EDIT',
          status: 'ACTIVE',
          createdAt: new Date().toISOString(),
          createdBy: 'user1',
          currentAccessCount: 0,
          maxAccessCount: null,
          expiresAt: null,
          password: null,
          allowDownload: true
        })
      })

      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })

      wrapper = mount(ShareButton, {
        props: {
          snippetId: 'test-snippet-id',
          permission: 'EDIT'
        }
      })

      await wrapper.find('.share-button').trigger('click')
    })
  })
})