import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { nextTick } from 'vue'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import ShareDialog from '../ShareDialog.vue'

// Mock the share service
vi.mock('@/services/shareService', () => ({
  shareService: {
    createShareToken: vi.fn(),
    validateShareRequest: vi.fn(),
    canCreateShareToken: vi.fn(),
    getShareLink: vi.fn()
  }
}))

// Mock the QRCode library
vi.mock('qrcode', () => ({
  default: {
    toCanvas: vi.fn((canvas, text, callback) => {
      if (typeof callback === 'function') {
        callback(null)
      }
    })
  }
}))

// Mock document.execCommand for clipboard functionality
Object.assign(document, {
  execCommand: vi.fn().mockReturnValue(true)
})

// Mock the clipboard API
Object.assign(navigator, {
  clipboard: {
    writeText: vi.fn()
  }
})

// Mock window.isSecureContext
Object.assign(window, {
  isSecureContext: true
})

// Mock window.open
vi.stubGlobal('open', vi.fn())

describe('ShareDialog', () => {
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
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      expect(wrapper.exists()).toBe(true)
      expect(wrapper.find('.share-dialog').exists()).toBe(true)
    })

    it('应该显示分享设置表单', () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      expect(wrapper.find('.share-form').exists()).toBe(true)
      expect(wrapper.find('.form-group').exists()).toBe(true)
    })

    it('应该显示默认权限选项', () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      const permissionSelect = wrapper.find('select[class="form-select"]')
      expect(permissionSelect.exists()).toBe(true)
      expect(permissionSelect.element.value).toBe('VIEW')
    })

    it('应该使用指定的默认权限', () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true,
          defaultPermission: 'EDIT'
        }
      })

      const permissionSelect = wrapper.find('select[class="form-select"]')
      expect(permissionSelect.element.value).toBe('EDIT')
    })
  })

  describe('表单交互测试', () => {
    it('应该能够选择分享权限', async () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      const permissionSelect = wrapper.find('select[class="form-select"]')
      await permissionSelect.setValue('EDIT')
      
      expect(wrapper.vm.form.permission).toBe('EDIT')
    })

    it('应该能够选择过期时间选项', async () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      // 找到第二个select（有效期选择）
      const expirySelect = wrapper.findAll('select[class="form-select"]')[1]
      await expirySelect.setValue('24h')
      
      expect(wrapper.vm.form.expiryOption).toBe('24h')
    })

    it('应该显示自定义过期时间输入', async () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      // 找到第二个select（有效期选择）
      const expirySelect = wrapper.findAll('select[class="form-select"]')[1]
      await expirySelect.setValue('custom')
      
      expect(wrapper.find('input[type="datetime-local"]').exists()).toBe(true)
    })

    it('应该能够启用密码保护', async () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      const passwordCheckbox = wrapper.find('.checkbox-label input[type="checkbox"]')
      await passwordCheckbox.setChecked(true)
      
      expect(wrapper.vm.form.hasPassword).toBe(true)
      expect(wrapper.find('.password-input-group').exists()).toBe(true)
    })

    it('应该能够启用访问限制', async () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      // 找到第二个checkbox（限制访问次数）
      const accessLimitCheckbox = wrapper.findAll('.checkbox-label input[type="checkbox"]')[1]
      await accessLimitCheckbox.setChecked(true)
      
      expect(wrapper.vm.form.enableAccessLimit).toBe(true)
      expect(wrapper.find('input[type="number"]').exists()).toBe(true)
    })

    it('应该能够切换密码显示', async () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      // 启用密码保护
      const passwordCheckbox = wrapper.find('.checkbox-label input[type="checkbox"]')
      await passwordCheckbox.setChecked(true)
      
      // 切换密码显示
      const toggleButton = wrapper.find('.password-toggle')
      await toggleButton.trigger('click')
      
      expect(wrapper.vm.showPassword).toBe(true)
      
      await toggleButton.trigger('click')
      
      expect(wrapper.vm.showPassword).toBe(false)
    })
  })

  describe('表单验证测试', () => {
    it('应该验证必填字段', async () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: '',
          visible: true
        }
      })

      await wrapper.vm.handleCreateShare()
      
      expect(wrapper.vm.error).toBe('代码片段ID不能为空')
    })

    it('应该验证自定义过期时间', async () => {
      const { shareService } = await import('@/services/shareService')
      
      // Mock canCreateShareToken to avoid undefined error
      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })

      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      // 选择自定义过期时间
      const expirySelect = wrapper.findAll('select[class="form-select"]')[1]
      await expirySelect.setValue('custom')
      
      // 不设置过期时间（expiresAt 应该为空）
      wrapper.vm.form.expiresAt = ''
      
      await wrapper.vm.handleCreateShare()
      
      expect(wrapper.vm.error).toBe('请设置过期时间')
    })

    it('应该验证密码设置', async () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      // 启用密码保护但不设置密码
      const passwordCheckbox = wrapper.find('.checkbox-label input[type="checkbox"]')
      await passwordCheckbox.setChecked(true)
      
      await wrapper.vm.handleCreateShare()
      
      expect(wrapper.vm.error).toBe('请设置访问密码')
    })

    it('应该验证访问次数设置', async () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      // 启用访问限制但设置无效次数
      const accessLimitCheckbox = wrapper.findAll('.checkbox-label input[type="checkbox"]')[1]
      await accessLimitCheckbox.setChecked(true)
      
      const accessCountInput = wrapper.find('input[type="number"]')
      await accessCountInput.setValue(-1)
      
      await wrapper.vm.handleCreateShare()
      
      expect(wrapper.vm.error).toBe('最大访问次数必须大于0')
    })
  })

  describe('分享创建测试', () => {
    it('应该能够创建分享令牌', async () => {
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

      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      await wrapper.vm.handleCreateShare()
      
      expect(shareService.createShareToken).toHaveBeenCalled()
      expect(wrapper.vm.shareToken).toEqual(mockShareToken)
      expect(wrapper.vm.isSubmitting).toBe(false)
    })

    it('应该构建正确的分享请求', async () => {
      const { shareService } = await import('@/services/shareService')
      
      shareService.createShareToken.mockImplementation((request) => {
        expect(request.snippetId).toBe('test-snippet-id')
        expect(request.permission).toBe('EDIT')
        expect(request.password).toBe('test-password')
        expect(request.maxAccessCount).toBe(10)
        expect(request.allowDownload).toBe(false)
        
        return Promise.resolve({
          id: 'test-token-id',
          token: 'test-token',
          snippetId: 'test-snippet-id',
          permission: 'EDIT',
          status: 'ACTIVE',
          createdAt: new Date().toISOString(),
          createdBy: 'user1',
          currentAccessCount: 0,
          maxAccessCount: 10,
          expiresAt: null,
          password: 'test-password',
          allowDownload: false
        })
      })

      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })

      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      // 设置表单值
      wrapper.vm.form.permission = 'EDIT'
      wrapper.vm.form.expiryOption = 'never'
      wrapper.vm.form.expiresAt = ''
      wrapper.vm.form.hasPassword = true
      wrapper.vm.form.password = 'test-password'
      wrapper.vm.form.enableAccessLimit = true
      wrapper.vm.form.maxAccessCount = 10
      wrapper.vm.form.allowDownload = false

      await wrapper.vm.handleCreateShare()
    })

    it('应该处理创建失败', async () => {
      const { shareService } = await import('@/services/shareService')
      
      shareService.createShareToken.mockRejectedValue(new Error('创建分享失败'))
      shareService.validateShareRequest.mockReturnValue({
        isValid: true,
        errors: []
      })

      shareService.canCreateShareToken.mockResolvedValue({
        canCreate: true
      })

      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      await wrapper.vm.handleCreateShare()
      
      expect(wrapper.vm.error).toBe('创建分享失败')
      expect(wrapper.vm.isSubmitting).toBe(false)
    })
  })

  describe('分享结果测试', () => {
    it('应该显示分享结果', async () => {
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

      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      await wrapper.vm.handleCreateShare()
      
      expect(wrapper.find('.share-result').exists()).toBe(true)
      expect(wrapper.find('.success-message').exists()).toBe(true)
      expect(wrapper.find('.share-link-input').exists()).toBe(true)
    })

    it('应该显示分享链接', async () => {
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

      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      await wrapper.vm.handleCreateShare()
      
      const linkInput = wrapper.find('.share-link-input input')
      expect(linkInput.element.value).toBe('https://example.com/share/test-token')
    })
  })

  describe('二维码测试', () => {
    it('应该生成二维码', async () => {
      const { shareService } = await import('@/services/shareService')
      const { default: QRCode } = await import('qrcode')
      
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

      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      await wrapper.vm.handleCreateShare()
      
      expect(QRCode.toCanvas).toHaveBeenCalledWith(
        expect.any(HTMLCanvasElement),
        'https://example.com/share/test-token',
        {
          width: 200,
          margin: 2,
          color: {
            dark: '#000000',
            light: '#FFFFFF'
          }
        }
      )
    })

    it('应该处理二维码生成失败', async () => {
      const { shareService } = await import('@/services/shareService')
      const { default: QRCode } = await import('qrcode')
      
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

      // Mock QRCode 生成失败
      QRCode.toCanvas.mockRejectedValue(new Error('生成二维码失败'))

      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      await wrapper.vm.handleCreateShare()
      
      // 即使二维码生成失败，分享结果应该仍然显示
      expect(wrapper.find('.share-result').exists()).toBe(true)
    })
  })

  describe('剪贴板测试', () => {
    it('应该复制分享链接', async () => {
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

      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      await wrapper.vm.handleCreateShare()
      
      // 等待DOM更新
      await nextTick()
      
      // 点击复制按钮
      const copyButton = wrapper.find('.share-link-input button')
      await copyButton.trigger('click')
      
      expect(navigator.clipboard.writeText).toHaveBeenCalledWith('https://example.com/share/test-token')
      expect(wrapper.vm.copied).toBe(true)
    })

    it('应该显示复制成功提示', async () => {
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

      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      await wrapper.vm.handleCreateShare()
      
      // 点击复制按钮
      await wrapper.find('.share-link-input button').trigger('click')
      
      expect(wrapper.find('.success-text').exists()).toBe(true)
      expect(wrapper.find('.success-text').text()).toBe('链接已复制到剪贴板')
    })
  })

  describe('社交分享测试', () => {
    it('应该分享到Twitter', async () => {
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

      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true,
          snippetTitle: '测试代码片段'
        }
      })

      await wrapper.vm.handleCreateShare()
      
      // 点击Twitter分享按钮
      const twitterButton = wrapper.find('.share-option-btn')
      await twitterButton.trigger('click')
      
      expect(window.open).toHaveBeenCalledWith(
        expect.stringContaining('twitter.com/intent/tweet'),
        '_blank',
        'width=600,height=400'
      )
    })

    it('应该分享到Facebook', async () => {
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

      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true,
          snippetTitle: '测试代码片段'
        }
      })

      await wrapper.vm.handleCreateShare()
      
      // 点击Facebook分享按钮（第二个按钮）
      const facebookButton = wrapper.findAll('.share-option-btn')[1]
      await facebookButton.trigger('click')
      
      expect(window.open).toHaveBeenCalledWith(
        expect.stringContaining('facebook.com/sharer/sharer.php'),
        '_blank',
        'width=600,height=400'
      )
    })

    it('应该分享到邮件', async () => {
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

      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true,
          snippetTitle: '测试代码片段'
        }
      })

      await wrapper.vm.handleCreateShare()
      
      // 点击邮件分享按钮（第四个按钮）
      const emailButton = wrapper.findAll('.share-option-btn')[3]
      await emailButton.trigger('click')
      
      expect(window.open).toHaveBeenCalledWith(
        expect.stringContaining('mailto:'),
        '_blank',
        'width=600,height=400'
      )
    })
  })

  describe('事件测试', () => {
    it('应该触发share-created事件', async () => {
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

      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      await wrapper.vm.handleCreateShare()
      
      expect(wrapper.emitted('share-created')).toBeTruthy()
      expect(wrapper.emitted('share-created')[0][0]).toEqual(mockShareToken)
    })

    it('应该触发cancel事件', async () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      await wrapper.find('.dialog-close').trigger('click')
      
      expect(wrapper.emitted('cancel')).toBeTruthy()
    })

    it('应该响应visible属性变化', async () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: false
        }
      })

      // 设置分享令牌
      wrapper.vm.shareToken = {
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

      // 显示对话框
      await wrapper.setProps({ visible: true })
      
      // 表单应该被重置
      expect(wrapper.vm.shareToken).toBe(null)
      expect(wrapper.vm.error).toBe('')
    })
  })

  describe('边界情况测试', () => {
    it('应该处理过期时间计算', async () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      // 测试1小时过期
      wrapper.vm.form.permission = 'VIEW'
      wrapper.vm.form.expiryOption = '1h'
      wrapper.vm.form.expiresAt = ''
      wrapper.vm.form.hasPassword = false
      wrapper.vm.form.password = ''
      wrapper.vm.form.enableAccessLimit = false
      wrapper.vm.form.maxAccessCount = 10
      wrapper.vm.form.allowDownload = true

      const { shareService } = await import('@/services/shareService')
      
      shareService.createShareToken.mockImplementation((request) => {
        const expiresAt = new Date(request.expiresAt!)
        const now = new Date()
        const diffHours = (expiresAt.getTime() - now.getTime()) / (1000 * 60 * 60)
        
        expect(diffHours).toBeCloseTo(1, 0)
        
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
          expiresAt: request.expiresAt,
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

      await wrapper.vm.handleCreateShare()
    })

    it('应该处理最小过期时间', async () => {
      wrapper = mount(ShareDialog, {
        props: {
          snippetId: 'test-snippet-id',
          visible: true
        }
      })

      // 选择自定义过期时间
      const expirySelect = wrapper.findAll('select[class="form-select"]')[1]
      await expirySelect.setValue('custom')
      
      // 检查最小日期时间
      const dateTimeInput = wrapper.find('input[type="datetime-local"]')
      const minDateTime = dateTimeInput.attributes('min')
      
      expect(minDateTime).toBeDefined()
      
      // 验证最小时间是当前时间+1小时
      const minDate = new Date(minDateTime)
      const now = new Date()
      
      // 由于时区问题，我们需要比较本地时间
      // 先获取当前时间的ISO字符串，然后+1小时
      const testNow = new Date()
      testNow.setHours(testNow.getHours() + 1)
      const testMinDateTime = testNow.toISOString().slice(0, 16)
      const testMinDate = new Date(testMinDateTime)
      
      const diffHours = (minDate.getTime() - testMinDate.getTime()) / (1000 * 60 * 60)
      
      // 允许一些时间差异，因为测试执行需要时间
      expect(diffHours).toBeGreaterThan(-0.1)
      expect(diffHours).toBeLessThan(0.1)
    })
  })
})