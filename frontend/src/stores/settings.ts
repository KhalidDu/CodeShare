import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { settingsService } from '@/services/settingsService'
import type { 
  SystemSettings, 
  SiteSettings, 
  SecuritySettings, 
  FeatureSettings, 
  EmailSettings,
  SettingsHistory,
  SettingsHistoryRequest,
  SettingsHistoryResponse,
  SettingsHistoryStatistics,
  SettingsValidationResult,
  UpdateSiteSettingsRequest,
  UpdateSecuritySettingsRequest,
  UpdateFeatureSettingsRequest,
  UpdateEmailSettingsRequest,
  SettingsPageState,
  SettingsHistoryState
} from '@/types/settings'

/**
 * 系统设置状态管理 - 使用Pinia进行状态管理
 */
export const useSettingsStore = defineStore('settings', () => {
  // 设置状态
  const settings = ref<SystemSettings | null>(null)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)
  const initialized = ref(false)
  const lastUpdated = ref<string | null>(null)

  // 页面状态
  const pageState = ref<SettingsPageState>({
    activeTab: 'site',
    loading: false,
    saving: false,
    error: null,
    hasChanges: false,
    originalSettings: null,
    currentSettings: null
  })

  // 历史记录状态
  const historyState = ref<SettingsHistoryState>({
    loading: false,
    items: [],
    totalCount: 0,
    currentPage: 1,
    pageSize: 20,
    filters: {},
    error: null
  })

  // 统计信息
  const statistics = ref<SettingsHistoryStatistics | null>(null)

  // 计算属性
  const isInitialized = computed(() => initialized.value)
  const hasError = computed(() => error.value !== null)
  const isLoading = computed(() => loading.value)
  const isSaving = computed(() => saving.value)
  const hasChanges = computed(() => pageState.value.hasChanges)

  const siteSettings = computed(() => settings.value?.siteSettings || null)
  const securitySettings = computed(() => settings.value?.securitySettings || null)
  const featureSettings = computed(() => settings.value?.featureSettings || null)
  const emailSettings = computed(() => settings.value?.emailSettings || null)

  const historyItems = computed(() => historyState.value.items)
  const historyTotalCount = computed(() => historyState.value.totalCount)
  const historyLoading = computed(() => historyState.value.loading)

  // 基础设置操作
  async function fetchSettings() {
    try {
      loading.value = true
      error.value = null
      
      const fetchedSettings = await settingsService.getSettings()
      settings.value = fetchedSettings
      initialized.value = true
      lastUpdated.value = new Date().toISOString()
      
      // 更新页面状态
      pageState.value.originalSettings = JSON.parse(JSON.stringify(fetchedSettings))
      pageState.value.currentSettings = JSON.parse(JSON.stringify(fetchedSettings))
      pageState.value.hasChanges = false
      
      return fetchedSettings
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取设置失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function fetchOrCreateSettings() {
    try {
      loading.value = true
      error.value = null
      
      const fetchedSettings = await settingsService.getOrCreateSettings()
      settings.value = fetchedSettings
      initialized.value = true
      lastUpdated.value = new Date().toISOString()
      
      // 更新页面状态
      pageState.value.originalSettings = JSON.parse(JSON.stringify(fetchedSettings))
      pageState.value.currentSettings = JSON.parse(JSON.stringify(fetchedSettings))
      pageState.value.hasChanges = false
      
      return fetchedSettings
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取或创建设置失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function updateSettings(newSettings: SystemSettings) {
    try {
      saving.value = true
      error.value = null
      
      const updatedSettings = await settingsService.updateSettings(newSettings)
      settings.value = updatedSettings
      lastUpdated.value = new Date().toISOString()
      
      // 更新页面状态
      pageState.value.originalSettings = JSON.parse(JSON.stringify(updatedSettings))
      pageState.value.currentSettings = JSON.parse(JSON.stringify(updatedSettings))
      pageState.value.hasChanges = false
      
      return updatedSettings
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新设置失败'
      throw err
    } finally {
      saving.value = false
    }
  }

  async function initializeDefaultSettings() {
    try {
      saving.value = true
      error.value = null
      
      const newSettings = await settingsService.initializeDefaultSettings()
      settings.value = newSettings
      initialized.value = true
      lastUpdated.value = new Date().toISOString()
      
      // 更新页面状态
      pageState.value.originalSettings = JSON.parse(JSON.stringify(newSettings))
      pageState.value.currentSettings = JSON.parse(JSON.stringify(newSettings))
      pageState.value.hasChanges = false
      
      return newSettings
    } catch (err) {
      error.value = err instanceof Error ? err.message : '初始化设置失败'
      throw err
    } finally {
      saving.value = false
    }
  }

  async function checkInitialization() {
    try {
      initialized.value = await settingsService.checkInitialization()
      return initialized.value
    } catch (err) {
      error.value = err instanceof Error ? err.message : '检查初始化状态失败'
      return false
    }
  }

  async function refreshCache() {
    try {
      await settingsService.refreshCache()
      // 重新获取设置
      await fetchSettings()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '刷新缓存失败'
      throw err
    }
  }

  // 站点设置
  async function updateSiteSettings(request: UpdateSiteSettingsRequest) {
    try {
      saving.value = true
      error.value = null
      
      const updatedSiteSettings = await settingsService.updateSiteSettings(request)
      
      if (settings.value) {
        settings.value.siteSettings = updatedSiteSettings
        lastUpdated.value = new Date().toISOString()
        
        // 更新页面状态
        if (pageState.value.currentSettings) {
          pageState.value.currentSettings.siteSettings = updatedSiteSettings
        }
        pageState.value.hasChanges = false
      }
      
      return updatedSiteSettings
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新站点设置失败'
      throw err
    } finally {
      saving.value = false
    }
  }

  // 安全设置
  async function updateSecuritySettings(request: UpdateSecuritySettingsRequest) {
    try {
      saving.value = true
      error.value = null
      
      const updatedSecuritySettings = await settingsService.updateSecuritySettings(request)
      
      if (settings.value) {
        settings.value.securitySettings = updatedSecuritySettings
        lastUpdated.value = new Date().toISOString()
        
        // 更新页面状态
        if (pageState.value.currentSettings) {
          pageState.value.currentSettings.securitySettings = updatedSecuritySettings
        }
        pageState.value.hasChanges = false
      }
      
      return updatedSecuritySettings
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新安全设置失败'
      throw err
    } finally {
      saving.value = false
    }
  }

  // 功能设置
  async function updateFeatureSettings(request: UpdateFeatureSettingsRequest) {
    try {
      saving.value = true
      error.value = null
      
      const updatedFeatureSettings = await settingsService.updateFeatureSettings(request)
      
      if (settings.value) {
        settings.value.featureSettings = updatedFeatureSettings
        lastUpdated.value = new Date().toISOString()
        
        // 更新页面状态
        if (pageState.value.currentSettings) {
          pageState.value.currentSettings.featureSettings = updatedFeatureSettings
        }
        pageState.value.hasChanges = false
      }
      
      return updatedFeatureSettings
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新功能设置失败'
      throw err
    } finally {
      saving.value = false
    }
  }

  // 邮件设置
  async function updateEmailSettings(request: UpdateEmailSettingsRequest) {
    try {
      saving.value = true
      error.value = null
      
      const updatedEmailSettings = await settingsService.updateEmailSettings(request)
      
      if (settings.value) {
        settings.value.emailSettings = updatedEmailSettings
        lastUpdated.value = new Date().toISOString()
        
        // 更新页面状态
        if (pageState.value.currentSettings) {
          pageState.value.currentSettings.emailSettings = updatedEmailSettings
        }
        pageState.value.hasChanges = false
      }
      
      return updatedEmailSettings
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新邮件设置失败'
      throw err
    } finally {
      saving.value = false
    }
  }

  // 设置历史
  async function fetchSettingsHistory(request: SettingsHistoryRequest = {}) {
    try {
      historyState.value.loading = true
      historyState.value.error = null
      
      const response = await settingsService.getSettingsHistory(request)
      historyState.value.items = response.items
      historyState.value.totalCount = response.totalCount
      historyState.value.currentPage = response.pageNumber
      historyState.value.pageSize = response.pageSize
      historyState.value.filters = request
      
      return response
    } catch (err) {
      historyState.value.error = err instanceof Error ? err.message : '获取设置历史失败'
      throw err
    } finally {
      historyState.value.loading = false
    }
  }

  async function fetchSettingsHistoryStatistics() {
    try {
      const stats = await settingsService.getSettingsHistoryStatistics()
      statistics.value = stats
      return stats
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取设置历史统计失败'
      throw err
    }
  }

  async function deleteSettingsHistory(historyId: string) {
    try {
      await settingsService.deleteSettingsHistory(historyId)
      // 重新获取历史记录
      await fetchSettingsHistory(historyState.value.filters)
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除设置历史失败'
      throw err
    }
  }

  // 页面状态管理
  function setActiveTab(tab: string) {
    pageState.value.activeTab = tab
  }

  function setCurrentSettings(newSettings: SystemSettings) {
    pageState.value.currentSettings = JSON.parse(JSON.stringify(newSettings))
    checkForChanges()
  }

  function checkForChanges() {
    if (!pageState.value.originalSettings || !pageState.value.currentSettings) {
      pageState.value.hasChanges = false
      return
    }

    const original = JSON.stringify(pageState.value.originalSettings)
    const current = JSON.stringify(pageState.value.currentSettings)
    pageState.value.hasChanges = original !== current
  }

  function resetChanges() {
    if (pageState.value.originalSettings) {
      pageState.value.currentSettings = JSON.parse(JSON.stringify(pageState.value.originalSettings))
      pageState.value.hasChanges = false
    }
  }

  // 导入导出
  async function exportSettings(format: 'json' | 'csv' | 'excel' = 'json') {
    try {
      return await settingsService.exportSettings(format)
    } catch (err) {
      error.value = err instanceof Error ? err.message : '导出设置失败'
      throw err
    }
  }

  async function importSettings(jsonData: string) {
    try {
      saving.value = true
      error.value = null
      
      const importedSettings = await settingsService.importSettings({ jsonData })
      settings.value = importedSettings
      initialized.value = true
      lastUpdated.value = new Date().toISOString()
      
      // 更新页面状态
      pageState.value.originalSettings = JSON.parse(JSON.stringify(importedSettings))
      pageState.value.currentSettings = JSON.parse(JSON.stringify(importedSettings))
      pageState.value.hasChanges = false
      
      return importedSettings
    } catch (err) {
      error.value = err instanceof Error ? err.message : '导入设置失败'
      throw err
    } finally {
      saving.value = false
    }
  }

  // 清理状态
  function $reset() {
    settings.value = null
    loading.value = false
    saving.value = false
    error.value = null
    initialized.value = false
    lastUpdated.value = null
    
    pageState.value = {
      activeTab: 'site',
      loading: false,
      saving: false,
      error: null,
      hasChanges: false,
      originalSettings: null,
      currentSettings: null
    }
    
    historyState.value = {
      loading: false,
      items: [],
      totalCount: 0,
      currentPage: 1,
      pageSize: 20,
      filters: {},
      error: null
    }
    
    statistics.value = null
  }

  return {
    // 状态
    settings,
    loading,
    saving,
    error,
    initialized,
    lastUpdated,
    pageState,
    historyState,
    statistics,
    
    // 计算属性
    isInitialized,
    hasError,
    isLoading,
    isSaving,
    hasChanges,
    siteSettings,
    securitySettings,
    featureSettings,
    emailSettings,
    historyItems,
    historyTotalCount,
    historyLoading,
    
    // 方法
    fetchSettings,
    fetchOrCreateSettings,
    updateSettings,
    initializeDefaultSettings,
    checkInitialization,
    refreshCache,
    updateSiteSettings,
    updateSecuritySettings,
    updateFeatureSettings,
    updateEmailSettings,
    fetchSettingsHistory,
    fetchSettingsHistoryStatistics,
    deleteSettingsHistory,
    setActiveTab,
    setCurrentSettings,
    checkForChanges,
    resetChanges,
    exportSettings,
    importSettings,
    $reset
  }
})