import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useSettingsStore } from '@/stores/settings'
import settingsService from '@/services/settingsService'

// Mock the settings service
vi.mock('@/services/settingsService', () => ({
  default: {
    getSettings: vi.fn(),
    updateSiteSettings: vi.fn(),
    updateSecuritySettings: vi.fn(),
    updateFeatureSettings: vi.fn(),
    updateEmailSettings: vi.fn(),
    exportSettings: vi.fn(),
    importSettings: vi.fn(),
    validateImportData: vi.fn(),
    getSettingsHistory: vi.fn(),
    getSettingsStatistics: vi.fn()
  }
}))

// Mock the auth store
vi.mock('@/stores/auth', () => ({
  useAuthStore: vi.fn(() => ({
    user: { id: '1', username: 'admin', role: 'admin' },
    isAuthenticated: true
  }))
}))

describe('SettingsStore', () => {
  let store: any

  beforeEach(() => {
    setActivePinia(createPinia())
    store = useSettingsStore()
  })

  afterEach(() => {
    vi.clearAllMocks()
  })

  describe('初始状态测试', () => {
    it('应该有正确的初始状态', () => {
      expect(store.settings).toBeNull()
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
      expect(store.activeTab).toBe('site')
      expect(store.history).toEqual([])
      expect(store.statistics).toBeNull()
    })
  })

  describe('获取设置测试', () => {
    it('应该成功获取设置', async () => {
      const mockSettings = {
        siteSettings: { siteName: '测试站点' },
        securitySettings: { minPasswordLength: 8 },
        featureSettings: { enableCodeSnippets: true },
        emailSettings: { smtpHost: 'smtp.test.com' }
      }
      
      settingsService.getSettings.mockResolvedValue({
        success: true,
        data: mockSettings
      })

      await store.fetchSettings()

      expect(settingsService.getSettings).toHaveBeenCalled()
      expect(store.settings).toEqual(mockSettings)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
    })

    it('应该处理获取设置失败', async () => {
      const errorMessage = '获取设置失败'
      settingsService.getSettings.mockRejectedValue(new Error(errorMessage))

      await store.fetchSettings()

      expect(settingsService.getSettings).toHaveBeenCalled()
      expect(store.settings).toBeNull()
      expect(store.loading).toBe(false)
      expect(store.error).toBe(errorMessage)
    })

    it('应该处理获取设置时的加载状态', async () => {
      settingsService.getSettings.mockImplementation(() => {
        return new Promise(resolve => {
          setTimeout(() => {
            resolve({
              success: true,
              data: { siteSettings: { siteName: '测试站点' } }
            })
          }, 100)
        })
      })

      const fetchPromise = store.fetchSettings()
      expect(store.loading).toBe(true)
      
      await fetchPromise
      expect(store.loading).toBe(false)
    })
  })

  describe('更新站点设置测试', () => {
    it('应该成功更新站点设置', async () => {
      const siteSettings = { siteName: '新站点名称' }
      const updatedSettings = {
        siteSettings,
        securitySettings: { minPasswordLength: 8 },
        featureSettings: { enableCodeSnippets: true },
        emailSettings: { smtpHost: 'smtp.test.com' }
      }
      
      settingsService.updateSiteSettings.mockResolvedValue({
        success: true,
        data: updatedSettings
      })

      await store.updateSiteSettings(siteSettings)

      expect(settingsService.updateSiteSettings).toHaveBeenCalledWith(siteSettings, 'admin')
      expect(store.settings).toEqual(updatedSettings)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
    })

    it('应该处理更新站点设置失败', async () => {
      const siteSettings = { siteName: '新站点名称' }
      const errorMessage = '更新站点设置失败'
      
      settingsService.updateSiteSettings.mockRejectedValue(new Error(errorMessage))

      await store.updateSiteSettings(siteSettings)

      expect(settingsService.updateSiteSettings).toHaveBeenCalledWith(siteSettings, 'admin')
      expect(store.loading).toBe(false)
      expect(store.error).toBe(errorMessage)
    })
  })

  describe('更新安全设置测试', () => {
    it('应该成功更新安全设置', async () => {
      const securitySettings = { minPasswordLength: 10 }
      const updatedSettings = {
        siteSettings: { siteName: '测试站点' },
        securitySettings,
        featureSettings: { enableCodeSnippets: true },
        emailSettings: { smtpHost: 'smtp.test.com' }
      }
      
      settingsService.updateSecuritySettings.mockResolvedValue({
        success: true,
        data: updatedSettings
      })

      await store.updateSecuritySettings(securitySettings)

      expect(settingsService.updateSecuritySettings).toHaveBeenCalledWith(securitySettings, 'admin')
      expect(store.settings).toEqual(updatedSettings)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
    })
  })

  describe('更新功能设置测试', () => {
    it('应该成功更新功能设置', async () => {
      const featureSettings = { enableCodeSnippets: false }
      const updatedSettings = {
        siteSettings: { siteName: '测试站点' },
        securitySettings: { minPasswordLength: 8 },
        featureSettings,
        emailSettings: { smtpHost: 'smtp.test.com' }
      }
      
      settingsService.updateFeatureSettings.mockResolvedValue({
        success: true,
        data: updatedSettings
      })

      await store.updateFeatureSettings(featureSettings)

      expect(settingsService.updateFeatureSettings).toHaveBeenCalledWith(featureSettings, 'admin')
      expect(store.settings).toEqual(updatedSettings)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
    })
  })

  describe('更新邮件设置测试', () => {
    it('应该成功更新邮件设置', async () => {
      const emailSettings = { smtpHost: 'new.smtp.com' }
      const updatedSettings = {
        siteSettings: { siteName: '测试站点' },
        securitySettings: { minPasswordLength: 8 },
        featureSettings: { enableCodeSnippets: true },
        emailSettings
      }
      
      settingsService.updateEmailSettings.mockResolvedValue({
        success: true,
        data: updatedSettings
      })

      await store.updateEmailSettings(emailSettings)

      expect(settingsService.updateEmailSettings).toHaveBeenCalledWith(emailSettings, 'admin')
      expect(store.settings).toEqual(updatedSettings)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
    })
  })

  describe('导出设置测试', () => {
    it('应该成功导出设置', async () => {
      const mockBlob = new Blob(['{"siteName":"测试站点"}'], { type: 'application/json' })
      const exportResult = {
        success: true,
        data: mockBlob,
        fileName: 'settings-export.json'
      }
      
      settingsService.exportSettings.mockResolvedValue(exportResult)

      const result = await store.exportSettings({
        format: 'json',
        includeSiteSettings: true,
        includeSecuritySettings: true,
        includeFeatureSettings: true,
        includeEmailSettings: true
      })

      expect(settingsService.exportSettings).toHaveBeenCalledWith({
        format: 'json',
        includeSiteSettings: true,
        includeSecuritySettings: true,
        includeFeatureSettings: true,
        includeEmailSettings: true
      })
      expect(result).toEqual(exportResult)
    })
  })

  describe('导入设置测试', () => {
    it('应该成功导入设置', async () => {
      const importData = {
        jsonData: '{"siteName":"导入站点"}',
        format: 'json',
        mode: 'merge',
        overwriteExisting: false
      }
      
      const importResult = {
        success: true,
        message: '导入成功',
        data: {
          siteSettings: { siteName: '导入站点' },
          securitySettings: { minPasswordLength: 8 },
          featureSettings: { enableCodeSnippets: true },
          emailSettings: { smtpHost: 'smtp.test.com' }
        }
      }
      
      settingsService.importSettings.mockResolvedValue(importResult)

      const result = await store.importSettings(importData)

      expect(settingsService.importSettings).toHaveBeenCalledWith(importData)
      expect(result).toEqual(importResult)
      expect(store.settings).toEqual(importResult.data)
    })

    it('应该处理导入设置失败', async () => {
      const importData = {
        jsonData: '{"siteName":"导入站点"}',
        format: 'json',
        mode: 'merge',
        overwriteExisting: false
      }
      
      const errorMessage = '导入失败'
      settingsService.importSettings.mockRejectedValue(new Error(errorMessage))

      const result = await store.importSettings(importData)

      expect(settingsService.importSettings).toHaveBeenCalledWith(importData)
      expect(result.success).toBe(false)
      expect(result.error).toBe(errorMessage)
    })
  })

  describe('验证导入数据测试', () => {
    it('应该成功验证导入数据', async () => {
      const validationData = {
        jsonData: '{"siteName":"测试站点"}',
        format: 'json'
      }
      
      const validationResult = {
        valid: true,
        errors: [],
        warnings: []
      }
      
      settingsService.validateImportData.mockResolvedValue(validationResult)

      const result = await store.validateImportData(validationData)

      expect(settingsService.validateImportData).toHaveBeenCalledWith(validationData)
      expect(result).toEqual(validationResult)
    })

    it('应该处理验证失败的情况', async () => {
      const validationData = {
        jsonData: '{"siteName":""}',
        format: 'json'
      }
      
      const validationResult = {
        valid: false,
        errors: ['站点名称不能为空'],
        warnings: []
      }
      
      settingsService.validateImportData.mockResolvedValue(validationResult)

      const result = await store.validateImportData(validationData)

      expect(settingsService.validateImportData).toHaveBeenCalledWith(validationData)
      expect(result).toEqual(validationResult)
    })
  })

  describe('获取设置历史测试', () => {
    it('应该成功获取设置历史', async () => {
      const mockHistory = [
        {
          id: '1',
          settingType: 'Site',
          settingKey: 'siteName',
          oldValue: '旧站点',
          newValue: '新站点',
          changedBy: 'admin',
          createdAt: new Date().toISOString()
        }
      ]
      
      settingsService.getSettingsHistory.mockResolvedValue({
        success: true,
        data: {
          items: mockHistory,
          totalCount: 1,
          pageNumber: 1,
          pageSize: 20
        }
      })

      await store.fetchSettingsHistory({
        pageNumber: 1,
        pageSize: 20
      })

      expect(settingsService.getSettingsHistory).toHaveBeenCalledWith({
        pageNumber: 1,
        pageSize: 20
      })
      expect(store.history).toEqual(mockHistory)
    })
  })

  describe('获取设置统计测试', () => {
    it('应该成功获取设置统计', async () => {
      const mockStatistics = {
        totalChanges: 10,
        todayChanges: 2,
        thisWeekChanges: 5,
        thisMonthChanges: 8,
        mostActiveUser: 'admin',
        mostChangedSetting: 'siteName'
      }
      
      settingsService.getSettingsStatistics.mockResolvedValue({
        success: true,
        data: mockStatistics
      })

      await store.fetchSettingsStatistics()

      expect(settingsService.getSettingsStatistics).toHaveBeenCalled()
      expect(store.statistics).toEqual(mockStatistics)
    })
  })

  describe('选项卡切换测试', () => {
    it('应该能够切换活动选项卡', () => {
      store.setActiveTab('security')
      expect(store.activeTab).toBe('security')
      
      store.setActiveTab('feature')
      expect(store.activeTab).toBe('feature')
    })
  })

  describe('错误清除测试', () => {
    it('应该能够清除错误', () => {
      store.error = '测试错误'
      store.clearError()
      expect(store.error).toBe(null)
    })
  })

  describe('状态重置测试', () => {
    it('应该能够重置状态', () => {
      store.settings = { siteSettings: { siteName: '测试站点' } }
      store.loading = true
      store.error = '测试错误'
      store.activeTab = 'security'
      store.history = [{ id: '1' }]
      store.statistics = { totalChanges: 10 }
      
      store.reset()
      
      expect(store.settings).toBeNull()
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
      expect(store.activeTab).toBe('site')
      expect(store.history).toEqual([])
      expect(store.statistics).toBeNull()
    })
  })
})