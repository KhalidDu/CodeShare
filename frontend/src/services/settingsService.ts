import { apiClient } from '@/services/apiClient'
import type { 
  SystemSettings, 
  SiteSettings, 
  SecuritySettings, 
  FeatureSettings, 
  EmailSettings,
  UpdateSiteSettingsRequest,
  UpdateSecuritySettingsRequest,
  UpdateFeatureSettingsRequest,
  UpdateEmailSettingsRequest,
  SettingsHistoryRequest,
  SettingsHistoryResponse,
  SettingsHistoryStatistics,
  SettingsValidationResult,
  TestEmailRequest,
  TestEmailResponse,
  ImportSettingsRequest,
  ValidateImportDataRequest,
  SettingsHistoryExportRequest,
  SettingsHistoryExportResponse,
  BatchDeleteSettingsHistoryRequest,
  RestoreFromBackupRequest,
  ApiResponse,
  PaginatedResponse
} from '@/types/settings'

/**
 * 系统设置服务 - 封装所有设置相关的API调用
 */
export class SettingsService {
  private readonly basePath = '/api/systemsettings'

  // 基础设置操作
  async getSettings(): Promise<SystemSettings> {
    const response = await apiClient.get<ApiResponse<SystemSettings>>(this.basePath)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '获取系统设置失败')
    }
    return response.data.data
  }

  async getOrCreateSettings(): Promise<SystemSettings> {
    const response = await apiClient.get<ApiResponse<SystemSettings>>(`${this.basePath}/get-or-create`)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '获取或创建系统设置失败')
    }
    return response.data.data
  }

  async updateSettings(settings: SystemSettings): Promise<SystemSettings> {
    const response = await apiClient.put<ApiResponse<SystemSettings>>(this.basePath, settings)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '更新系统设置失败')
    }
    return response.data.data
  }

  async initializeDefaultSettings(): Promise<SystemSettings> {
    const response = await apiClient.post<ApiResponse<SystemSettings>>(`${this.basePath}/initialize`)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '初始化系统设置失败')
    }
    return response.data.data
  }

  async checkInitialization(): Promise<boolean> {
    const response = await apiClient.get<ApiResponse<boolean>>(`${this.basePath}/check-initialization`)
    return response.data?.data ?? false
  }

  async refreshCache(): Promise<void> {
    await apiClient.post(`${this.basePath}/refresh-cache`)
  }

  // 站点设置
  async getSiteSettings(): Promise<SiteSettings> {
    const response = await apiClient.get<ApiResponse<SiteSettings>>(`${this.basePath}/site`)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '获取站点设置失败')
    }
    return response.data.data
  }

  async updateSiteSettings(request: UpdateSiteSettingsRequest): Promise<SiteSettings> {
    const response = await apiClient.put<ApiResponse<SiteSettings>>(`${this.basePath}/site`, request)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '更新站点设置失败')
    }
    return response.data.data
  }

  async validateSiteSettings(settings: SiteSettings): Promise<SettingsValidationResult> {
    const response = await apiClient.post<ApiResponse<SettingsValidationResult>>(`${this.basePath}/site/validate`, settings)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '验证站点设置失败')
    }
    return response.data.data
  }

  // 安全设置
  async getSecuritySettings(): Promise<SecuritySettings> {
    const response = await apiClient.get<ApiResponse<SecuritySettings>>(`${this.basePath}/security`)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '获取安全设置失败')
    }
    return response.data.data
  }

  async updateSecuritySettings(request: UpdateSecuritySettingsRequest): Promise<SecuritySettings> {
    const response = await apiClient.put<ApiResponse<SecuritySettings>>(`${this.basePath}/security`, request)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '更新安全设置失败')
    }
    return response.data.data
  }

  async validateSecuritySettings(settings: SecuritySettings): Promise<SettingsValidationResult> {
    const response = await apiClient.post<ApiResponse<SettingsValidationResult>>(`${this.basePath}/security/validate`, settings)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '验证安全设置失败')
    }
    return response.data.data
  }

  // 功能设置
  async getFeatureSettings(): Promise<FeatureSettings> {
    const response = await apiClient.get<ApiResponse<FeatureSettings>>(`${this.basePath}/features`)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '获取功能设置失败')
    }
    return response.data.data
  }

  async updateFeatureSettings(request: UpdateFeatureSettingsRequest): Promise<FeatureSettings> {
    const response = await apiClient.put<ApiResponse<FeatureSettings>>(`${this.basePath}/features`, request)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '更新功能设置失败')
    }
    return response.data.data
  }

  async validateFeatureSettings(settings: FeatureSettings): Promise<SettingsValidationResult> {
    const response = await apiClient.post<ApiResponse<SettingsValidationResult>>(`${this.basePath}/features/validate`, settings)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '验证功能设置失败')
    }
    return response.data.data
  }

  // 邮件设置
  async getEmailSettings(): Promise<EmailSettings> {
    const response = await apiClient.get<ApiResponse<EmailSettings>>(`${this.basePath}/email`)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '获取邮件设置失败')
    }
    return response.data.data
  }

  async updateEmailSettings(request: UpdateEmailSettingsRequest): Promise<EmailSettings> {
    const response = await apiClient.put<ApiResponse<EmailSettings>>(`${this.basePath}/email`, request)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '更新邮件设置失败')
    }
    return response.data.data
  }

  async validateEmailSettings(settings: EmailSettings): Promise<SettingsValidationResult> {
    const response = await apiClient.post<ApiResponse<SettingsValidationResult>>(`${this.basePath}/email/validate`, settings)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '验证邮件设置失败')
    }
    return response.data.data
  }

  async sendTestEmail(request: TestEmailRequest): Promise<TestEmailResponse> {
    const response = await apiClient.post<ApiResponse<TestEmailResponse>>(`${this.basePath}/email/test`, request)
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '发送测试邮件失败')
    }
    return response.data.data
  }

  // 设置历史
  async getSettingsHistory(request: SettingsHistoryRequest = {}): Promise<SettingsHistoryResponse> {
    const params = new URLSearchParams()
    Object.entries(request).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        params.append(key, String(value))
      }
    })

    const response = await apiClient.get<ApiResponse<SettingsHistoryResponse>>(
      `${this.basePath}/history?${params.toString()}`
    )
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '获取设置历史失败')
    }
    return response.data.data
  }

  async getSettingsHistoryStatistics(): Promise<SettingsHistoryStatistics> {
    const response = await apiClient.get<ApiResponse<SettingsHistoryStatistics>>(
      `${this.basePath}/history/statistics`
    )
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '获取设置历史统计失败')
    }
    return response.data.data
  }

  async deleteSettingsHistory(historyId: string): Promise<void> {
    await apiClient.delete(`${this.basePath}/history/${historyId}`)
  }

  async batchDeleteSettingsHistory(request: BatchDeleteSettingsHistoryRequest): Promise<number> {
    const response = await apiClient.delete<ApiResponse<number>>(
      `${this.basePath}/history/batch`,
      { data: request }
    )
    if (!response.data?.success || response.data.data === undefined) {
      throw new Error(response.data?.message || '批量删除设置历史失败')
    }
    return response.data.data
  }

  // 导入导出
  async exportSettings(format: 'json' | 'csv' | 'excel' = 'json'): Promise<string> {
    const response = await apiClient.get<string>(
      `${this.basePath}/export?format=${format}`,
      { responseType: 'text' }
    )
    return response.data
  }

  async importSettings(request: ImportSettingsRequest): Promise<SystemSettings> {
    const response = await apiClient.post<ApiResponse<SystemSettings>>(
      `${this.basePath}/import`,
      request
    )
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '导入设置失败')
    }
    return response.data.data
  }

  async validateImportData(request: ValidateImportDataRequest): Promise<SettingsValidationResult> {
    const response = await apiClient.post<ApiResponse<SettingsValidationResult>>(
      `${this.basePath}/import/validate`,
      request
    )
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '验证导入数据失败')
    }
    return response.data.data
  }

  async exportSettingsHistory(request: SettingsHistoryExportRequest): Promise<SettingsHistoryExportResponse> {
    const params = new URLSearchParams()
    Object.entries(request).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        params.append(key, String(value))
      }
    })

    const response = await apiClient.get<ApiResponse<SettingsHistoryExportResponse>>(
      `${this.basePath}/history/export?${params.toString()}`
    )
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '导出设置历史失败')
    }
    return response.data.data
  }

  async generateBackup(includeHistory: boolean = true): Promise<string> {
    const response = await apiClient.get<string>(
      `${this.basePath}/backup?includeHistory=${includeHistory}`,
      { responseType: 'text' }
    )
    return response.data
  }

  async restoreFromBackup(request: RestoreFromBackupRequest): Promise<SystemSettings> {
    const response = await apiClient.post<ApiResponse<SystemSettings>>(
      `${this.basePath}/restore`,
      request
    )
    if (!response.data?.success || !response.data.data) {
      throw new Error(response.data?.message || '从备份恢复设置失败')
    }
    return response.data.data
  }

  // 工具方法
  async downloadSettings(format: 'json' | 'csv' | 'excel' = 'json'): Promise<void> {
    const data = await this.exportSettings(format)
    const blob = new Blob([data], { 
      type: format === 'json' ? 'application/json' : 'text/csv' 
    })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `system_settings_${new Date().toISOString().split('T')[0]}.${format}`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
  }

  async downloadSettingsHistory(request: SettingsHistoryExportRequest): Promise<void> {
    const response = await this.exportSettingsHistory(request)
    if (!response.data) return

    const blob = new Blob([response.data], { 
      type: response.contentType 
    })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = response.fileName
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
  }

  async downloadBackup(includeHistory: boolean = true): Promise<void> {
    const data = await this.generateBackup(includeHistory)
    const blob = new Blob([data], { type: 'application/json' })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `settings_backup_${new Date().toISOString().replace(/[:.]/g, '-')}.json`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
  }
}

// 导出单例实例
export const settingsService = new SettingsService()