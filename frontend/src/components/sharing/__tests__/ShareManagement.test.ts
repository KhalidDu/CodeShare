import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import ShareManagement from '../ShareManagement.vue'

// Mock the stores
vi.mock('@/stores/share', () => ({
  useShareStore: vi.fn(() => ({
    shareTokens: [],
    activeShareTokens: [],
    expiredShareTokens: [],
    revokedShareTokens: [],
    totalCount: 0,
    currentPage: 1,
    pageSize: 10,
    isLoading: false,
    isCreating: false,
    isUpdating: false,
    isDeleting: false,
    error: null,
    fetchShareTokens: vi.fn(),
    revokeShareToken: vi.fn(),
    deleteShareToken: vi.fn(),
    batchShareTokens: vi.fn(),
    getShareLink: vi.fn(),
    copyShareLink: vi.fn()
  }))
}))

vi.mock('@/stores/toast', () => ({
  useToastStore: vi.fn(() => ({
    success: vi.fn(),
    error: vi.fn(),
    info: vi.fn()
  }))
}))

vi.mock('@/stores/auth', () => ({
  useAuthStore: vi.fn(() => ({
    isAuthenticated: true,
    user: { id: 'user1', name: 'Test User', isAdmin: false }
  }))
}))

// Mock the clipboard API
Object.assign(navigator, {
  clipboard: {
    writeText: vi.fn()
  }
})

describe('ShareManagement', () => {
  let wrapper: any
  let shareStore: any
  let toastStore: any
  let authStore: any

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    
    // 获取store实例
    const { useShareStore } = require('@/stores/share')
    const { useToastStore } = require('@/stores/toast')
    const { useAuthStore } = require('@/stores/auth')
    
    shareStore = useShareStore()
    toastStore = useToastStore()
    authStore = useAuthStore()
  })

  afterEach(() => {
    if (wrapper) {
      wrapper.unmount()
    }
  })

  describe('基本渲染测试', () => {
    it('应该能够挂载组件', () => {
      wrapper = mount(ShareManagement)
      
      expect(wrapper.exists()).toBe(true)
      expect(wrapper.find('.share-management').exists()).toBe(true)
    })

    it('应该显示页面标题', () => {
      wrapper = mount(ShareManagement)
      
      expect(wrapper.find('.page-title').text()).toBe('分享管理')
      expect(wrapper.find('.page-description').text()).toBe('管理您创建的代码分享链接')
    })

    it('应该显示搜索栏', () => {
      wrapper = mount(ShareManagement)
      
      expect(wrapper.find('.search-bar').exists()).toBe(true)
      expect(wrapper.find('.search-input').exists()).toBe(true)
    })

    it('应该显示筛选控件', () => {
      wrapper = mount(ShareManagement)
      
      expect(wrapper.find('.filter-controls').exists()).toBe(true)
      expect(wrapper.find('.filter-select').exists()).toBe(true)
    })

    it('应该显示统计信息', () => {
      wrapper = mount(ShareManagement)
      
      expect(wrapper.find('.stats-section').exists()).toBe(true)
      expect(wrapper.find('.stat-card').exists()).toBe(true)
    })

    it('应该显示分享列表', () => {
      wrapper = mount(ShareManagement)
      
      expect(wrapper.find('.share-list').exists()).toBe(true)
      expect(wrapper.find('.table-header').exists()).toBe(true)
    })
  })

  describe('搜索功能测试', () => {
    it('应该能够输入搜索关键词', async () => {
      wrapper = mount(ShareManagement)
      
      const searchInput = wrapper.find('.search-input')
      await searchInput.setValue('test search')
      
      expect(wrapper.vm.searchQuery).toBe('test search')
    })

    it('应该清除搜索内容', async () => {
      wrapper = mount(ShareManagement)
      
      const searchInput = wrapper.find('.search-input')
      await searchInput.setValue('test search')
      
      const clearButton = wrapper.find('.clear-search-btn')
      await clearButton.trigger('click')
      
      expect(wrapper.vm.searchQuery).toBe('')
    })

    it('应该防抖搜索', async () => {
      wrapper = mount(ShareManagement)
      
      const searchInput = wrapper.find('.search-input')
      await searchInput.setValue('test search')
      
      // 等待防抖时间
      await new Promise(resolve => setTimeout(resolve, 350))
      
      expect(shareStore.fetchShareTokens).toHaveBeenCalledWith({
        search: 'test search',
        page: 1,
        pageSize: 10
      })
    })

    it('应该响应回车键搜索', async () => {
      wrapper = mount(ShareManagement)
      
      const searchInput = wrapper.find('.search-input')
      await searchInput.setValue('test search')
      await searchInput.trigger('keyup.enter')
      
      expect(shareStore.fetchShareTokens).toHaveBeenCalledWith({
        search: 'test search',
        page: 1,
        pageSize: 10
      })
    })
  })

  describe('筛选功能测试', () => {
    it('应该能够按状态筛选', async () => {
      wrapper = mount(ShareManagement)
      
      const statusSelect = wrapper.find('.filter-select').at(0)
      await statusSelect.setValue('ACTIVE')
      
      expect(wrapper.vm.selectedStatus).toBe('ACTIVE')
      expect(shareStore.fetchShareTokens).toHaveBeenCalledWith({
        status: 'ACTIVE',
        page: 1,
        pageSize: 10
      })
    })

    it('应该能够按权限筛选', async () => {
      wrapper = mount(ShareManagement)
      
      const permissionSelect = wrapper.find('.filter-select').at(1)
      await permissionSelect.setValue('EDIT')
      
      expect(wrapper.vm.selectedPermission).toBe('EDIT')
      expect(shareStore.fetchShareTokens).toHaveBeenCalledWith({
        permission: 'EDIT',
        page: 1,
        pageSize: 10
      })
    })

    it('应该能够按排序方式筛选', async () => {
      wrapper = mount(ShareManagement)
      
      const sortSelect = wrapper.find('.filter-select').at(2)
      await sortSelect.setValue('accessCount_desc')
      
      expect(wrapper.vm.sortBy).toBe('accessCount_desc')
      expect(shareStore.fetchShareTokens).toHaveBeenCalledWith({
        sortBy: 'accessCount',
        sortOrder: 'desc',
        page: 1,
        pageSize: 10
      })
    })

    it('应该显示清除筛选按钮', async () => {
      wrapper = mount(ShareManagement)
      
      // 设置筛选条件
      await wrapper.setData({
        searchQuery: 'test',
        selectedStatus: 'ACTIVE',
        selectedPermission: 'EDIT'
      })
      
      expect(wrapper.find('.clear-filters-btn').exists()).toBe(true)
    })

    it('应该清除所有筛选条件', async () => {
      wrapper = mount(ShareManagement)
      
      // 设置筛选条件
      await wrapper.setData({
        searchQuery: 'test',
        selectedStatus: 'ACTIVE',
        selectedPermission: 'EDIT',
        sortBy: 'accessCount_desc'
      })
      
      const clearButton = wrapper.find('.clear-filters-btn')
      await clearButton.trigger('click')
      
      expect(wrapper.vm.searchQuery).toBe('')
      expect(wrapper.vm.selectedStatus).toBe('')
      expect(wrapper.vm.selectedPermission).toBe('')
      expect(wrapper.vm.sortBy).toBe('createdAt_desc')
      expect(shareStore.fetchShareTokens).toHaveBeenCalled()
    })
  })

  describe('分享列表测试', () => {
    beforeEach(() => {
      // Mock分享令牌数据
      shareStore.shareTokens = [
        {
          id: 'token1',
          token: 'abc123',
          snippetId: 'snippet1',
          snippetTitle: '测试代码片段1',
          snippetLanguage: 'JavaScript',
          permission: 'VIEW',
          status: 'ACTIVE',
          createdAt: '2024-01-01T00:00:00Z',
          createdBy: 'user1',
          currentAccessCount: 10,
          maxAccessCount: 100,
          expiresAt: null,
          password: null,
          allowDownload: true
        },
        {
          id: 'token2',
          token: 'def456',
          snippetId: 'snippet2',
          snippetTitle: '测试代码片段2',
          snippetLanguage: 'Python',
          permission: 'EDIT',
          status: 'EXPIRED',
          createdAt: '2024-01-02T00:00:00Z',
          createdBy: 'user1',
          currentAccessCount: 5,
          maxAccessCount: null,
          expiresAt: '2024-01-10T00:00:00Z',
          password: 'test123',
          allowDownload: false
        }
      ]
    })

    it('应该显示分享令牌列表', () => {
      wrapper = mount(ShareManagement)
      
      const tableRows = wrapper.findAll('.table-row')
      expect(tableRows.length).toBe(2)
    })

    it('应该显示分享令牌信息', () => {
      wrapper = mount(ShareManagement)
      
      const firstRow = wrapper.findAll('.table-row')[0]
      expect(firstRow.find('.snippet-title').text()).toBe('测试代码片段1')
      expect(firstRow.find('.snippet-language').text()).toBe('JavaScript')
      expect(firstRow.find('.token-code').text()).toBe('abc123')
    })

    it('应该显示状态徽章', () => {
      wrapper = mount(ShareManagement)
      
      const firstRow = wrapper.findAll('.table-row')[0]
      const statusBadge = firstRow.find('.status-badge')
      expect(statusBadge.exists()).toBe(true)
      expect(statusBadge.text()).toBe('活跃')
      expect(statusBadge.classes()).toContain('active')
    })

    it('应该显示权限徽章', () => {
      wrapper = mount(ShareManagement)
      
      const firstRow = wrapper.findAll('.table-row')[0]
      const permissionBadge = firstRow.find('.permission-badge')
      expect(permissionBadge.exists()).toBe(true)
      expect(permissionBadge.text()).toBe('仅查看')
      expect(permissionBadge.classes()).toContain('view')
    })

    it('应该显示访问统计', () => {
      wrapper = mount(ShareManagement)
      
      const firstRow = wrapper.findAll('.table-row')[0]
      const accessCount = firstRow.find('.access-count')
      expect(accessCount.text()).toContain('10')
      expect(accessCount.text()).toContain('/ 100')
    })

    it('应该显示过期状态', () => {
      wrapper = mount(ShareManagement)
      
      const secondRow = wrapper.findAll('.table-row')[1]
      expect(secondRow.classes()).toContain('expired')
      const statusBadge = secondRow.find('.status-badge')
      expect(statusBadge.text()).toBe('已过期')
      expect(statusBadge.classes()).toContain('expired')
    })
  })

  describe('批量操作测试', () => {
    beforeEach(() => {
      shareStore.shareTokens = [
        {
          id: 'token1',
          token: 'abc123',
          snippetId: 'snippet1',
          snippetTitle: '测试代码片段1',
          snippetLanguage: 'JavaScript',
          permission: 'VIEW',
          status: 'ACTIVE',
          createdAt: '2024-01-01T00:00:00Z',
          createdBy: 'user1',
          currentAccessCount: 10,
          maxAccessCount: 100,
          expiresAt: null,
          password: null,
          allowDownload: true
        },
        {
          id: 'token2',
          token: 'def456',
          snippetId: 'snippet2',
          snippetTitle: '测试代码片段2',
          snippetLanguage: 'Python',
          permission: 'EDIT',
          status: 'ACTIVE',
          createdAt: '2024-01-02T00:00:00Z',
          createdBy: 'user1',
          currentAccessCount: 5,
          maxAccessCount: null,
          expiresAt: null,
          password: null,
          allowDownload: false
        }
      ]
    })

    it('应该显示批量操作栏', async () => {
      wrapper = mount(ShareManagement)
      
      // 选择第一个令牌
      await wrapper.vm.handleTokenSelect('token1')
      
      expect(wrapper.find('.bulk-actions').exists()).toBe(true)
      expect(wrapper.find('.bulk-count').text()).toBe('已选择 1 个分享链接')
    })

    it('应该能够选择单个令牌', async () => {
      wrapper = mount(ShareManagement)
      
      await wrapper.vm.handleTokenSelect('token1')
      
      expect(wrapper.vm.selectedTokens).toContain('token1')
      expect(wrapper.find('.table-row').classes()).toContain('selected')
    })

    it('应该能够选择所有令牌', async () => {
      wrapper = mount(ShareManagement)
      
      const selectAllCheckbox = wrapper.find('.checkbox-cell input[type="checkbox"]')
      await selectAllCheckbox.setChecked(true)
      
      expect(wrapper.vm.selectedTokens).toEqual(['token1', 'token2'])
    })

    it('应该显示全选状态', async () => {
      wrapper = mount(ShareManagement)
      
      const selectAllCheckbox = wrapper.find('.checkbox-cell input[type="checkbox"]')
      
      // 初始状态
      expect(selectAllCheckbox.element.checked).toBe(false)
      expect(selectAllCheckbox.element.indeterminate).toBe(false)
      
      // 选择一个令牌
      await wrapper.vm.handleTokenSelect('token1')
      expect(selectAllCheckbox.element.indeterminate).toBe(true)
      
      // 选择所有令牌
      await wrapper.vm.handleTokenSelect('token2')
      expect(selectAllCheckbox.element.checked).toBe(true)
      expect(selectAllCheckbox.element.indeterminate).toBe(false)
    })

    it('应该能够取消选择', async () => {
      wrapper = mount(ShareManagement)
      
      // 选择令牌
      await wrapper.vm.handleTokenSelect('token1')
      expect(wrapper.vm.selectedTokens).toContain('token1')
      
      // 取消选择
      await wrapper.vm.handleTokenSelect('token1')
      expect(wrapper.vm.selectedTokens).not.toContain('token1')
    })

    it('应该能够清除所有选择', async () => {
      wrapper = mount(ShareManagement)
      
      // 选择所有令牌
      await wrapper.vm.handleTokenSelect('token1')
      await wrapper.vm.handleTokenSelect('token2')
      expect(wrapper.vm.selectedTokens).toEqual(['token1', 'token2'])
      
      // 清除选择
      const clearButton = wrapper.find('.bulk-btn.clear')
      await clearButton.trigger('click')
      
      expect(wrapper.vm.selectedTokens).toEqual([])
    })
  })

  describe('操作按钮测试', () => {
    beforeEach(() => {
      shareStore.shareTokens = [
        {
          id: 'token1',
          token: 'abc123',
          snippetId: 'snippet1',
          snippetTitle: '测试代码片段1',
          snippetLanguage: 'JavaScript',
          permission: 'VIEW',
          status: 'ACTIVE',
          createdAt: '2024-01-01T00:00:00Z',
          createdBy: 'user1',
          currentAccessCount: 10,
          maxAccessCount: 100,
          expiresAt: null,
          password: null,
          allowDownload: true
        }
      ]
    })

    it('应该能够复制令牌', async () => {
      wrapper = mount(ShareManagement)
      
      const copyButton = wrapper.find('.copy-btn')
      await copyButton.trigger('click')
      
      expect(navigator.clipboard.writeText).toHaveBeenCalledWith('abc123')
      expect(toastStore.success).toHaveBeenCalledWith('令牌已复制到剪贴板')
    })

    it('应该能够复制分享链接', async () => {
      shareStore.getShareLink.mockReturnValue('https://example.com/share/abc123')
      
      wrapper = mount(ShareManagement)
      
      const copyLinkButton = wrapper.find('.action-btn.copy-link')
      await copyLinkButton.trigger('click')
      
      expect(navigator.clipboard.writeText).toHaveBeenCalledWith('https://example.com/share/abc123')
      expect(toastStore.success).toHaveBeenCalledWith('分享链接已复制到剪贴板')
    })

    it('应该能够查看统计', async () => {
      wrapper = mount(ShareManagement)
      
      const statsButton = wrapper.find('.action-btn.view-stats')
      await statsButton.trigger('click')
      
      expect(wrapper.vm.showStatsDialog).toBe(true)
      expect(wrapper.vm.selectedStatsTokenId).toBe('token1')
    })

    it('应该能够撤销分享', async () => {
      wrapper = mount(ShareManagement)
      
      const revokeButton = wrapper.find('.action-btn.revoke')
      await revokeButton.trigger('click')
      
      expect(wrapper.vm.showConfirmDialog).toBe(true)
      expect(wrapper.vm.confirmDialog.action).toBe('revoke')
      expect(wrapper.vm.confirmDialog.data.tokenId).toBe('token1')
    })

    it('应该能够删除分享', async () => {
      wrapper = mount(ShareManagement)
      
      const deleteButton = wrapper.find('.action-btn.delete')
      await deleteButton.trigger('click')
      
      expect(wrapper.vm.showConfirmDialog).toBe(true)
      expect(wrapper.vm.confirmDialog.action).toBe('delete')
      expect(wrapper.vm.confirmDialog.data.tokenId).toBe('token1')
    })
  })

  describe('批量操作确认测试', () => {
    beforeEach(() => {
      shareStore.shareTokens = [
        {
          id: 'token1',
          token: 'abc123',
          snippetId: 'snippet1',
          snippetTitle: '测试代码片段1',
          snippetLanguage: 'JavaScript',
          permission: 'VIEW',
          status: 'ACTIVE',
          createdAt: '2024-01-01T00:00:00Z',
          createdBy: 'user1',
          currentAccessCount: 10,
          maxAccessCount: 100,
          expiresAt: null,
          password: null,
          allowDownload: true
        },
        {
          id: 'token2',
          token: 'def456',
          snippetId: 'snippet2',
          snippetTitle: '测试代码片段2',
          snippetLanguage: 'Python',
          permission: 'EDIT',
          status: 'ACTIVE',
          createdAt: '2024-01-02T00:00:00Z',
          createdBy: 'user1',
          currentAccessCount: 5,
          maxAccessCount: null,
          expiresAt: null,
          password: null,
          allowDownload: false
        }
      ]
    })

    it('应该能够批量撤销', async () => {
      wrapper = mount(ShareManagement)
      
      // 选择所有令牌
      await wrapper.setData({ selectedTokens: ['token1', 'token2'] })
      
      const bulkRevokeButton = wrapper.find('.bulk-btn.revoke')
      await bulkRevokeButton.trigger('click')
      
      expect(wrapper.vm.showConfirmDialog).toBe(true)
      expect(wrapper.vm.confirmDialog.action).toBe('bulkRevoke')
      expect(wrapper.vm.confirmDialog.data.tokenIds).toEqual(['token1', 'token2'])
    })

    it('应该能够批量删除', async () => {
      wrapper = mount(ShareManagement)
      
      // 选择所有令牌
      await wrapper.setData({ selectedTokens: ['token1', 'token2'] })
      
      const bulkDeleteButton = wrapper.find('.bulk-btn.delete')
      await bulkDeleteButton.trigger('click')
      
      expect(wrapper.vm.showConfirmDialog).toBe(true)
      expect(wrapper.vm.confirmDialog.action).toBe('bulkDelete')
      expect(wrapper.vm.confirmDialog.data.tokenIds).toEqual(['token1', 'token2'])
    })
  })

  describe('确认对话框测试', () => {
    beforeEach(() => {
      shareStore.shareTokens = [
        {
          id: 'token1',
          token: 'abc123',
          snippetId: 'snippet1',
          snippetTitle: '测试代码片段1',
          snippetLanguage: 'JavaScript',
          permission: 'VIEW',
          status: 'ACTIVE',
          createdAt: '2024-01-01T00:00:00Z',
          createdBy: 'user1',
          currentAccessCount: 10,
          maxAccessCount: 100,
          expiresAt: null,
          password: null,
          allowDownload: true
        }
      ]
    })

    it('应该显示撤销确认对话框', async () => {
      wrapper = mount(ShareManagement)
      
      // 设置确认对话框状态
      await wrapper.setData({
        showConfirmDialog: true,
        confirmDialog: {
          title: '撤销分享',
          message: '确定要撤销这个分享链接吗？撤销后将无法访问。',
          confirmText: '撤销',
          cancelText: '取消',
          type: 'warning',
          action: 'revoke',
          data: { tokenId: 'token1' }
        }
      })
      
      expect(wrapper.find('.confirm-dialog').exists()).toBe(true)
      expect(wrapper.find('.confirm-dialog .title').text()).toBe('撤销分享')
    })

    it('应该能够确认操作', async () => {
      wrapper = mount(ShareManagement)
      
      // 设置确认对话框状态
      await wrapper.setData({
        showConfirmDialog: true,
        confirmDialog: {
          title: '撤销分享',
          message: '确定要撤销这个分享链接吗？撤销后将无法访问。',
          confirmText: '撤销',
          cancelText: '取消',
          type: 'warning',
          action: 'revoke',
          data: { tokenId: 'token1' }
        }
      })
      
      // 确认操作
      await wrapper.vm.handleConfirm()
      
      expect(shareStore.revokeShareToken).toHaveBeenCalledWith({
        tokenId: 'token1'
      })
      expect(toastStore.success).toHaveBeenCalledWith('分享链接已撤销')
      expect(wrapper.vm.showConfirmDialog).toBe(false)
    })

    it('应该能够取消操作', async () => {
      wrapper = mount(ShareManagement)
      
      // 设置确认对话框状态
      await wrapper.setData({
        showConfirmDialog: true,
        confirmDialog: {
          title: '撤销分享',
          message: '确定要撤销这个分享链接吗？撤销后将无法访问。',
          confirmText: '撤销',
          cancelText: '取消',
          type: 'warning',
          action: 'revoke',
          data: { tokenId: 'token1' }
        }
      })
      
      // 取消操作
      await wrapper.vm.hideConfirmDialog()
      
      expect(wrapper.vm.showConfirmDialog).toBe(false)
      expect(wrapper.vm.confirmDialog.action).toBe('')
    })
  })

  describe('统计对话框测试', () => {
    beforeEach(() => {
      shareStore.shareTokens = [
        {
          id: 'token1',
          token: 'abc123',
          snippetId: 'snippet1',
          snippetTitle: '测试代码片段1',
          snippetLanguage: 'JavaScript',
          permission: 'VIEW',
          status: 'ACTIVE',
          createdAt: '2024-01-01T00:00:00Z',
          createdBy: 'user1',
          currentAccessCount: 10,
          maxAccessCount: 100,
          expiresAt: null,
          password: null,
          allowDownload: true
        }
      ]
    })

    it('应该显示统计对话框', async () => {
      wrapper = mount(ShareManagement)
      
      // 设置统计对话框状态
      await wrapper.setData({
        showStatsDialog: true,
        selectedStatsTokenId: 'token1'
      })
      
      expect(wrapper.find('.stats-dialog').exists()).toBe(true)
      expect(wrapper.find('.stats-dialog-title').text()).toBe('分享统计')
    })

    it('应该能够关闭统计对话框', async () => {
      wrapper = mount(ShareManagement)
      
      // 设置统计对话框状态
      await wrapper.setData({
        showStatsDialog: true,
        selectedStatsTokenId: 'token1'
      })
      
      // 关闭对话框
      await wrapper.vm.closeStatsDialog()
      
      expect(wrapper.vm.showStatsDialog).toBe(false)
      expect(wrapper.vm.selectedStatsTokenId).toBe('')
    })

    it('应该能够从统计对话框撤销分享', async () => {
      wrapper = mount(ShareManagement)
      
      // 设置统计对话框状态
      await wrapper.setData({
        showStatsDialog: true,
        selectedStatsTokenId: 'token1'
      })
      
      // 从统计对话框撤销分享
      await wrapper.vm.handleRevokeFromStats('token1')
      
      expect(wrapper.vm.showStatsDialog).toBe(false)
      expect(wrapper.vm.showConfirmDialog).toBe(true)
      expect(wrapper.vm.confirmDialog.action).toBe('revoke')
      expect(wrapper.vm.confirmDialog.data.tokenId).toBe('token1')
    })
  })

  describe('空状态测试', () => {
    it('应该显示空状态', () => {
      shareStore.shareTokens = []
      shareStore.isLoading = false
      
      wrapper = mount(ShareManagement)
      
      expect(wrapper.find('.empty-state').exists()).toBe(true)
      expect(wrapper.find('.empty-title').text()).toBe('暂无分享链接')
      expect(wrapper.find('.empty-description').text()).toBe('您还没有创建任何分享链接')
    })

    it('应该显示加载状态', () => {
      shareStore.isLoading = true
      
      wrapper = mount(ShareManagement)
      
      expect(wrapper.find('.loading-state').exists()).toBe(true)
      expect(wrapper.find('.loading-text').text()).toBe('加载中...')
    })
  })

  describe('格式化功能测试', () => {
    beforeEach(() => {
      shareStore.shareTokens = [
        {
          id: 'token1',
          token: 'abc123',
          snippetId: 'snippet1',
          snippetTitle: '测试代码片段1',
          snippetLanguage: 'JavaScript',
          permission: 'VIEW',
          status: 'ACTIVE',
          createdAt: new Date(Date.now() - 3600000).toISOString(), // 1小时前
          createdBy: 'user1',
          currentAccessCount: 10,
          maxAccessCount: 100,
          expiresAt: null,
          password: null,
          allowDownload: true,
          lastAccessedAt: new Date(Date.now() - 1800000).toISOString() // 30分钟前
        }
      ]
    })

    it('应该格式化时间显示', () => {
      wrapper = mount(ShareManagement)
      
      const firstRow = wrapper.find('.table-row')
      const createDate = firstRow.find('.create-date').text()
      const lastAccess = firstRow.find('.last-access').text()
      
      expect(createDate).toBe('1小时前')
      expect(lastAccess).toBe('30分钟前')
    })

    it('应该显示状态标签', () => {
      wrapper = mount(ShareManagement)
      
      const statusLabel = wrapper.vm.getStatusLabel('ACTIVE')
      expect(statusLabel).toBe('活跃')
      
      const permissionLabel = wrapper.vm.getPermissionLabel('VIEW')
      expect(permissionLabel).toBe('仅查看')
    })
  })

  describe '错误处理测试', () => {
    it('应该处理复制失败', async () => {
      navigator.clipboard.writeText.mockRejectedValue(new Error('复制失败'))
      
      wrapper = mount(ShareManagement)
      
      const copyButton = wrapper.find('.copy-btn')
      await copyButton.trigger('click')
      
      expect(toastStore.error).toHaveBeenCalledWith('复制令牌失败')
    })

    it('应该处理API错误', async () => {
      shareStore.revokeShareToken.mockRejectedValue(new Error('撤销失败'))
      
      wrapper = mount(ShareManagement)
      
      // 设置确认对话框状态
      await wrapper.setData({
        showConfirmDialog: true,
        confirmDialog: {
          title: '撤销分享',
          message: '确定要撤销这个分享链接吗？撤销后将无法访问。',
          confirmText: '撤销',
          cancelText: '取消',
          type: 'warning',
          action: 'revoke',
          data: { tokenId: 'token1' }
        }
      })
      
      // 确认操作
      await wrapper.vm.handleConfirm()
      
      expect(toastStore.error).toHaveBeenCalledWith('操作失败')
    })
  })

  describe('响应式设计测试', () => {
    beforeEach(() => {
      shareStore.shareTokens = [
        {
          id: 'token1',
          token: 'abc123',
          snippetId: 'snippet1',
          snippetTitle: '测试代码片段1',
          snippetLanguage: 'JavaScript',
          permission: 'VIEW',
          status: 'ACTIVE',
          createdAt: '2024-01-01T00:00:00Z',
          createdBy: 'user1',
          currentAccessCount: 10,
          maxAccessCount: 100,
          expiresAt: null,
          password: null,
          allowDownload: true
        }
      ]
    })

    it('应该在移动端隐藏某些列', () => {
      wrapper = mount(ShareManagement)
      
      // 模拟移动端视图
      global.innerWidth = 500
      global.dispatchEvent(new Event('resize'))
      
      // 重新渲染组件
      wrapper = mount(ShareManagement)
      
      // 检查是否隐藏了某些列
      expect(wrapper.find('.access-cell').exists()).toBe(false)
      expect(wrapper.find('.date-cell').exists()).toBe(false)
      expect(wrapper.find('.actions-cell').exists()).toBe(false)
    })
  })

  describe('权限控制测试', () => {
    it('应该根据用户权限显示操作按钮', () => {
      wrapper = mount(ShareManagement)
      
      const revokeButton = wrapper.find('.action-btn.revoke')
      const deleteButton = wrapper.find('.action-btn.delete')
      
      // 活跃状态的令牌应该显示撤销按钮
      expect(revokeButton.exists()).toBe(true)
      
      // 所有令牌都应该显示删除按钮
      expect(deleteButton.exists()).toBe(true)
    })

    it('应该在加载时禁用按钮', () => {
      shareStore.isUpdating = true
      
      wrapper = mount(ShareManagement)
      
      const revokeButton = wrapper.find('.action-btn.revoke')
      const deleteButton = wrapper.find('.action-btn.delete')
      
      expect(revokeButton.attributes('disabled')).toBeDefined()
      expect(deleteButton.attributes('disabled')).not.toBeDefined()
    })

    it('应该在删除时禁用按钮', () => {
      shareStore.isDeleting = true
      
      wrapper = mount(ShareManagement)
      
      const deleteButton = wrapper.find('.action-btn.delete')
      
      expect(deleteButton.attributes('disabled')).toBeDefined()
    })
  })
})